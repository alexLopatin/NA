using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace ConsoleApp1
{
    class SolverNewton
    {
        public double X { get; private set; }
        double eps;
        Func<double, double> function;
        Func<double, double> derivative;
        public SolverNewton(Func<double, double> function, Func<double, double> derivative, double eps, double first)
        {
            this.function = function;
            this.derivative = derivative;
            this.eps = eps;
            X = first;
        }
        public void Solve()
        {
            double dist = eps;
            while(dist >= eps)
            {
                double next = X - function(X) / derivative(X);
                dist = Math.Abs(next - X);
                X = next;
            }
        }
    }
    class SolverIterative
    {
        public double X { get; private set; }
        double eps;
        Func<double, double> inverse;
        public SolverIterative(Func<double, double> inverse, double eps, double first)
        {
            this.inverse = inverse;
            this.eps = eps;
            X = first;
        }
        public void Solve()
        {
            double dist = eps;
            while (dist >= eps)
            {
                double next = inverse(X);
                dist = Math.Abs(next - X);
                X = next;
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            double eps = double.Parse(File.ReadAllText("in.txt"));
            Func<double, double> function = (x) => { return Math.Sin(x) - x * x + 1; };
            Func<double, double> derivative = (x) => { return Math.Cos(x) - 2 * x; };
            Func<double, double> inverse = (x) => { return Math.Sqrt(Math.Sin(x) + 1); };
            SolverNewton solverNewton = new SolverNewton(function, derivative, eps, 0);
            solverNewton.Solve();
            SolverIterative solverIterative = new SolverIterative(inverse, eps, 0);
            solverIterative.Solve();
            File.WriteAllText("out.txt", solverNewton.X + "\n" + solverIterative.X);
            Console.ReadKey();
        }
    }
}
