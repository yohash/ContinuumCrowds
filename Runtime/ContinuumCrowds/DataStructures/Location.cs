using System;

[Serializable]
public partial struct Location : IEquatable<Location>
{
  public readonly int x;
  public readonly int y;

  public Location(int x, int y)
  {
    this.x = x;
    this.y = y;
  }

  public Location(double x, double y)
  {
    this.x = (int)Math.Round(x, 0);
    this.y = (int)Math.Round(y, 0);
  }

  public override string ToString()
  {
    return $"({x}, {y})";
  }

  // *******************************************************************
  //    IEquatable
  // *******************************************************************
  public bool Equals(Location l2)
  {
    return x == l2.x && y == l2.y;
  }

  public override bool Equals(object obj)
  {
    return obj is Location l && Equals(l);
  }

  public static bool Equals(Location l1, Location l2)
  {
    return l1.Equals(l2);
  }

  public static bool operator ==(Location l1, Location l2)
  {
    return l1.Equals(l2);
  }

  public static bool operator !=(Location l1, Location l2)
  {
    return !l1.Equals(l2);
  }

  public override int GetHashCode()
  {
    int hash = 17;
    hash = (31 * hash) + x;
    hash = (31 * hash) + y;
    return hash;
  }
}
