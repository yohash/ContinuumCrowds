using UnityEngine;
using System;
using System.Collections.Generic;
using Yohash.PriorityQueue;
using Yohash.Tools;

namespace Yohash.ContinuumCrowds
{
  /// ******************************************************************************************
  /// 							THE EIKONAL SOLVER
  /// ******************************************************************************************
  // 3 labels for each node (Location): far, considered, accepted
  //	labels are tracked by:
  //              far - has huge value (Mathf.Infinite)
  //							considered - placed in a priorityQueue
  //							accepted - stored in List<Location>
  //
  /// The Algorithm:
  //  1) set all nodes (xi=Ui=inf) to far, set nodes in goal (xi=Ui=0) to accepted
  //  2) for each accepted node, use eikonal update formula to find new U', and
  //		if U'<Ui, then Ui=U', and xi -> considered
  // 	[proposed change to 2)]
  //	2) instead of marking each node accepted, mark them considered, and begin the loop
  //		They will naturally become accepted, as their value of 0 gives them highest priority.
  //
  ///	The Loop:
  //  -->	3) let xt be the considered node with smallest Ui
  //	|  	4) for each neighbor (xi) of xt that is NOT accepted, calculate U'
  //  |  	5) if U'<Ui, then Ui=U' and label xi as considered
  //  ---	6) if there is a considered node, repeat from step 3

  public class EikonalSolver
  {
    /// Continuum Crowd fields (we're solving for these)
    // potential field
    private float[,] phi;
    // potential field gradient
    private Vector2[,] dPhi;
    // final velocity
    public Vector2[,] Velocity;

    /// Initiating fields
    // speed field
    private Vector4[,] f;
    // Cost field
    private Vector4[,] C;
    // discomfort field
    private float[,] g;

    // local cached vars
    //private bool[,] accepted;
    //private bool[,] goal;
    private HashSet<FastLocation> goal;
    private HashSet<FastLocation> accepted;
    private FastPriorityQueue<FastLocation> considered;
    // cache an oft-used variable
    private FastLocation neighbor;

    // store the dimensions for easy iteration
    private int N;
    private int M;

    // callback on complete
    private Action<EikonalSolver> onComplete;

    // this array of Vect2's correlates to our data format: Vector4(x, y, z, w) = (+x, +y, -x, -y)
    private Vector2[] DIR_ENWS = new Vector2[] { Vector2.right, Vector2.up, Vector2.left, Vector2.down };

    // *************************************************************************
    //    ACCESSOR
    // *************************************************************************
    public EikonalSolver() { }

    public void Solve(
        Tile tile,
        List<Location> goal,
        Action<EikonalSolver> onComplete
    )
    {
      f = tile.f;
      C = tile.C;
      g = tile.g;

      this.onComplete = onComplete;

      N = f.GetLength(0);
      M = f.GetLength(1);

      phi = new float[N, M];

      dPhi = new Vector2[N, M];
      Velocity = new Vector2[N, M];

      //accepted = new bool[N, M];
      //this.goal = new bool[N, M];
      this.goal = new HashSet<FastLocation>();
      this.accepted = new HashSet<FastLocation>();

      considered = new FastPriorityQueue<FastLocation>(N * M);

      neighbor = new FastLocation(0, 0);

      var offsetGoal = new List<Location>();
      for (int i = 0; i < goal.Count; i++) {
        offsetGoal.Add(goal[i] - tile.Corner);
      }
      computeContinuumCrowdsFields(offsetGoal);
    }

    private void computeContinuumCrowdsFields(List<Location> goal)
    {
      // calculate potential field (Eikonal solver)
      computePotentialField(goal);
      // calculate the gradient
      calculatePotentialGradientAndNormalize();
      // calculate velocity field
      calculateVelocityField();
      // report completion via callback
      onComplete(this);
    }

    private void computePotentialField(List<Location> goal)
    {
      eikonalSolver(goal);
    }

    // *************************************************************************
    //    THE EIKONAL SOLVER
    // *************************************************************************
    /// <summary>
    /// The Eikonal Solver using the Fast Marching approach
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="goal"></param>
    private void eikonalSolver(List<Location> goal)
    {
      // start by assigning all values of potential a huge number to in-effect label them 'far'
      for (int n = 0; n < N; n++) {
        for (int m = 0; m < M; m++) {
          phi[n, m] = Mathf.Infinity;
        }
      }

      // initiate by setting potential to 0 in the goal, and adding all goal locations to the considered list
      // goal locations will naturally become accepted nodes, as their 0-cost gives them 1st priority, and this
      // way, they are quickly added to accepted, while simultaneously propagating their lists and beginning the
      // algorithm loop
      FastLocation loc;
      foreach (Location l in goal) {
        if (isPointValid(l.x, l.y)) {
          phi[l.x, l.y] = 0f;
        }

        loc = new FastLocation(l.x, l.y);
        markGoal(loc);
        considered.Enqueue(loc, 0f);
      }

      /// THE EIKONAL UPDATE LOOP
      // next, we start the eikonal update loop, by initiating it with each goal point as 'considered'.
      // this will check each neighbor to see if it's a valid point (EikonalLocationValidityTest==true)
      // and if so, update if necessary
      while (considered.Count > 0) {
        FastLocation current = considered.Dequeue();
        EikonalUpdateFormula(current);
        markAccepted(current);
      }
    }

    /// <summary>
    /// The EikonalUpdateFormula computes the actual solution potential
    /// </summary>
    /// <param name="l"></param>
    private void EikonalUpdateFormula(FastLocation l)
    {
      float phi_proposed = Mathf.Infinity;

      // cycle through directions to check all neighbors and perform the eikonal
      // update cycle on them
      for (int d = 0; d < DIR_ENWS.Length; d++) {
        int xInto = l.x + (int)DIR_ENWS[d].x;
        int yInto = l.y + (int)DIR_ENWS[d].y;

        neighbor = new FastLocation(xInto, yInto);

        if (isEikonalLocationValidAsNeighbor(neighbor)) {
          // The point is valid. Now, we pull values from THIS location's
          // 4 neighbors and use them in the calculation
          Vector4 phi_m;

          phi_m = Vector4.one * Mathf.Infinity;

          // track cost of moving into each nearby space
          for (int dd = 0; dd < DIR_ENWS.Length; dd++) {
            int xIInto = neighbor.x + (int)DIR_ENWS[dd].x;
            int yIInto = neighbor.y + (int)DIR_ENWS[dd].y;

            if (isEikonalLocationValidToMoveInto(new FastLocation(xIInto, yIInto))) {
              phi_m[dd] = phi[xIInto, yIInto] + C[neighbor.x, neighbor.y][dd];
            }
            // track the scenario where a goal JUST off-tile is being looked at
            else if (goal.Contains(new FastLocation(xIInto, yIInto))) {
              // goal locations have phi of 0
              phi_m[dd] = 0f + C[neighbor.x, neighbor.y][dd];
            }
          }
          // select out cheapest
          float phi_mx = Math.Min(phi_m[0], phi_m[2]);
          float phi_my = Math.Min(phi_m[1], phi_m[3]);

          // now assign C_mx based on which direction was chosen
          float C_mx = phi_mx == phi_m[0] ? C[neighbor.x, neighbor.y][0] : C[neighbor.x, neighbor.y][2];
          // now assign C_my based on which direction was chosen
          float C_my = phi_my == phi_m[1] ? C[neighbor.x, neighbor.y][1] : C[neighbor.x, neighbor.y][3];

          // solve for our proposed Phi[neighbor] value using the quadratic solution to the
          // approximation of the eikonal equation
          float C_mx_Sq = C_mx * C_mx;
          float C_my_Sq = C_my * C_my;
          float phi_mDiff_Sq = (phi_mx - phi_my) * (phi_mx - phi_my);

          float valTest = C_mx_Sq + C_my_Sq - 1f / (C_mx_Sq * C_my_Sq);
          //float valTest = C_mx_Sq + C_my_Sq - 1f;

          // test the quadratic
          if (phi_mDiff_Sq > valTest) {
            // use the simplified solution for phi_proposed
            float phi_min = Math.Min(phi_mx, phi_my);
            float cost_min = phi_min == phi_mx ? C_mx : C_my;
            phi_proposed = cost_min + phi_min;
          } else {
            // solve the quadratic
            var radical = Math.Sqrt(C_mx_Sq * C_my_Sq * (C_mx_Sq + C_my_Sq - phi_mDiff_Sq));

            var soln1 = (C_my_Sq * phi_mx + C_mx_Sq * phi_my + radical) / (C_mx_Sq + C_my_Sq);
            var soln2 = (C_my_Sq * phi_mx + C_mx_Sq * phi_my - radical) / (C_mx_Sq + C_my_Sq);

            // max - prefers diagonals
            //phi_proposed = (float)Math.Max(soln1, soln2);

            // min - prefers cardinals
            //phi_proposed = (float)Math.Min(soln1, soln2);

            // mean - better mix but still prefer diagonals
            //phi_proposed = (float)(soln1 + soln2) / 2;

            // geometric mean - seems identical to mean
            //phi_proposed = (float)Math.Sqrt(soln1 * soln2);

            // weighted mean - appears to offer the best compromise
            var max = (float)Math.Max(soln1, soln2);
            var min = (float)Math.Min(soln1, soln2);
            float wMax = Constants.Values.maxWeight;
            float wMin = Constants.Values.minWeight;
            phi_proposed = (float)(max * wMax + min * wMin) / (wMax + wMin);
          }

          // we now have a phi_proposed

          // we are re-writing the phi-array real time, so we simply compare to the current slot
          if (phi_proposed < phi[neighbor.x, neighbor.y]) {
            // save the value of the lower phi
            phi[neighbor.x, neighbor.y] = phi_proposed;

            if (considered.Contains(neighbor)) {
              // re-write the old value in the queue
              considered.UpdatePriority(neighbor, phi_proposed);
            } else {
              // -OR- add this value to the queue
              considered.Enqueue(neighbor, phi[neighbor.x, neighbor.y]);
            }
          }
        }
      }
    }

    private bool isEikonalLocationValidAsNeighbor(FastLocation l)
    {
      // A valid neighbor point is:
      //		1) not outisde the local grid
      //		3) NOT in the goal
      //            (everything below this is checked elsewhere)
      //		2) NOT accepted
      //		4) NOT on a global discomfort grid
      //            (this occurs in isPointValid() )
      //		5) NOT outside the global grid
      //            (this occurs in isPointValid() )
      //if (!isEikonalLocationInsideLocalGrid(l)) { return false; }
      if (isLocationInGoal(l)) { return false; }
      return (isEikonalLocationAcceptedandValid(l));
    }

    private bool isEikonalLocationValidToMoveInto(FastLocation l)
    {
      // location must be tested to ensure that it does not attempt to assess a point
      // that is not valid to move into. a valid point is:
      //		1) not outisde the local grid
      //		2) NOT accepted
      //		3) NOT on a global discomfort grid
      //              (this occurs in isPointValid() )
      //		4) NOT outside the global grid
      //              (this occurs in isPointValid() )
      if (!isEikonalLocationInsideLocalGrid(l)) { return false; }
      return (isEikonalLocationAcceptedandValid(l));
    }

    private bool isEikonalLocationInsideLocalGrid(FastLocation l)
    {
      if (l.x < 0 || l.y < 0 || l.x > N - 1 || l.y > M - 1) {
        return false;
      }
      return true;
    }

    private bool isEikonalLocationAcceptedandValid(FastLocation l)
    {
      if (isLocationAccepted(l)) { return false; }
      if (!isPointValid(l)) { return false; }
      return true;
    }

    private void calculatePotentialGradientAndNormalize()
    {
      for (int i = 0; i < N; i++) {
        for (int k = 0; k < M; k++) {
          if (i != 0 && i != N - 1 && k != 0 && k != M - 1) {
            // generic spot
            writeNormalizedPotentialGradientFieldData(i, k, i - 1, i + 1, k - 1, k + 1);
          } else if (i == 0 && k == M - 1) {
            // upper left corner
            writeNormalizedPotentialGradientFieldData(i, k, i, i + 1, k - 1, k);
          } else if (i == N - 1 && k == 0) {
            // bottom left corner
            writeNormalizedPotentialGradientFieldData(i, k, i - 1, i, k, k + 1);
          } else if (i == 0 && k == 0) {
            // upper left corner
            writeNormalizedPotentialGradientFieldData(i, k, i, i + 1, k, k + 1);
          } else if (i == N - 1 && k == M - 1) {
            // bottom right corner
            writeNormalizedPotentialGradientFieldData(i, k, i - 1, i, k - 1, k);
          } else if (i == 0) {
            // top edge
            writeNormalizedPotentialGradientFieldData(i, k, i, i + 1, k - 1, k + 1);
          } else if (i == N - 1) {
            // bot edge
            writeNormalizedPotentialGradientFieldData(i, k, i - 1, i, k - 1, k + 1);
          } else if (k == 0) {
            // left edge
            writeNormalizedPotentialGradientFieldData(i, k, i - 1, i + 1, k, k + 1);
          } else if (k == M - 1) {
            // right edge
            writeNormalizedPotentialGradientFieldData(i, k, i - 1, i + 1, k - 1, k);
          }
        }
      }
    }

    private void writeNormalizedPotentialGradientFieldData(int x, int y, int xMin, int xMax, int yMin, int yMax)
    {
      float phiXmin = phi[xMin, y];
      float phiXmax = phi[xMax, y];
      float phiYmin = phi[x, yMin];
      float phiYmax = phi[x, yMax];

      float dPhidx;
      float dPhidy;

      dPhidx = (phiXmax - phiXmin) / (xMax - xMin);
      dPhidy = (phiYmax - phiYmin) / (yMax - yMin);

      if (float.IsInfinity(phiXmin) && float.IsInfinity(phiXmax)) {
        dPhidx = 0f;
      } else if (float.IsInfinity(phiXmin) || float.IsInfinity(phiXmax)) {
        dPhidx = Mathf.Sign(phiXmax - phiXmin);
      }

      if (float.IsInfinity(phiYmin) && float.IsInfinity(phiYmax)) {
        dPhidy = 0f;
      } else if (float.IsInfinity(phiYmin) || float.IsInfinity(phiYmax)) {
        dPhidy = Mathf.Sign(phiYmax - phiYmin);
      }

      dPhi[x, y] = (new Vector2(dPhidx, dPhidy)).normalized;
    }

    private void calculateVelocityField()
    {
      for (int i = 0; i < N; i++) {
        for (int k = 0; k < M; k++) {
          Velocity[i, k] = new Vector2(
            dPhi[i, k].x > 0
                ? -f[i, k][2] * dPhi[i, k].x
                : -f[i, k][0] * dPhi[i, k].x,
            dPhi[i, k].y > 0
                ? -f[i, k][3] * dPhi[i, k].y
                : -f[i, k][1] * dPhi[i, k].y
          );
        }
      }
    }

    private void markAccepted(FastLocation l)
    {
      //accepted[l.x, l.y] = true;
      accepted.Add(l);
    }

    private bool isLocationAccepted(FastLocation l)
    {
      //return accepted[l.x, l.y];
      return accepted.Contains(l);
    }

    private void markGoal(FastLocation l)
    {
      //goal[l.x, l.y] = true;
      goal.Add(l);
    }

    private bool isLocationInGoal(FastLocation l)
    {
      //return goal[l.x, l.y];
      return goal.Contains(l);
    }

    private bool isPointValid(FastLocation l)
    {
      return isPointValid(l.x, l.y);
    }

    private bool isPointValid(Vector2 v)
    {
      return isPointValid((int)v.x, (int)v.y);
    }

    private bool isPointValid(int x, int y)
    {
      // check to make sure the point is not outside the grid
      if (x < 0 || y < 0 || x > N - 1 || y > M - 1) {
        return false;
      }
      // check to make sure the point is not on a place of absolute discomfort (g >= 1)
      if (g[x, y] >= 1) { return false; }

      return true;
    }
  }
}
