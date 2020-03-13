using System;
using System.IO;

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
            Matrix matrix = GenerateSymMatrix(200);
            File.Delete("out.txt");
            var stream = File.OpenWrite("out.txt");
            matrix.WriteStream(stream);
            stream.Close();

            RotationAlg rotationAlg = new RotationAlg(matrix, 0.01d);
            rotationAlg.Calculate();
            Console.WriteLine("end");
        }
    }
}
