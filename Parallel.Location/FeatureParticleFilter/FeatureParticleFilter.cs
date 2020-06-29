﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using Parallel.Location;

 namespace ParticleFilter
{
    public class FeatureLocationCalculator : ILocationCalculator
    {
        private readonly int _variance;
        private IDictionary<int, IAnchor> _anchors;
        private readonly IParticleFilter<FeatureParticle> _filter;
        private readonly Dictionary<int, ICoordinate> _previousLocations;

        public FeatureLocationCalculator(int numberOfParticles, int variance)
        {
            _variance = variance;
            _previousLocations = new Dictionary<int, ICoordinate>();
            _filter = new FeatureParticleFilter();
            var (width, height) = CalculateWorldSize(50);
            _filter.GenerateParticles(numberOfParticles, new List<IDistribution>()
            {
                new GaussianDistribution((int)width),
                new GaussianDistribution((int)height)
            });
        }

        private (double width, double height) CalculateWorldSize(int maxDistance)
        {
            double minH = double.MaxValue;
            double maxH = 0;
            double maxW = 0;
            double minW = double.MaxValue;
            foreach (var anchor in _anchors.Values)
            {
                if (maxH < anchor.Z)
                {
                    maxH = anchor.Z;
                }

                if (minH > anchor.Z)
                {
                    minH = anchor.Z;
                }

                if (maxW < anchor.X)
                {
                    maxW = anchor.X;
                }

                if (minW > anchor.X)
                {
                    minW = anchor.X;
                }
            }

            maxH += maxDistance;
            minH -= maxDistance;
            maxW += maxDistance;
            minW -= maxDistance;
            return (maxW - minW, maxH - minH);
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

        public IAnchor this[int anchorId] => _anchors[anchorId];

        public ICoordinate GetResult(int id, params IDistance[] distances)
        {
            // if (!_previousLocations.ContainsKey(id))
            // {
            //     _previousLocations.Add(id, new Coordinate());
            // }
            // _filter.Predict(0.9f);
            // _filter.Update(new FeatureParticle(_variance)
            // {
            //     
            // });
            throw new NotImplementedException();
        }
    }
    public class FeatureParticleFilter : IParticleFilter<FeatureParticle>
    {
        IList<FeatureParticle> _particles;

        public FeatureParticleFilter()
        {
            _particles = new List<FeatureParticle>();
        }

        public IList<FeatureParticle> Particles => _particles;

        public void GenerateParticles(int numberOfParticles, IList<IDistribution> distributions)
        {
            if (_particles == null)
                throw new ArgumentNullException("The provided collection must not be null.");

            var nDim = distributions.Count;

            for (int i = 0; i < numberOfParticles; i++)
            {
                var randomParam = new double[nDim];
                for (int dimIdx = 0; dimIdx < nDim; dimIdx++)
                {
                    randomParam[dimIdx] = distributions[dimIdx].Generate();
                }

                var p = FeatureParticle.FromArray(randomParam);
                p.Weight = 1d / numberOfParticles;

                _particles.Add(p);
            }
        }

        public IList<FeatureParticle> Resample(int sampleCount)
        {
            var resampledParticles = new List<FeatureParticle>(sampleCount);

            var filteredParticles = _filterParticles(_particles.Count);
            foreach (var dP in filteredParticles)
            {
                var newP = (FeatureParticle)dP.Clone();
                newP.Weight = 1d / _particles.Count;
                resampledParticles.Add(newP);
            }

            return resampledParticles;
        }

        private IEnumerable<FeatureParticle> _filterParticles(int sampleCount)
        {
            double[] cumulativeWeights = new double[_particles.Count];
            
            int cumSumIdx = 0;
            double cumSum = 0;
            foreach (var p in _particles)
            {
                cumSum += p.Weight;
                cumulativeWeights[cumSumIdx++] = cumSum;
            }

            var maxCumWeight = cumulativeWeights[_particles.Count - 1];
            var minCumWeight = cumulativeWeights[0];

            var filteredParticles = new List<FeatureParticle>();

            double initialWeight = 1d / _particles.Count;
            
            for (int i = 0; i < sampleCount; i++)
            {
                var randWeight = minCumWeight + RandomProportional.NextDouble(1) * (maxCumWeight - minCumWeight);
            
                int particleIdx = 0;
                while (cumulativeWeights[particleIdx] < randWeight) 
                {
                    particleIdx++;
                }
            
                var p = _particles[particleIdx];
                filteredParticles.Add(p);
            }

            return filteredParticles;
        }

        public void Predict(float effectiveCountMinRatio)
        {
            List<FeatureParticle> newParticles = null;
            double effectiveCountRatio = EffectiveParticleCount(GetNormalizedWeights(_particles)) / _particles.Count;
            if (effectiveCountRatio > float.Epsilon && 
                effectiveCountRatio < effectiveCountMinRatio)
            {
                newParticles = Resample(_particles.Count).ToList();
            }
            else
            {
                newParticles = _particles
                               .Select(x => (FeatureParticle)x.Clone())
                               .ToList();
            }

            foreach (FeatureParticle p in newParticles)
            {
                p.Diffuse();
            }
            _particles = new List<FeatureParticle>(newParticles);
        }

        private static double EffectiveParticleCount(IEnumerable<double> weights)
        {
            weights = weights as double[] ?? weights.ToArray();
            double sumSqr = weights.Sum(x => x * x) + float.Epsilon;
            return weights.Sum() / sumSqr;
        }

        public IEnumerable<double> GetNormalizedWeights(IEnumerable<IParticle> particles)
        {
            var normalizedWeights = new List<double>();

            IEnumerable<IParticle> enumerable = particles as IParticle[] ?? particles.ToArray();
            var weightSum = enumerable.Sum(x => x.Weight) + float.Epsilon;

            foreach (var p in enumerable)
            {
                var normalizedWeight = p.Weight / weightSum;
                normalizedWeights.Add(normalizedWeight);
            }

            return normalizedWeights;
        }

        public void Update(FeatureParticle measure)
        {
            foreach (var p in _particles)
            {
                var dX = p.Position.X - measure.Position.X;
                var dY = p.Position.Y - measure.Position.Y;
                p.Weight = 1 / (Math.Sqrt(dX * dX + dY * dY));
            }
        }
    }
}
