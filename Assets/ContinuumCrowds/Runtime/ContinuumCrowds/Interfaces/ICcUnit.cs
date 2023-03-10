using System;
using UnityEngine;

namespace Yohash.ContinuumCrowds
{
  public interface ICcUnit
  {
    int UniqueId();
    Vector2 Velocity();
    float Speed();
    Vector2 Position();
    float Rotation();
    Vector2 Size();
    float[,] Footprint();
    void SetVelocity(Vector2 velocity);
  }
}
