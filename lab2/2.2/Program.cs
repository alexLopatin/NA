using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace ConsoleApp1
{
    class SolverNewton
    {
        public Matrix X { get; private set; }
        double eps;
        public FunctionMatrix functions;
        FunctionMatrix jacobi;
        public SolverNewton(List<Func<double[], double>> functions, List<Func<double[], double>> derivatives, double eps, Matrix first)
        {
            this.functions = new FunctionMatrix(functions, functions.Count, 1);
            jacobi = new FunctionMatrix(derivatives, (int)Math.Sqrt(derivatives.Count), (int)Math.Sqrt(derivatives.Count));
            this.eps = eps;
            X = first;
        }
        public void Solve()
        {
            double dist = eps;
            while(dist >= eps)
            {
                var vectorX = X.ToVectorArray();
                Matrix next = X - jacobi.GetMatrix(vectorX).Inverse() * functions.GetMatrix(vectorX);
                dist = (next - X).Magnitude();
                X = next;
            }
        }
    }
    class FunctionMatrix
    {
        Matrix mat;
        List<Func<double[], double>> functions;
        public Matrix GetMatrix(double[] arguments)
        {
            for (int i = 0; i < mat.Rows; i++)
                for (int j = 0; j < mat.Columns; j++)
                    mat[i, j] = functions[i * mat.Columns + j](arguments);
            return mat;
        }
        public FunctionMatrix(List<Func <double[], double>> functions, int rows, int columns)
        {
            mat = new Matrix(rows, (int)Math.Sqrt(functions.Count));
            this.functions = functions;
        }
    }
    class SolverIterative
    {
        public Matrix X { get; private set; }
        double eps;
        FunctionMatrix inverse;
        public SolverIterative(List<Func<double[], double>> inverse, double eps, Matrix first)
        {
            this.inverse = new FunctionMatrix(inverse, inverse.Count, 1);
            this.eps = eps;
            X = first;
        }
        public void Solve()
        {
            double dist = eps;
            while (dist >= eps)
            {
                var vectorX = X.ToVectorArray();
                var next = inverse.GetMatrix(vectorX);
                dist = (next - X).Magnitude();
                X = next;
            }

        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //initialization
            double eps = double.Parse(File.ReadAllText("in.txt"), CultureInfo.InvariantCulture);
            List<Func<double[], double>> functions = new List<Func<double[], double>>();
            functions.Add((x) => { return x[0] * x[0]/ 16 + x[1]*x[1]/4 - 1; });
            functions.Add((x) => { return 4 * x[1] - Math.Pow(Math.E, x[0]) - x[0]; });
            List<Func<double[], double>> derivatives = new List<Func<double[], double>>();
            derivatives.Add((x) => { return x[0] / 8; });
            derivatives.Add((x) => { return x[1] / 2; });
            derivatives.Add((x) => { return -Math.Pow(Math.E, x[0]) - 1; });
            derivatives.Add((x) => { return 4; });
            List<Func<double[], double>> inverseFunctions = new List<Func<double[], double>>();
            inverseFunctions.Add((x) => { return 0.5*Math.Sqrt(16 - x[1] * x[1]); });
            inverseFunctions.Add((x) => { return -Math.Pow(Math.E, x[0]) + x[1] * 4; });
            Matrix first = new Matrix(new double[] { 1, 1 });


            SolverNewton solverNewton = new SolverNewton(functions, derivatives, eps, first);
            solverNewton.Solve();
            SolverIterative solverIterative = new SolverIterative(inverseFunctions, eps, first);
            solverIterative.Solve();
            solverNewton.functions.GetMatrix(solverIterative.X.ToVectorArray()).Log("diff");
            File.WriteAllText("out.txt", solverNewton.X.ToString());
            Console.ReadKey();
        }
    }
}
