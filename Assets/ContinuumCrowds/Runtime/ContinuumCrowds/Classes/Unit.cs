using UnityEngine;
using System;
using System.Collections.Generic;
using Yohash.Tools;

namespace Yohash.ContinuumCrowds
{
  public class Unit
  {
    /// <summary>
    /// The status of this unit's Nav solution
    /// </summary>
    public enum SolutionStatus { None, Requested, Waiting, Has_Path }
    private SolutionStatus _status;
    public SolutionStatus Status {
      get { return _status; }
    }
    /// <summary>
    /// Tell the unit that we're processing a request for a solution
    /// </summary>
    public void ProcessingRequest() { _status = SolutionStatus.Waiting; }
    /// <summary>
    /// Tell the unit that we've found a path for their recently requested solution
    /// </summary>
    public void PathFound() { _status = SolutionStatus.Has_Path; }
    /// <summary>
    /// Tell the unit that we've reached our destination
    /// </summary>
    public void DestinationReached()
    {
      _status = SolutionStatus.None;
      SetVelocity(new Vector2(0, 0));
    }

    /// <summary>
    /// This unit's ultimate destination
    /// </summary>
    private Vector2 _destination;
    public Vector2 Destination {
      get { return _destination; }
      set {
        _destination = value;
        _status = SolutionStatus.Requested;
      }
    }

    private float _falloff;
    public float FootprintRadialFalloff {
      get {
        if (_falloff != Constants.Values.u_unitRadialFalloff) {
          _falloff = Constants.Values.u_unitRadialFalloff;
          _baseFootprint = computeBaseFootprint();
        }
        return _falloff;
      }
    }

    /// <summary>
    /// Continuum Crowds unit interface access
    /// </summary>
    private IUnit _unit;

    /// <summary>
    /// Set the velocity on the cc unit interface
    /// </summary>
    /// <param name="v">Velocity</param>
    public void SetVelocity(Vector2 v) { _unit.SetVelocity(v); }

    /// <summary>
    /// a Unique (hashable) id for this unit
    /// </summary>
    public int Id => _unit.UniqueId;

    /// <summary>
    /// The 2D position of a unit
    /// </summary>
    public Vector2 Position => _unit.Position;

    /// <summary>
    /// The y-axis (xz-plane) rotation in degrees
    /// </summary>
    public float Rotation => _unit.Rotation;

    /// <summary>
    /// The 2D velocity of a unit, pulled from the cc unit interface
    /// </summary>
    public Vector2 Velocity => _unit.Velocity;

    /// <summary>
    /// The 2D size of a unit, assumed facing in the +y direction
    /// </summary>
    public Vector2 Size => _unit.Size;

    /// <summary>
    /// The total mass of the unit
    /// </summary>
    public float Mass => _unit.Mass;

    /// <summary>
    /// The 2D footprint of a unit, with radial dropoff
    /// </summary>
    private float[,] _baseFootprint;
    public float[,] BaseFootprint {
      get {
        if (_baseFootprint == null) {
          _baseFootprint = computeBaseFootprint();
        }
        return _baseFootprint;
      }
    }

    /// <summary>
    /// The unit footprint according to current rotation and speed
    /// </summary>
    private float[,] _footprint;
    public float[,] Footprint {
      get {
        if (_footprint == null) {
          computeStationaryFootprint();
        }
        return _footprint;
      }
    }

    /// <summary>
    /// This is the world-space point from which the footprint must
    /// be offset in order to properly center about the unit
    /// </summary>
    private Vector2 _anchor;
    public Vector2 Anchor {
      get { return _anchor; }
    }

    /// <summary>
    /// The id corresponding to the last update cycle this Unit calculated
    /// their fields
    /// </summary>
    private int _lastUpdateId;
    public int LastUpdateId {
      get { return _lastUpdateId; }
    }

    /// <summary>
    /// The current Continuum Crowds solution to which this unit is
    /// subscribed.
    ///
    /// TODO - remove this coupling between CCUnit and CCSolution.
    ///         perform this subscribe/unsub in NavSystem somehow
    /// </summary>
    private Solution _subscribedSolution;
    public Solution SubscribedSolution {
      get { return _subscribedSolution; }
      set {
        if (_subscribedSolution == value) { return; }
        if (_subscribedSolution != null) {
          _subscribedSolution.Unsubscribe(_unit.UniqueId);
        }
        if (value != null) {
          value.Subscribe(_unit.UniqueId);
        }
        _subscribedSolution = value;
      }
    }

    /// <summary>
    /// A list of the tiles that this CcUnit will affect. This
    /// list is computed by an external algorithm, and stored
    /// here locally for diffing.
    /// </summary>
    private List<Tile> currentTiles = new List<Tile>();

    // local tracking variable for position diffing
    private Vector2 _position;

    /// <summary>
    /// A Continuum Crowds Unit is instantiated with the
    /// CC Unit interface implemented by the unit represented
    /// </summary>
    /// <param name="ccUnitInterface"></param>
    public Unit(IUnit ccUnitInterface)
    {
      _unit = ccUnitInterface;
      _position = ccUnitInterface.Position;
      computeStationaryFootprint();
    }

    /// <summary>
    /// In this destructor, unsubscribe from all our delegates
    /// </summary>
    ~Unit()
    {
      foreach (var tile in currentTiles) {
        tile.Unsubscribe(_unit.UniqueId);
      }
      if (_subscribedSolution != null) {
        _subscribedSolution.Unsubscribe(_unit.UniqueId);
      }
    }

    /// <summary>
    /// Receive a list of the tiles that this unit is affecting.
    /// We will internally diff this list with a stored copy,
    /// and subscribe/unsubscribe to updates from new/old tiles.
    ///
    /// TODO - should the unit have to track their influencing tiles?
    ///     Feels like extra work that an outside agent should manage
    /// </summary>
    /// <param name="newTiles"></param>
    public void DiffInfluencingTiles(List<Tile> newTiles)
    {
      foreach (var newTile in newTiles) {
        if (!currentTiles.Contains(newTile)) {
          // this is a new tile. Subscribe
          newTile.Subscribe(_unit.UniqueId);
        }
      }
      foreach (var currentTile in currentTiles) {
        if (!newTiles.Contains(currentTile)) {
          // we no longer affect this tile. Unsubscribe
          currentTile.Unsubscribe(_unit.UniqueId);
        }
      }
      // clear list
      currentTiles.Clear();
      // add new tiles
      currentTiles.AddRange(newTiles);
    }

    public bool DidUnitMove()
    {
      var oldPosition = _position;
      _position = _unit.Position;
      return oldPosition.x != _position.x || oldPosition.y != _position.y;
    }

    public bool ShouldUpdate(int updateId)
    {
      return updateId != _lastUpdateId;
    }

    public int Update(int updateId)
    {
      // TODO - round velocity and angle to "nearest values", to see if we can
      //        eliminate re-computing footprints for insignificant changes

      // TODO - instead of a "Speed Threashold" we should calculate whether the
      //        predictive velocity as a factor of this unit's speed will result in
      //        meaningful footprint modification (ie. 3+ tiles)

      // CcUnit update task
      //      - get velocity
      //      - rotate unit footprint
      //      - create "predictive" unit footprint
      if (_unit.Speed < Constants.Values.v_dynamicFootprintThreshold) {
        computeStationaryFootprint();
      } else {
        computeMobileFootprint(Constants.Values.v_predictiveSeconds);
      }

      // Update this value last, so a query made to this Unit's ShouldUpdate()
      // will return the update state as-queried
      _lastUpdateId = updateId;

      return Id;
    }

    private float[,] computeBaseFootprint()
    {
      return Fields.RectWithRadialFadeout((int)Size.x + 1, (int)Size.y + 1, FootprintRadialFalloff);
    }

    public Vector2 DriverReference {
      get {
        var yOff = new Vector2(0, Size.y / 2f + Constants.Values.u_driverSeatOffset);
        yOff = yOff.Rotate(-_unit.Rotation * Mathf.Deg2Rad);
        return Position + yOff;
      }
    }

    private void computeStationaryFootprint()
    {
      // TODO - remove this debugging code, used to trigger basefootprint rebuilds
      //        while we're modifying them in the editor
      var ft = FootprintRadialFalloff;
      // rotate the base footprint to match unit's rotation
      _footprint = BaseFootprint.Rotate(-_unit.Rotation);
      // compute the footprint's half-dimensions
      var xHalf = _footprint.GetLength(0) / 2f;
      var yHalf = _footprint.GetLength(1) / 2f;
      // an offset to counter the (+1, +1) added to create equivalent volumes
      var offset = 0.5F * Vector2.one;
      // translate the anchor so the footprint is centered about our unit
      _anchor = _unit.Position - new Vector2(xHalf, yHalf) + offset;
      // perform bilinear interpolation of the footprint at our anchor
      _footprint = _footprint.BilinearInterpolation(_anchor);
    }

    private void computeMobileFootprint(float distanceScalar)
    {
      // fetch unit properties
      var speed = _unit.Speed;
      //var footprint = _ccUnitInterface.Footprint();

      // (1) compute values
      var distance = (int)Math.Ceiling(speed * distanceScalar);
      var footprintEnd = Mathf.FloorToInt(FootprintRadialFalloff + Size.x + 1);

      var start = Constants.Values.v_scaleMax;
      var end = Constants.Values.v_scaleMin;

      var predictive = Fields.LinearFadeout(BaseFootprint, footprintEnd, distance, start, end);

      // (2) rotate the rect
      var yEuler = Rotation;

      // Unity y-euler rotations start at +z (+y in 2D) and move CW.
      // Academic rotations are described as CCW from the +x axis, which is what
      // many of our derivations are based, so we convert here.
      var degrees = Mathf.Repeat(90 - yEuler, 360);
      var rotated = predictive.Rotate(degrees);

      // (3) determine anchor position - do this by taking the "perfect" center
      //     and applying the same translations/rotations that our rotate process
      //     applies
      var height = BaseFootprint.GetLength(1);
      //   (i) declare unit location in original footprint center
      var unitOffset = new Vector2(BaseFootprint.GetLength(0) / 2f, height / 2f);
      //   (ii) translate by predictive velocity half-shape to center on (0,0)
      unitOffset += new Vector2(-predictive.GetLength(0) / 2f, -height / 2f);
      //   (iii) rotate the point about (0,0) by our unit's rotation
      unitOffset = unitOffset.Rotate(degrees * Mathf.Deg2Rad);
      //   (iv) translate back by rotated shape half-space
      unitOffset += new Vector2(rotated.GetLength(0) / 2f, rotated.GetLength(1) / 2f);

      // (4) finally, translate the anchor to be positioned on the unit
      _anchor = Position - unitOffset + Vector2.one * 0.5f;
      // (5) inteprolate the final result
      _footprint = rotated.BilinearInterpolation(_anchor);
    }

    public override int GetHashCode()
    {
      return _unit.UniqueId;
    }
  }
}