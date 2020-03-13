using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

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
        static void Main(string[] args)
        {
            Matrix matrix = GenerateSymMatrix(150);
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
            Console.WriteLine("Async time: {0}; Sync time: {1}.", timeAsync, timeSync);
        }
    }
}
