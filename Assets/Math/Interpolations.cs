using UnityEngine;
using System;

public static class Interpolations
{
  public static float InterpolateHighestNearestValuePreserving(this float[,] matrix, float x, float y)
  {
    // store constants
    var n = matrix.GetLength(0);
    var m = matrix.GetLength(1);

    // helper function to determine if the given indeces are outside our original matrix
    bool outside(double x, double y) { return x < 0 || y < 0 || x > n - 1 || y > m - 1; }

    // precompute some quantities
    // (these quantities are inverse to the actual equations because we are translating
    // from a transformed coordinate plane Back Into the original)
    var dx = modulus(x, 1);
    var dy = modulus(y, 1);

    var dx1 = dx;
    var dx2 = 1f - dx;

    var dy1 = dy;
    var dy2 = 1f - dy;

    // return the special case: we are asked to interpolate on a direct value
    if ((dx1 == 0 && dy1 == 0) || (dx2 == 0 && dy2 == 0)) {
      return matrix[(int)x, (int)y];
    }

    // get the surrounding grid's coordinates starting with ceilings
    var xc = (int)Math.Ceiling(x);
    var yc = (int)Math.Ceiling(y);
    var xf = xc - 1;
    var yf = yc - 1;

    // if any components are outside the grid, assign a 0 to that entry
    var Q11 = outside(xf, yf) ? 0 : matrix[xf, yf];
    var Q12 = outside(xf, yc) ? 0 : matrix[xf, yc];
    var Q21 = outside(xc, yf) ? 0 : matrix[xc, yf];
    var Q22 = outside(xc, yc) ? 0 : matrix[xc, yc];

    // define primed constants
    var dx1p = dx1;
    var dx2p = dx2;

    var dy1p = dy1;
    var dy2p = dy2;

    // perform one's preseving interpolation
    // *********************************************************************
    // first check point Q11
    if (Q11 > Q21 && Q11 > Q12 && Q11 > Q22) {
      if (dx1 < 0.5f && dy1 < 0.5f) { return Q11; }

      dx1p = dx1 > 0.5f ? -2 * dx1 + 2 : 1;
      dx2p = dx1 > 0.5f ? 2 * dx1 - 1 : 0;

      dy1p = dy1 > 0.5f ? -2 * dy1 + 2 : 1;
      dy2p = dy1 > 0.5f ? 2 * dy1 - 1 : 0;
    }
    // check point Q21
    else if (Q21 > Q11 && Q21 > Q12 && Q21 > Q22) {
      if (dx2 < 0.5f && dy1 < 0.5f) { return Q21; }

      dx1p = dx2 > 0.5f ? 2 * dx2 - 1 : 0;
      dx2p = dx2 > 0.5f ? -2 * dx2 + 2 : 1;

      dy1p = dy1 > 0.5f ? -2 * dy1 + 2 : 1;
      dy2p = dy1 > 0.5f ? 2 * dy1 - 1 : 0;
    }
    // check point Q12
    else if (Q12 > Q11 && Q12 > Q21 && Q12 > Q22) {
      if (dx1 < 0.5f && dy2 < 0.5f) { return Q12; }

      dx1p = dx1 > 0.5f ? -2 * dx1 + 2 : 1;
      dx2p = dx1 > 0.5f ? 2 * dx1 - 1 : 0;

      dy1p = dy2 > 0.5f ? 2 * dy2 - 1 : 0;
      dy2p = dy2 > 0.5f ? -2 * dy2 + 2 : 1;
    }
    // check point Q22
    else if (Q22 > Q11 && Q22 > Q21 && Q22 > Q12) {
      if (dx2 < 0.5f && dy2 < 0.5f) { return Q22; }

      dx1p = dx2 > 0.5f ? 2 * dx2 - 1 : 0;
      dx2p = dx2 > 0.5f ? -2 * dx2 + 2 : 1;

      dy1p = dy2 > 0.5f ? 2 * dy2 - 1 : 0;
      dy2p = dy2 > 0.5f ? -2 * dy2 + 2 : 1;
    }
    // *********************************************************************
    // Q11 && Q12
    else if (Q11 == Q12 && Q11 > Q21 && Q11 > Q22) {
      if (dx1 < 0.5f) { return Q11; }
      // dy doesn't change, interps at same rate
      dx1p = -2 * dx1 + 2;
      dx2p = 2 * dx1 - 1;
    }
    // Q11 && Q21
    else if (Q11 == Q21 && Q11 > Q12 && Q11 > Q22) {
      if (dy1 < 0.5f) { return Q11; }
      // dx doesn't change, interps at same rate
      dy1p = -2 * dy1 + 2;
      dy2p = 2 * dy1 - 1;
    }
    // Q12 && Q22
    else if (Q12 == Q22 && Q22 > Q21 && Q22 > Q11) {
      if (dy2 < 0.5f) { return Q22; }
      // dx doesn't change, interps at same rate
      dy1p = 2 * dy2 - 1;
      dy2p = -2 * dy2 + 2;
    }
    // Q21 && Q22
    else if (Q21 == Q22 && Q22 > Q12 && Q22 > Q11) {
      if (dx2 < 0.5f) { return Q22; }
      // dy doesn't change, interps at same rate
      dx1p = 2 * dx2 - 1;
      dx2p = -2 * dx2 + 2;
    }
    // *********************************************************************
    // Q11 - Q12 - Q21
    else if (Q11 == Q12 && Q11 == Q21 && Q11 > Q22) {
      if (dx1 < 0.5f || dy1 < 0.5f) { return Q11; }

      dx1p = -2 * dx1 + 2;
      dx2p = 2 * dx1 - 1;

      dy1p = -2 * dy1 + 2;
      dy2p = 2 * dy1 - 1;
    }
    // Q11 - Q21 - Q22
    else if (Q11 == Q21 && Q11 == Q22 && Q11 > Q12) {
      if (dx2 < 0.5f || dy1 < 0.5f) { return Q11; }

      dx1p = 2 * dx2 - 1;
      dx2p = -2 * dx2 + 2;

      dy1p = -2 * dy1 + 2;
      dy2p = 2 * dy1 - 1;
    }
    // Q11 - Q12 - Q22
    else if (Q11 == Q12 && Q11 == Q22 && Q11 > Q21) {
      if (dx1 < 0.5f || dy2 < 0.5f) { return Q11; }

      dx1p = -2 * dx1 + 2;
      dx2p = 2 * dx1 - 1;

      dy1p = 2 * dy2 - 1;
      dy2p = -2 * dy2 + 2;
    }
    // Q12 - Q21 - Q22
    else if (Q22 == Q21 && Q22 == Q12 && Q22 > Q11) {
      if (dx2 < 0.5f || dy2 < 0.5f) { return Q22; }

      dx1p = 2 * dx2 - 1;
      dx2p = -2 * dx2 + 2;

      dy1p = 2 * dy2 - 1;
      dy2p = -2 * dy2 + 2;
    }
    // *********************************************************************
    else if (Q11 == Q12 && Q11 == Q21 && Q11 == Q22) {
      return Q11;
    }

    // compute the interpolated value
    return dy1p * (dx1p * Q11 + dx2p * Q21) + dy2p * (dx1p * Q12 + dx2p * Q22);
  }

  public static float InterpolateOnesPreserving(this float[,] matrix, float x, float y)
  {
    // store constants
    var n = matrix.GetLength(0);
    var m = matrix.GetLength(1);

    // helper function to determine if the given indeces are outside our original matrix
    bool outside(double x, double y) { return x < 0 || y < 0 || x > n - 1 || y > m - 1; }

    // precompute some quantities
    // (these quantities are inverse to the actual equations because we are translating
    // from a transformed coordinate plane Back Into the original)
    var dx = modulus(x, 1);
    var dy = modulus(y, 1);

    var dx1 = dx;
    var dx2 = 1f - dx;

    var dy1 = dy;
    var dy2 = 1f - dy;

    // return the special case: we are asked to interpolate on a direct value
    if ((dx1 == 0 && dy1 == 0) || (dx2 == 0 && dy2 == 0)) {
      return matrix[(int)x, (int)y];
    }

    // get the surrounding grid's coordinates starting with ceilings
    var xc = (int)Math.Ceiling(x);
    var yc = (int)Math.Ceiling(y);
    var xf = xc - 1;
    var yf = yc - 1;

    // if any components are outside the grid, assign a 0 to that entry
    var Q11 = outside(xf, yf) ? 0 : matrix[xf, yf];
    var Q12 = outside(xf, yc) ? 0 : matrix[xf, yc];
    var Q21 = outside(xc, yf) ? 0 : matrix[xc, yf];
    var Q22 = outside(xc, yc) ? 0 : matrix[xc, yc];

    // define primed constants
    var dx1p = dx1;
    var dx2p = dx2;

    var dy1p = dy1;
    var dy2p = dy2;

    // perform one's preseving interpolation
    // *********************************************************************
    // first check point Q11
    if (Q11 == 1 && Q21 != 1 && Q12 != 1 && Q22 != 1) {
      if (dx1 < 0.5f && dy1 < 0.5f) { return 1; }

      dx1p = dx1 > 0.5f ? -2 * dx1 + 2 : 1;
      dx2p = dx1 > 0.5f ? 2 * dx1 - 1 : 0;

      dy1p = dy1 > 0.5f ? -2 * dy1 + 2 : 1;
      dy2p = dy1 > 0.5f ? 2 * dy1 - 1 : 0;
    }
    // check point Q21
    else if (Q11 != 1 && Q21 == 1 && Q12 != 1 && Q22 != 1) {
      if (dx2 < 0.5f && dy1 < 0.5f) { return 1; }

      dx1p = dx2 > 0.5f ? 2 * dx2 - 1 : 0;
      dx2p = dx2 > 0.5f ? -2 * dx2 + 2 : 1;

      dy1p = dy1 > 0.5f ? -2 * dy1 + 2 : 1;
      dy2p = dy1 > 0.5f ? 2 * dy1 - 1 : 0;
    }
    // check point Q12
    else if (Q11 != 1 && Q21 != 1 && Q12 == 1 && Q22 != 1) {
      if (dx1 < 0.5f && dy2 < 0.5f) { return 1; }

      dx1p = dx1 > 0.5f ? -2 * dx1 + 2 : 1;
      dx2p = dx1 > 0.5f ? 2 * dx1 - 1 : 0;

      dy1p = dy2 > 0.5f ? 2 * dy2 - 1 : 0;
      dy2p = dy2 > 0.5f ? -2 * dy2 + 2 : 1;
    }
    // check point Q22
    else if (Q11 != 1 && Q21 != 1 && Q12 != 1 && Q22 == 1) {
      if (dx2 < 0.5f && dy2 < 0.5f) { return 1; }

      dx1p = dx2 > 0.5f ? 2 * dx2 - 1 : 0;
      dx2p = dx2 > 0.5f ? -2 * dx2 + 2 : 1;

      dy1p = dy2 > 0.5f ? 2 * dy2 - 1 : 0;
      dy2p = dy2 > 0.5f ? -2 * dy2 + 2 : 1;
    }
    // *********************************************************************
    // Q11 && Q12
    else if (Q11 == 1 && Q21 != 1 && Q12 == 1 && Q22 != 1) {
      if (dx1 < 0.5f) { return 1; }
      // dy doesn't change, interps at same rate
      dx1p = -2 * dx1 + 2;
      dx2p = 2 * dx1 - 1;
    }
    // Q11 && Q21
    else if (Q11 == 1 && Q21 == 1 && Q12 != 1 && Q22 != 1) {
      if (dy1 < 0.5f) { return 1; }
      // dx doesn't change, interps at same rate
      dy1p = -2 * dy1 + 2;
      dy2p = 2 * dy1 - 1;
    }
    // Q12 && Q22
    else if (Q11 != 1 && Q21 != 1 && Q12 == 1 && Q22 == 1) {
      if (dy2 < 0.5f) { return 1; }
      // dx doesn't change, interps at same rate
      dy1p = 2 * dy2 - 1;
      dy2p = -2 * dy2 + 2;
    }
    // Q21 && Q22
    else if (Q11 != 1 && Q21 == 1 && Q12 != 1 && Q22 == 1) {
      if (dx2 < 0.5f) { return 1; }
      // dy doesn't change, interps at same rate
      dx1p = 2 * dx2 - 1;
      dx2p = -2 * dx2 + 2;
    }
    // *********************************************************************
    // Q11 - Q12 - Q21
    else if (Q11 == 1 && Q21 == 1 && Q12 == 1 && Q22 != 1) {
      if (dx1 < 0.5f || dy1 < 0.5f) { return 1; }

      dx1p = -2 * dx1 + 2;
      dx2p = 2 * dx1 - 1;

      dy1p = -2 * dy1 + 2;
      dy2p = 2 * dy1 - 1;
    }
    // Q11 - Q21 - Q22
    else if (Q11 == 1 && Q21 == 1 && Q12 != 1 && Q22 == 1) {
      if (dx2 < 0.5f || dy1 < 0.5f) { return 1; }

      dx1p = 2 * dx2 - 1;
      dx2p = -2 * dx2 + 2;

      dy1p = -2 * dy1 + 2;
      dy2p = 2 * dy1 - 1;
    }
    // Q11 - Q12 - Q22
    else if (Q11 == 1 && Q21 != 1 && Q12 == 1 && Q22 == 1) {
      if (dx1 < 0.5f || dy2 < 0.5f) { return 1; }

      dx1p = -2 * dx1 + 2;
      dx2p = 2 * dx1 - 1;

      dy1p = 2 * dy2 - 1;
      dy2p = -2 * dy2 + 2;
    }
    // Q12 - Q21 - Q22
    else if (Q11 != 1 && Q21 == 1 && Q12 == 1 && Q22 == 1) {
      if (dx2 < 0.5f || dy2 < 0.5f) { return 1; }

      dx1p = 2 * dx2 - 1;
      dx2p = -2 * dx2 + 2;

      dy1p = 2 * dy2 - 1;
      dy2p = -2 * dy2 + 2;
    }
    // *********************************************************************
    else if (Q11 == 1 && Q21 == 1 && Q12 == 1 && Q22 == 1) {
      return 1;
    }

    // compute the interpolated value
    return dy1p * (dx1p * Q11 + dx2p * Q21) + dy2p * (dx1p * Q12 + dx2p * Q22);
  }

  /// <summary>
  /// Interpolate the float value at non-discrete (x,y) inside the float[,] grid
  /// </summary>
  /// <param name="x">The x point to interpolate</param>
  /// <param name="y">The y point to interpolate</param>
  /// <param name="array">The array from which the interpolated value is
  /// calculated</param>
	public static float Interpolate(this float[,] array, float x, float y)
  {
    float xcomp;

    int xl = array.GetLength(0);
    int yl = array.GetLength(1);

    int topLeftX = (int)Mathf.Floor(x);
    int topLeftY = (int)Mathf.Floor(y);

    float xAmountRight = x - topLeftX;
    float xAmountLeft = 1.0f - xAmountRight;
    float yAmountBottom = y - topLeftY;
    float yAmountTop = 1.0f - yAmountBottom;

    Vector4 valuesX = Vector4.zero;

    // helper function to determine if the given indeces are outside our original matrix
    bool outside(double x, double y) { return x < 0 || y < 0 || x > xl - 1 || y > yl - 1; }

    if (!outside(topLeftX, topLeftY)) {
      valuesX[0] = array[topLeftX, topLeftY];
    }
    if (!outside(topLeftX + 1, topLeftY)) {
      valuesX[1] = array[topLeftX + 1, topLeftY];
    }
    if (!outside(topLeftX, topLeftY + 1)) {
      valuesX[2] = array[topLeftX, topLeftY + 1];
    }
    if (!outside(topLeftX + 1, topLeftY + 1)) {
      valuesX[3] = array[topLeftX + 1, topLeftY + 1];
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(valuesX[n])) {
        valuesX[n] = 0f;
      }
      if (float.IsInfinity(valuesX[n])) {
        valuesX[n] = 0f;
      }
    }

    float averagedXTop = valuesX[0] * xAmountLeft + valuesX[1] * xAmountRight;
    float averagedXBottom = valuesX[2] * xAmountLeft + valuesX[3] * xAmountRight;

    xcomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    return xcomp;
  }

  /// <summary>
  /// Interpolate the Vector2 value at (x,y) inside the Vector2[,] grid
  /// </summary>
  /// <param name="x">The x point to interpolate</param>
  /// <param name="y">The y point to interpolate</param>
  /// <param name="array">The array from which the interpolated value is
  /// calculated</param>
  public static Vector2 Interpolate(this Vector2[,] array, float x, float y, bool clamped = false)
  {
    float xcomp, ycomp;

    int xl = array.GetLength(0);
    int yl = array.GetLength(1);

    int topLeftX = (int)Mathf.Floor(x);
    int topLeftY = (int)Mathf.Floor(y);

    int tlXint = Mathf.FloorToInt(x);
    int tlYint = Mathf.FloorToInt(y);

    float xAmountRight = x - topLeftX;
    float xAmountLeft = 1.0f - xAmountRight;
    float yAmountBottom = y - topLeftY;
    float yAmountTop = 1.0f - yAmountBottom;

    var valuesX = Vector4.zero;

    // helper function to determine if the given indeces are outside our original matrix
    bool outside(double x, double y) { return x < 0 || y < 0 || x > xl - 1 || y > yl - 1; }
    Vector2 nearest(double x, double y)
    {
      int xin = (int)(x < 0 ? 0 : (x > xl - 1 ? xl - 1 : x));
      int yin = (int)(y < 0 ? 0 : (y > yl - 1 ? yl - 1 : y));
      return array[xin, yin];
    }


    if (clamped) {
      topLeftX = topLeftX < 0 ? 0 : topLeftX > xl - 1 ? xl - 1 : topLeftX;
      topLeftY = topLeftY < 0 ? 0 : topLeftY > yl - 1 ? yl - 1 : topLeftY;
    }

    if (!outside(topLeftX, topLeftY)) {
      valuesX[0] = array[topLeftX, topLeftY].x;
    } else if (outside(topLeftY, topLeftY) && clamped) {
      valuesX[0] = nearest(topLeftX, topLeftY).x;
    }
    if (!outside(topLeftX + 1, topLeftY)) {
      valuesX[1] = array[topLeftX + 1, topLeftY].x;
    } else if (outside(topLeftX + 1, topLeftY) && clamped) {
      valuesX[1] = nearest(topLeftX + 1, topLeftY).x;
    }
    if (!outside(topLeftX, topLeftY + 1)) {
      valuesX[2] = array[topLeftX, topLeftY + 1].x;
    } else if (outside(topLeftX, topLeftY + 1) && clamped) {
      valuesX[2] = nearest(topLeftX, topLeftY + 1).x;
    }
    if (!outside(topLeftX + 1, topLeftY + 1)) {
      valuesX[3] = array[topLeftX + 1, topLeftY + 1].x;
    } else if (outside(topLeftX + 1, topLeftY + 1) && clamped) {
      valuesX[3] = nearest(topLeftX + 1, topLeftY + 1).x;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(valuesX[n])) {
        valuesX[n] = 0f;
      }
      if (float.IsInfinity(valuesX[n])) {
        valuesX[n] = 0f;
      }
    }

    float averagedXTop = valuesX[0] * xAmountLeft + valuesX[1] * xAmountRight;
    float averagedXBottom = valuesX[2] * xAmountLeft + valuesX[3] * xAmountRight;

    xcomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    // y component of the vector 2
    Vector4 valuesY = Vector4.zero;

    if (!outside(topLeftX, topLeftY)) {
      valuesY[0] = array[topLeftX, topLeftY].y;
    } else if (outside(topLeftY, topLeftY) && clamped) {
      valuesY[0] = nearest(topLeftX, topLeftY).y;
    }
    if (!outside(topLeftX + 1, topLeftY)) {
      valuesY[1] = array[topLeftX + 1, topLeftY].y;
    } else if (outside(topLeftX + 1, topLeftY) && clamped) {
      valuesY[1] = nearest(topLeftX + 1, topLeftY).y;
    }
    if (!outside(topLeftX, topLeftY + 1)) {
      valuesY[2] = array[topLeftX, topLeftY + 1].y;
    } else if (outside(topLeftX, topLeftY + 1) && clamped) {
      valuesY[2] = nearest(topLeftX, topLeftY + 1).y;
    }
    if (!outside(topLeftX + 1, topLeftY + 1)) {
      valuesY[3] = array[topLeftX + 1, topLeftY + 1].y;
    } else if (outside(topLeftX + 1, topLeftY + 1) && clamped) {
      valuesY[3] = nearest(topLeftX + 1, topLeftY + 1).y;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(valuesY[n])) {
        valuesY[n] = 0f;
      }
      if (float.IsInfinity(valuesY[n])) {
        valuesY[n] = 0f;
      }
    }

    averagedXTop = valuesY[0] * xAmountLeft + valuesY[1] * xAmountRight;
    averagedXBottom = valuesY[2] * xAmountLeft + valuesY[3] * xAmountRight;

    ycomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    return (new Vector2(xcomp, ycomp));
  }

  /// <summary>
  /// Interpolate the Vector2 value at (x,y) inside the Vector2[,] grid
  /// </summary>
  /// <param name="x">The x point to interpolate</param>
  /// <param name="y">The y point to interpolate</param>
  /// <param name="array">The array from which the interpolated value is
  /// calculated</param>
  public static Vector4 Interpolate(this Vector4[,] array, float x, float y)
  {
    float wcomp, xcomp, ycomp, zcomp;

    int xl = array.GetLength(0);
    int yl = array.GetLength(1);

    int topLeftX = (int)Mathf.Floor(x);
    int topLeftY = (int)Mathf.Floor(y);

    float xAmountRight = x - topLeftX;
    float xAmountLeft = 1.0f - xAmountRight;
    float yAmountBottom = y - topLeftY;
    float yAmountTop = 1.0f - yAmountBottom;

    // x component of the vector 4
    var values = Vector4.zero;

    // helper function to determine if the given indeces are outside our original matrix
    bool outside(double x, double y) { return x < 0 || y < 0 || x > xl - 1 || y > yl - 1; }

    if (!outside(topLeftX, topLeftY)) {
      values[0] = array[topLeftX, topLeftY].x;
    }
    if (!outside(topLeftX + 1, topLeftY)) {
      values[1] = array[topLeftX + 1, topLeftY].x;
    }
    if (!outside(topLeftX, topLeftY + 1)) {
      values[2] = array[topLeftX, topLeftY + 1].x;
    }
    if (!outside(topLeftX + 1, topLeftY + 1)) {
      values[3] = array[topLeftX + 1, topLeftY + 1].x;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(values[n])) {
        values[n] = 0f;
      }
      if (float.IsInfinity(values[n])) {
        values[n] = 0f;
      }
    }

    float averagedXTop = values[0] * xAmountLeft + values[1] * xAmountRight;
    float averagedXBottom = values[2] * xAmountLeft + values[3] * xAmountRight;

    xcomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    // y component of the vector 4
    values = Vector4.zero;
    if (!outside(topLeftX, topLeftY)) {
      values[0] = array[topLeftX, topLeftY].y;
    }
    if (!outside(topLeftX + 1, topLeftY)) {
      values[1] = array[topLeftX + 1, topLeftY].y;
    }
    if (!outside(topLeftX, topLeftY + 1)) {
      values[2] = array[topLeftX, topLeftY + 1].y;
    }
    if (!outside(topLeftX + 1, topLeftY + 1)) {
      values[3] = array[topLeftX + 1, topLeftY + 1].y;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(values[n])) {
        values[n] = 0f;
      }
      if (float.IsInfinity(values[n])) {
        values[n] = 0f;
      }
    }

    averagedXTop = values[0] * xAmountLeft + values[1] * xAmountRight;
    averagedXBottom = values[2] * xAmountLeft + values[3] * xAmountRight;

    ycomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    // z component of the vector 4
    values = Vector4.zero;
    if (!outside(topLeftX, topLeftY)) {
      values[0] = array[topLeftX, topLeftY].z;
    }
    if (!outside(topLeftX + 1, topLeftY)) {
      values[1] = array[topLeftX + 1, topLeftY].z;
    }
    if (!outside(topLeftX, topLeftY + 1)) {
      values[2] = array[topLeftX, topLeftY + 1].z;
    }
    if (!outside(topLeftX + 1, topLeftY + 1)) {
      values[3] = array[topLeftX + 1, topLeftY + 1].z;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(values[n])) {
        values[n] = 0f;
      }
      if (float.IsInfinity(values[n])) {
        values[n] = 0f;
      }
    }

    averagedXTop = values[0] * xAmountLeft + values[1] * xAmountRight;
    averagedXBottom = values[2] * xAmountLeft + values[3] * xAmountRight;

    zcomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    // w component of the vector 4
    values = Vector4.zero;
    if (!outside(topLeftX, topLeftY)) {
      values[0] = array[topLeftX, topLeftY].w;
    }
    if (!outside(topLeftX + 1, topLeftY)) {
      values[1] = array[topLeftX + 1, topLeftY].w;
    }
    if (!outside(topLeftX, topLeftY + 1)) {
      values[2] = array[topLeftX, topLeftY + 1].w;
    }
    if (!outside(topLeftX + 1, topLeftY + 1)) {
      values[3] = array[topLeftX + 1, topLeftY + 1].w;
    }
    for (int n = 0; n < 4; n++) {
      if (float.IsNaN(values[n])) {
        values[n] = 0f;
      }
      if (float.IsInfinity(values[n])) {
        values[n] = 0f;
      }
    }

    averagedXTop = values[0] * xAmountLeft + values[1] * xAmountRight;
    averagedXBottom = values[2] * xAmountLeft + values[3] * xAmountRight;

    wcomp = averagedXTop * yAmountTop + averagedXBottom * yAmountBottom;

    return new Vector4(xcomp, ycomp, zcomp, wcomp);
  }


  /// <summary>
  /// "Splat" a value onto a 2x2 matrix at a point (x,y) where
  /// (0,0) < (x,y) < (1,1). Fractionally breaks the single value
  /// onto each of the 4 grid points based on the (x,y) location
  /// </summary>
  /// <param name="x">x</param>
  /// <param name="y">y</param>
  /// <param name="scalar">scale the result by this parameter, default = 1</param>
  public static float[,] Linear1stOrderSplat(float x, float y, float scalar = 1)
  {
    // roll the range up to (0 < x < 1) to obtain the delta
    float dx = modulus(x, 1);
    float dy = modulus(y, 1);

    // return matrix of form:
    //   mat[0, 0] = Math.Min(1 - dx, 1 - dy) * scalar;
    //   mat[0, 1] = Math.Min(1 - dx, dy) * scalar;
    //   mat[1, 0] = Math.Min(dx, 1 - dy) * scalar;
    //   mat[1, 1] = Math.Min(dx, dy) * scalar;
    return new float[,] {
        { Math.Min(1 - dx, 1 - dy) * scalar,  Math.Min(1 - dx, dy) * scalar },
        { Math.Min(dx, 1 - dy) * scalar,      Math.Min(dx, dy) * scalar }
    };
  }

  /// <summary>
  /// This variant of the bilinear interpolation will preserve ones values.
  /// Ie. a 1x1 grid of value 1, when interpolated onto a [2x2] grid can appear
  /// as [.25, .25 ; .25, .25]. This method prohibits that and preserves ones.
  /// </summary>
  public static float[,] BilinearInterpolationOnesPreserving(this float[,] grid, Vector2 offset)
  {
    var x = offset.x;
    var y = offset.y;

    // precompute some quantities
    // (these quantities are inverse to the actual equations because we are translating
    // from a transformed coordinate plane Back Into the original)
    var dx = modulus(x, 1);
    var dy = modulus(y, 1);

    var dx1 = dx;
    var dx2 = 1f - dx;

    var dy1 = dy;
    var dy2 = 1f - dy;

    // early exit if the grid does not need to be interpolated
    if (dx1 == 0 && dy1 == 0) { return grid; }

    // store reference to original grid dimensions [n, m]
    int n = grid.GetLength(0);
    int m = grid.GetLength(1);

    // helper function to determine if the given indeces are outside our original matrix
    bool outside(double x, double y) { return x < 0 || y < 0 || x > n - 1 || y > m - 1; }

    // declare interpolated grid dimensions [N, M]
    int N = n + (dx1 != 0 ? 1 : 0);
    int M = m + (dy1 != 0 ? 1 : 0);
    var interp = new float[N, M];

    // iterate the new larger grid, interpolating at each point
    // our iteration variable would "x/y-ceiling", the upper bound
    for (int xc = 0; xc < N; xc++) {
      for (int yc = 0; yc < M; yc++) {
        // "x/y-floor" is the lower bound for each dimension, from xc/yc
        var xf = xc - 1;
        var yf = yc - 1;

        // if any components are outside the grid, assign a 0 to that entry
        var Q11 = outside(xf, yf) ? 0 : grid[xf, yf];
        var Q12 = outside(xf, yc) ? 0 : grid[xf, yc];
        var Q21 = outside(xc, yf) ? 0 : grid[xc, yf];
        var Q22 = outside(xc, yc) ? 0 : grid[xc, yc];

        // define primed constants
        var dx1p = dx1;
        var dx2p = dx2;

        var dy1p = dy1;
        var dy2p = dy2;

        // perform one's preseving interpolation
        // *********************************************************************
        // first check point Q11
        if (Q11 == 1 && Q21 != 1 && Q12 != 1 && Q22 != 1) {
          if (dx1 > 0.5f && dy1 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }

          dx1p = dx1 < 0.5f ? 2 * dx1 : 1;
          dx2p = dx1 < 0.5f ? -2 * dx1 + 1 : 0;

          dy1p = dy1 < 0.5f ? 2 * dy1 : 1;
          dy2p = dy1 < 0.5f ? -2 * dy1 + 1 : 0;
        }
        // check point Q21
        else if (Q11 != 1 && Q21 == 1 && Q12 != 1 && Q22 != 1) {
          if (dx2 > 0.5f && dy1 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }

          dx1p = dx2 < 0.5f ? -2 * dx2 + 1 : 0;
          dx2p = dx2 < 0.5f ? 2 * dx2 : 1;

          dy1p = dy1 < 0.5f ? 2 * dy1 : 1;
          dy2p = dy1 < 0.5f ? -2 * dy1 + 1 : 0;
        }
        // check point Q12
        else if (Q11 != 1 && Q21 != 1 && Q12 == 1 && Q22 != 1) {
          if (dx1 > 0.5f && dy2 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }

          dx1p = dx1 < 0.5f ? 2 * dx1 : 1;
          dx2p = dx1 < 0.5f ? -2 * dx1 + 1 : 0;

          dy1p = dy2 < 0.5f ? -2 * dy2 + 1 : 0;
          dy2p = dy2 < 0.5f ? 2 * dy2 : 1;
        }
        // check point Q22
        else if (Q11 != 1 && Q21 != 1 && Q12 != 1 && Q22 == 1) {
          if (dx2 > 0.5f && dy2 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }

          dx1p = dx2 < 0.5f ? -2 * dx2 + 1 : 0;
          dx2p = dx2 < 0.5f ? 2 * dx2 : 1;

          dy1p = dy2 < 0.5f ? -2 * dy2 + 1 : 0;
          dy2p = dy2 < 0.5f ? 2 * dy2 : 1;
        }
        // *********************************************************************
        // Q11 && Q12
        else if (Q11 == 1 && Q21 != 1 && Q12 == 1 && Q22 != 1) {
          if (dx1 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }
          // dy doesn't change, interps at same rate
          dx1p = 2 * dx1;
          dx2p = -2 * dx1 + 1;
        }
        // Q11 && Q21
        else if (Q11 == 1 && Q21 == 1 && Q12 != 1 && Q22 != 1) {
          if (dy1 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }
          // dx doesn't change, interps at same rate
          dy1p = 2 * dy1;
          dy2p = -2 * dy1 + 1;
        }
        // Q12 && Q22
        else if (Q11 != 1 && Q21 != 1 && Q12 == 1 && Q22 == 1) {
          if (dy2 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }
          // dx doesn't change, interps at same rate
          dy1p = -2 * dy2 + 1;
          dy2p = 2 * dy2;
        }
        // Q21 && Q22
        else if (Q11 != 1 && Q21 == 1 && Q12 != 1 && Q22 == 1) {
          if (dx2 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }
          // dy doesn't change, interps at same rate
          dx1p = -2 * dx2 + 1;
          dx2p = 2 * dx2;
        }
        // *********************************************************************
        // Q11 - Q12 - Q21
        else if (Q11 == 1 && Q21 == 1 && Q12 == 1 && Q22 != 1) {
          if (dx1 > 0.5f || dy1 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }

          dx1p = 2 * dx1;
          dx2p = -2 * dx1 + 1;

          dy1p = 2 * dy1;
          dy2p = -2 * dy1 + 1;
        }
        // Q11 - Q21 - Q22
        else if (Q11 == 1 && Q21 == 1 && Q12 != 1 && Q22 == 1) {
          if (dx2 > 0.5f || dy1 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }

          dx1p = -2 * dx2 + 1;
          dx2p = 2 * dx2;

          dy1p = 2 * dy1;
          dy2p = -2 * dy1 + 1;
        }
        // Q11 - Q12 - Q22
        else if (Q11 == 1 && Q21 != 1 && Q12 == 1 && Q22 == 1) {
          if (dx1 > 0.5f || dy2 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }

          dx1p = 2 * dx1;
          dx2p = -2 * dx1 + 1;

          dy1p = -2 * dy2 + 1;
          dy2p = 2 * dy2;
        }
        // Q12 - Q21 - Q22
        else if (Q11 != 1 && Q21 == 1 && Q12 == 1 && Q22 == 1) {
          if (dx2 > 0.5f || dy2 > 0.5f) {
            interp[xc, yc] = 1;
            continue;
          }

          dx1p = -2 * dx2 + 1;
          dx2p = 2 * dx2;

          dy1p = -2 * dy2 + 1;
          dy2p = 2 * dy2;
        }
        // *********************************************************************
        else if (Q11 == 1 && Q21 == 1 && Q12 == 1 && Q22 == 1) {
          interp[xc, yc] = 1;
          continue;
        }

        // compute the interpolated value
        interp[xc, yc] = dy1p * (dx1p * Q11 + dx2p * Q21) + dy2p * (dx1p * Q12 + dx2p * Q22);
      }
    }
    return interp;
  }

  /// </summary>
  public static float[,] BilinearInterpolationNearestHighestValuePreserving(this float[,] grid, Vector2 offset)
  {
    var x = offset.x;
    var y = offset.y;

    // precompute some quantities
    // (these quantities are inverse to the actual equations because we are translating
    // from a transformed coordinate plane Back Into the original)
    var dx = modulus(x, 1);
    var dy = modulus(y, 1);

    var dx1 = dx;
    var dx2 = 1f - dx;

    var dy1 = dy;
    var dy2 = 1f - dy;

    // early exit if the grid does not need to be interpolated
    if (dx1 == 0 && dy1 == 0) { return grid; }

    // store reference to original grid dimensions [n, m]
    int n = grid.GetLength(0);
    int m = grid.GetLength(1);

    // helper function to determine if the given indeces are outside our original matrix
    bool outside(double x, double y) { return x < 0 || y < 0 || x > n - 1 || y > m - 1; }

    // declare interpolated grid dimensions [N, M]
    int N = n + (dx1 != 0 ? 1 : 0);
    int M = m + (dy1 != 0 ? 1 : 0);
    var interp = new float[N, M];

    // iterate the new larger grid, interpolating at each point
    // our iteration variable would "x/y-ceiling", the upper bound
    for (int xc = 0; xc < N; xc++) {
      for (int yc = 0; yc < M; yc++) {
        // "x/y-floor" is the lower bound for each dimension, from xc/yc
        var xf = xc - 1;
        var yf = yc - 1;

        // if any components are outside the grid, assign a 0 to that entry
        var Q11 = outside(xf, yf) ? 0 : grid[xf, yf];
        var Q12 = outside(xf, yc) ? 0 : grid[xf, yc];
        var Q21 = outside(xc, yf) ? 0 : grid[xc, yf];
        var Q22 = outside(xc, yc) ? 0 : grid[xc, yc];

        // define primed constants
        var dx1p = dx1;
        var dx2p = dx2;

        var dy1p = dy1;
        var dy2p = dy2;

        // perform one's preseving interpolation
        // *********************************************************************
        // first check point Q11
        if (Q11 > Q21 && Q11 > Q12 && Q11 > Q22) {
          if (dx1 > 0.5f && dy1 > 0.5f) {
            interp[xc, yc] = Q11;
            continue;
          }

          dx1p = dx1 < 0.5f ? 2 * dx1 : 1;
          dx2p = dx1 < 0.5f ? -2 * dx1 + 1 : 0;

          dy1p = dy1 < 0.5f ? 2 * dy1 : 1;
          dy2p = dy1 < 0.5f ? -2 * dy1 + 1 : 0;
        }
        // check point Q21
        else if (Q21 > Q11 && Q21 > Q12 && Q21 > Q22) {
          if (dx2 > 0.5f && dy1 > 0.5f) {
            interp[xc, yc] = Q21;
            continue;
          }

          dx1p = dx2 < 0.5f ? -2 * dx2 + 1 : 0;
          dx2p = dx2 < 0.5f ? 2 * dx2 : 1;

          dy1p = dy1 < 0.5f ? 2 * dy1 : 1;
          dy2p = dy1 < 0.5f ? -2 * dy1 + 1 : 0;
        }
        // check point Q12
        else if (Q12 > Q11 && Q12 > Q21 && Q12 > Q22) {
          if (dx1 > 0.5f && dy2 > 0.5f) {
            interp[xc, yc] = Q12;
            continue;
          }

          dx1p = dx1 < 0.5f ? 2 * dx1 : 1;
          dx2p = dx1 < 0.5f ? -2 * dx1 + 1 : 0;

          dy1p = dy2 < 0.5f ? -2 * dy2 + 1 : 0;
          dy2p = dy2 < 0.5f ? 2 * dy2 : 1;
        }
        // check point Q22
        else if (Q22 > Q11 && Q22 > Q21 && Q22 > Q12) {
          if (dx2 > 0.5f && dy2 > 0.5f) {
            interp[xc, yc] = Q22;
            continue;
          }

          dx1p = dx2 < 0.5f ? -2 * dx2 + 1 : 0;
          dx2p = dx2 < 0.5f ? 2 * dx2 : 1;

          dy1p = dy2 < 0.5f ? -2 * dy2 + 1 : 0;
          dy2p = dy2 < 0.5f ? 2 * dy2 : 1;
        }
        // *********************************************************************
        // Q11 && Q12
        else if (Q11 == Q12 && Q11 > Q21 && Q11 > Q22) {
          if (dx1 > 0.5f) {
            interp[xc, yc] = Q11;
            continue;
          }
          // dy doesn't change, interps at same rate
          dx1p = 2 * dx1;
          dx2p = -2 * dx1 + 1;
        }
        // Q11 && Q21
        else if (Q11 == Q21 && Q11 > Q12 && Q11 > Q22) {
          if (dy1 > 0.5f) {
            interp[xc, yc] = Q11;
            continue;
          }
          // dx doesn't change, interps at same rate
          dy1p = 2 * dy1;
          dy2p = -2 * dy1 + 1;
        }
        // Q12 && Q22
        else if (Q12 == Q22 && Q22 > Q21 && Q22 > Q11) {
          if (dy2 > 0.5f) {
            interp[xc, yc] = Q22;
            continue;
          }
          // dx doesn't change, interps at same rate
          dy1p = -2 * dy2 + 1;
          dy2p = 2 * dy2;
        }
        // Q21 && Q22
        else if (Q21 == Q22 && Q22 > Q12 && Q22 > Q11) {
          if (dx2 > 0.5f) {
            interp[xc, yc] = Q22;
            continue;
          }
          // dy doesn't change, interps at same rate
          dx1p = -2 * dx2 + 1;
          dx2p = 2 * dx2;
        }
        // *********************************************************************
        // Q11 - Q12 - Q21
        else if (Q11 == Q12 && Q11 == Q21 && Q11 > Q22) {
          if (dx1 > 0.5f || dy1 > 0.5f) {
            interp[xc, yc] = Q11;
            continue;
          }

          dx1p = 2 * dx1;
          dx2p = -2 * dx1 + 1;

          dy1p = 2 * dy1;
          dy2p = -2 * dy1 + 1;
        }
        // Q11 - Q21 - Q22
        else if (Q11 == Q21 && Q11 == Q22 && Q11 > Q12) {
          if (dx2 > 0.5f || dy1 > 0.5f) {
            interp[xc, yc] = Q11;
            continue;
          }

          dx1p = -2 * dx2 + 1;
          dx2p = 2 * dx2;

          dy1p = 2 * dy1;
          dy2p = -2 * dy1 + 1;
        }
        // Q11 - Q12 - Q22
        else if (Q11 == Q12 && Q11 == Q22 && Q11 > Q21) {
          if (dx1 > 0.5f || dy2 > 0.5f) {
            interp[xc, yc] = Q11;
            continue;
          }

          dx1p = 2 * dx1;
          dx2p = -2 * dx1 + 1;

          dy1p = -2 * dy2 + 1;
          dy2p = 2 * dy2;
        }
        // Q12 - Q21 - Q22
        else if (Q22 == Q21 && Q22 == Q12 && Q22 > Q11) {
          if (dx2 > 0.5f || dy2 > 0.5f) {
            interp[xc, yc] = Q22;
            continue;
          }

          dx1p = -2 * dx2 + 1;
          dx2p = 2 * dx2;

          dy1p = -2 * dy2 + 1;
          dy2p = 2 * dy2;
        }
        // *********************************************************************
        else if (Q11 == Q12 && Q11 == Q21 && Q11 == Q22) {
          interp[xc, yc] = Q11;
          continue;
        }

        // compute the interpolated value
        interp[xc, yc] = dy1p * (dx1p * Q11 + dx2p * Q21) + dy2p * (dx1p * Q12 + dx2p * Q22);
      }
    }
    return interp;
  }

  /// <summary>
  /// Uses bilinear interpolation to shift a grid of points by [offset],
  /// where [offset] is wrapped via modulus to the range
  ///   0 <  [offset] < gridSize
  /// </summary>
  /// <param name="grid"></param>
  /// <param name="offset"></param>
  public static float[,] BilinearInterpolation(this float[,] grid, Vector2 offset)
  {
    var x = offset.x;
    var y = offset.y;

    // precompute some quantities
    // (these quantities are inverse to the actual equations because we are translating
    // from a transformed coordinate plane Back Into the original)
    var dx = modulus(x, 1);
    var dy = modulus(y, 1);

    var dx1 = dx;
    var dx2 = 1f - dx;

    var dy1 = dy;
    var dy2 = 1f - dy;

    // early exit if the grid does not need to be interpolated
    if (dx1 == 0 && dy1 == 0) { return grid; }

    // store reference to original grid dimensions [n, m]
    int n = grid.GetLength(0);
    int m = grid.GetLength(1);

    // helper function to determine if the given indeces are outside our original matrix
    bool outside(double x, double y) { return x < 0 || y < 0 || x > n - 1 || y > m - 1; }

    // declare interpolated grid dimensions [N, M]
    int N = n + (dx1 != 0 ? 1 : 0);
    int M = m + (dy1 != 0 ? 1 : 0);
    var interp = new float[N, M];
    // iterate the new larger grid, interpolating at each point
    // our iteration variable would "x/y-ceiling", the upper bound
    for (int xc = 0; xc < N; xc++) {
      for (int yc = 0; yc < M; yc++) {
        // "x/y-floor" is the lower bound for each dimension, from xc/yc
        var xf = xc - 1;
        var yf = yc - 1;
        // if any components are outside the grid, assign a 0 to that entry
        var Q11 = outside(xf, yf) ? 0 : grid[xf, yf];
        var Q12 = outside(xf, yc) ? 0 : grid[xf, yc];
        var Q21 = outside(xc, yf) ? 0 : grid[xc, yf];
        var Q22 = outside(xc, yc) ? 0 : grid[xc, yc];
        // compute the interpolated value
        interp[xc, yc] = dy1 * (dx1 * Q11 + dx2 * Q21) + dy2 * (dx1 * Q12 + dx2 * Q22);
      }
    }
    return interp;
  }

  public static float Modulus(this float x, float m)
  {
    return (x % m + m) % m;
  }

  public static double Modulus(this double x, double m)
  {
    return (x % m + m) % m;
  }

  private static float modulus(float x, float m)
  {
    return (x % m + m) % m;
  }
  private static double modulus(double x, double m)
  {
    return (x % m + m) % m;
  }
}