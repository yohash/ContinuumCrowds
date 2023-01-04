using System.Collections.Generic;

namespace Yohash.ContinuumCrowds
{
  public interface IPathable
  {
    IEnumerable<IPathable> Neighbors();
    float Heuristic(Location endGoal);
    float Cost(IPathable neighbor);
    Location AsLocation();
  }
}
