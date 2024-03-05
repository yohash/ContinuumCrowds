using UnityEngine;

namespace Yohash.ContinuumCrowds
{
  public interface IUnit
  {
    int UniqueId { get; }
    Vector2 Velocity { get; }
    float Speed { get; }
    Vector2 Position { get; }
    float Rotation { get; }
    Vector2 Size { get; }
    float Mass { get; }
    float[,] Footprint();
    void SetVelocity(Vector2 velocity);
  }
}
