using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collection of very useful computational geometry mathematics
/// taken from https://www.habrador.com/
///
/// (specifically, https://www.habrador.com/tutorials/math/)
/// </summary>
public static class Polygon
{
  //The list describing the polygon has to be sorted either clockwise or counter-clockwise because we have to identify its edges
  public static bool ContainsPoint(List<Vector2> polygonPoints, Vector2 point)
  {
    //Step 1. Find a point outside of the polygon
    //Pick a point with a x position larger than the polygons max x position, which is always outside
    Vector2 maxXPosVertex = polygonPoints[0];

    for (int i = 1; i < polygonPoints.Count; i++) {
      if (polygonPoints[i].x > maxXPosVertex.x) {
        maxXPosVertex = polygonPoints[i];
      }
    }

    //The point should be outside so just pick a number to make it outside
    Vector2 pointOutside = maxXPosVertex + new Vector2(10f, 0f);

    //Step 2. Create an edge between the point we want to test with the point thats outside
    Vector2 l1_p1 = point;
    Vector2 l1_p2 = pointOutside;

    //Step 3. Find out how many edges of the polygon this edge is intersecting
    int numberOfIntersections = 0;

    for (int i = 0; i < polygonPoints.Count; i++) {
      //Line 2
      Vector2 l2_p1 = polygonPoints[i];

      int iPlusOne = ClampListIndex(i + 1, polygonPoints.Count);

      Vector2 l2_p2 = polygonPoints[iPlusOne];

      //Are the lines intersecting?
      if (Lines.AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true)) {
        numberOfIntersections += 1;
      }
    }

    //Step 4. Is the point inside or outside?
    bool isInside = true;

    //The point is outside the polygon if number of intersections is even or 0
    if (numberOfIntersections == 0 || numberOfIntersections % 2 == 0) {
      isInside = false;
    }

    return isInside;
  }

  // Clamp list indices
  // Will even work if index is larger/smaller than listSize, so can loop multiple times
  public static int ClampListIndex(int index, int listSize)
  {
    index = ((index % listSize) + listSize) % listSize;

    return index;
  }

  public static bool TriangleContainsPoint(Triangle t, Vector2 p)
  {
    return TriangleContainsPoint(t.p1, t.p2, t.p3, p);
  }

  //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
  //p is the testpoint, and the other points are corners in the triangle
  public static bool TriangleContainsPoint(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
  {
    bool isWithinTriangle = false;

    //Based on Barycentric coordinates
    float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

    float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
    float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
    float c = 1 - a - b;

    ////The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
    //if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f) {
    //  isWithinTriangle = true;
    //}

    //The point is within the triangle
    if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f) {
      isWithinTriangle = true;
    }

    return isWithinTriangle;
  }
}
