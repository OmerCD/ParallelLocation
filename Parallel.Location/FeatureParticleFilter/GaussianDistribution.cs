﻿using System;
using Accord.Statistics.Distributions.Univariate;

namespace ParticleFilter
{
    public class GaussianDistribution : IDistribution
    {
        private readonly int _maxValue;

        public GaussianDistribution(int maxValue)
        {
            _maxValue = maxValue;
        }

        public double Generate()
        {
            return Math.Abs(NormalDistribution.Random()) * _maxValue;
        }
    }
}