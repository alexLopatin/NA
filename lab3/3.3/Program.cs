using System;
using System.IO;
using System.Collections.Generic;

namespace ConsoleApp1
{
    struct Point
    {
        public double X;
        public double Y;
        public Point(double x, double y) => (X, Y) = (x, y);
        public static implicit operator Point((double, double) point) => new Point(point.Item1, point.Item2);
    }
    class LeastSquares
    {
        public List<Point> Points;
        public int Degree;
        public LeastSquares(List<Point> points, int degree) => (Points, Degree) = (points, degree);
        public Polynom Calculate()
        {
            Matrix mat = new Matrix(Degree + 1, Degree + 1);
            double[] b = new double[Degree + 1];
            for (int i = 0; i < b.Length; i++)
                for (int j = 0; j < Points.Count; j++)
                    b[i] += Points[i].Y * Math.Pow(Points[i].X, i);
            for (int i = 0; i < Degree + 1; i++)
                for (int j = 0; j < Degree + 1; j++)
                    for (int k = 0; k < Points.Count; k++)
                        mat[i, j] += Math.Pow(Points[k].X, i + j);
            var a = mat.Calculate(b);
            Polynom res = new Polynom();
            for (int i = 0; i <= Degree; i++)
                res[i] = a[i];
            return res;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<Point> points = new List<Point>();
            points.Add((1d, 3.4142));
            points.Add((1.9, 2.9818));
            points.Add((2.8, 3.3095));
            points.Add((3.7, 3.8184));
            points.Add((4.6, 4.3599));
            points.Add((5.5, 4.8318));

            LeastSquares LS = new LeastSquares(points, 2);
            var polySquare = LS.Calculate();
            Console.ReadKey();
        }
    }
}