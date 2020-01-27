using System;
using Accord.Extensions.Math.Geometry;

namespace Parallel.Location.Intersection
{
    public class CCIntersection
    {
        private bool CheckIfCirclesIntersect(double cx0, double cy0, double radius0,
            double cx1, double cy1, double radius1)
        {
            double dx = cx0 - cx1;
            double dy = cy0 - cy1;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // See how many solutions there are.
            if (dist > radius0 + radius1)
            {
                return false;
            }
            else if (dist < Math.Abs(radius0 - radius1))
            {
                return false;
            }
            else if ((dist == 0) && (radius0 == radius1))
            {
                return false;
            }

            return true;
        }
        private double FindCircleCircleIntersections(
            double cx0, double cy0, double radius0,
            double cx1, double cy1, double radius1,
            out Point<double> intersection1, out Point<double> intersection2)
        {
            // Find the distance between the centers.
            double dx = cx0 - cx1;
            double dy = cy0 - cy1;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // See how many solutions there are.
            if (dist > radius0 + radius1)
            {
                // No solutions, the circles are too far apart.
                intersection1 = new Point<double>() {X = double.NaN, Y = double.NaN};
                intersection2 = new Point<double>() {X = double.NaN, Y = double.NaN};
                return 0;
            }
            else if (dist < Math.Abs(radius0 - radius1))
            {
                // No solutions, one circle contains the other.
                intersection1 = new Point<double>() {X = double.NaN, Y = double.NaN};
                intersection2 = new Point<double>() {X = double.NaN, Y = double.NaN};
                return 0;
            }
            else if ((dist == 0) && (radius0 == radius1))
            {
                // No solutions, the circles coincide.
                intersection1 = new Point<double>() {X = double.NaN, Y = double.NaN};
                intersection2 = new Point<double>() {X = double.NaN, Y = double.NaN};
                return 0;
            }
            else
            {
                // Find a and h.
                double a = (radius0 * radius0 -
                            radius1 * radius1 + dist * dist) / (2 * dist);
                double h = Math.Sqrt(radius0 * radius0 - a * a);

                // Find P2.
                double cx2 = cx0 + a * (cx1 - cx0) / dist;
                double cy2 = cy0 + a * (cy1 - cy0) / dist;

                // Get the points P3.
                intersection1 = new Point<double>
                {
                    X = (double) (cx2 + h * (cy1 - cy0) / dist),
                    Y = (double) (cy2 - h * (cx1 - cx0) / dist)
                };
                intersection2 = new Point<double>
                    {
                        X = (double) (cx2 - h * (cy1 - cy0) / dist),
                        Y = (double) (cy2 + h * (cx1 - cx0) / dist)
                    };

                // See if we have 1 or 2 solutions.
                if (Math.Abs(dist - (radius0 + radius1)) < 0.00001) return 1;
                return 2;
            }
        }
    }
}