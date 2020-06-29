﻿namespace Parallel.Location.ParticleFilter
{
    public class Particle3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public int NumberOfCircleDistances { get; private set; }
        public int Weight { get; set; }

        public Particle3D(double x, double y,double z,
            int numberOfCircleDistances = 0)
        {
            X = x;
            Y = y;
            Z = z;
            NumberOfCircleDistances = numberOfCircleDistances;
        }

        public void SetNoise(int numberOfCircleDistances)
        {
            NumberOfCircleDistances = numberOfCircleDistances;
        }
    }
}