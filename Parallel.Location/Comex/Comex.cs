using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math.Optimization;
using Parallel.Location;

namespace Fingerprinting
{
    public class ComexCalculator : ILocationCalculator
    {
        private IDictionary<int, IAnchor> _anchors;

        public int MinAnchorCount => 3;

        public void SetAnchors(IEnumerable<IAnchor> values)
        {
            _anchors = values.ToDictionary(x => x.Id);
        }

        public void SetAnchors(IAnchor[] values)
        {
            SetAnchors(values.AsEnumerable());
        }

        public void SetAnchor(int anchorId, IAnchor value)
        {
            if (_anchors.ContainsKey(anchorId))
            {
                _anchors[anchorId] = value;
            }
        }

        public IEnumerable<IAnchor> CurrentAnchors => _anchors.Values;

        public ICoordinate GetResult(int id, params IDistance[] distances)
        {
            var bestThree = distances.OrderBy(x => x.Distance).Take(3).ToArray();
            var beacons = new BeaconBase[3];
            for (var i = 0; i < bestThree.Length; i++)
            {
                var anchor = _anchors[bestThree[i].FromAnchorId];
                beacons[i] = new BeaconBase
                {
                    Id = anchor.Id,
                    X = anchor.X,
                    Z = anchor.Z,
                    ReadDistance = bestThree[i].Distance
                };
            }

            var comex = new Comex(beacons[0], beacons[1], beacons[2]);
            var result = comex.Calculate();
            return new Coordinate(result[0], result[1], 0);
        }
    }

    public class Comex
    {
        private IBeacon _beacon1;
        private IBeacon _beacon2;
        private IBeacon _beacon3;

        private double[,] _matrixA;

        public double[,] AMatrix => _matrixA;

        public Comex(IBeacon beacon1, IBeacon beacon2, IBeacon beacon3)
        {
            _beacon1 = beacon1;
            _beacon2 = beacon2;
            _beacon3 = beacon3;

            _beacon1.Id = 1;
            _beacon2.Id = 2;
            _beacon3.Id = 3;

            AssignDistances(_beacon1, new[] {_beacon2, beacon3});
            AssignDistances(_beacon2, new[] {_beacon1, beacon3});
            AssignDistances(_beacon3, new[] {_beacon2, beacon1});
            _matrixA = new double[3, 3];
            _matrixA[0, 0] = _beacon2.To(3) * 2;
            _matrixA[0, 1] = _beacon1.To(2) - _beacon1.To(3) - _beacon2.To(3);
            _matrixA[0, 2] = _beacon1.To(3) - _beacon2.To(3) - _beacon1.To(2);
            _matrixA[1, 0] = _matrixA[0, 1];
            _matrixA[1, 1] = _beacon1.To(3) * 2;
            _matrixA[1, 2] = _beacon2.To(3) - _beacon1.To(2) - _beacon1.To(3);
            _matrixA[2, 0] = _matrixA[0, 2];
            _matrixA[2, 1] = _matrixA[1, 2];
            _matrixA[2, 2] = _beacon1.To(2) * 2;
        }

        public double[] Calculate()
        {
            var values = CalculateBs();
            var errors = CalculateErrors(values);
            var estimatedDistances = new EstimatedDistances(new[] {_beacon1, _beacon2, _beacon3}, errors);
            var res = Trilateration.Trilaterate2DLinear(
                new[] {_beacon1.X, _beacon1.Z},
                new[] {_beacon2.X, _beacon2.Z},
                new[] {_beacon3.X, _beacon3.Z},
                estimatedDistances.D1,
                estimatedDistances.D2,
                estimatedDistances.D3);
            return res;
        }

        private Error CalculateErrors(Values values)
        {
            double e1 = 0, e2 = 0, e3 = 0, l = 0;
            double[] bs = new double[]
            {
                values.B1,
                values.B2,
                values.B3,
            };
            double[,] best =
            {
                {values.B1, values.B2},
                {values.B2, values.C},
            };
            var quadraticOF = new QuadraticObjectiveFunction("x² + y² + z²");
            var constraints = new IConstraint[]
            {
                new QuadraticConstraint(quadraticOF, AMatrix, bs, ConstraintType.EqualTo, -values.C)
            };
            var lagrangian = new AugmentedLagrangian(quadraticOF, constraints);
            lagrangian.Minimize();
            var solution = lagrangian.Solution;
            return new Error()
            {
                E1 = solution[0],
                E2 = solution[1],
                E3 = solution[2]
            };
        }

        private class Values
        {
            public double B1 { get; set; }
            public double B2 { get; set; }
            public double B3 { get; set; }
            public double C { get; set; }
        }

        private Values CalculateBs()
        {
            var values = new Values();
            double twoToThree = _beacon2.To(3);
            double beacon1Rdr = _beacon1.RDR;
            double oneToTwo = _beacon1.To(2);
            double oneToThree = _beacon1.To(3);
            double beacon2Rdr = _beacon2.RDR;
            double beacon3Rdr = _beacon3.RDR;
            double twoTwoThree = (oneToTwo - oneToThree - twoToThree);
            double threeTwoThree = (oneToThree - oneToTwo - twoToThree);
            double threeOneTThree = (twoToThree - oneToTwo - oneToThree);

            values.B1 = 4 * twoToThree * beacon1Rdr
                        + 2 * twoTwoThree * beacon2Rdr
                        + 2 * threeTwoThree * beacon3Rdr
                        + 2 * twoToThree * threeOneTThree;

            values.B2 = 4 * oneToThree * beacon2Rdr
                        + 2 * twoTwoThree * beacon1Rdr
                        + 2 * threeOneTThree * beacon3Rdr
                        + 2 * oneToThree * threeTwoThree;

            values.B3 = 4 * oneToTwo * beacon3Rdr
                        + 2 * threeTwoThree * beacon1Rdr
                        + 2 * threeOneTThree * beacon2Rdr
                        + 2 * oneToTwo * twoTwoThree;

            double beacon1Rdr2 = _beacon1.RDR2;
            double beacon2Rdr2 = _beacon2.RDR2;
            double beacon3Rdr2 = _beacon3.RDR2;
            values.C = 2 * oneToTwo * oneToThree * twoToThree
                       + 2 * twoToThree * beacon1Rdr2
                       + 2 * oneToThree * beacon2Rdr2
                       + 2 * oneToTwo * beacon3Rdr2
                       + 2 * twoTwoThree * beacon1Rdr * beacon2Rdr
                       + 2 * threeTwoThree * beacon1Rdr * beacon3Rdr
                       + 2 * threeOneTThree * beacon2Rdr * beacon3Rdr
                       + 2 * twoToThree * threeOneTThree * beacon1Rdr
                       + 2 * oneToThree * threeTwoThree * beacon2Rdr
                       + 2 * oneToTwo * twoTwoThree * beacon3Rdr;

            return values;
        }

        public static void AssignDistances(IBeacon baseBeacon, IEnumerable<IBeacon> targets)
        {
            var distances = new Dictionary<int, IBeaconDistance>();
            foreach (IBeacon target in targets)
            {
                distances.Add(target.Id, new BeaconDistanceBase
                {
                    BeaconId = target.Id,
                    Distance = GetDistances(baseBeacon, target)
                });
            }

            baseBeacon.BeaconDistances = distances;
        }

        public static double GetDistances(IBeacon beaconOne, IBeacon beaconTwo)
        {
            return Math.Sqrt(((beaconOne.X - beaconTwo.X) * (beaconOne.X - beaconTwo.X) +
                              (beaconOne.Z - beaconTwo.Z) * (beaconOne.Z - beaconTwo.Z)));
        }
    }

    internal class EstimatedDistances
    {
        public double D1 { get; set; }
        public double D2 { get; set; }
        public double D3 { get; set; }

        public EstimatedDistances(IBeacon[] beacons, Error errors)
        {
            D1 = Math.Sqrt(beacons[0].RDR + errors.E1);
            D2 = Math.Sqrt(beacons[1].RDR + errors.E2);
            D3 = Math.Sqrt(beacons[2].RDR + errors.E3);
        }
    }

    internal class Error
    {
        public double E1 { get; set; }
        public double E2 { get; set; }
        public double E3 { get; set; }
    }

    public interface IBeacon
    {
        int Id { get; set; }
        double X { get; set; }
        double Z { get; set; }
        IDictionary<int, IBeaconDistance> BeaconDistances { get; set; }
        double ReadDistance { get; set; }
        double RDR => Math.Pow(ReadDistance, 2);
        double RDR2 => Math.Pow(ReadDistance, 4);

        double To(int i)
        {
            return BeaconDistances[i].DistanceSquared;
        }

        IBeaconDistance this[int index] => BeaconDistances[index];
    }

    public interface IBeaconDistance
    {
        int BeaconId { get; set; }
        double Distance { get; set; }
        double DistanceSquared => Math.Pow(Distance, 2);
    }

    public class BeaconDistanceBase : IBeaconDistance
    {
        public int BeaconId { get; set; }
        public double Distance { get; set; }
    }

    public class BeaconBase : IBeacon
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Z { get; set; }
        public IDictionary<int, IBeaconDistance> BeaconDistances { get; set; }
        public double ReadDistance { get; set; }

        public static implicit operator BeaconBase((double, double) coordinate)
        {
            (double x, double z) = coordinate;
            return new BeaconBase
            {
                X = x,
                Z = z
            };
        }
    }

    public static class BeaconExtensions
    {
    }
}