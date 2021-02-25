using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.LinearAlgebra
{
	public class IterationSolver
	{
		Matrix alpha;
		Matrix betta;
		double eps;
		public Matrix X { get; private set; }
		public IterationSolver(Matrix a, Matrix b, double eps)
		{
			this.eps = eps;
			betta = new Matrix(b);
			for (int i = 0; i < betta.Rows; i++)
				betta[i, 0] = b[i, 0] / a[i, i];
			alpha = new Matrix(a.Rows, a.Columns);
			for (int i = 0; i < a.Rows; i++)
				for (int j = 0; j < a.Columns; j++)
					if (i != j)
						alpha[i, j] = -a[i, j] / a[i, i];
					else
						alpha[i, j] = 0;
			X = new Matrix(betta);
		}

		private double Distance(Matrix a, Matrix b)
		{
			double res = 0;
			for (int i = 0; i < a.Rows; i++)
				for (int j = 0; j < a.Columns; j++)
					res += (a[i, j] - b[i, j]) * (a[i, j] - b[i, j]);
			return Math.Sqrt(res);
		}

		public void Solve()
		{
			while (true)
			{
				var xNext = new Matrix(betta);
				xNext += alpha * X;
				double epsK = Distance(X, xNext);
				if (epsK < eps)
					break;
				X = xNext;
			}
		}

		public void ZeidelSolve()
		{
			var B = alpha.LowTriangal();
			var C = alpha.UpTriangal();
			var firstMat = (Matrix.Identity(alpha.Rows) - B).Inverse() * C;
			var secondMat = (Matrix.Identity(alpha.Rows) - B).Inverse() * betta;
			while (true)
			{
				var xNext = firstMat * X;
				xNext += secondMat;
				double epsK = Distance(X, xNext);
				if (epsK < eps)
					break;
				X = xNext;
			}
		}

		public void OverRelaxation(double w = 1)
		{
			var B = w * alpha.LowTriangal();
			var C = w * alpha.UpTriangal();

			for (int i = 0; i < B.Columns; i++)
			{
				B[i, i] = alpha[i, i];
				C[i, i] = (1 - w) * alpha[i, i];
			}

			var firstMat = (Matrix.Identity(alpha.Rows) - B).Inverse() * C;
			var secondMat = (Matrix.Identity(alpha.Rows) - B).Inverse() * betta * w;
			while (true)
			{
				var xNext = firstMat * X;
				xNext += secondMat;
				double epsK = Distance(X, xNext);
				if (epsK < eps)
					break;
				X = xNext;
			}
		}
	}
}
