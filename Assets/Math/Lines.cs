using System;
using UnityEngine;

public static class Lines
{
  /// <summary>
  /// Basic algorithm to find the distance squared from a point (p) to a line
  /// defined by two points (l1, and l2)
  ///
  /// https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
  /// </summary>
  /// <param name="l1">Point 1 on the line</param>
  /// <param name="l2">Point 2 on the line</param>
  /// <param name="p">The point to test distance</param>
  /// <returns>The distance squared from a point to the line</returns>
  public static float DistanceSqFromPointToLine(Vector2 l1, Vector2 l2, Vector2 p)
  {
    float dx = l2.x - l1.x;
    float dy = l2.y - l1.y;

    float numerator = dy * p.x - dx * p.y + l2.x * l1.y - l2.y * l1.x;

    return numerator * numerator / (dy * dy + dx * dx);
  }

  public static float DistanceFromPointToLine(Vector2 l1, Vector2 l2, Vector2 p)
  {
    float dx = l2.x - l1.x;
    float dy = l2.y - l1.y;

    float numerator = dy * p.x - dx * p.y + l2.x * l1.y - l2.y * l1.x;

    return (float)(Math.Abs(numerator) / Math.Sqrt(dy * dy + dx * dx));
  }

  // http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
  public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
  {
    bool isIntersecting = false;

    float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

    //Make sure the denominator is > 0, if not the lines are parallel
    if (denominator != 0f) {
      float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
      float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

      //Are the line segments intersecting if the end points are the same
      if (shouldIncludeEndPoints) {
        //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
        if (u_a >= 0f && u_a <= 1f && u_b >= 0f && u_b <= 1f) {
          isIntersecting = true;
        }
      } else {
        //Is intersecting if u_a and u_b are between 0 and 1
        if (u_a > 0f && u_a < 1f && u_b > 0f && u_b < 1f) {
          isIntersecting = true;
        }
      }
    }

    return isIntersecting;
  }
}
