using System;
using System.Collections.Generic;
using NumericMethods.Core.Common;

namespace NumericMethods.Core.Numerics
{
	public class Derivative
	{
		public static List<Polynom> FindPolynomByPoints(List<Point> points, int degree)
		{
			var derivatives = new List<Polynom>();

			if (degree == 1)
			{
				for (int i = 0; i < points.Count - 2; i++)
					derivatives.Add(
						new Polynom(new double[] { (points[i + 1].Y - points[i].Y) / (points[i + 1].X - points[i].X) })
						+ new Polynom(new double[] { -points[i].X - points[i + 1].X, 2 }) *
						((points[i + 2].Y - points[i + 1].Y) / (points[i + 2].X - points[i + 1].X) - (points[i + 1].Y - points[i].Y) / (points[i + 1].X - points[i].X)) / (points[i + 2].X - points[i].X));
				derivatives.Add(new Polynom(new double[] { (points[points.Count - 1].Y - points[points.Count - 2].Y) / (points[points.Count - 1].X - points[points.Count - 2].X) }));
			}
			else
			{
				for (int i = 0; i < points.Count - 2; i++)
					derivatives.Add(
						new Polynom(new double[] { 2 * ((points[i + 2].Y - points[i + 1].Y) / (points[i + 2].X - points[i + 1].X) - (points[i + 1].Y - points[i].Y) / (points[i + 1].X - points[i].X)) / (points[i + 2].X - points[i].X) }));
				derivatives.Add(new Polynom(new double[] { 0 }));

			}

			return derivatives;
		}

		public static double[] FindByFunction(Func<double, double> f, int degree, double start, double end, int steps)
		{
			var derivative = new double[steps + 1];
			var h = (end - start) / steps;
			Func<int, double> x = (i) => h * i + start;

			for (int i = 0; i <= steps; i++)
			{
				switch (degree)
				{
					case 1:
						derivative[i] = (f(x(i - 1)) - f(x(i + 1))) / (2 * h);
						break;
					case 2:
						derivative[i] = (f(x(i - 1)) - 2 * f(x(i)) + f(x(i + 1))) / (h * h);
						break;
					default:
						throw new NotImplementedException();
				}
			}

			return derivative;
		}
	}
}