using UnityEngine;

namespace Yohash.ContinuumCrowds
{
  public static class TileExtensions
  {
    public static bool ContainsLocalPoint(this IContinuumTile tile, int x, int y)
    {
      return 0 <= x && x <= tile.SizeX - 1
          && 0 <= y && y <= tile.SizeY - 1;
    }

    public static bool ContainsGlobalPoint(this IContinuumTile tile, Location l)
    {
      return tile.ContainsGlobalPoint(l.x, l.y);
    }

    public static bool ContainsGlobalPoint(this IContinuumTile tile, int x, int y)
    {
      return tile.Corner.x <= x && x <= tile.Corner.x + tile.SizeX - 1
          && tile.Corner.y <= y && y <= tile.Corner.y + tile.SizeY - 1;
    }

    public static Vector2Int LocalFromGlobal(this IContinuumTile tile, int x, int y)
    {
      return new Vector2Int(x - tile.Corner.x, y - tile.Corner.y);
    }

    public static bool IsLocalPointValid(this IContinuumTile tile, int x, int y)
    {
      // check to make sure the point is not outside the tile
      if (!tile.ContainsLocalPoint(x, y)) {
        return false;
      }
      // check to see if the point is in a place of absolute discomfort
      if (tile.g[x, y] >= 1) {
        return false;
      }

      return true;
    }
  }
}
