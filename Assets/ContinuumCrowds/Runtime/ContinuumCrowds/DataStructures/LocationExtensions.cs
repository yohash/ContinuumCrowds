using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using UnityEngine;

public static class LocationExtensions
{
  public static double sqrMagnitude(this Location l)
  {
    return (l.x * l.x) + (l.y * l.y);
  }
  public static double magnitude(this Location l)
  {
    return Math.Sqrt((l.x * l.x) + (l.y * l.y));
  }
  public static Vector2 ToVector2(this Location l)
  {
    return new Vector2(l.x, l.y);
  }
  public static Vector3 ToVector3(this Location l)
  {
    return new Vector3(l.x, 0, l.y);
  }
  public static Vector3 ToVector3(this Location l, float y)
  {
    return new Vector3(l.x, y, l.y);
  }
  public static Location Average(this List<Location> locations)
  {
    if (locations.Count == 0) {
      return Location.Zero;
    }

    var l = Location.Zero;
    foreach (var item in locations) {
      l += item;
    }
    return l /= locations.Count;
  }
}

public partial struct Location
{
  private static Location zero = new Location(0, 0);
  private static Location left = new Location(1, 0);
  private static Location right = new Location(-1, 0);
  private static Location up = new Location(0, 1);
  private static Location down = new Location(0, -1);
  private static Location upleft = new Location(1, 1);
  private static Location upright = new Location(-1, 1);
  private static Location downleft = new Location(-1, -1);
  private static Location downright = new Location(-1, -1);
  // *******************************************************************
  //    Extensions
  // *******************************************************************
  public static Location Zero { get { return zero; } }
  public static Location Left { get { return left; } }
  public static Location Right { get { return right; } }
  public static Location Up { get { return up; } }
  public static Location Down { get { return down; } }
  public static IEnumerable<Location> Cardinal()
  {
    yield return left;
    yield return up;
    yield return right;
    yield return down;
  }

  public static IEnumerable<Location> Ordinal()
  {
    yield return left;
    yield return upleft;
    yield return up;
    yield return upright;
    yield return right;
    yield return downright;
    yield return down;
    yield return downleft;
  }

  public static Location operator +(Location l1, Location l2)
  {
    return new Location(l1.x + l2.x, l1.y + l2.y);
  }
  public static Location operator -(Location l1, Location l2)
  {
    return new Location(l1.x - l2.x, l1.y - l2.y);
  }
  public static Location operator /(Location l, float d)
  {
    return new Location(l.x / d, l.y / d);
  }
  // *******************************************************************
  //    Extensions for Unity classes
  // *******************************************************************
  public static Location operator +(Location l1, Vector2 l2)
  {
    return new Location(l1.x + (int)Math.Round(l2.x, 0), l1.y + (int)Math.Round(l2.y, 0));
  }
  public static Location operator -(Location l1, Vector2 l2)
  {
    return new Location(l1.x - (int)Math.Round(l2.x, 0), l1.y - (int)Math.Round(l2.y, 0));
  }
  public static Location operator +(Location l1, Vector2Int l2)
  {
    return new Location(l1.x + l2.x, l1.y + l2.y);
  }
  public static Location operator -(Location l1, Vector2Int l2)
  {
    return new Location(l1.x - l2.x, l1.y - l2.y);
  }
}
