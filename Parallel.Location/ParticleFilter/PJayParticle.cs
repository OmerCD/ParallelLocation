﻿using System;

namespace Parallel.Location.ParticleFilter
{
    public class PJayParticle
    {
        public double X { get; set; }
        public double Z { get; set; }
        public int NumberOfCircleDistances { get; private set; }
        public int Weight { get; set; }

        public PJayParticle(double x, double z,
            int numberOfCircleDistances = 0)
        {
            X = x;
            Z = z;
            NumberOfCircleDistances = numberOfCircleDistances;
        }

        public void SetNoise(int numberOfCircleDistances)
        {
            NumberOfCircleDistances = numberOfCircleDistances;
        }
    }
}