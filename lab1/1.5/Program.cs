using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace CMLab1
{
    class QR
    {
        Matrix A;
        double eps;
        public QR(Matrix a, double eps)
        {
            A = a;
            this.eps = eps;
            isComplex = new bool[a.Rows];
            complices = new Complex[a.Rows];
        }
        bool[] isComplex;
        Complex[] complices;
        public bool Check()
        {
            double res = 0;
            for (int i = 0; i < A.Rows - 1; i++)
                isComplex[i] = (Math.Abs(A[i + 1, i]) > eps);
            bool isAllGoodForComplices = false;
            //проверяем мнимые
            for (int i = 0; i < A.Rows; i++)
                if (isComplex[i])
                {
                    double a = A[i, i];
                    double b = A[i, i + 1];
                    double c = A[i + 1, i + 1];
                    double d = A[i + 1, i];
                    Complex lambda = new Complex((a + c) / 2, 0);
                    lambda += Complex.Sqrt((a + c) * (a + c) / 4 - a * c + b * d);
                    if (lambda.Imaginary < eps)
                        isComplex[i] = false;
                    else
                        isAllGoodForComplices = isAllGoodForComplices || Complex.Abs(lambda - complices[i]) > eps;
                    complices[i] = lambda;
                }
            //проверяем вещественные корни
            for (int i = 0; i < A.Rows; i++)
                for (int j = 0; j < i; j++)
                    if(!isComplex[j] || i != j + 1)
                        res += A[i, j] * A[i, j];
            res = Math.Sqrt(res);
            return (res > eps) || isAllGoodForComplices;
        }
        public Complex[] Find()
        {
            A.Log("A");
            while (Check())
            {
                Matrix Q, R;
                (Q, R) = FindQR(A);
                A = R * Q;
                A.Log("A");
            }
            Complex[] res = new Complex[A.Rows];
            for (int i = 0; i < A.Rows; i++)
                if(isComplex[i])
                {
                    res[i] = complices[i];
                    res[i + 1] = Complex.Conjugate(complices[i]);
                    i++;
                }
                else
                    res[i] = A[i, i];
            return res;
        }
        private (Matrix, Matrix) FindQR(Matrix mat)
        {
            var copyMat = new Matrix(mat);
            var Q = Matrix.Identity(mat.Columns);
            for(int i = 0; i < mat.Columns - 1; i++)
            {
                Matrix H = Matrix.Identity(mat.Columns);
                Matrix v = new Matrix(mat.Rows, 1);
                for (int j = i; j < mat.Rows; j++)
                    v[j] = copyMat[j, i];
                double vi = 0;
                for (int j = i; j < mat.Rows; j++)
                    vi += copyMat[j, i] * copyMat[j, i];
                vi = Math.Sign(copyMat[i, i]) *Math.Sqrt(vi);
                v[i] += vi;
                H -= 2 * (v * v.Transpose()) / (double)(v.Transpose() * v);
                copyMat = H * copyMat;
                Q *= H;
            }
            return (Q, copyMat);
        }
    }
    class Matrix
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public Matrix L { get; private set; }
        public Matrix U { get; private set; }
        public double Trace()
        {
            if (Rows != Columns)
                throw new Exception("Can't get trace from not square matrix");
            double res = 0;
            for (int i = 0; i < Rows; i++)
                res += this[i, i];
            return res;
        }
        public Matrix LowTriangal()
        {
            Matrix res = new Matrix(Rows, Columns);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < i; j++)
                    res[i, j] = this[i, j];
            return res;
        }
        public Matrix UpTriangal()
        {
            Matrix res = new Matrix(Rows, Columns);
            for (int i = 0; i < Rows; i++)
                for (int j = i; j < Columns; j++)
                    res[i, j] = this[i, j];
            return res;
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
                    result[i, j] = ((i + j) % 2 == 0 ? (1) : (-1)) * GetMinorSubMatrix(i, j).Determinant() / det;
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
                if(U[0, 0] != 0)
                    L[j, 0] = this[j, 0] / U[0, 0];
                else
                    L[j, 0] = 0;
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
                    if (U[i, i] != 0)
                        L[j, i] /= U[i, i];
                    else
                        L[j, i] = 0;
                }
            }
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
            Rows = other.Rows;
            Columns = other.Columns;
            matrix = new double[Rows][];
            for (int i = 0; i < Rows; i++)
                matrix[i] = new double[Columns];

            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Columns; j++)
                    matrix[i][j] = other[i, j];
            
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
        public Matrix(double[] vector)
        {
            Rows = vector.Length;
            Columns = 1;
            matrix = new double[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                matrix[i] = new double[1];
                matrix[i][0] = vector[i];
            }
                
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
        public static Matrix operator *(double a, Matrix b)
        {
            Matrix result = new Matrix(b);
            for (int i = 0; i < b.Rows; i++)
                for (int j = 0; j < b.Columns; j++)
                    result[i, j] *= a;
            return result;
        }
        public static Matrix operator *(Matrix b, double a)
        {
            Matrix result = new Matrix(b);
            for (int i = 0; i < b.Rows; i++)
                for (int j = 0; j < b.Columns; j++)
                    result[i, j] *= a;
            return result;
        }
        public static Matrix operator /(double a, Matrix b)
        {
            Matrix result = new Matrix(b);
            for (int i = 0; i < b.Rows; i++)
                for (int j = 0; j < b.Columns; j++)
                    result[i, j] /= a;
            return result;
        }
        public static Matrix operator /(Matrix b, double a)
        {
            Matrix result = new Matrix(b);
            for (int i = 0; i < b.Rows; i++)
                for (int j = 0; j < b.Columns; j++)
                    result[i, j] /= a;
            return result;
        }
        public static Matrix operator +(Matrix a, Matrix b)
        {
            if (a.Columns != b.Columns || a.Rows != b.Rows)
                throw new Exception("Sum dimensions incorrect");
            Matrix result = new Matrix(a);
            for (int i = 0; i < a.Rows; i++)
                for (int j = 0; j < a.Columns; j++)
                    result[i, j] += b[i, j];
            return result;
        }
        public static Matrix operator -(Matrix a, Matrix b)
        {
            if (a.Columns != b.Columns || a.Rows != b.Rows)
                throw new Exception("Sum dimensions incorrect");
            Matrix result = new Matrix(a);
            for (int i = 0; i < a.Rows; i++)
                for (int j = 0; j < a.Columns; j++)
                    result[i, j] -= b[i, j];
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
        public static implicit operator double(Matrix matrix)
        {
            if (matrix.Columns != 1 || matrix.Rows != 1)
                throw new Exception("Can't cast " + matrix.Rows.ToString() + "x" + matrix.Columns.ToString() + " Matrix to a single number");
            return matrix[0, 0];
        }
        public double this[int i, int j]
        {
            get { return matrix[i][j]; }
            set { matrix[i][j] = value; }
        }
        public double this[int i]
        {
            get 
            {
                if (Columns == 1)
                    return matrix[i][0];
                else if (Rows == 1)
                    return matrix[0][i];
                else
                    throw new Exception("For matrices use [i, j] indexer instead of [i] for vectors");
            }
            set {
                if (Columns == 1)
                    matrix[i][0] = value;
                else if (Rows == 1)
                    matrix[0][i] = value;
                else
                    throw new Exception("For matrices use [i, j] indexer instead of [i] for vectors");
            }
        }
    }
    class Program
    {
        static string ComplexToString(Complex complex)
        {
            string resString = "";
            if (complex.Imaginary != 0)
            {
                if(complex.Imaginary > 0)
                    resString += String.Format("{0} + {1}i", complex.Real.ToString("0.00"), complex.Imaginary.ToString("0.00")) + "\n";
                else
                    resString += String.Format("{0} - {1}i", complex.Real.ToString("0.00"), (Math.Abs(complex.Imaginary)).ToString("0.00")) + "\n";
            }
            else
                resString += complex.Real.ToString("0.00") + "\n";
            return resString;
        }
        static void Main(string[] args)
        {
            var txt = File.ReadAllLines("in.txt");
            var matrixArr = new double[txt.Length - 1][];
            var columns = txt[1].Split(" ");
            for (int i = 0; i < matrixArr.Length; i++)
                matrixArr[i] = new double[columns.Length];
            double eps = double.Parse(txt[0], CultureInfo.InvariantCulture);
            for (int i = 1; i < txt.Length; i++)
            {
                columns = txt[i].Split(" ");
                for (int j = 0; j < columns.Length; j++)
                        matrixArr[i - 1][j] = double.Parse(columns[j], CultureInfo.InvariantCulture);
            }
            Matrix inMat = new Matrix(matrixArr);
            QR qr = new QR(inMat, eps);
            var res = qr.Find();
            string resString = "";
            for (int i = 0; i < res.Length; i++)
                resString += ComplexToString(res[i]);
            File.WriteAllText("out.txt", resString);
            Console.ReadKey();
        }
    }
}
