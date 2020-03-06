using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Math.Distances;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Running;
using Parallel.Location.ParticleFilter;

namespace Parallel.Location.ParticleAreaFilter
{
    public class ParticleAreaFilter : ILocationCalculator
    {
        private IDictionary<int, ParticleHistory> _particles;
        private IDictionary<int, IAnchor> _anchors;
        private readonly int _numberOfParticles;
        private readonly int _numberOfCircleDistances;
        private readonly Euclidean _euclidean;

        public ParticleAreaFilter(int numberOfParticles, int numberOfCircleDistances)
        {
            _numberOfParticles = numberOfParticles;
            _numberOfCircleDistances = numberOfCircleDistances;
            _particles = new ConcurrentDictionary<int, ParticleHistory>();
            _euclidean = new Euclidean();
        }

        public ParticleAreaFilter()
        {
        }

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

        private static bool IfSpheresColliding(ICoordinate sphere1, double radius1, ICoordinate sphere2, double radius2)
        {
            var difference = Math.Pow(sphere1.X - sphere2.X, 2) + Math.Pow(sphere1.Y - sphere2.Y, 2) +
                             Math.Pow(sphere1.Z - sphere2.Z, 2);
            return difference < (Math.Pow(radius1, 2) + Math.Pow(radius2, 2));
        }

        private static bool IsInsideOfSphere(ICoordinate coordinate, ICoordinate circleCoordinate, double radius)
        {
            var dx = Math.Pow(coordinate.X - circleCoordinate.X, 2);
            var dy = Math.Pow(coordinate.Y - circleCoordinate.Y, 2);
            var dz = Math.Pow(coordinate.Z - circleCoordinate.Z, 2);

            var difference = dx + dy + dz;
            return difference < Math.Pow(radius, 2);
        }

        private bool CheckIfAnchorsColliding(IEnumerable<IDistance> distances)
        {
            var filteredDistances = distances.Where(x => _anchors.ContainsKey(x.FromAnchorId));
            var distanceArray = filteredDistances.ToArray();
            if (distanceArray.Length < 2)
            {
                return false;
            }

            for (int i = 0; i < distanceArray.Length; i++)
            {
                for (int j = i + 1; j < distanceArray.Length; j++)
                {
                    if (!IfSpheresColliding(_anchors[distanceArray[i].FromAnchorId], distanceArray[i].Distance,
                        _anchors[distanceArray[j].FromAnchorId], distanceArray[j].Distance))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public IEnumerable<IAnchor> CurrentAnchors => _anchors.Values;

        public IAnchor this[int anchorId] => _anchors[anchorId];

        public ICoordinate GetResult(int id, params IDistance[] distances)
        {
            if (CheckIfAnchorsColliding(distances))
            {
                // TODO: Kesişim yoksa ParticleFilter algoritmasının çalışması gerekiyor.
                return null;
            }

            double minH = double.MaxValue;
            double maxH = 0;
            double maxW = 0;
            double minW = double.MaxValue;

            foreach (IDistance distance in distances)
            {
                var anchor = _anchors[distance.FromAnchorId];
                if (maxH < anchor.Z + distance.Distance)
                {
                    maxH = anchor.Z + distance.Distance;
                }

                if (minH > anchor.Z - distance.Distance)
                {
                    minH = anchor.Z - distance.Distance;
                }

                if (maxW < anchor.X + distance.Distance)
                {
                    maxW = anchor.X + distance.Distance;
                }

                if (minW > anchor.X - distance.Distance)
                {
                    minW = anchor.X - distance.Distance;
                }
            }

            maxH += 50;
            minH -= 50;
            maxW += 50;
            minW -= 50;

            List<PJayParticle> particles;
            bool doesExist = false;
            if (_particles.ContainsKey(id))
            {
                particles = _particles[id].Particles;
                doesExist = true;
            }
            else
            {
                particles = new List<PJayParticle>(_numberOfParticles);
                for (int i = 0; i < _numberOfParticles; i++)// todo : Create Semiphore ~20 Threads
                {
                    double x = ParticleFilter.ParticleFilter.Random.Next((int) minW,
                        (int) maxW);
                    double y = ParticleFilter.ParticleFilter.Random.Next((int) minH,
                        (int) maxH);

                    var particle = new PJayParticle(x, y);
                    particle.SetNoise(_numberOfCircleDistances);
                    particles.Add(particle);
                }

                _particles.Add(id, new ParticleHistory(10)
                {
                    Particles = particles,
                });
            }

            var result = GenerateCoordinates2(distances, particles, out IList<PJayParticle> trainedParticles, doesExist);
            var averageX = trainedParticles.Average(x => x.X);
            var averageY = trainedParticles.Average(x => x.Y);

            for (int i = trainedParticles.Count; i < _numberOfParticles; i++)
            {
                double x = averageX + ParticleFilter.ParticleFilter.Random.Next(
                               -_numberOfCircleDistances, _numberOfCircleDistances);
                double y = averageY + ParticleFilter.ParticleFilter.Random.Next(
                               -_numberOfCircleDistances, _numberOfCircleDistances);

                var particle = new PJayParticle(x, y);
                particle.SetNoise(_numberOfCircleDistances);
                trainedParticles.Add(particle);
            }

            _particles[id].Particles = trainedParticles as List<PJayParticle> ?? trainedParticles.ToList();
            var now = DateTime.Now;
            if (_particles[id].GetAcceleration(result, now, out var acceleration) && !double.IsNaN(acceleration))
            {
                var std = _particles[id].AccelerationStd;
                if (!double.IsNaN(std))
                {
                    var kalmanFilter = new KalmanFilter2D(3, acceleration, _particles[id].AccelerationStd);
                    foreach (CoordinateHistory coordinateHistory in _particles[id].GeneratedCoordinate)
                    {
                        kalmanFilter.Push(coordinateHistory.Coordinate.X, coordinateHistory.Coordinate.Z);
                    }

                    result.X = (result.X + kalmanFilter.X) * .5;
                    result.Z = (result.Z + kalmanFilter.Y) * .5;
                }
            }

            _particles[id].AddHistoric(result, now);

            return result;
        }

        private static double GaussianProbabilityDistribution(double mu, double sigma, double x)
        {
            var a = Math.Exp(-1 * Math.Pow((mu - x), 2) / Math.Pow(sigma, 2) / 2.0);
            var b = Math.Sqrt(2.0 * Math.PI * (Math.Pow(sigma, 2)));
            return a / b;
        }

        private ICoordinate CircleMiddlefier(ICoordinate circle1, double radius1, ICoordinate circle2, double radius2)
        {
            var distance = _euclidean.Distance(new[] {circle1.X, circle1.Z}, new[] {circle2.X, circle2.Z});
            var s = Math.Pow(radius1, 2) - Math.Pow(radius2, 2) + Math.Pow(distance, 2);
            var bf = s / (2 * distance);
            var verticalDistance = circle2.Z - circle1.Z;
            var fHeight = (verticalDistance * bf) / distance;
            var horizontalDistance = Math.Sqrt(Math.Pow(bf, 2) - Math.Pow(fHeight, 2));

            return new Coordinate(circle1.X + horizontalDistance, circle1.Z + fHeight, 0);
        }

        private IEnumerable<ICoordinate> GetIntersectionCenters(IDistance[] distances)
        {
            for (int i = 0; i < distances.Length; i++)
            {
                var anchorBase = _anchors[distances[i].FromAnchorId];
                for (int j = i + 1; j < distances.Length; j++)
                {
                    var anchorCheck = _anchors[distances[j].FromAnchorId];
                    yield return CircleMiddlefier(anchorBase, distances[i].Distance, anchorCheck,
                        distances[j].Distance);
                }
            }
        }

        /// <summary>
        /// Yapay Zekâ
        /// Rastgele noktalar oluşturuyor.
        /// </summary>
        /// <param name="distances"></param>
        /// <param name="particles"></param>
        /// <param name="trainedParticles"></param>
        /// <param name="secondTime"></param>
        /// <returns></returns>
        private ICoordinate GenerateCoordinates2(IEnumerable<IDistance> distances, IList<PJayParticle> particles,
            out IList<PJayParticle> trainedParticles, bool secondTime)
        {
            IDistance[] distanceArray = distances.ToArray();
            var maxWeight = 0;
            foreach (PJayParticle particle in particles)
            {
                foreach (IDistance distance in distanceArray)
                {
                    IAnchor anchor = _anchors[distance.FromAnchorId];
                    var realDistance = distance.Distance;
                    if (Math.Abs(anchor.Y) > 0.000001)
                    {
                        realDistance = Math.Sqrt(Math.Pow(distance.Distance, 2) - Math.Pow(anchor.Y, 2));
                    }
                    if (IsInsideOfSphere(new Coordinate(particle.X, particle.Y, 0),anchor, realDistance))
                    {
                        particle.Weight++;
                    }

                    if (maxWeight < particle.Weight)
                    {
                        maxWeight = particle.Weight;
                    }
                }
            }

            var weighted = particles.Where(x => x.Weight == maxWeight);
            var count = 0;
            var summed = new Coordinate(0,0,0);
            var pJayParticles = weighted.ToList();
            foreach (var particle in pJayParticles)
            {
                summed.X += particle.X;
                summed.Z += particle.Y;
                particle.Weight = 0;
                count++;
            }

            summed.X /= count;
            summed.Z /= count;

            trainedParticles = pJayParticles;
            return summed;
        }
        private ICoordinate GenerateCoordinates(IEnumerable<IDistance> distances, IList<PJayParticle> particles,
            out IList<PJayParticle> trainedParticles, bool secondTime)
        {
            var weights = new List<double>(_numberOfParticles);
            var maxWeight = 0d;
            distances = distances.OrderBy(x => x.Distance);
            IDistance[] distanceArray = distances.ToArray();
            int distanceCount = distanceArray.Length;
            double weightStep = 1 / (double) distanceCount;
            var intersectionCenters = Enumerable.ToArray(GetIntersectionCenters(distanceArray));
            
            foreach (PJayParticle pJay in particles)
            {
                double prob = 1;
                var pCoordinate = new Coordinate(pJay.X, pJay.Y, 0);
                foreach (ICoordinate intersectionCenter in intersectionCenters)
                {
                    var dist = _euclidean.Distance(new[] {pJay.X, pJay.Y},
                        new[] {intersectionCenter.X, intersectionCenter.Z});
                    // prob *= Normal.Gaussian2D(Math.Pow(80,2), dist, 0);
                    prob *= GaussianProbabilityDistribution(dist, 150, 0);
                }

                weights.Add(prob);
                if (maxWeight < prob)
                {
                    maxWeight = prob;
                }
            }

            var newParticles = new List<PJayParticle>(_numberOfParticles);
            double beta = (secondTime ? 0.9 : 0.7) * maxWeight;
            var resultPosition = new Coordinate(0, 0, 0);
            var weightTotal = 0d;
            for (int i = 0; i < _numberOfParticles; i++)
            {
                if (beta < weights[i])
                {
                    newParticles.Add(particles[i]);
                    resultPosition.X += particles[i].X * weights[i];
                    resultPosition.Z += particles[i].Y * weights[i];
                    weightTotal += weights[i];
                }
            }

            resultPosition.X /= weightTotal;
            resultPosition.Z /= weightTotal;

            if (!secondTime)
            {
                var i = 0;
                int currentParticleNumber = newParticles.Count;
                while (_numberOfParticles > newParticles.Count)
                {
                    i %= currentParticleNumber;
                    var xx = newParticles[i].X +
                             ParticleFilter.ParticleFilter.Random.Next(-_numberOfCircleDistances,
                                 _numberOfCircleDistances);
                    var zz = newParticles[i].Y +
                             ParticleFilter.ParticleFilter.Random.Next(-_numberOfCircleDistances,
                                 _numberOfCircleDistances);

                    var particle = new PJayParticle(xx, zz);
                    particle.SetNoise(_numberOfCircleDistances);
                    newParticles.Add(particle);
                    i++;
                }
            }

            particles = newParticles;
            trainedParticles = newParticles;
            if (!secondTime)
            {
                return GenerateCoordinates(distanceArray, particles, out trainedParticles, true);
            }

            return resultPosition;
        }
    }
}