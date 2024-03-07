using UnityEngine;

namespace Yohash.ContinuumCrowds
{
  public interface IUnit
  {
    /// <summary>
    /// A unique identifier for this unit
    /// </summary>
    int Id { get; }
    /// <summary>
    /// The current velocity of this unit
    /// </summary>
    Vector2 Velocity { get; }
    /// <summary>
    /// The mass of this unit
    /// </summary>
    float Mass { get; }
    /// <summary>
    /// The current speed of this unit
    /// </summary>
    float Speed { get; }
    /// <summary>
    /// The current position of this unit
    /// </summary>
    Vector2 Position { get; }
    /// <summary>
    /// The current rotation of this unit about the
    /// y (up) axis, moving clockwise. The forward
    /// axis aligned with world space z (forward) is
    /// 0-degrees
    /// </summary>
    float Rotation { get; }
    /// <summary>
    /// The size of this unit
    /// </summary>
    Vector2 Size { get; }
    /// <summary>
    /// The footprint of this unit, from which the
    /// density is calculated. Keeping in mind:
    /// regarding the density computations:
    ///   > ...each person should contribute no less
    ///   > than rho_bar to their own grid cell, but
    ///   > no more than rho_bar to any neighboring
    ///   > grid cell.
    /// The computation of this footprint must comply
    /// with these conditions
    /// </summary>
    float[,] Footprint { get; }
    /// <summary>
    /// The corner of this unit's footprint
    /// </summary>
    Vector2Int Corner { get; }
    /// <summary>
    ///
    /// </summary>
    /// <param name="velocity"></param>
    void SetVelocity(Vector2 velocity);
  }
}
