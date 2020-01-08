using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Parallel.Location
{
    public static class Extensions
    {
        public static double[,] ToArray(this IEnumerable<ICoordinate> coordinates)
        {
            var array = new double[1, 3];
            var count = 0;

            // ReSharper disable once GenericEnumeratorNotDisposed
            using IEnumerator<ICoordinate> enumerator = coordinates.GetEnumerator();
            while (enumerator.MoveNext())
            {
                int length = array.GetLength(0);
                if (count == length)
                {
                    array = ResizeMultiArray(length * 2, array);
                }

                ICoordinate current = enumerator.Current;
                Debug.Assert(current != null, nameof(current) + " != null");
                array[count, 0] = current.X;
                array[count, 1] = current.Z;
                array[count, 2] = current.Y;
            }

            return array;
        }

        public static double[,] ToArray(this ICoordinate[] coordinates)
        {
            var array = new double[coordinates.Length, 3];
            for (var i = 0; i < coordinates.Length; i++)
            {
                array[i, 0] = coordinates[i].X;
                array[i, 1] = coordinates[i].Z;
                array[i, 2] = coordinates[i].Y;
            }

            return array;
        }

        private static double[,] ResizeMultiArray(int length, double[,] array)
        {
            var tempArray = new double[length, 3];
            for (var xIndex = 0; xIndex < length; xIndex++)
            {
                for (var yIndex = 0; yIndex < 3; yIndex++)
                {
                    tempArray[xIndex, yIndex] = array[xIndex, yIndex];
                }
            }

            array = tempArray;
            return array;
        }
    }

    public static class ServiceExtensions
    {
        public static IServiceCollection AddLocationCalculator<TLocation, TAnchor>(
            this IServiceCollection serviceProvider,
            Action<LocationCalculatorBuilder<TLocation, TAnchor>> builderAction)
            where TLocation : ILocationCalculator, new()
            where TAnchor : IAnchor, new()
        {
            var locationCalculatorBuilder = new LocationCalculatorBuilder<TLocation, TAnchor>();
            builderAction(locationCalculatorBuilder);
            serviceProvider.AddSingleton<ILocationCalculator>(locationCalculatorBuilder.Build());
            return serviceProvider;
        }
    }

    public class LocationCalculatorBuilder<TLocation, TAnchor>
        where TLocation : ILocationCalculator, new()
        where TAnchor : IAnchor, new()
    {
        private readonly TLocation _locationCalculator;
        private readonly Dictionary<int, IAnchor> _anchors;

        public LocationCalculatorBuilder()
        {
            _locationCalculator = new TLocation();
            _anchors = new Dictionary<int, IAnchor>();
        }

        public LocationCalculatorBuilder(TLocation locationCalculator)
        {
            _locationCalculator = locationCalculator;
            _anchors = new Dictionary<int, IAnchor>();
        }

        public LocationCalculatorBuilder<TLocation, TAnchor> WithAnchor(IAnchor anchor)
        {
            if (!_anchors.ContainsKey(anchor.Id))
            {
                _anchors.Add(anchor.Id, anchor);
            }

            return this;
        }

        public LocationCalculatorBuilder<TLocation, TAnchor> WithAnchors(params IAnchor[] anchors)
        {
            for (var i = 0; i < anchors.Length; i++)
            {
                if (!_anchors.ContainsKey(anchors[i].Id))
                {
                    _anchors.Add(anchors[i].Id, anchors[i]);
                }
            }


            return this;
        }

        public LocationCalculatorBuilder<TLocation, TAnchor> WithAnchor(double x, double z, double y, int id)
        {
            if (!_anchors.ContainsKey(id))
            {
                _anchors.Add(id, new TAnchor
                {
                    Id = id,
                    X = x,
                    Z = z,
                    Y = y
                });
            }

            return this;
        }

        public TLocation Build()
        {
            _locationCalculator.SetAnchors(_anchors.Values);
            return _locationCalculator;
        }
    }
}