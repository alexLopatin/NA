using System;
using System.IO;
using System.Collections.Generic;

namespace ConsoleApp1
{
    class Lagrange
    {
        List<(double, double)> Points;
        public Lagrange(List<(double, double)> points) => Points = points;
        public Polynom Calculate()
        {
            Polynom L = new Polynom();
            for (int i = 0; i < Points.Count; i++)
            {
                Polynom l = new Polynom(new double[] { 1 });
                for (int j = 0; j < Points.Count; j++)
                    if(i != j)
                        l *= new Polynom(new double[] { -Points[j].Item1, 1 }) / (Points[i].Item1 - Points[j].Item1);
                l *= Points[i].Item2;
                L += l;
            }
            return L;
        }
    }
    class Newton
    {
        List<(double, double)> Points;
        public Newton(List<(double, double)> points) => (Points, f) = (points, new double[points.Count*points.Count]);
        double[] f;
        public Polynom Calculate()
        {
            int n = Points.Count;
            for (int i = 0; i < n; i++)
                f[i] = Points[i].Item2;
            for (int i = 1; i < n; i++)
                for (int j = 0; j < n - i; j++)
                    f[i * n + j] = (f[(i - 1) * n + j] - f[(i - 1) * n + j + 1]) / (Points[j].Item1 - Points[i + j].Item1);
            Polynom F = new Polynom();
            for(int i = 0; i < n; i++)
            {
                Polynom c = new Polynom(new double[] { f[n * i] });
                for(int j = 0; j < i; j++)
                    c *= new Polynom(new double[] { -Points[j].Item1, 1 });
                F += c;
            }
            return F;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Func<double, double> y = (x) => 1/Math.Tan(x) + x;
            List<(double, double)> points = new List<(double, double)>();
            points.Add((Math.PI / 8, y(Math.PI / 8)));
            points.Add((2 * Math.PI / 8, y(2 * Math.PI / 8)));
            points.Add((3 * Math.PI / 8, y(3 * Math.PI / 8)));
            points.Add((4 * Math.PI / 8, y(4 * Math.PI / 8)));
            Lagrange LAlg = new Lagrange(points);
            var L = LAlg.Calculate();
            var dL = Math.Abs(L.GetValue(3 * Math.PI / 16) - y(3 * Math.PI / 16));
            Newton NAlg = new Newton(points);
            var F = NAlg.Calculate();
            var dF = Math.Abs(F.GetValue(3 * Math.PI / 16) - y(3 * Math.PI / 16));
            Console.WriteLine("Lagrange: {0}; dL = {1}", L.ToString(), dL);
            Console.WriteLine("Newton: {0}; dL = {1}", F.ToString(), dF);
        }
    }
}