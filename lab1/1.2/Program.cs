using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace CMLab1
{
    class Matrix
    {
        public int n, m;
        private int[][] matrix;
        public Matrix(string rawString)
        {
            var rows = rawString.Split("\n");
            n = rows.Length;
            var columns = rows[0].Split(" ");
            m = columns.Length;
            matrix = new int[n][];
            for (int i = 0; i < n; i++)
                matrix[i] = new int[m];
            for(int i = 0; i < n; i++)
            {
                columns = rows[i].Split(" ");
                for (int j = 0; j < m; j++)
                    matrix[i][j] = Int32.Parse(columns[j]);
            }
        }
        public int this[int i, int j]
        {
            get { return matrix[i][j]; }
            set { matrix[i][j] = value; }
        }
        private int A(int index)
        {
            if (index < n && index > 0)
                return matrix[index][index - 1];
            else
                return 0;
        }
        private int B(int index)
        {
            if (index < n)
                return matrix[index][index];
            else
                return 0;
        }
        private int C(int index)
        {
            if (index < n - 1)
                return matrix[index][index + 1];
            else
                return 0;
        }
        private int D(int index)
        {
            return matrix[index][m - 1];
        }
        public double[] Calculate()
        {
            double[] x = new double[n];
            double[] P = new double[n];
            double[] Q = new double[n];
            P[0] = -(double)C(0) / B(0);
            Q[0] = (double)D(0) / B(0);
            for(int i = 1; i < n; i++)
            {
                P[i] = (-C(i)) / (B(i) + A(i) * P[i - 1]);
                Q[i] = (D(i)- A(i) * Q[i - 1]) / (B(i) + A(i) * P[i - 1]);
            }
            x[n - 1] = Q[n - 1];
            for (int i = n - 2; i >= 0; i--)
                x[i] = P[i] * x[i + 1] + Q[i];
            return x;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Matrix inMat = new Matrix(File.ReadAllText("in.txt"));
            var result = inMat.Calculate();
            string resStr = "";
            for (int i = 0; i < result.Length; i++)
                resStr += result[i].ToString() + "\n";
            Console.WriteLine(resStr);
            File.WriteAllText("out.txt", resStr);
            Console.ReadKey();
        }
    }
}
