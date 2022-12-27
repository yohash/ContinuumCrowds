using System.Collections.Generic;
using UnityEngine;

public struct Triangle
{
  public Vector2 p1;
  public Vector2 p2;
  public Vector2 p3;
}

/// <summary>
/// Draw basic shapes by pixel.
/// </summary>
public static class Shapes
{
  /// <summary>
  /// Fill a circle defined by a center point and a radius
  /// </summary>
  public static List<Vector2> FillCircle(int x, int y, int r)
  {
    List<Vector2> circle = new List<Vector2>();
    float rSq = r * r;
    for (int xL = x - r; xL < x + r * 2; xL++) {
      for (int yL = y - r; yL < y + r * 2; yL++) {
        if ((new Vector2(xL, yL) - new Vector2(x, y)).sqrMagnitude < rSq) {
          circle.Add(new Vector2(xL, yL));
        }
      }
    }
    return circle;
  }
  public static List<Vector2> FillCircle(Vector2 c, int r)
  {
    return FillCircle(Mathf.RoundToInt(c.x), Mathf.RoundToInt(c.y), r);
  }

  /// <summary>
  /// Draw a line on a Texture2D with thickness by creating the polygon (rectangle) to represent the
  /// line outline and fill
  /// </summary>
  public static List<Vector2> DrawLine(Vector2 start, Vector2 end, int size)
  {
    if (size < 1) { size = 1; }
    if (size == 1) { return DrawLine(start, end); }

    // find the angle that the line faces, and rotate by 90 degrees
    float degrees = Vector2.SignedAngle(Vector2.right, end - start) + 90;
    // define the vector perpendicular to the line, with magnitude = 1/2 thickness
    Vector2 perpendicular = Vector2.right.Rotate(Mathf.Deg2Rad * degrees) * size / 2f;

    // fill polygon defined by rectangle of the "outer edge" of the line
    return FillPolygon(new List<Vector2> {
        start + perpendicular,
        start - perpendicular,
        end - perpendicular,
        end + perpendicular
    });
  }

  /// <summary>
  /// Draw a 1 pixel line defined by the start and end vectors
  /// </summary>
  public static List<Vector2> DrawLine(Vector2 start, Vector2 end)
  {
    return DrawLine(
        Mathf.RoundToInt(start.x),
        Mathf.RoundToInt(start.y),
        Mathf.RoundToInt(end.x),
        Mathf.RoundToInt(end.y)
    );
  }

  /// <summary>
  /// Draw a 1-pixel line on a Texture2D using Bresenham's Line Algorithm
  /// https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
  /// </summary>
  public static List<Vector2> DrawLine(int x0, int y0, int x1, int y1)
  {
    // return list
    List<Vector2> line = new List<Vector2>();

    int dx = Mathf.Abs(x1 - x0);
    int sx = x0 < x1 ? 1 : -1;
    int dy = -Mathf.Abs(y1 - y0);
    int sy = y0 < y1 ? 1 : -1;

    int delta = dx + dy;

    int x = x0;
    int y = y0;

    while (x != x1 || y != y1) {
      line.Add(new Vector2(x, y));
      int d2 = 2 * delta;
      if (d2 >= dy) {
        delta += dy;
        x += sx;
      }
      if (d2 <= dx) {
        delta += dx;
        y += sy;
      }
    }

    // fill start and end pixels
    line.Add(new Vector2(x0, y0));
    line.Add(new Vector2(x1, y1));

    return line;
  }

  /// <summary>
  /// Sort the input list of polygon points clockwise or counter-clockwise
  /// </summary>
  public static List<Vector2> FillPolygon(List<Vector2> polygon)
  {
    // not a polygon
    if (polygon.Count < 3) { return polygon; }

    // return list
    List<Vector2> filled = new List<Vector2>();
    // minimize our Scan-Line algorithm to the bounds of the polygon
    Rect bounds = polygon.Bounds();

    // fill using a basic Scan-Line Algorithm
    for (int y = Mathf.FloorToInt(bounds.y); y <= Mathf.CeilToInt(bounds.y + bounds.height); y++) {
      for (int x = Mathf.FloorToInt(bounds.x); x <= Mathf.CeilToInt(bounds.x + bounds.width); x++) {
        // tests for triangles are more efficient/accurate with this algorithm
        if (polygon.Count == 3 && Polygon.TriangleContainsPoint(polygon[0], polygon[1], polygon[2], new Vector2(x, y))) {
          filled.Add(new Vector2(x, y));
        } else if (Polygon.ContainsPoint(polygon, new Vector2(x, y))) {
          filled.Add(new Vector2(x, y));
        }
      }
    }

    return filled;
  }
}
