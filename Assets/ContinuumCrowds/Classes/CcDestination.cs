using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CcDestination : IEquatable<CcDestination>
{
  /// <summary>
  /// The corner location of the tile
  /// </summary>
  [SerializeField] private readonly Location _location;
  public Location TileHash {
    get { return _location; }
  }

  /// <summary>
  /// List of locations defining this destination
  /// </summary>
  [SerializeField] private readonly List<Location> _goal;
  public List<Location> Goal {
    get { return _goal; }
  }

  // cache the hash code in effort to reduce re-compute
  private int _hash;
  private bool hashed = false;

  public CcDestination(CcTile tile, List<Location> goal)
    => (_location, _goal) = (tile.Corner, goal);

  public bool GoalContainsPoint(Vector2 point)
  {
    var vInt = Vector2Int.FloorToInt(point);
    return _goal.Any(loc => loc.x == vInt.x && loc.y == vInt.y);
  }

  // *******************************************************************
  //    IEquatable
  // *******************************************************************
  public bool Equals(CcDestination destination)
  {
    return destination.TileHash == TileHash &&
      Enumerable.SequenceEqual(destination.Goal, Goal);
  }

  public override bool Equals(object obj)
  {
    return obj is CcDestination destination
      && _location.Equals(destination._location)
      && Enumerable.SequenceEqual(destination._goal, _goal);
  }

  public override int GetHashCode()
  {
    if (!hashed) {
      _hash = generateHash();
      hashed = true;
    }
    return _hash;
  }

  private int generateHash()
  {
    unchecked {
      int hashCode = -1244036622;
      hashCode = hashCode * -1521134295 ^ _location.GetHashCode();
      for (int i = 0; i < _goal.Count; i++) {
        hashCode = hashCode * -1521134295 ^ _goal[i].GetHashCode();
      }
      return hashCode;
    }
  }
}
