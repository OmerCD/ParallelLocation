﻿using System;
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
    public class ParticleFilter3D : ILocationCalculator
    {
        public static readonly Random Random = new Random();
        private readonly int _numberOfParticles;
        private readonly int _numberOfCircleDistances;
        private IDictionary<int, IAnchor> _anchors;
        private readonly ConcurrentDictionary<int, ParticleHistory3D> _particles;

        public ParticleFilter3D(int numberOfParticles,
            int numberOfCircleDistances)
        {
            _numberOfParticles = numberOfParticles;
            _numberOfCircleDistances = numberOfCircleDistances;
            _particles = new ConcurrentDictionary<int, ParticleHistory3D>();
        }

        public ParticleFilter3D()
        {
        }


        private ICoordinate GenerateCoordinates(IEnumerable<IDistance> distances, IList<Particle3D> particles,
            out IList<Particle3D> trainedParticles, bool secondTime)
        {
            var weights = new List<double>(_numberOfParticles);
            var maxWeight = 0d;
            distances = distances.OrderBy(x => x.Distance);
            IDistance[] distanceArray = distances.ToArray();
            double distanceSum = distanceArray.Sum(x => x.Distance);

            foreach (var pJay in particles)
            {
                double prob = 1;
                foreach (IDistance distance in distanceArray)
                {
                    IAnchor anchor = _anchors[distance.FromAnchorId];
                    double dist = Math.Sqrt(Math.Pow(pJay.X - anchor.X, 2) + Math.Pow(pJay.Y - anchor.Y, 2) + Math.Pow(pJay.Z - anchor.Z,2));
                    prob *= GaussianProbabilityDistribution(dist, pJay.NumberOfCircleDistances, distance.Distance)
                    // * (1 - (distance.Distance / distanceSum))
                    ;
                }

                weights.Add(prob);
                if (maxWeight < prob)
                {
                    maxWeight = prob;
                }
            }

            var newParticles = new List<Particle3D>(_numberOfParticles);
            double beta = (secondTime ? 0.9 : 0.7) * maxWeight;
            var resultPosition = new Coordinate(0, 0, 0);
            var weightTotal = 0d;
            for (var i = 0; i < _numberOfParticles; i++)
            {
                try
                {
                    if (beta < weights[i])
                    {
                        newParticles.Add(particles[i]);
                        resultPosition.X += particles[i].X * weights[i];
                        resultPosition.Y += particles[i].Y * weights[i];
                        resultPosition.Z += particles[i].Z * weights[i];
                        weightTotal += weights[i];
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            resultPosition.X /= weightTotal;
            resultPosition.Z /= weightTotal;
            resultPosition.Y /= weightTotal;

            if (!secondTime)
            {
                var i = 0;
                int currentParticleNumber = newParticles.Count;
                while (_numberOfParticles > newParticles.Count && currentParticleNumber > 0)
                {
                    i %= currentParticleNumber;
                    double xx = newParticles[i].X + Random.Next(-_numberOfCircleDistances, _numberOfCircleDistances);
                    double yy = newParticles[i].Y + Random.Next(-_numberOfCircleDistances, _numberOfCircleDistances);
                    double zz = newParticles[i].Z + Random.Next(-_numberOfCircleDistances, _numberOfCircleDistances);

                    var particle = new Particle3D(xx, yy,zz);
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
            double a = Math.Exp(-1 * Math.Pow((mu - x), 2) / Math.Pow(sigma, 2) / 2.0);
            double b = Math.Sqrt(2.0 * Math.PI * (Math.Pow(sigma, 2)));
            return a / b;
        }

        public int MinAnchorCount => 3;

        public void SetAnchors(IEnumerable<IAnchor> values)
        {
            _anchors = values.Distinct(new AnchorIdComparer()).ToDictionary(x => x.Id);
        }

        private class AnchorIdComparer : IEqualityComparer<IAnchor>
        {
            public bool Equals(IAnchor x, IAnchor y)
            {
                return x?.Id == y?.Id;
            }

            public int GetHashCode(IAnchor obj)
            {
                return obj.Id.GetHashCode();
            }
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

        public IAnchor this[int anchorId] => _anchors[anchorId];

        public ICoordinate GetResult(int objectId, params IDistance[] distances)
        {
            List<Particle3D> particles = CreateParticles(objectId, distances, out bool doesExist);

            ICoordinate result;
            if (distances.Length == 1)
            {
                var distance = distances[0];
                var anchor = _anchors[distance.FromAnchorId];
                result = new Coordinate(anchor.X, anchor.Z, anchor.Y);
                return result;
            }
            else
            {
                result = GenerateCoordinates(distances, particles, out IList<Particle3D> trainedParticles, doesExist);

                if (trainedParticles.Count == 0)
                {
                    return null;
                }

                double averageX = trainedParticles.Average(x => x.X);
                double averageY = trainedParticles.Average(x => x.Y);
                double averageZ = trainedParticles.Average(x => x.Z);

                for (int i = trainedParticles.Count; i < _numberOfParticles; i++)
                {
                    double x = averageX + Random.Next(-_numberOfCircleDistances, _numberOfCircleDistances);
                    double y = averageY + Random.Next(-_numberOfCircleDistances, _numberOfCircleDistances);
                    double z = averageZ + Random.Next(-_numberOfCircleDistances, _numberOfCircleDistances);

                    var particle = new Particle3D(x, y, z);
                    particle.SetNoise(_numberOfCircleDistances);
                    trainedParticles.Add(particle);
                }

                _particles[objectId].Particles = trainedParticles as List<Particle3D> ?? trainedParticles.ToList();
                DateTime now = DateTime.Now;
                ApplyKalmanFilter(objectId, result, now);
                _particles[objectId].AddHistoric(result, now);
            }

            return result;
        }

        private void ApplyKalmanFilter(int objectId, ICoordinate result, DateTime time)
        {
            if (_particles[objectId].GetAcceleration(result, time, out double acceleration) &&
                !double.IsNaN(acceleration))
            {
                double std = _particles[objectId].AccelerationStd;
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
        }

        private List<Particle3D> CreateParticles(int objectId, IDistance[] distances, out bool doesExist)
        {
            double minH = double.MaxValue;
            double maxH = 0;
            double maxW = 0;
            double minW = double.MaxValue;
            double minL = double.MaxValue;
            double maxL = 0;

            foreach (IDistance distance in distances)
            {
                IAnchor anchor = _anchors[distance.FromAnchorId];
                if (maxH < anchor.Y + distance.Distance)
                {
                    maxH = anchor.Y + distance.Distance;
                }

                if (minH > anchor.Y - distance.Distance)
                {
                    minH = anchor.Y - distance.Distance;
                }

                if (maxW < anchor.X + distance.Distance)
                {
                    maxW = anchor.X + distance.Distance;
                }

                if (minW > anchor.X - distance.Distance)
                {
                    minW = anchor.X - distance.Distance;
                }

                if (maxL < anchor.Z + distance.Distance)
                {
                    maxL = anchor.Z + distance.Distance;
                }

                if (minL > anchor.Z - distance.Distance)
                {
                    minL = anchor.Z - distance.Distance;
                }
            }

            maxH += 50;
            minH -= 50;
            maxW += 50;
            minW -= 50;
            maxL += 50;
            minL -= 50;

            var worldSizeInfo = new WorldSizeInfo();
            worldSizeInfo.WorldSizeWidthMin = minW;
            worldSizeInfo.WorldSizeWidthMax = maxW;
            worldSizeInfo.WorldSizeHeightMin = 50;
            worldSizeInfo.WorldSizeHeightMax = maxH;
            worldSizeInfo.WorldSizeLengthMax = maxL;
            worldSizeInfo.WorldSizeLengthMin = minL;
            List<Particle3D> particles;
            doesExist = false;
            lock (_particles)
            {
                if (_particles.ContainsKey(objectId))
                {
                    particles = _particles[objectId].Particles;
                    doesExist = true;
                }
                else
                {
                    particles = new List<Particle3D>(_numberOfParticles);
                    for (int i = 0; i < _numberOfParticles; i++)
                    {
                        double x = Random.Next((int) worldSizeInfo.WorldSizeWidthMin,
                            (int) worldSizeInfo.WorldSizeWidthMax);
                        double y = Random.Next((int) worldSizeInfo.WorldSizeHeightMin,
                            (int) worldSizeInfo.WorldSizeHeightMax);
                        double z = Random.Next((int) worldSizeInfo.WorldSizeLengthMin,
                            (int) worldSizeInfo.WorldSizeLengthMax);

                        var particle = new Particle3D(x, y, z);
                        particle.SetNoise(_numberOfCircleDistances);
                        particles.Add(particle);
                    }

                    _particles.TryAdd(objectId, new ParticleHistory3D(10)
                    {
                        Particles = particles,
                    });
                }
            }

            return particles;
        }

        private class WorldSizeInfo
        {
            public double WorldSizeWidthMin { get; set; }
            public double WorldSizeHeightMin { get; set; }
            public double WorldSizeWidthMax { get; set; }
            public double WorldSizeHeightMax { get; set; }
            public double WorldSizeLengthMax { get; set; }
            public double WorldSizeLengthMin { get; set; }
        }
    }
}