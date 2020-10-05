using NumericMethods.Core.Common;
using NumericMethods.Core.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.Interpolation
{
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
					b[i] += Points[j].Y * Math.Pow(Points[j].X, i);
			for (int i = 0; i < Degree + 1; i++)
				for (int j = 0; j < Degree + 1; j++)
					for (int k = 0; k < Points.Count; k++)
						mat[i, j] += Math.Pow(Points[k].X, i + j);
			var a = mat.Solve(b);
			Polynom res = new Polynom();
			for (int i = 0; i <= Degree; i++)
				res[i] = a[i];
			return res;
		}
	}
}
