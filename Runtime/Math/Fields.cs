using System;
using System.Collections.Generic;
using UnityEngine;

public static class Fields
{
  /// <summary>
  /// Computes matrix of float values ranging from 0->1. Builds a "footprint"
  /// of (sizeX, sizeY), with all elements inside this footprint given a value of 1.
  /// Then radially drops off the value from 1 to 0, extending
  /// in all directions outside the base "footprint" for a given distance defined
  /// by radius.
  /// </summary>
  /// <param name="sizeX">x dimension of the base footprint</param>
  /// <param name="sizeY">y dimension of the base footprint</param>
  /// <param name="radius">distance in each direction to radially drop off</param>
  /// <returns></returns>
  public static float[,] RectWithRadialFadeout(int sizeX, int sizeY, float radius)
  {
    // initialize 'positions' with the standard grid and dimensions provided
    int buffer = (int)Math.Ceiling(radius);
    int cols = sizeX + buffer * 2;
    int rows = sizeY + buffer * 2;

    // init the footprint
    var footprint = new float[cols, rows];
    // helper functions to clean up footprint construction
    bool xInside(int x) { return x >= buffer && x < buffer + sizeX; }
    bool yInside(int y) { return y >= buffer && y < buffer + sizeY; }

    // construct unit's footprint
    for (int x = 0; x < cols; x++) {
      for (int y = 0; y < rows; y++) {
        // check for the different zones
        // (1) within the main footprint range
        // (2) within the buffer to the left/right or top/bottom of the main footprint
        //      where footprint drops off linearly
        // (3) one of the 4 corners, where footprint drops off radially

        // if we're inside the footprint
        if (xInside(x) && yInside(y)) {
          // within the main footprint range, everything is 1
          footprint[x, y] = 1;
        }
        // if we're outside the x range, but inside y
        else if (!xInside(x) && yInside(y)) {
          // footprint drops off linearly over x
          // get x distance
          float xVar = x < buffer ? x + 1 : cols - x;
          footprint[x, y] = xVar / (buffer + 1);
        }
        // if we're outside the y range, but inside x
        else if (xInside(x) && !yInside(y)) {
          // footprint drops off linearly over y
          float yVar = y < buffer ? y + 1 : rows - y;
          footprint[x, y] = yVar / (buffer + 1);
        }
        // anything else, we're in a corner, drop off radially
        else {
          // determine how far x and y are away from closest corner
          float xVar = x < buffer ? buffer - x : buffer + x + 1 - cols;
          float yVar = y < buffer ? buffer - y : buffer + y + 1 - rows;
          // use distance formula to determine distance from corner
          float dd = (float)Math.Sqrt(xVar * xVar + yVar * yVar);

          // invert the distance by buffer+1
          dd = buffer + 1 - dd;
          if (dd < 0) dd = 0;
          // scale and record
          footprint[x, y] = dd / (buffer + 1);
        }
      }
    }
    return footprint;
  }

  public static float[,] RadialFadeoutFootprint(float sizeX, float sizeY, float radius)
  {
    return RectWithRadialFadeout((int)sizeX, (int)sizeY, radius);
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="footprint"></param>
  /// <param name="fadeFrom"></param>
  /// <param name="distance"></param>
  /// <param name="startScalar"></param>
  /// <param name="endScalar"></param>
  /// <returns></returns>
  public static float[,] LinearFadeout(
      float[,] footprint,
      int fadeFrom,
      int distance,
      float startScalar,
      float endScalar
  )
  {
    var height = footprint.GetLength(1);

    // (1) create a rect with Length = distance, Height = footprint height
    var faded = new float[fadeFrom + distance, height];

    // (2) build half of the footprint into the predictive rect
    for (int i = 0; i < fadeFrom; i++) {
      for (int k = 0; k < height; k++) {
        faded[i, k] = footprint[i, k];
      }
    }

    // (3a) record the "vertical slice" of the footprint center
    var slice = new float[height];
    // the fadeFrom is the line where our fadeout starts, so we sample
    // the "slice" from the index directly before it
    var index = Math.Max(0, fadeFrom - 1);
    for (int i = 0; i < slice.Length; i++) {
      slice[i] = footprint[index, i];
    }

    // (3b) scale the vertical slice along the length of the rect
    // determine falloff rates
    // track iteration
    int c = 0;
    for (int i = fadeFrom; i < faded.GetLength(0); i++) {
      // taper from <start> down to <end>
      var scalar = (endScalar - startScalar) / distance * c + startScalar;
      c++;
      for (int k = 0; k < height; k++) {
        // build the fading out rect in front of the footprint
        faded[i, k] = slice[k] * scalar;
      }
    }

    return faded;
  }
}