
namespace Yohash.ContinuumCrowds
{
  public static class CcTileExtensions
  {
    public static bool ContainsLocalPoint(this CcTile tile, int x, int y)
    {
      return 0 <= x && x <= tile.SizeX - 1
          && 0 <= y && y <= tile.SizeY - 1;
    }

    public static bool ContainsGlobalPoint(this CcTile tile, Location l)
    {
      return tile.ContainsGlobalPoint(l.x, l.y);
    }

    public static bool ContainsGlobalPoint(this CcTile tile, int x, int y)
    {
      return tile.Corner.x <= x && x <= tile.Corner.x + tile.SizeX - 1
          && tile.Corner.y <= y && y <= tile.Corner.y + tile.SizeY - 1;
    }

    public static bool IsLocalPointValid(this CcTile tile, int x, int y)
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
