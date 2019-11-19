using System;
using System.Collections.Generic;
using System.Linq;

namespace Parallel.Location
{
    public static class Extensions
    {
        public static double[,] ToArray(this IEnumerable<ICoordinate> coordinates)
        {
            var array = new double[1, 3];
            var count = 0;
            
            using var coorEnum = coordinates.GetEnumerator();
            while (coorEnum.MoveNext())
            {
                var length = array.GetLength(0);
                if (count == length)
                {
                    array = ResizeMultiArray(length * 2, array);
                }

                var current = coorEnum.Current;
                array[count, 0] = current.X;
                array[count, 1] = current.Z;
                array[count, 2] = current.Y;
            }

            return array;
        }

        public static double[,] ToArray(this ICoordinate[] coordinates)
        {
            var array = new double[coordinates.Length, 3];
            for (int i = 0; i < coordinates.Length; i++)
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
            for (int xIndex = 0; xIndex < length; xIndex++)
            {
                for (int yIndex = 0; yIndex < 3; yIndex++)
                {
                    tempArray[xIndex, yIndex] = array[xIndex, yIndex];
                }
            }

            array = tempArray;
            return array;
        }
    }
}