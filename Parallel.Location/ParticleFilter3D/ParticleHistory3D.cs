﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Accord.Math.Distances;
using MathNet.Numerics.Statistics;

namespace Parallel.Location.ParticleFilter
{
    class ParticleHistory3D
    {
        private readonly Euclidean _euclid;
        private readonly int _historyCount;
        private ConcurrentQueue<double> _lastAccelerations;
        public double AccelerationAverage => _lastAccelerations.Average();
        public double AccelerationStd => _lastAccelerations.StandardDeviation();
        public List<Particle3D> Particles { get; set; }
        public IList<CoordinateHistory> GeneratedCoordinate { get; private set; }

        public ParticleHistory3D(int historyCount)
        {
            _historyCount = historyCount;
            GeneratedCoordinate = new List<CoordinateHistory>(historyCount);
            _lastAccelerations = new ConcurrentQueue<double>();
            _euclid = new Euclidean();
        }

        public void AddHistoric(ICoordinate coordinate, DateTime timeStamp)
        {
            lock (GeneratedCoordinate)
            {
                if (GeneratedCoordinate.Count >= _historyCount)
                {
                    GeneratedCoordinate.RemoveAt(0);
                }
            }

            if (GetAcceleration(coordinate,timeStamp,out var acceleration) && !double.IsNaN(acceleration))
            {
                while (_lastAccelerations.Count >= _historyCount)
                {
                    _lastAccelerations.TryDequeue(out _);
                }

                _lastAccelerations.Enqueue(acceleration);
            }
            GeneratedCoordinate.Add(new CoordinateHistory(coordinate, timeStamp));
        }

        public bool GetAcceleration(ICoordinate currentCoordinate, DateTime timeStamp, out double acceleration)
        {
            if (GeneratedCoordinate.Count >= 2)
            {
                
                var historics = GeneratedCoordinate.Skip(Math.Max(0, GeneratedCoordinate.Count() - 2)).ToArray();
                var historicOne = historics[1];
                var historicTwo = historics[0];
                var firstDistance = _euclid.Distance(new[] {currentCoordinate.X, currentCoordinate.Z},
                    new [] {historicOne.Coordinate.X, historicOne.Coordinate.Z});
                var secondDistance = _euclid.Distance(new[] {historicOne.Coordinate.X, historicOne.Coordinate.Z},
                    new [] {historicTwo.Coordinate.X, historicTwo.Coordinate.Z});
               
                var speed1 = firstDistance / timeStamp.Subtract(historicOne.TimeStamp).TotalMilliseconds * 1000;

                var speed2 = secondDistance / historicOne.TimeStamp.Subtract(historicTwo.TimeStamp).TotalMilliseconds * 1000;

                acceleration = speed1 - speed2;
                return true;
            }

            acceleration = 0;
            return false;
        }
    }
}