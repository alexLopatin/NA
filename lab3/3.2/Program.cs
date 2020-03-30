using System;
using System.IO;
using System.Collections.Generic;

namespace ConsoleApp1
{
    class CubicSpline
    {
        List<(double, double)> Points;
        public CubicSpline(List<(double, double)> points) => Points = points;
        public List<Polynom> Calculate()
        {
            Matrix A = new Matrix(Points.Count, Points.Count);
            for (int i = 0; i < A.Rows; i++)
                A[i, i] = 2;
            for (int i = 2; i < A.Rows; i++)
                A[i - 1, i] = (Points[i].Item1 - Points[i-1].Item1) / (Points[i].Item1 - Points[i-2].Item1);
            for (int i = 0; i < A.Rows - 2; i++)
                A[i + 1, i] = (Points[i + 1].Item1 - Points[i].Item1) / (Points[i + 2].Item1 - Points[i].Item1);
            A[0, 0] = 1;
            A[A.Rows - 1, A.Rows - 1] = 1;
            A.Log("A");

            double[] b = new double[Points.Count];
            for (int i = 1; i < Points.Count - 1; i++)
                b[i] = 6*((Points[i + 1].Item2 - Points[i].Item2) /(Points[i + 1].Item1 - Points[i].Item1) -
                    (Points[i].Item2 - Points[i - 1].Item2) / (Points[i].Item1 - Points[i - 1].Item1))
                    / (Points[i + 1].Item1 - Points[i - 1].Item1);

            var M = A.Calculate(b);
            List<Polynom> res = new List<Polynom>();

            for(int i = 1; i < Points.Count; i++)
            {
                double h = Points[i].Item1 - Points[i - 1].Item1;
                Polynom p = new Polynom(new double[] { Points[i].Item1, -1 });
                p *= new Polynom(new double[] { Points[i].Item1, -1 }) * new Polynom(new double[] { Points[i].Item1, -1 });
                p *= (M[i - 1] / (6 * h));

                Polynom p1 = new Polynom(new double[] { -Points[i  - 1].Item1, 1 });
                p1  *= new Polynom(new double[] { -Points[i - 1].Item1, 1 }) * new Polynom(new double[] { -Points[i - 1].Item1, 1 });
                p += p1 * (M[i] / (6 * h));

                Polynom p2 = new Polynom(new double[] { Points[i].Item1, -1 }) * (Points[i - 1].Item2 / h - (M[i - 1] * h)/6);
                p += p2;

                Polynom p3 = new Polynom(new double[] { -Points[i - 1].Item1, 1 }) * (Points[i].Item2 / h - (M[i] * h) / 6);
                p += p3;
                res.Add(p);
            }
            return res;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<(double, double)> points = new List<(double, double)>();
            points.Add((1, 2.8069));
            points.Add((1.9, 1.8279));
            points.Add((2.8, 1.6091));
            points.Add((3.7, 1.5713));
            CubicSpline cubicSpline = new CubicSpline(points);
            var spline = cubicSpline.Calculate();
            double val = spline[1].GetValue(2.66666667);
            for(int i = 0; i < spline.Count; i++)
            {
                Console.WriteLine("Errors {0} spline:", i + 1);
                Console.WriteLine(points[i].Item2 - spline[i].GetValue(points[i].Item1));
                Console.WriteLine(points[i + 1].Item2 - spline[i].GetValue(points[i + 1].Item1));
            }
            Console.ReadKey();
        }
    }
}