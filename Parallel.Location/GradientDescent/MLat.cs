using System;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;

namespace Parallel.Location
{
    public class MLAT
    {
        public class GradientDescentResult
        {
            public Vector<double> Estimator { get; set; }
            public Matrix<double> EstimatorCandidate { get; }
            public Vector<double> Error { get; }

            public GradientDescentResult(int n, int dim)
            {
                EstimatorCandidate = DenseMatrix.Build.Dense(n, dim);
                Error = DenseVector.Build.Dense(n);
            }
        }

        private static double GetEuclidean(Vector<double> p1, Vector<double> p2)
        {
            return Distance.Euclidean(p1, p2);
        }

        private static GradientDescentResult GradientDescent(Matrix<double> anchorsIn, Vector<double> rangesIn,
            Matrix<double> boundsIn = null, int nTrial = 100, double alpha = 0.001, double timeThreshold = 0)
        {
            System.Random random = new SystemRandomSource();

            int n = anchorsIn.RowCount;
            int dim = anchorsIn.ColumnCount;
            var gradientDescentResult = new GradientDescentResult(nTrial, dim);

            if (boundsIn == null)
            {
                boundsIn = DenseMatrix.Build.Dense(1, dim);
            }
            Matrix<double> boundsTemp = anchorsIn.Stack(boundsIn);
            Matrix<double> bounds = DenseMatrix.Build.Dense(2, dim);
            for (int i = 0; i < dim; i++)
            {
                bounds[0, i] = boundsTemp.Column(i).Min();
                bounds[1, i] = boundsTemp.Column(i).Max();
            }

            if (timeThreshold == 0)
            {
                timeThreshold = 1.0 / nTrial;
            }

            Vector<double> ranges = DenseVector.Build.Dense(n);
            for (int i = 0; i < nTrial; i++)
            {
                Vector<double> estimator0 = DenseVector.Build.Dense(dim);
                for (int j = 0; j < dim; j++)
                {
                    estimator0[j] = random.NextDouble() * (bounds[1, j] - bounds[0, j]) + bounds[0, j];
                }
                Vector<double> estimator = DenseVector.OfVector(estimator0);

                Stopwatch stopwatch = new Stopwatch();
                while (true)
                {
                    for (int j = 0; j < n; j++)
                    {
                        ranges[j] = GetEuclidean(anchorsIn.Row(j), estimator);
                    }
                    double error = GetEuclidean(rangesIn, ranges);

                    Vector<double> delta = DenseVector.Build.Dense(dim);
                    for (int j = 0; j < n; j++)
                    {
                        delta += (rangesIn[j] - ranges[j]) / ranges[j] * (estimator - anchorsIn.Row(j));
                    }
                    delta *= 2 * alpha;

                    Vector<double> estimatorNext = estimator - delta;
                    for (int j = 0; j < n; j++)
                    {
                        ranges[j] = MLAT.GetEuclidean(anchorsIn.Row(j), estimatorNext);
                    }
                    double errorNext = MLAT.GetEuclidean(rangesIn, ranges);
                    if (errorNext < error)
                    {
                        estimator = estimatorNext;
                    }
                    else
                    {
                        gradientDescentResult.EstimatorCandidate.SetRow(i, estimator);
                        gradientDescentResult.Error[i] = error;
                        break;
                    }
                    if (stopwatch.ElapsedMilliseconds > timeThreshold)
                    {
                        gradientDescentResult.Error[i] = double.MaxValue;
                        break;
                    }
                }
            }

            return gradientDescentResult;
        }

        public static GradientDescentResult Mlat(Matrix<double> anchorsIn, Vector<double> rangesIn,
            Matrix<double> boundsIn = null, int nTrial = 200, double alpha = 0.01, double timeThreshold = 0)
        {
            GradientDescentResult gradientDescentResult = GradientDescent(anchorsIn, rangesIn, boundsIn, nTrial, alpha, timeThreshold);

            int idx = -1;
            double error = double.MaxValue;
            for (int i = 0; i < gradientDescentResult.Error.Count; i++)
            {
                if (gradientDescentResult.Error[i] < error)
                {
                    idx = i;
                    error = gradientDescentResult.Error[i];
                }
            }
            gradientDescentResult.Estimator = gradientDescentResult.EstimatorCandidate.Row(idx);
            return gradientDescentResult;
        }
    }
}