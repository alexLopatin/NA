using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace CMLab1
{
    class Matrix
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public Matrix L { get; private set; }
        public Matrix U { get; private set; }

        public double[] Solve(double[] b)
        {
            double[] x = new double[b.Length];
            double[] z = new double[b.Length];
            RecalculateLU();
            for(int i = 0; i < Rows; i++)
            {
                z[i] = b[i];
                for (int j = 0; j < i; j++)
                    z[i] -= L[i, j] * z[j];
            }
            for (int i = Rows - 1; i >= 0; i--)
            {
                x[i] = z[i];
                for (int j = i + 1; j < Rows; j++)
                    x[i] -= U[i, j] * x[j];
                x[i] /= U[i, i];
            }
            return x;
        }

        public double Determinant()
        {
            if (Rows != Columns)
                throw new Exception("Can't get determinant from not square matrix");
            RecalculateLU();
            double res = 1;
            for (int i = 0; i < Rows; i++)
                res *= L[i, i] * U[i, i];
            return res;
        }
        public Matrix Inverse()
        {
            if (Rows != Columns)
                throw new Exception("Can't inverse not square matrix");
            Matrix result = new Matrix(Rows, Columns);
            var det = Determinant();
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    result[i, j] =  ((i + j) % 2 == 0 ? (1) : (-1)) * GetMinorSubMatrix(i, j).Determinant()/det; 
            return result.Transpose();
        }
        public void RecalculateLU()
        {
            if (Rows != Columns)
                throw new Exception("Can't get LU from not square matrix");
            L = new Matrix(Rows, Columns);
            U = new Matrix(Rows, Columns);
            for (int j = 0; j < U.Columns; j++)
                U[0, j] = this[0, j];
            for (int j = 0; j < U.Rows; j++)
                L[j, 0] = this[j, 0] / U[0, 0];
            for (int i = 1; i < Rows; i++)
            {
                for (int j = i; j < U.Columns; j++)
                {
                    U[i, j] = this[i, j];
                    for (int k = 0; k < i; k++)
                        U[i, j] -= L[i, k] * U[k, j];
                }

                for (int j = i; j < U.Rows; j++)
                {
                    L[j, i] = this[j, i];
                    for (int k = 0; k < i; k++)
                        L[j, i] -= L[j, k] * U[k, i];
                    L[j, i] /= U[i, i];
                }
            }
        }

        public double Trace()
        {
            if (Rows != Columns)
                throw new Exception("Can't get trace from not square matrix");
            double res = 0;
            for (int i = 0; i < Rows; i++)
                res += this[i, i];
            return res;
        }

        private double[][] matrix;
        public Matrix(string rawString)
        {
            var rows = rawString.Split("\n");
            Rows = rows.Length;
            var columns = rows[0].Split(" ");
            Columns = columns.Length;
            matrix = new double[Rows][];
            for (int i = 0; i < Rows; i++)
                matrix[i] = new double[Columns];
            for (int i = 0; i < Rows; i++)
            {
                columns = rows[i].Split(" ");
                for (int j = 0; j < Columns; j++)
                    matrix[i][j] = double.Parse(columns[j], CultureInfo.InvariantCulture);
            }
        }
        public Matrix(string[] lineString)
        {
            var rows = lineString;
            Rows = rows.Length;
            var columns = rows[0].Split(" ");
            Columns = columns.Length;
            matrix = new double[Rows][];
            for (int i = 0; i < Rows; i++)
                matrix[i] = new double[Columns];
            for (int i = 0; i < Rows; i++)
            {
                columns = rows[i].Split(" ");
                for (int j = 0; j < Columns; j++)
                    matrix[i][j] = double.Parse(columns[j], CultureInfo.InvariantCulture);
            }
        }

        public Matrix GetMinorSubMatrix(int row, int column)
        {
            Matrix result = new Matrix(Rows - 1, Columns - 1);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    if(i < row)
                    {
                        if (j < column)
                            result[i, j] = this[i, j];
                        if (j > column)
                            result[i, j - 1] = this[i, j];
                    }
                    else if(i > row)
                    {
                        if (j < column)
                            result[i - 1, j] = this[i, j];
                        if (j > column)
                            result[i - 1, j - 1] = this[i, j];
                    }
            return result;
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
            Rows = other.Rows;
            Columns = other.Columns;
        }
        public Matrix(double[][] arr)
        {
            matrix = arr;
            Rows = arr.Length;
            Columns = arr[0].Length;
        }

        public Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            matrix = new double[Rows][];
            for (int i = 0; i < Rows; i++)
                matrix[i] = new double[Columns];
        }

        public Matrix Transpose()
        {
            Matrix transposed = new Matrix(Columns, Rows);
            for (int i = 0; i < Columns; i++)
                for (int j = 0; j < Rows; j++)
                    transposed[i, j] = this[j, i];
            return transposed;
        }

        public (int, int) FindMax(Func<int, int, bool> predicate)
        {
            (int, int) maxIndex = (0, 0);
            double maxValue = double.NegativeInfinity;

            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    if (predicate(i, j) && maxValue <  Math.Abs(matrix[i][j]))
                        (maxIndex, maxValue) = ((i, j), Math.Abs(matrix[i][j]));

            return maxIndex;
        }
        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.Columns != b.Rows)
                throw new Exception("Multiplication dimensions incorrect");
            Matrix result = new Matrix(a.Rows, b.Columns);
            for (int i = 0; i < result.Rows; i++)
                for (int j = 0; j < result.Columns; j++)
                    for (int k = 0; k < a.Columns; k++)
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
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                    res += Math.Round( this[i, j], 2).ToString("0.00") + " ";
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
            var matrixArr = new double[txt.Length][];
            var columns = txt[0].Split(" ");
            for (int i = 0; i < matrixArr.Length; i++)
                matrixArr[i] = new double[columns.Length - 1];

            double[] b = new double[matrixArr.Length];

            for(int i = 0; i < txt.Length; i++)
            {
                columns = txt[i].Split(" ");
                for (int j = 0; j < columns.Length; j++)
                    if (j < columns.Length - 1)
                        matrixArr[i][j] = double.Parse(columns[j], CultureInfo.InvariantCulture);
                    else
                        b[i] = double.Parse(columns[j], CultureInfo.InvariantCulture);
            }
            Matrix inMat = new Matrix(matrixArr);
            inMat.RecalculateLU();
            var res = inMat.Solve(b);
            var stringRes = "";
            for (int i = 0; i < res.Length; i++)
                stringRes += Math.Round(res[i], 2).ToString() + " ";
            File.WriteAllText("out.txt", stringRes);
            Console.ReadKey();
        }
    }
}
