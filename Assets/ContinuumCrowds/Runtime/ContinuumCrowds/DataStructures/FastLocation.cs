using Yohash.PriorityQueue;

namespace Yohash.ContinuumCrowds
{
  public class FastLocation : FastPriorityQueueNode
  {
    public readonly int x;
    public readonly int y;

    public FastLocation(int x, int y)
    {
      this.x = x;
      this.y = y;
    }

    public bool Equals(FastLocation l2)
    {
      return x == l2.x && y == l2.y;
    }

    public override bool Equals(object obj)
    {
      return obj is FastLocation l && Equals(l);
    }

    public static bool Equals(FastLocation l1, FastLocation l2)
    {
      return l1.Equals(l2);
    }

    public static bool operator ==(FastLocation l1, FastLocation l2)
    {
      return l1.Equals(l2);
    }

    public static bool operator !=(FastLocation l1, FastLocation l2)
    {
      return !l1.Equals(l2);
    }

    public override int GetHashCode()
    {
      int hash = 17;
      hash = hash * 23 + x.GetHashCode();
      hash = hash * 23 + y.GetHashCode();
      return hash;
    }

    public override string ToString()
    {
      return $"({x}, {y})";
    }
  }
}
