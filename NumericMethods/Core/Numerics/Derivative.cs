using System;
using System.IO;
using System.Collections.Generic;
using NumericMethods.Core.Common;

namespace NumericMethods.Core.Numerics
{
	public class Derivative
	{
		private List<Point> Points;
		public Derivative(List<Point> points) => Points = points;
		public List<Polynom> Find(int degree)
		{
			List<Polynom> derivatives = new List<Polynom>();
			if (degree == 1)
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
						new Polynom(new double[] { 2 * ((Points[i + 2].Y - Points[i + 1].Y) / (Points[i + 2].X - Points[i + 1].X) - (Points[i + 1].Y - Points[i].Y) / (Points[i + 1].X - Points[i].X)) / (Points[i + 2].X - Points[i].X) }));
				derivatives.Add(new Polynom(new double[] { 0 }));

			}
			return derivatives;
		}
	}
}