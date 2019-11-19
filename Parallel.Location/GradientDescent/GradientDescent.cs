using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Parallel.Location
{
    public class GradientDescent : ILocationCalculator
    {
        private Dictionary<int, IAnchor> _anchorsDic;

        public int MinAnchorCount => 3;

        public void SetAnchors(IEnumerable<IAnchor> values)
        {
            _anchorsDic = values.ToDictionary(x => x.Id);
        }

        public void SetAnchors(IAnchor[] values)
        {
            _anchorsDic = values.ToDictionary(x => x.Id);
        }

        public ICoordinate GetResult(params IDistance[] distances)
        {
            var (ranges, anchors) = CreateRangeArray(distances);
            var gradientResult = MLAT.Mlat(DenseMatrix.OfArray(anchors), ranges, null);
            var estimate = gradientResult.Estimator;
            return new Coordinate(estimate[0], estimate[1], estimate[2]);
        }

        private (Vector<double> ranges, double[,] anchors) CreateRangeArray(IDistance[] distances)
        {
            var anchorMatrix = new double[distances.Length, 3];
            var ranges = Vector<double>.Build.Dense(distances.Length);
            for (int i = 0; i < distances.Length; i++)
            {
                var currentAnchor = _anchorsDic[distances[i].FromAnchorId];
                anchorMatrix[i, 0] = currentAnchor.X;
                anchorMatrix[i, 1] = currentAnchor.Z;
                anchorMatrix[i, 2] = currentAnchor.Y;
                ranges[i] = distances[i].Distance;
            }

            return (ranges, anchorMatrix);
        }
    }
}