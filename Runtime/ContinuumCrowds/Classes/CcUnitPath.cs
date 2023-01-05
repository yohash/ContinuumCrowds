using System;
using System.Linq;
using System.Collections.Generic;
using Yohash.DataStructures;

namespace Yohash.ContinuumCrowds
{
  public class CcUnitPath
  {
    // IF the unit is on a tile that doesn't exist in this solution,
    //      we need to determine the best way back to the nearest tile

    // maintain a linked list of destinations as well, so we can try
    // daisy-chaining our references
    private LinkedList<CcDestination> destinations;

    /// <summary>
    /// Tracking the current destination
    /// </summary>
    private LinkedListNode<CcDestination> _current;
    public CcDestination CurrentDestination {
      get { return _current.Value; }
    }

    /// <summary>
    /// This path is the ordered set of CcDestinations that must
    /// be traversed in order to arrive at a goal
    /// </summary>
    /// <param name="portals"></param>
    /// <param name="navSystem"></param>
    public CcUnitPath(
      IEnumerable<Portal> portals,
      Dictionary<Location, CcTile> tiles,
      Func<Location, Location> hashTileLocation
    )
    {
      destinations = new LinkedList<CcDestination>();

      foreach (var portal in portals) {
        // see if this is the starting or end portal
        var singular = portal.borderA.SequenceEqual(portal.borderB);
        if (singular && destinations.Count == 0) {
          var hashed = hashTileLocation(portal.borderA[0]);
          var tiled = tiles[hashed];
          var dest = new CcDestination(
            tiled,
            portal.borderA.ToList()
          );

          // this is the start
          if (destinations.Count == 0) {
            destinations.AddFirst(dest);
          } else {
            // this is the ending portal
            destinations.AddLast(dest);
          }
          continue;
        }
        // see if this is the ending portal
        else if (singular) {
          // this is the start
          var hashed = hashTileLocation(portal.borderA[0]);
          var tiled = tiles[hashed];
          var desty = new CcDestination(
            tiled,
            portal.borderA.ToList()
          );
          destinations.AddLast(desty);
          continue;
        }

        // based on the location from which we're coming, choose which
        // portal we want

        // (1) look at the most recent destination and grab the tile
        var lastTile = tiles[destinations.Last().TileHash];
        // (2) compare both borders in this portal to this tile
        var nextPortal =
          lastTile.ContainsGlobalPoint(portal.borderA[0])
            // (a) our last tile bontains border A, we want to path to border B
            ? portal.borderB
            // (b) our last tiles doesn't contain border B, so we path to A
            : portal.borderA;

        // grab the next tile based on the chosen portal,
        var nextTile = tiles[nextPortal[0]];
        var destination = new CcDestination(
          // TODO - VERIFY that I can path off an edge like this
          lastTile,
          new List<Location>(nextPortal)
        );
        destinations.AddLast(destination);
      }

      // initialize this class
      _current = destinations.First;
    }


    public void Advance()
    {
      _current = _current.Next;
    }
  }
}
