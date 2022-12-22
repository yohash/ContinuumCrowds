using System.Collections.Generic;

public class Portal : IPathable
{
  // Portal properties
  public int Width;

  public Location[] borderA;
  public Location[] borderB;

  // IPathable cost dictionary
  private Dictionary<IPathable, float> _costByNode;
  private Dictionary<IPathable, float> costByNode {
    get {
      if (_costByNode == null) {
        _costByNode = new Dictionary<IPathable, float>();
      }
      return _costByNode;
    }
  }

  public Location Center {
    get {
      return borderA != null
        ? borderA[borderA.Length / 2]
        : borderB != null
        ? borderB[borderB.Length / 2]
        : Location.Zero;
    }
  }

  // *******************************************************************
  //    Portal
  // *******************************************************************
  public void AddConnection(IPathable node, float cost)
  {
    if (node.Equals(this)) { return; }
    costByNode[node] = cost;
  }

  public override string ToString()
  {
    return $"{GetType()}: {Center} - x{Width}";
  }

  private void purgeConnections()
  {
    var destroy = new List<IPathable>();
    foreach (var pathable in costByNode.Keys) {
      if (pathable == null) { destroy.Add(pathable); }
    }
    foreach (var pathable in destroy) {
      costByNode.Remove(pathable);
    }
  }

  // *******************************************************************
  //    IPathable
  // *******************************************************************
  public float Cost(IPathable neighbor)
  {
    return costByNode.ContainsKey(neighbor) ?
      costByNode[neighbor] :
      float.MaxValue;
  }
  public float Heuristic(Location endGoal)
  {
    return (float)(Center - endGoal).magnitude();
  }
  public IEnumerable<IPathable> Neighbors()
  {
    purgeConnections();
    foreach (var node in costByNode.Keys) {
      yield return node;
    }
  }
  public Location AsLocation()
  {
    return Center;
  }
}
