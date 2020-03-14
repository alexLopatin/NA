using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

namespace NMCP
{
    class Program
    {
        static Matrix GenerateSymMatrix(int n)
        {
            Matrix res = new Matrix(n, n);
            Random rand = new Random();
            for (int i = 0; i < n; i++)
                for (int j = i; j < n; j++)
                {
                    int num = rand.Next(-100, 100);
                    (res[i, j], res[j, i]) = (num, num);
                }
            return res;
        }
        //check if Eigen values/vectors were correctly calculated
        static bool Check(Matrix matrix, double[] EigenValues, List<double[]> EigenVectors)
        {
            for (int i = 0; i < EigenVectors.Count; i++)
            {
                double eigenValue = EigenValues[i];
                Matrix vector = new Matrix(EigenVectors[i]);
                Matrix l = matrix * vector;
                vector *= eigenValue;
                double e = 0;
                for (int j = 0; j < l.Rows; j++)
                    if (e < Math.Abs(l[i] - vector[i]))
                        e = Math.Abs(l[i] - vector[i]);
                if (e > 1)
                    return false;
            }
            return true;
        }
        static void Main(string[] args)
        {
            int size = int.Parse(args[0]);
            Matrix matrix = GenerateSymMatrix(size);
            File.Delete("out.txt");
            var stream = File.OpenWrite("out.txt");
            matrix.WriteStream(stream);
            stream.Close();

            RotationAlgAsync rotationAlgAsync = new RotationAlgAsync(new Matrix(matrix), 0.01d);
            RotationAlgSync rotationAlgSync = new RotationAlgSync(new Matrix(matrix), 0.01d);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            rotationAlgAsync.Calculate();
            long timeAsync = stopWatch.ElapsedMilliseconds;
            stopWatch.Reset();
            stopWatch.Start();
            rotationAlgSync.Calculate();
            long timeSync = stopWatch.ElapsedMilliseconds;

            double error = 0;
            var EigenValuesAsync = rotationAlgAsync.EigenValues;
            Array.Sort(EigenValuesAsync);
            var EigenValuesSync = rotationAlgSync.EigenValues;
            Array.Sort(EigenValuesSync);
            for (int i = 0; i < rotationAlgAsync.EigenValues.Length; i++)
                if (error < Math.Abs(EigenValuesAsync[i] - EigenValuesSync[i]))
                    error = Math.Abs(EigenValuesAsync[i] - EigenValuesSync[i]);

            Console.WriteLine("Async time: {0} ms; Sync time: {1} ms; Error: {2}.", timeAsync, timeSync, error);
            Console.WriteLine("Synс correctness: {0}; Async correctness: {1}.",
                Check(matrix, rotationAlgSync.EigenValues, rotationAlgSync.EigenVectors),
                Check(matrix, rotationAlgAsync.EigenValues, rotationAlgAsync.EigenVectors));
        }
    }
}
