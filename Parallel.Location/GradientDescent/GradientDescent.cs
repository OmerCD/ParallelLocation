using System;
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

        public void SetAnchor(int anchorId, IAnchor value)
        {
            if (_anchorsDic.ContainsKey(anchorId))
            {
                _anchorsDic[anchorId] = value;
            }
        }

        public IEnumerable<IAnchor> CurrentAnchors => _anchorsDic.Values;

        public ICoordinate GetResult(int objectId, params IDistance[] distances)
        {
            (Vector<double> ranges, double[,] anchors) = CreateRangeArray(distances);
            MLAT.GradientDescentResult gradientResult = MLAT.Mlat(DenseMatrix.OfArray(anchors), ranges, null);
            Vector<double> estimate = gradientResult.Estimator;
            return new Coordinate(estimate[0], estimate[1], estimate[2]);
        }

        private (Vector<double> ranges, double[,] anchors) CreateRangeArray(IReadOnlyList<IDistance> distances)
        {
            var anchorMatrix = new double[distances.Count, 3];
            Vector<double> ranges = Vector<double>.Build.Dense(distances.Count);
            for (var i = 0; i < distances.Count; i++)
            {
                if (!_anchorsDic.ContainsKey(distances[i].FromAnchorId))
                {
                    continue;
                }
                IAnchor currentAnchor = _anchorsDic[distances[i].FromAnchorId];
                anchorMatrix[i, 0] = currentAnchor.X;
                anchorMatrix[i, 1] = currentAnchor.Z;
                anchorMatrix[i, 2] = currentAnchor.Y;
                ranges[i] = distances[i].Distance;
            }

            return (ranges, anchorMatrix);
        }
    }
}