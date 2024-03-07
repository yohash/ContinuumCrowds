using UnityEngine;

namespace Yohash.ContinuumCrowds
{
  public interface IContinuumUnit
  {
    /// <summary>
    /// The current velocity of this unit
    /// </summary>
    Vector2 Velocity { get; }
    /// <summary>
    /// The mass of this unit
    /// </summary>
    float Mass { get; }
    /// <summary>
    /// The footprint of this unit, from which the
    /// density is calculated. Keeping in mind:
    /// regarding the density computations:
    ///
    ///   > ...each person should contribute no less
    ///   > than rho_bar to their own grid cell, but
    ///   > no more than rho_bar to any neighboring
    ///   > grid cell.
    ///
    /// The computation of this footprint must comply
    /// with these conditions
    /// </summary>
    float[,] Footprint { get; }
    /// <summary>
    /// The corner of this unit's footprint
    /// </summary>
    Vector2Int Corner { get; }
  }
}
