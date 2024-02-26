using System;
using System.Collections.Generic;
using UnityEngine;
using Yohash.Tools;

namespace Yohash.ContinuumCrowds
{
  /// <summary>
  /// Continuum Crowds dynamic global fields.
  /// </summary>
  public static class DynamicGlobalFields
  {
    /// <summary>
    /// TODO - Summary
    /// </summary>
    /// <param name="cct"></param>
    /// <param name="tiles"></param>
    public static void InitiateTile(
        Tile cct,
        ref Dictionary<Location, Tile> tiles
    )
    {
      computeSpeedField(cct, ref tiles);
      computeCostField(cct, ref tiles);
      cct.StoreCurrentSpeedAndCostFields();
    }

    /// <summary>
    /// TODO - Summary
    /// </summary>
    /// <param name="updateId"></param>
    /// <param name="cct"></param>
    /// <param name="tiles"></param>
    /// <param name="units"></param>
    public static void UpdateTile(
        int updateId,
        Tile cct,
        ref Dictionary<Location, Tile> tiles,
        ref Dictionary<int, Unit> units
    )
    {
      // first, clear the tile
      cct.ResetTile();

      // update the unit specific elements (rho, vAve)
      foreach (var id in cct.ImpactingUnitsIds()) {
        // (1) apply stationary unit density field (rho)
        // (2) apply predictive density/velocity field (vave)
        computeUnitFields(units[id], cct);
      }

      // these next values are derived from rho and vAve

      // (3) 	now that the velocity field and density fields are computed,
      // 		  divide the velocity by density to get average velocity field
      computeAverageVelocityField(cct);
      // (4)	now that the average velocity field is computed, and the density
      // 		  field is in place, we calculate the speed field, f
      computeSpeedField(cct, ref tiles);
      // (5) 	the cost field depends only on f and g, so it can be computed in its
      //		  entirety now as well
      computeCostField(cct, ref tiles);

      // Update the Id last, so that queries to this tile will know that
      // the current (as queried) state of this tile is not-current
      cct.MarkComplete(updateId);
    }

    // ******************************************************************************************
    // 							FIELD SOLVING FUNCTIONS
    // ******************************************************************************************
    private static void computeUnitFields(Unit ccu, Tile cct)
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
      var footprint = ccu.Footprint;

      // offsets - floor produces smoothest interpolated position stamps
      var xOffset = Mathf.FloorToInt(ccu.Anchor.x);
      var yOffset = Mathf.FloorToInt(ccu.Anchor.y);

      // grab velocity to scale the footprint
      var velocity = ccu.Velocity;
      // scan the grid, stamping the footprint onto the tile
      for (int x = 0; x < footprint.GetLength(0); x++) {
        for (int y = 0; y < footprint.GetLength(1); y++) {
          float vu = footprint[x, y];
          // only perform storage functions if there is a footprint value
          if (vu <= 0) { continue; }
          // translate to local coords
          var xIndex = x + xOffset;
          var yIndex = y + yOffset;
          var xLocal = xIndex - cct.Corner.x;
          var yLocal = yIndex - cct.Corner.y;
          // ensure we aren't indexing out of range
          if (!cct.ContainsLocalPoint(xLocal, yLocal)) { continue; }
          // add rho to the in-place density
          cct.rho[xLocal, yLocal] += vu;
          // add velocity to existing data
          cct.vAve[xLocal, yLocal] += vu * velocity;
        }
      }
    }

    // **********************************************************************
    // 		tile fields
    // **********************************************************************
    // average velocity fields will just iterate over each tile, since information
    // doesnt 'bleed' into or out from nearby tiles
    private static void computeAverageVelocityField(Tile cct)
    {
      for (int n = 0; n < cct.SizeX; n++) {
        for (int m = 0; m < cct.SizeY; m++) {
          var v = cct.vAve[n, m];
          float r = cct.rho[n, m];

          if (r != 0) {
            v /= r;
          }
          cct.vAve[n, m] = v;
        }
      }
    }

    private static void computeSpeedField(Tile cct, ref Dictionary<Location, Tile> tiles)
    {
      for (int n = 0; n < cct.SizeX; n++) {
        for (int m = 0; m < cct.SizeY; m++) {
          for (int d = 0; d < Constants.Values.ENSW.Length; d++) {
            cct.f[n, m][d] = computeSpeedFieldPoint(n, m, cct, Constants.Values.ENSW[d], ref tiles);
          }
        }
      }
    }

    private static float computeSpeedFieldPoint(
      int tileX,
      int tileY,
      Tile cct,
      Vector2 direction,
      ref Dictionary<Location, Tile> tiles
    )
    {
      int xLocalInto = tileX + (int)direction.x;
      int yLocalInto = tileY + (int)direction.y;

      int xGlobalInto = cct.Corner.x + xLocalInto;
      int yGlobalInto = cct.Corner.y + yLocalInto;

      // if the global "into" is not valid, return min speed
      if (!isGlobalPointValid(cct.SizeX, xGlobalInto, yGlobalInto, ref tiles)) {
        return Constants.Values.f_speedMin;
      }

      // otherwise, run the speed field calculation
      float ff, ft, fv;

      // grab density for the region INTO WHICH we look
      float rho = !cct.ContainsLocalPoint(xLocalInto, yLocalInto)
        // test to see if the point we're looking INTO is in another tile, and if so, pull it
        ? readDataFromPoint_rho(cct.SizeX, xGlobalInto, yGlobalInto, ref tiles)
        : cct.rho[xLocalInto, yLocalInto];

      // test the density INTO WHICH we move:
      if (rho < Constants.Values.f_rhoMin) {
        // rho < rho_min calc
        ft = computeTopographicalSpeed(readDataFromPoint_dh(cct.SizeX, xGlobalInto, yGlobalInto, ref tiles), direction);
        ff = ft;
      } else if (rho > Constants.Values.f_rhoMax) {
        // rho > rho_max calc
        fv = computeFlowSpeed(readDataFromPoint_vAve(cct.SizeX, xGlobalInto, yGlobalInto, ref tiles), direction);
        ff = fv;
      } else {
        // rho in-between calc
        fv = computeFlowSpeed(readDataFromPoint_vAve(cct.SizeX, xGlobalInto, yGlobalInto, ref tiles), direction);
        ft = computeTopographicalSpeed(readDataFromPoint_dh(cct.SizeX, xGlobalInto, yGlobalInto, ref tiles), direction);
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
      return Math.Max(Constants.Values.f_speedMin, dot);
    }

    private static void computeCostField(Tile cct, ref Dictionary<Location, Tile> tiles)
    {
      for (int n = 0; n < cct.SizeX; n++) {
        for (int m = 0; m < cct.SizeY; m++) {
          for (int d = 0; d < Constants.Values.ENSW.Length; d++) {
            cct.C[n, m][d] = computeCostFieldValue(n, m, d, Constants.Values.ENSW[d], cct, ref tiles);
          }
        }
      }
    }

    private static float computeCostFieldValue(
      int tileX,
      int tileY,
      int d,
      Vector2 direction,
      Tile cct,
      ref Dictionary<Location, Tile> tiles
    )
    {
      int xLocalInto = tileX + (int)direction.x;
      int yLocalInto = tileY + (int)direction.y;

      int xGlobalInto = cct.Corner.x + xLocalInto;
      int yGlobalInto = cct.Corner.y + yLocalInto;

      // if we're looking in an invalid direction, dont store this value
      if (cct.f[tileX, tileY][d] == 0 || !isGlobalPointValid(cct.SizeX, xGlobalInto, yGlobalInto, ref tiles)) {
        return Mathf.Infinity;
      }

      // grab discomfort for the region INTO WHICH we look
      float g = !cct.ContainsLocalPoint(xLocalInto, yLocalInto)
        // test to see if the point we're looking INTO is in another tile, and if so, pull it
        ? readDataFromPoint_g(cct.SizeX, xGlobalInto, yGlobalInto, ref tiles)
        : cct.g[xLocalInto, yLocalInto];

      // clamp g to make sure it's not > 1
      if (g > 1) { g = 1; } else if (g < 0) { g = 0; }

      // compute the cost weighted by our coefficients
      var f = cct.f[tileX, tileY][d];
      float cost = Constants.Values.C_alpha
                  + Constants.Values.C_beta * 1 / f
                  + Constants.Values.C_gamma * g / f;

      return cost;
    }

    // *****************************************************************************
    //			TOOLS AND UTILITIES
    // *****************************************************************************
    private static bool isGlobalPointValid(int tileSize, int xGlobal, int yGlobal, ref Dictionary<Location, Tile> tiles)
    {
      var corner = tileCornerFromGlobalCoords(tileSize, xGlobal, yGlobal);
      // if this tile does not exist, the point is not valid
      if (!tiles.ContainsKey(corner)) {
        return false;
      }
      // return validity for the tile point
      return tiles[corner].IsLocalPointValid(xGlobal - corner.x, yGlobal - corner.y);
    }

    private static Location tileCornerFromGlobalCoords(int tileSize, int xGlobal, int yGlobal)
    {
      var loc = new Location(
        Math.Floor((double)xGlobal / tileSize) * tileSize,
        Math.Floor((double)yGlobal / tileSize) * tileSize
      );
      return loc;
    }

    private static Location tileCoordsFromGlobal(Location l, int tileSize, int xGlobal, int yGlobal)
    {
      return new Location(xGlobal % tileSize, yGlobal % tileSize);
    }

    // ******************************************************************************************
    //                 TILE READ/WRITE OPS
    //
    //  Primary focus of this area is to convert global points (what CC Dynamic Global Fields
    //  works with) into local points, and then find the relevant tile
    // ******************************************************************************************
    private static Vector2 readDataFromPoint_dh(int tileSize, int xGlobal, int yGlobal, ref Dictionary<Location, Tile> tiles)
    {
      var location = tileCornerFromGlobalCoords(tileSize, xGlobal, yGlobal);
      var local = tileCoordsFromGlobal(location, tileSize, xGlobal, yGlobal);
      return tiles[location].dh[local.x, local.y];
    }

    private static float readDataFromPoint_rho(int tileSize, int xGlobal, int yGlobal, ref Dictionary<Location, Tile> tiles)
    {
      var location = tileCornerFromGlobalCoords(tileSize, xGlobal, yGlobal);
      var local = tileCoordsFromGlobal(location, tileSize, xGlobal, yGlobal);
      return tiles[location].rho[local.x, local.y];
    }

    private static float readDataFromPoint_g(int tileSize, int xGlobal, int yGlobal, ref Dictionary<Location, Tile> tiles)
    {
      var location = tileCornerFromGlobalCoords(tileSize, xGlobal, yGlobal);
      var local = tileCoordsFromGlobal(location, tileSize, xGlobal, yGlobal);
      return tiles[location].g[local.x, local.y];
    }

    private static Vector2 readDataFromPoint_vAve(int tileSize, int xGlobal, int yGlobal, ref Dictionary<Location, Tile> tiles)
    {
      var location = tileCornerFromGlobalCoords(tileSize, xGlobal, yGlobal);
      var local = tileCoordsFromGlobal(location, tileSize, xGlobal, yGlobal);
      return tiles[location].vAve[local.x, local.y];
    }
  }
}
