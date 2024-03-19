using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yohash.ContinuumCrowds
{
  /// <summary>
  /// Continuum Crowds dynamic global fields.
  /// </summary>
  public static class DynamicGlobalFields
  {
    /// <summary>
    /// Initializes a tile with all baseline speed and cost fields. These
    /// values are computed for the tile's topology. These baseline fields
    /// are later used to "hot-reset" the tile.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="tiles"></param>
    public static void InitiateTile(
        IContinuumTile tile,
        ref Dictionary<Location, IContinuumTile> tiles,
        Func<Location, Location> hash
    )
    {
      computeSpeedField(tile, ref tiles, hash);
      computeCostField(tile, ref tiles, hash);
      tile.StoreBaselineFields();
    }

    /// <summary>
    /// Update the tile's fields with the current state of the units that are
    /// near enough to impact the tile.
    /// </summary>
    /// <param name="updateId"></param>
    /// <param name="tile"></param>
    /// <param name="tiles"></param>
    /// <param name="units"></param>
    public static void UpdateTile(
        IContinuumTile tile,
        ref Dictionary<Location, IContinuumTile> tiles,
        ref Dictionary<int, IContinuumUnit> units,
        Func<Location, Location> hash
    )
    {
      // first, clear the tile
      tile.ResetToBaseline();

      // update the unit specific elements (rho, vAve)
      foreach (var id in tile.ImpactingUnitsIds()) {
        // defensive check to ensure the unit is still in the simulation
        if (!units.ContainsKey(id)) { continue; }
        // (1) apply stationary unit density field (rho)
        // (2) apply predictive density/velocity field (vave)
        computeUnitFields(units[id], tile);
      }

      // these next values are derived from rho and vAve

      // (3) 	now that the velocity field and density fields are computed,
      // 		  divide the velocity by density to get average velocity field
      computeAverageVelocityField(tile);
      // (4)	now that the average velocity field is computed, and the density
      // 		  field is in place, we calculate the speed field, f
      computeSpeedField(tile, ref tiles, hash);
      // (5) 	the cost field depends only on f and g, so it can be computed in its
      //		  entirety now as well
      computeCostField(tile, ref tiles, hash);
    }

    // ******************************************************************************************
    // 							FIELD SOLVING FUNCTIONS
    // ******************************************************************************************
    private static void computeUnitFields(IContinuumUnit unit, IContinuumTile tile)
    {
      // TODO: Only apply unit fields to continuous segments, ie.
      //      if a portion of this field is blocked by impassable
      //      terrain, we should not apply the field beyond that
      //      point

      // TODO: Use an algorithm that will grab only the indeces of the
      //      unit's footprint that overlap with the tile, so we do not
      //      have to perform tile-intersection tests each time, and
      //      we only iterate over the unit footprint points that are
      //      relevant to this tile

      // fetch the unit's footprint
      var footprint = unit.Footprint;

      // offsets - floor produces smoothest interpolated position stamps
      var xOffset = Mathf.FloorToInt(unit.Corner.x);
      var yOffset = Mathf.FloorToInt(unit.Corner.y);

      // grab velocity to scale the footprint
      var velocity = unit.Velocity;
      var mass = unit.Mass;
      // scan the grid, stamping the footprint onto the tile
      for (int x = 0; x < footprint.GetLength(0); x++) {
        for (int y = 0; y < footprint.GetLength(1); y++) {
          float vu = footprint[x, y];
          // only perform storage functions if there is a footprint value
          if (vu <= 0) { continue; }
          // translate to local coords
          var xIndex = x + xOffset;
          var yIndex = y + yOffset;
          var xLocal = xIndex - tile.Corner.x;
          var yLocal = yIndex - tile.Corner.y;
          // ensure we aren't indexing out of range
          if (!tile.ContainsLocalPoint(xLocal, yLocal)) { continue; }
          // add rho to the in-place density
          tile.rho[xLocal, yLocal] += vu * mass;
          // add velocity to existing data
          tile.vAve[xLocal, yLocal] += vu * mass * velocity;
        }
      }
    }

    // **********************************************************************
    // 		tile fields
    // **********************************************************************
    // average velocity fields will just iterate over each tile, since information
    // doesnt 'bleed' into or out from nearby tiles
    private static void computeAverageVelocityField(IContinuumTile tile)
    {
      for (int n = 0; n < tile.SizeX; n++) {
        for (int m = 0; m < tile.SizeY; m++) {
          var v = tile.vAve[n, m];
          float r = tile.rho[n, m];

          if (r != 0) {
            v /= r;
          }
          tile.vAve[n, m] = v;
        }
      }
    }

    private static void computeSpeedField(
      IContinuumTile tile,
      ref Dictionary<Location, IContinuumTile> tiles,
      Func<Location, Location> hash
    )
    {
      for (int n = 0; n < tile.SizeX; n++) {
        for (int m = 0; m < tile.SizeY; m++) {
          for (int d = 0; d < Constants.Values.ENSW.Length; d++) {
            tile.f[n, m][d] = computeSpeedFieldPoint(n, m, tile, Constants.Values.ENSW[d], ref tiles, hash);
          }
        }
      }
    }

    private static float computeSpeedFieldPoint(
      int tileX,
      int tileY,
      IContinuumTile tile,
      Vector2 direction,
      ref Dictionary<Location, IContinuumTile> tiles,
      Func<Location, Location> hash
    )
    {
      int xLocalInto = tileX + (int)direction.x;
      int yLocalInto = tileY + (int)direction.y;

      int xGlobalInto = tile.Corner.x + xLocalInto;
      int yGlobalInto = tile.Corner.y + yLocalInto;

      // if the global "into" is not valid, return min speed
      if (!isGlobalPointValid(tile, xGlobalInto, yGlobalInto, ref tiles, hash)) {
        return Constants.Values.f_speedMin;
      }

      // otherwise, run the speed field calculation
      float ff;
      float ft;
      float fv;

      // grab density for the region INTO WHICH we look
      var rho = readDataFromPoint_rho(tile, xGlobalInto, yGlobalInto, ref tiles, hash);

      // test the density INTO WHICH we move:
      if (rho < Constants.Values.f_rhoMin) {
        // rho < rho_min calc
        var dh = readDataFromPoint_dh(tile, xGlobalInto, yGlobalInto, ref tiles, hash);
        ft = computeTopographicalSpeed(dh, direction);
        ff = ft;
      } else if (rho > Constants.Values.f_rhoMax) {
        // rho > rho_max calc
        var vAve = readDataFromPoint_vAve(tile, xGlobalInto, yGlobalInto, ref tiles, hash);
        fv = computeFlowSpeed(vAve, direction);
        ff = fv;
      } else {
        // rho in-between calc
        var vAve = readDataFromPoint_vAve(tile, xGlobalInto, yGlobalInto, ref tiles, hash);
        var dh = readDataFromPoint_dh(tile, xGlobalInto, yGlobalInto, ref tiles, hash);
        fv = computeFlowSpeed(vAve, direction);
        ft = computeTopographicalSpeed(dh, direction);
        ff = ft + (fv - ft) * (rho - Constants.Values.f_rhoMin) / (Constants.Values.f_rhoMax - Constants.Values.f_rhoMin);
      }
      return Mathf.Clamp(ff, Constants.Values.f_speedMin, Constants.Values.f_speedMax);
    }

    private static float computeTopographicalSpeed(Vector2 dh, Vector2 direction)
    {
      // first, calculate the gradient in the direction we are looking.
      // By taking the dot with Direction,
      // we extract the direction we're looking and assign it a proper sign
      // i.e. if we look left (x = -1) we want -dhdx(x,y), because the
      // gradient is assigned with a positive x
      // 		therefore:		also, Vector.left = [-1,0]
      //						Vector2.Dot(Vector.left, dh[x,y]) = -dhdx;
      float dhInDirection = direction.x * dh.x + direction.y * dh.y;
      // calculate the speed field from the equation
      return Constants.Values.f_speedMax
          + (dhInDirection - Constants.Values.f_slopeMin) / (Constants.Values.f_slopeMax - Constants.Values.f_slopeMin)
          * (Constants.Values.f_speedMin - Constants.Values.f_speedMax);
    }

    private static float computeFlowSpeed(Vector2 vAve, Vector2 direction)
    {
      // the flow speed is simply the average velocity field of the region
      // INTO WHICH we are looking, dotted with the direction vector
      float dot = vAve.x * direction.x + vAve.y * direction.y;
      // We ignore negative dot products here. From the text:
      //      > the flow speed is clamped to be nonnegative, implying that
      //      > the crowd can slow people down, but never carry them backwards.
      return Math.Max(0, dot);
    }

    private static void computeCostField(
      IContinuumTile tile,
      ref Dictionary<Location, IContinuumTile> tiles,
      Func<Location, Location> hash
    )
    {
      for (int n = 0; n < tile.SizeX; n++) {
        for (int m = 0; m < tile.SizeY; m++) {
          for (int d = 0; d < Constants.Values.ENSW.Length; d++) {
            tile.C[n, m][d] = computeCostFieldValue(n, m, d, Constants.Values.ENSW[d], tile, ref tiles, hash);
          }
        }
      }
    }

    private static float computeCostFieldValue(
      int tileX,
      int tileY,
      int d,
      Vector2 direction,
      IContinuumTile tile,
      ref Dictionary<Location, IContinuumTile> tiles,
      Func<Location, Location> hash
    )
    {
      int xLocalInto = tileX + (int)direction.x;
      int yLocalInto = tileY + (int)direction.y;

      int xGlobalInto = tile.Corner.x + xLocalInto;
      int yGlobalInto = tile.Corner.y + yLocalInto;

      // if we're looking in an invalid direction, or if the speed field is exactly 0, return infinity
      // a 0-valued speed field will cause a divide-by-zero error
      if (tile.f[tileX, tileY][d] == 0 || !isGlobalPointValid(tile, xGlobalInto, yGlobalInto, ref tiles, hash)) {
        return Mathf.Infinity;
      }

      // grab discomfort for the region INTO WHICH we look
      var g = readDataFromPoint_g(tile, xGlobalInto, yGlobalInto, ref tiles, hash);

      // clamp g to make sure it's not > 1
      if (g > 1) { g = 1; } else if (g < 0) { g = 0; }

      // compute the cost weighted by our coefficients
      var f = tile.f[tileX, tileY][d];
      float cost = Constants.Values.C_alpha
                  + Constants.Values.C_beta * 1 / f
                  + Constants.Values.C_gamma * g / f;

      return cost;
    }

    // *****************************************************************************
    //			TOOLS AND UTILITIES
    // *****************************************************************************
    private static bool isGlobalPointValid(
      IContinuumTile tile,
      int xGlobal,
      int yGlobal,
      ref Dictionary<Location, IContinuumTile> tiles,
      Func<Location, Location> hash
    )
    {
      // is the point local? if so, just grab the data
      if (tile.ContainsGlobalPoint(xGlobal, yGlobal)) {
        var localPoint = tile.LocalFromGlobal(xGlobal, yGlobal);
        return tile.IsLocalPointValid(localPoint.x, localPoint.y);
      }

      // point is not local, so we need to find the tile it's in
      var tileHash = hash(new Location(xGlobal, yGlobal));
      var globalTile = tiles[tileHash];
      var local = globalTile.LocalFromGlobal(xGlobal, yGlobal);
      return globalTile.IsLocalPointValid(local.x, local.y);
    }

    // *****************************************************************************
    //                 TILE READ/WRITE OPS
    //
    //  Primary focus of this area is to convert global points (what Dynamic
    //  Global Fields works with) into local points, and then find the relevant tile
    // *****************************************************************************
    private static Vector2 readDataFromPoint_dh(
      IContinuumTile tile,
      int xGlobal,
      int yGlobal,
      ref Dictionary<Location, IContinuumTile> tiles,
      Func<Location, Location> hash
    )
    {
      // is the point local? if so, just grab the data
      if (tile.ContainsGlobalPoint(xGlobal, yGlobal)) {
        var localPoint = tile.LocalFromGlobal(xGlobal, yGlobal);
        return tile.dh[localPoint.x, localPoint.y];
      }

      // point is not local, so we need to find the tile it's in
      var tileHash = hash(new Location(xGlobal, yGlobal));
      var globalTile = tiles[tileHash];
      var local = globalTile.LocalFromGlobal(xGlobal, yGlobal);
      return globalTile.dh[local.x, local.y];
    }

    private static float readDataFromPoint_rho(
      IContinuumTile tile,
      int xGlobal,
      int yGlobal,
      ref Dictionary<Location, IContinuumTile> tiles,
      Func<Location, Location> hash
    )
    {
      if (tile.ContainsGlobalPoint(xGlobal, yGlobal)) {
        var localPoint = tile.LocalFromGlobal(xGlobal, yGlobal);
        return tile.rho[localPoint.x, localPoint.y];
      }

      var tileHash = hash(new Location(xGlobal, yGlobal));
      var globalTile = tiles[tileHash];
      var local = globalTile.LocalFromGlobal(xGlobal, yGlobal);
      return globalTile.rho[local.x, local.y];
    }

    private static float readDataFromPoint_g(
      IContinuumTile tile,
      int xGlobal,
      int yGlobal,
      ref Dictionary<Location, IContinuumTile> tiles,
      Func<Location, Location> hash
    )
    {
      if (tile.ContainsGlobalPoint(xGlobal, yGlobal)) {
        var localPoint = tile.LocalFromGlobal(xGlobal, yGlobal);
        return tile.g[localPoint.x, localPoint.y];
      }

      var tileHash = hash(new Location(xGlobal, yGlobal));
      var globalTile = tiles[tileHash];
      var local = globalTile.LocalFromGlobal(xGlobal, yGlobal);
      return globalTile.g[local.x, local.y];
    }

    private static Vector2 readDataFromPoint_vAve(
      IContinuumTile tile,
      int xGlobal,
      int yGlobal,
      ref Dictionary<Location, IContinuumTile> tiles,
      Func<Location, Location> hash
    )
    {
      if (tile.ContainsGlobalPoint(xGlobal, yGlobal)) {
        var localPoint = tile.LocalFromGlobal(xGlobal, yGlobal);
        return tile.vAve[localPoint.x, localPoint.y];
      }

      var tileHash = hash(new Location(xGlobal, yGlobal));
      var globalTile = tiles[tileHash];
      var local = globalTile.LocalFromGlobal(xGlobal, yGlobal);
      return globalTile.vAve[local.x, local.y];
    }
  }
}
