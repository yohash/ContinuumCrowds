using UnityEngine;
using System;

public static class Matrix
{
  private static double radiansToDegrees = 180 / Math.PI;
  private static double degressToRadians = Math.PI / 180;

  public static float[,] SubMatrix(this float[,] matrix, int startX, int startY, int sizeX, int sizeY)
  {
    var sub = new float[sizeX, sizeY];

    for (int x = 0; x < sizeX; x++) {
      for (int y = 0; y < sizeY; y++) {
        sub[x, y] = matrix[startX + x, startY + y];
      }
    }

    return sub;
  }

  public static Vector2[,] SubMatrix(this Vector2[,] matrix, int startX, int startY, int sizeX, int sizeY)
  {
    var sub = new Vector2[sizeX, sizeY];

    for (int x = 0; x < sizeX; x++) {
      for (int y = 0; y < sizeY; y++) {
        sub[x, y] = matrix[startX + x, startY + y];
      }
    }

    return sub;
  }

  public static Vector2[,] Normalize(this Vector2[,] matrix, float normValue)
  {
    var normalized = new Vector2[matrix.GetLength(0), matrix.GetLength(1)];
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        normalized[i, k] = matrix[i, k] / normValue;
      }
    }

    return normalized;
  }

  public static Vector2[,] Normalize(this Vector2[,] matrix)
  {
    float max = 0;
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        if (matrix[i, k].sqrMagnitude > max) { max = matrix[i, k].sqrMagnitude; }
      }
    }

    if (max == 0) { return matrix; }
    max = Mathf.Sqrt(max);

    return matrix.Normalize(max);
  }

  /// <summary>
  /// Normalize a matrix of floats to the largest absolute value
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
  public static float[,] Normalize(this float[,] matrix)
  {
    float max = 0f;
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        if (Mathf.Abs(matrix[i, k]) > max) { max = Mathf.Abs(matrix[i, k]); }
      }
    }

    return matrix.Normalize(max);
  }

  /// <summary>
  /// Normzlize a matrix of floats to the input value
  /// </summary>
  /// <param name="matrix"></param>
  /// <param name="normValue"></param>
  /// <returns></returns>
  public static float[,] Normalize(this float[,] matrix, float normValue)
  {
    var normalized = new float[matrix.GetLength(0), matrix.GetLength(1)];
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        // clamp the normalized matrix value to the input norm value
        normalized[i, k] = Mathf.Clamp(Mathf.Abs(matrix[i, k]) / normValue, -normValue, normValue);
      }
    }

    return normalized;
  }

  /// <summary>
  /// Compute the matrix of absolute maximum values from a vector matrix
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
  public static float[,] AbsoluteValue(this Vector2[,] matrix)
  {
    var abs = new float[matrix.GetLength(0), matrix.GetLength(1)];
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        abs[i, k] = Mathf.Max(Mathf.Abs(matrix[i, k].x), Mathf.Abs(matrix[i, k].y));
      }
    }
    return abs;
  }

  /// <summary>
  /// Perform a center-point gradient
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
  public static Vector2[,] Gradient(this float[,] matrix)
  {
    int id = matrix.GetLength(0);
    int kd = matrix.GetLength(1);

    var gradient = new Vector2[id, kd];

    void computeGradient(int x, int y, int xMin, int xMax, int yMin, int yMax)
    {
      var xGrad = (matrix[xMax, y] - matrix[xMin, y]) / (xMax - xMin);
      var yGrad = (matrix[x, yMax] - matrix[x, yMin]) / (yMax - yMin);
      gradient[x, y] = new Vector2(xGrad, yGrad);
    }

    for (int i = 0; i < id; i++) {
      for (int k = 0; k < kd; k++) {
        if ((i != 0) && (i != id - 1) && (k != 0) && (k != kd - 1)) {
          // generic spot
          computeGradient(i, k, i - 1, i + 1, k - 1, k + 1);
        } else if ((i == 0) && (k == kd - 1)) {
          // upper left corner
          computeGradient(i, k, i, i + 1, k - 1, k);
        } else if ((i == id - 1) && (k == 0)) {
          // bottom left corner
          computeGradient(i, k, i - 1, i, k, k + 1);
        } else if ((i == 0) && (k == 0)) {
          // upper left corner
          computeGradient(i, k, i, i + 1, k, k + 1);
        } else if ((i == id - 1) && (k == kd - 1)) {
          // bottom right corner
          computeGradient(i, k, i - 1, i, k - 1, k);
        } else if (i == 0) {
          // top edge
          computeGradient(i, k, i, i + 1, k - 1, k + 1);
        } else if (i == id - 1) {
          // bot edge
          computeGradient(i, k, i - 1, i, k - 1, k + 1);
        } else if (k == 0) {
          // left edge
          computeGradient(i, k, i - 1, i + 1, k, k + 1);
        } else if (k == kd - 1) {
          // right edge
          computeGradient(i, k, i - 1, i + 1, k - 1, k);
        }
      }
    }
    return gradient;
  }

  /// <summary>
  /// Strip the x-dimention from a matrix of vectors
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
  public static float[,] x(this Vector2[,] matrix)
  {
    var x = new float[matrix.GetLength(0), matrix.GetLength(1)];
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        x[i, k] = matrix[i, k].x;
      }
    }
    return x;
  }

  /// <summary>
  /// Strip the y-dimension from a matrix of vectors
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
  public static float[,] y(this Vector2[,] matrix)
  {
    var y = new float[matrix.GetLength(0), matrix.GetLength(1)];
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        y[i, k] = matrix[i, k].y;
      }
    }
    return y;
  }

  public static string ToString<T>(this T[,] matrix)
  {
    string s = "";
    for (int i = 0; i < matrix.GetLength(0); i++) {
      for (int k = 0; k < matrix.GetLength(1); k++) {
        s += matrix[i, k].ToString() + ",\t";
      }
      s += "\n";
    }
    return s;
  }

  /// <summary>
  /// Rotates a matrix counter-clockwise by given degrees.
  ///
  /// Performs rotation via "inverse transformation". The rotated
  /// matrix (u, v) is mapped into the space (frame of reference)
  /// of the source matrix (x, y), and each point is determined
  /// by bilinear interpolation of the 4 nearest-neighbor points in
  /// the source matrix.
  ///
  /// Returns a new matrix whose dimensions are be greater than or
  /// equal to the dimensions of the source matrix. Requires
  /// additional allocations
  ///
  /// TODO: make a non-alloc variant
  /// </summary>
  /// <param name="matrix"></param>
  /// <param name="degrees"></param>
  public static float[,] Rotate(this float[,] matrix, float degrees)
  {
    int precision = 3;

    var radians = degrees * degressToRadians;
    // store constants
    var n = matrix.GetLength(0);
    var m = matrix.GetLength(1);

    // determine the components that sum the side dimensions of our new matrix
    var na = Math.Round(Math.Abs(n * Math.Cos(radians)), precision);
    var nb = Math.Round(Math.Abs(m * Math.Sin(radians)), precision);
    var ma = Math.Round(Math.Abs(m * Math.Cos(radians)), precision);
    var mb = Math.Round(Math.Abs(n * Math.Sin(radians)), precision);

    // compute new dimensions of the matrix required to hold the rotated matrix
    var N = Math.Ceiling(na + nb);
    var M = Math.Ceiling(ma + mb);

    // build a helper function to transform a matrix point
    float invTransform(int u, int v)
    {
      // (1) translate to midpoint
      var ut = u - (N - 1) / 2f;
      var vt = v - (M - 1) / 2f;
      // (2) apply rotation matrix to rotate about (0, 0)
      var ur = ut * Math.Cos(radians) + vt * Math.Sin(radians);
      var vr = -ut * Math.Sin(radians) + vt * Math.Cos(radians);
      // (3) re-scale to original frame of reference
      var x = Math.Round(ur + (n - 1) / 2f, precision);
      var y = Math.Round(vr + (m - 1) / 2f, precision);
      // (4) interpolate the rotated value from our original matrix
      return (float)matrix.Interpolate((float)x, (float)y);
    }

    // declare new rotated matrix with previously determined dimensions
    var rotated = new float[(int)N, (int)M];

    // build the rotated matrix
    for (int u = 0; u < N; u++) {
      for (int v = 0; v < M; v++) {
        rotated[u, v] = invTransform(u, v);
      }
    }

    return rotated;
  }
}
