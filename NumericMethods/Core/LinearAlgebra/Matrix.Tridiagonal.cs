using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.LinearAlgebra
{
	public partial class Matrix
	{


		private double A(int index)
		{
			if (index < Rows && index > 0)
				return this[index, index - 1];
			else
				return 0;
		}

		private double B(int index)
		{
			if (index < Rows)
				return _matrix[index][index];
			else
				return 0;
		}

		private double C(int index)
		{
			if (index < Rows - 1)
				return _matrix[index][index + 1];
			else
				return 0;
		}

		public double[] SolveTridiagonal(double[] D)
		{
			int n = Rows;
			double[] x = new double[n];
			double[] P = new double[n];
			double[] Q = new double[n];
			P[0] = -C(0) / B(0);
			Q[0] = D[0] / B(0);
			for (int i = 1; i < n; i++)
			{
				P[i] = (-C(i)) / (B(i) + A(i) * P[i - 1]);
				Q[i] = (D[i] - A(i) * Q[i - 1]) / (B(i) + A(i) * P[i - 1]);
			}
			x[n - 1] = Q[n - 1];
			for (int i = n - 2; i >= 0; i--)
				x[i] = P[i] * x[i + 1] + Q[i];
			return x;
		}
	}
}
