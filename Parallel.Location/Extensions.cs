using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
}