using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace CMLab1
{
    class RotationAlg
    {
        private Matrix A;
        private double eps;
        private Matrix eigenVectorsRaw;
        public RotationAlg(Matrix inMat, double eps)
        {
            A = inMat;
            this.eps = eps;
            eigenVectorsRaw = Matrix.Identity(A.n);
        }
        public void Rotate()
        {
            Matrix U = Matrix.Identity(A.n);
            int i, j = 0;
            (i, j) = A.FindMax((i, j) => i != j);
            double fi = 0.5 * Math.Atan(2*A[i,j]/(A[i, i] - A[j, j]));
            U[i, i] = Math.Cos(fi);
            U[i, j] = -Math.Sin(fi);
            U[j, j] = Math.Cos(fi);
            U[j, i] = Math.Sin(fi);
            A = U.Transpose() * A * U;
            A.Log("A");
            eigenVectorsRaw = eigenVectorsRaw * U;
        }
        public bool IsEnough()
        {
            double sum = 0;
            for (int i = 0; i < A.n; i++)
                for (int j = i + 1; j < A.m; j++)
                    sum += A[i, j] * A[i, j];
            sum = Math.Sqrt(sum);
            return sum < eps;
        }

        public void Calculate()
        {
            while (!IsEnough())
                Rotate();
            //return A;
        }

        public double[] EigenValues
        {
            get
            {
                double[] res = new double[A.n];
                for (int i = 0; i < A.n; i++)
                    res[i] = A[i, i];
                return res;
            }
        }
        public List<double[]> EigenVectors
        {
            get
            {
                List<double[]> res = new List<double[]>();
                for(int i = 0; i < eigenVectorsRaw.n; i++)
                {
                    double[] eigenVector = new double[eigenVectorsRaw.n];
                    for (int j = 0; j < eigenVectorsRaw.m; j++)
                        eigenVector[j] = eigenVectorsRaw[j, i];
                    res.Add(eigenVector);
                }
                return res;
            }
        }
        public string GetRawResult()
        {
            var eigenValues = EigenValues;
            var eigenVectors = EigenVectors;
            var outTxt = "";
            foreach (var eigenValue in eigenValues)
                outTxt += eigenValue.ToString() + " ";
            outTxt += "\n";
            foreach (var eigenVector in eigenVectors)
            {
                for (int i = 0; i < eigenVector.Length; i++)
                    outTxt += eigenVector[i].ToString() + " ";
                outTxt += "\n";
            }
            return outTxt;
        }
    }
    class Matrix
    {
        public int n, m;
        private double[][] matrix;
        public Matrix(string rawString)
        {
            var rows = rawString.Split("\n");
            n = rows.Length;
            var columns = rows[0].Split(" ");
            m = columns.Length;
            matrix = new double[n][];
            for (int i = 0; i < n; i++)
                matrix[i] = new double[m];
            for (int i = 0; i < n; i++)
            {
                columns = rows[i].Split(" ");
                for (int j = 0; j < m; j++)
                    matrix[i][j] = double.Parse(columns[j], CultureInfo.InvariantCulture);
            }
        }
        public Matrix(string[] lineString)
        {
            var rows = lineString;
            n = rows.Length;
            var columns = rows[0].Split(" ");
            m = columns.Length;
            matrix = new double[n][];
            for (int i = 0; i < n; i++)
                matrix[i] = new double[m];
            for (int i = 0; i < n; i++)
            {
                columns = rows[i].Split(" ");
                for (int j = 0; j < m; j++)
                    matrix[i][j] = double.Parse(columns[j], CultureInfo.InvariantCulture);
            }
        }

        public void Log(string matrixName)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine(matrixName);
            Console.WriteLine(ToString());
        }
        public Matrix(Matrix other)
        {
            matrix = other.matrix;
            n = other.n;
            m = other.m;
        }
        public Matrix(int rows, int columns)
        {
            n = rows;
            m = columns;
            matrix = new double[n][];
            for (int i = 0; i < n; i++)
                matrix[i] = new double[m];
        }

        public Matrix Transpose()
        {
            Matrix transposed = new Matrix(m, n);
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    transposed[i, j] = this[j, i];
            return transposed;
        }

        public (int, int) FindMax(Func<int, int, bool> predicate)
        {
            (int, int) maxIndex = (0, 0);
            double maxValue = double.NegativeInfinity;

            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    if (predicate(i, j) && maxValue <  Math.Abs(matrix[i][j]))
                        (maxIndex, maxValue) = ((i, j), Math.Abs(matrix[i][j]));

            return maxIndex;
        }
        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.m != b.n)
                throw new Exception("Multiplication dimensions incorrect");
            Matrix result = new Matrix(a.n, b.m);
            for (int i = 0; i < result.n; i++)
                for (int j = 0; j < result.m; j++)
                    for (int k = 0; k < a.m; k++)
                        result[i, j] += a[i, k] * b[k, j];
            return result;
        }

        public static Matrix Identity(int size)
        { 
            Matrix matrix = new Matrix(size, size);
            for (int i = 0; i < size; i++)
                matrix[i, i] = 1;
            return matrix;
        }

        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                    res += this[i, j].ToString("0.00") + " ";
                res += System.Environment.NewLine;
            }
            return res;
        }

        public double this[int i, int j]
        {
            get { return matrix[i][j]; }
            set { matrix[i][j] = value; }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var txt = File.ReadAllLines("in.txt");
            double eps = double.Parse(txt[0], CultureInfo.InvariantCulture);
            string[] matrixRaw = new string[txt.Length - 1];
            for (int i = 0; i < matrixRaw.Length; i++)
                matrixRaw[i] = txt[i + 1];
            Matrix inMat = new Matrix(matrixRaw);
            RotationAlg alg = new RotationAlg(inMat, eps);
            alg.Calculate();
            File.WriteAllText("out.txt", alg.GetRawResult());
            Console.ReadKey();
        }
    }
}
