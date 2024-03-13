using System.Collections.Generic;
using UnityEngine;

namespace Yohash.ContinuumCrowds
{
  public interface IContinuumTile
  {
    int SizeX { get; }
    int SizeY { get; }
    Vector2Int Size { get { return new Vector2Int(SizeX, SizeY); } }

    /// <summary>
    /// This function returns the ids of the units that are
    /// close enough to influence this tile with the density field.
    /// </summary>
    IEnumerable<int> ImpactingUnitsIds();
    /// <summary>
    /// This function should be called after the tiles are initialized
    /// to provide baseline speed and cost fields for easy reset
    /// </summary>
    void StoreBaselineFields();
    /// <summary>
    /// Rest to the baseline speed and cost fields
    /// </summary>
    void ResetToBaseline();

    /// <summary>
    /// The lowest x,y corner of this tile
    /// </summary>
    Location Corner { get; }

    /// The Continuum Crowds Dynamic Global fields inputs.
    // density field
    ref float[,] rho { get; }
    // average velocity field
    ref Vector2[,] vAve { get; }
    // height map gradient
    ref Vector2[,] dh { get; }

    /// The Continuum Crowds Dynamic Global fields outputs.
    /// These are read by the Eikonal Solvers.
    // discomfort
    ref float[,] g { get; }
    // speed field, data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
    ref Vector4[,] f { get; }
    // cost field, data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
    ref Vector4[,] C { get; }
  }
}
