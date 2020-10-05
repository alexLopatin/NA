using NumericMethods.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.Interpolation
{
	public class LagrangeInterpolation
	{
		List<(double, double)> Points;

		public LagrangeInterpolation(List<(double, double)> points) => Points = points;

		public Polynom Calculate()
		{
			Polynom L = new Polynom();

			for (int i = 0; i < Points.Count; i++)
			{
				Polynom l = new Polynom(new double[] { 1 });

				for (int j = 0; j < Points.Count; j++)
					if (i != j)
						l *= new Polynom(new double[] { -Points[j].Item1, 1 }) / (Points[i].Item1 - Points[j].Item1);

				l *= Points[i].Item2;
				L += l;
			}

			return L;
		}
	}
}
