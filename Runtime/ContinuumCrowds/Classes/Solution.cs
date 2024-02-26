using System.Collections.Generic;
using UnityEngine;
using Yohash.Tools;

namespace Yohash.ContinuumCrowds
{
  public class Solution
  {
    /// <summary>
    /// The status of this solutions solving process
    /// </summary>
    public enum State { New, Is_Solving, Has_Solution }
    private State _status;
    public State Status {
      get { return _status; }
    }
    public void IsSolving() { _status = State.Is_Solving; }
    public void HasSolution()
    {
      // TODO - write an algorithm that helps determine this update interval
      //_nextUpdate = 0.1f;
      _status = State.Has_Solution;
    }

    /// <summary>
    /// The destination of this Continuum Crowds solution
    /// </summary>
    private readonly Destination _destination;
    public Destination Destination {
      get { return _destination; }
    }

    /// <summary>
    /// The time remaining until this solution needs update again
    /// </summary>
    private float _nextUpdate;
    public float NextUpdate {
      get { return _nextUpdate; }
    }

    /// <summary>
    /// Units subscribed to this solution
    /// </summary>
    private List<int> _subscribedUnitIds = new List<int>();
    public void Subscribe(int unitId) { _subscribedUnitIds.Add(unitId); }
    public void Unsubscribe(int unitId) { _subscribedUnitIds.Remove(unitId); }

    public bool HasSubscribedUnits { get { return _subscribedUnitIds.Count > 0; } }

    /// <summary>
    /// The Continuum Crowds Solution functions as a stately tracker
    /// connecting units to an Eikonal Solution.
    /// </summary>
    /// <param name="destination"></param>
    public Solution(Destination destination)
    {
      _destination = destination;
    }

    /// <summary>
    /// The minimum time-delay after an update occurs, before which we will
    /// re-solve this solution.
    /// TODO - can I make this time dynamic? Perhaps to how many units are in the tile?
    ///      - and down the line, can see how close they are to each other?
    /// </summary>
    private float _updateDelay = 0.1f;
    public bool ShouldUpdate(float dt)
    {
      _nextUpdate -= dt;
      //Debug.Log($"Solution {_destination.TileLocation} _nextUpdate - {dt} = {_nextUpdate} ({_nextUpdate < 0})");
      var shouldUpdate = _nextUpdate < 0;
      if (shouldUpdate) { _nextUpdate = _updateDelay; }
      return shouldUpdate;
    }

    /// <summary>
    /// TODO - consider if we can mod this, currently we
    /// (a) upon receiving the UpdateUnits() call, we tell all units what their velocity
    ///     SHOULD be based on their interpolated position in the velocity map
    /// can we find a way to:
    /// (b) give the units a handle to the velocity function, so they can constantly get
    ///     their current most-accurate interpolated position
    ///
    /// with (a), a unit will drive in the direciton they were told to go for the timestamp
    /// within which they received their velocity. If they are told to maneuver around a unit,
    /// but dont get another update for 1s, they will drive straight for 1s. With (b), they can
    /// get the most-current-for-position velocity, so they will still track around the
    /// flow field
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="unitsByIdRef"></param>
    public void UpdateUnits(ref Vector2[,] velocity, ref Dictionary<int, Unit> unitsByIdRef)
    {
      // pull our ref to the Eikonal Solver
      // call our unit update with their velocity
      foreach (var id in _subscribedUnitIds) {
        var unit = unitsByIdRef[id];
        var pos = unit.DriverReference;
        // localize the position
        pos = pos - _destination.TileHash.ToVector2();
        var vel = velocity.Interpolate(pos.x, pos.y, true);
        unit.SetVelocity(vel);
      }
    }
  }
}
