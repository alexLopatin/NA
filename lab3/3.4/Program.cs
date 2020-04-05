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
    class Derivative
    {
        public List<Point> Points;
        public Derivative(List<Point> points) => Points = points;
        public List<Polynom> Find(int degree)
        {
            List<Polynom> derivatives = new List<Polynom>();
            if(degree == 1)
            {
                for (int i = 0; i < Points.Count - 2; i++)
                    derivatives.Add(
                        new Polynom(new double[] { (Points[i + 1].Y - Points[i].Y) / (Points[i + 1].X - Points[i].X) })
                        + new Polynom(new double[] { -Points[i].X - Points[i + 1].X, 2 }) *
                        ((Points[i + 2].Y - Points[i + 1].Y) / (Points[i + 2].X - Points[i + 1].X) - (Points[i + 1].Y - Points[i].Y) / (Points[i + 1].X - Points[i].X)) / (Points[i + 2].X - Points[i].X));
                derivatives.Add(new Polynom(new double[] { (Points[Points.Count - 1].Y - Points[Points.Count - 2].Y) / (Points[Points.Count - 1].X - Points[Points.Count - 2].X) }));
            }
            else
            {
                for (int i = 0; i < Points.Count - 2; i++)
                    derivatives.Add(
                        new Polynom(new double[] { 2*((Points[i + 2].Y - Points[i + 1].Y) / (Points[i + 2].X - Points[i + 1].X) - (Points[i + 1].Y - Points[i].Y) / (Points[i + 1].X - Points[i].X)) / (Points[i + 2].X - Points[i].X) }));
                derivatives.Add(new Polynom(new double[] { 0 }));

            }
            return derivatives;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<Point> points = new List<Point>();
            
            points.Add((0, 1));
            points.Add((0.1, 1.1052));
            points.Add((0.2, 1.2214));
            points.Add((0.3, 1.3499));
            points.Add((0.4, 1.4918));

            Derivative LS = new Derivative(points);
            var polySquare = LS.Find(2);
            Console.ReadKey();
        }
    }
}