using System.Collections.Generic;
using UnityEngine;
using Yohash.Tools;

namespace Yohash.ContinuumCrowds
{
  public class Tile
  {
    /// <summary>
    /// Units who will impact this tile
    /// </summary>
    private HashSet<int> impactingUnitsIds = new HashSet<int>();
    public void Subscribe(int unitId) { impactingUnitsIds.Add(unitId); }
    public void Unsubscribe(int unitId) { impactingUnitsIds.Remove(unitId); }
    public IEnumerable<int> ImpactingUnitsIds()
    { foreach (var id in impactingUnitsIds) { yield return id; } }

    public int SizeX { get { return g.GetLength(0); } }
    public int SizeY { get { return g.GetLength(1); } }
    public Vector2Int Size { get { return new Vector2Int(SizeX, SizeY); } }

    /// <summary>
    /// The lowest x,y corner of this tile
    /// </summary>
    private Location _corner;
    public Location Corner {
      get { return _corner; }
    }

    /// <summary>
    /// The id corresponding to the last update cycle this Unit calculated
    /// their fields
    /// </summary>
    private int _lastUpdateId;

    /// The Continuum Crowds Dynamic Global fields inputs.
    // density field
    public float[,] rho;
    // average velocity field
    public Vector2[,] vAve;
    // height map
    public float[,] h;
    // height map gradient
    public Vector2[,] dh;

    /// The Continuum Crowds Dynamic Global fields outputs.
    /// These are read by the Eikonal Solvers.
    // discomfort
    public float[,] g;
    // speed field, data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
    public Vector4[,] f;
    // cost field, data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
    public Vector4[,] C;

    /// These baseline fields consider just static objects, and are used
    /// to quickly reset the tile when unit considerations need to be updated.
    private float[,] _gbaseline;
    private Vector4[,] _fbaseline;
    private Vector4[,] _Cbaseline;

    public Tile(Location location, float[,] g, float[,] h, Vector2[,] dh)
    {
      _corner = location;

      this.g = g;
      this.h = h;
      this.dh = dh;

      int x = g.GetLength(0);
      int y = g.GetLength(1);

      rho = new float[x, y];
      vAve = new Vector2[x, y];
      f = new Vector4[x, y];
      C = new Vector4[x, y];

      _gbaseline = new float[x, y];
      _fbaseline = new Vector4[x, y];
      _Cbaseline = new Vector4[x, y];

      float f0 = Constants.Values.FlatSpeed;

      // initialize speed and cost fields
      Vector4 f_init = f0 * Vector4.one;
      Vector4 C_init = Vector4.one * (f0 * Constants.Values.C_alpha + Constants.Values.C_beta) / f0;

      for (int i = 0; i < x; i++) {
        for (int k = 0; k < y; k++) {
          f[i, k] = f_init;
          C[i, k] = C_init;

          _gbaseline[i, k] = g[i, k];
          _fbaseline[i, k] = f_init;
          _Cbaseline[i, k] = C_init;
        }
      }
    }

    public bool ShouldUpdate(int updateId)
    {
      return _lastUpdateId != updateId;
    }

    public void MarkComplete(int updateId)
    {
      _lastUpdateId = updateId;
    }

    /// <summary>
    /// Set the current Speed field and Cost fields as defaults.
    /// These defaults will be loaded when tiles reset.
    /// </summary>
    public void StoreCurrentSpeedAndCostFields()
    {
      for (int x = 0; x < g.GetLength(0); x++) {
        for (int y = 0; y < g.GetLength(1); y++) {
          _fbaseline[x, y] = f[x, y];
          _Cbaseline[x, y] = C[x, y];
        }
      }
    }

    /// <summary>
    /// Reset all the tile values to 0.
    /// Speed and Cost fields are reset to the stored defaults.
    /// </summary>
    public void ResetTile()
    {
      for (int x = 0; x < g.GetLength(0); x++) {
        for (int y = 0; y < g.GetLength(1); y++) {
          rho[x, y] = 0;
          g[x, y] = _gbaseline[x, y];
          vAve[x, y] = Vector2.zero;
          f[x, y] = _fbaseline[x, y];
          C[x, y] = _Cbaseline[x, y];
        }
      }
    }
  }
}