using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Accord.Extensions.Statistics.Filters;
using Accord.Math;
using Accord.Math.Random;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Running;

namespace Parallel.Location.ParticleFilter
{
    public class ParticleFilter : ILocationCalculator
    {
        public static readonly Random Random = new Random();
        private readonly int _numberOfParticles;
        private readonly int _numberOfCircleDistances;
        private IDictionary<int, IAnchor> _anchors;
        private IDictionary<int, ParticleHistory> _particles;

        public ParticleFilter(int numberOfParticles,
            int numberOfCircleDistances)
        {
            _numberOfParticles = numberOfParticles;
            _numberOfCircleDistances = numberOfCircleDistances;
            _particles = new ConcurrentDictionary<int, ParticleHistory>();
        }

        public ParticleFilter()
        {
        }

        public static double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            IEnumerable<double> enumerable = values as double[] ?? values.ToArray();
            if (enumerable.Any())
            {
                //Compute the Average      
                double avg = enumerable.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = enumerable.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (enumerable.Count() - 1));
            }

            return ret;
        }

        private ICoordinate GenerateCoordinates(IEnumerable<IDistance> distances, IList<PJayParticle> particles,
            out IList<PJayParticle> trainedParticles, bool secondTime)
        {
            var weights = new List<double>(_numberOfParticles);
            var maxWeight = 0d;
            distances = distances.OrderBy(x => x.Distance);
            IDistance[] distanceArray = distances.ToArray();
            var distanceSum = distanceArray.Sum(x => x.Distance);

            foreach (PJayParticle pJay in particles)
            {
                // if (!secondTime)
                // {
                //     pJay.X = Random.Next((int) worldSizeInfo.WorldSizeWidthMin,
                //         (int) worldSizeInfo.WorldSizeWidthMax);
                //     pJay.Y = Random.Next((int) worldSizeInfo.WorldSizeHeightMin,
                //         (int) worldSizeInfo.WorldSizeHeightMax);
                // }

                double prob = 1;
                foreach (IDistance distance in distanceArray)
                {
                    IAnchor anchor = _anchors[distance.FromAnchorId];
                    double dist = Math.Sqrt(Math.Pow(pJay.X - anchor.X, 2) + Math.Pow(pJay.Y - anchor.Z, 2));
                    prob *= GaussianProbabilityDistribution(dist, pJay.NumberOfCircleDistances, distance.Distance) *
                            (1 - (distance.Distance / distanceSum));
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
                    var xx = newParticles[i].X + Random.Next(-_numberOfCircleDistances, _numberOfCircleDistances);
                    var zz = newParticles[i].Y + Random.Next(-_numberOfCircleDistances, _numberOfCircleDistances);

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

        private static double GaussianProbabilityDistribution(double mu, double sigma, double x)
        {
            var a = Math.Exp(-1 * Math.Pow((mu - x), 2) / Math.Pow(sigma, 2) / 2.0);
            var b = Math.Sqrt(2.0 * Math.PI * (Math.Pow(sigma, 2)));
            return a / b;
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

        public IEnumerable<IAnchor> CurrentAnchors => _anchors.Values;

        public ICoordinate GetResult(int objectId, params IDistance[] distances)
        {
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

            var worldSizeInfo = new WorldSizeInfo();
            worldSizeInfo.WorldSizeWidthMin = minW;
            worldSizeInfo.WorldSizeWidthMax = maxW;
            worldSizeInfo.WorldSizeHeightMin = minH;
            worldSizeInfo.WorldSizeHeightMax = maxH;
            List<PJayParticle> particles;
            bool doesExist = false;
            if (_particles.ContainsKey(objectId))
            {
                particles = _particles[objectId].Particles;
                doesExist = true;
            }
            else
            {
                particles = new List<PJayParticle>(_numberOfParticles);
                for (int i = 0; i < _numberOfParticles; i++)
                {
                    double x = Random.Next((int) worldSizeInfo.WorldSizeWidthMin,
                        (int) worldSizeInfo.WorldSizeWidthMax);
                    double y = Random.Next((int) worldSizeInfo.WorldSizeHeightMin,
                        (int) worldSizeInfo.WorldSizeHeightMax);

                    var particle = new PJayParticle(x, y);
                    particle.SetNoise(_numberOfCircleDistances);
                    particles.Add(particle);
                }

                _particles.Add(objectId, new ParticleHistory(10)
                {
                    Particles = particles,
                });
            }
            // particles = new List<PJayParticle>(_numberOfParticles);


            var result = GenerateCoordinates(distances, particles, out IList<PJayParticle> trainedParticles, doesExist);
            var averageX = trainedParticles.Average(x => x.X);
            var averageY = trainedParticles.Average(x => x.Y);

            for (int i = trainedParticles.Count; i < _numberOfParticles; i++)
            {
                double x = averageX + Random.Next(-_numberOfCircleDistances, _numberOfCircleDistances);
                double y = averageY + Random.Next(-_numberOfCircleDistances, _numberOfCircleDistances);

                var particle = new PJayParticle(x, y);
                particle.SetNoise(_numberOfCircleDistances);
                trainedParticles.Add(particle);
            }

            _particles[objectId].Particles = trainedParticles as List<PJayParticle> ?? trainedParticles.ToList();
            var now = DateTime.Now;
            if (_particles[objectId].GetAcceleration(result,now,out var acceleration) && !double.IsNaN(acceleration))
            {
                var std = _particles[objectId].AccelerationStd;
                if (!double.IsNaN(std))
                {
                    var kalmanFilter = new KalmanFilter2D(3, acceleration, _particles[objectId].AccelerationStd);
                    foreach (CoordinateHistory coordinateHistory in _particles[objectId].GeneratedCoordinate)
                    {
                        kalmanFilter.Push(coordinateHistory.Coordinate.X, coordinateHistory.Coordinate.Z);
                    }
            
                    result.X = (result.X + kalmanFilter.X) * .5;
                    result.Z = (result.Z + kalmanFilter.Y) * .5;
                }
            }
            _particles[objectId].AddHistoric(result, now);

            return result;
        }

        private class WorldSizeInfo
        {
            public double WorldSizeWidthMin { get; set; }
            public double WorldSizeHeightMin { get; set; }
            public double WorldSizeWidthMax { get; set; }
            public double WorldSizeHeightMax { get; set; }
        }
    }
}