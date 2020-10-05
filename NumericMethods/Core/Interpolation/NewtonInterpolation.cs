using NumericMethods.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.Interpolation
{
	public class NewtonInterpolation
	{
		List<(double, double)> Points;
		public NewtonInterpolation(List<(double, double)> points) => (Points, f) = (points, new double[points.Count * points.Count]);
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

			for (int i = 0; i < n; i++)
			{
				Polynom c = new Polynom(new double[] { f[n * i] });
				for (int j = 0; j < i; j++)
					c *= new Polynom(new double[] { -Points[j].Item1, 1 });
				F += c;
			}

			return F;
		}
	}
}
