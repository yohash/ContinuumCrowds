using UnityEngine;

namespace Yohash.ContinuumCrowds
{
  [CreateAssetMenu(menuName = "New Continuum Crowds Constants", fileName = "CCConstants")]
  public class Constants : ScriptableObject
  {
    /// <summary>
    /// Assign this to a static variable when your code instantiates to
    /// reference a local Scriptable Object. This will allow you to
    /// modify the values in the Unity Editor and have them update in realtime
    /// </summary>
    public static Constants Instance;
    public static Constants Values {
      get {
        return Instance ?? default;
      }
    }

    // how far a unit's footprint will radially extend beyond its given size
    [Header("How far a unit's footprint will extend beyond its size")]
    public float u_unitRadialFalloff = 0f;

    // how far into future we predict the path due to unit with velocity
    [Header("Speed value over which a dynamic footprint is computed vs. static")]
    public float v_dynamicFootprintThreshold = 0.25f;

    // how far into future we predict the path due to unit with velocity
    [Header("Number of seconds to extrapolate unit's velocity")]
    public float v_predictiveSeconds = 1f;

    [Header("Max and min scalars to weight unit's extrapolated velocity")]
    public float v_scaleMax = 0.3f;
    public float v_scaleMin = 0.25f;

    // everything above this must be clamped to 'unpassable' discomfort map
    [Header("Max and Min slopes to scale topographical speed")]
    public float f_slopeMax = 1f;
    public float f_slopeMin = -1f;

    [Header("Max and min densities to determine flow speed, or topographical speed")]
    public float f_rhoMax = 0.8f;
    public float f_rhoMin = 0.3f;

    [Header("Max and min speed field")]
    // set to zero to clamp flow speed
    public float f_speedMin = 0f;
    public float f_speedMax = 20f;

    // path length field weight
    [Header("Weights: Path Length")]
    public float C_alpha = 1f;
    // time weight (inverse of speed)
    [Header("Weights: Time (speed field inverse)")]
    public float C_beta = 1f;
    // discomfort weight
    [Header("Weights: Discomfort")]
    public float C_gamma = 1f;

    [Header("Weighted average for Eikonal solutions")]
    // Eikonal solver weighted average, max weight
    public float maxWeight = 2.5f;
    // Eikonal solver weighted average, min weight
    public float minWeight = 1f;

    public float FlatSpeed {
      get {
        return f_speedMax + (-f_slopeMin) / (f_slopeMax - f_slopeMin) * (f_speedMin - f_speedMax);
      }
    }

    // this array of Vect2's correlates to our data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
    public Vector2[] ENSW = new Vector2[] {
      Vector2.right,
      Vector2.up,
      Vector2.left,
      Vector2.down
    };
    public Vector2Int[] ENSWint = new Vector2Int[] {
      Vector2Int.right,
      Vector2Int.up,
      Vector2Int.left,
      Vector2Int.down
    };
  }
}
