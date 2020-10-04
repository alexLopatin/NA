using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.LinearAlgebra
{
	public partial class Matrix
	{
		private Matrix L;
		private Matrix U;

		public double[] Solve(double[] b)
		{
			double[] x = new double[b.Length];
			double[] z = new double[b.Length];
			RecalculateLU();
			for (int i = 0; i < Rows; i++)
			{
				z[i] = b[i];
				for (int j = 0; j < i; j++)
					z[i] -= L[i, j] * z[j];
			}
			for (int i = Rows - 1; i >= 0; i--)
			{
				x[i] = z[i];
				for (int j = i + 1; j < Rows; j++)
					x[i] -= U[i, j] * x[j];
				x[i] /= U[i, i];
			}
			return x;
		}

		public Matrix Solve(Matrix b)
		{
			Matrix x = new Matrix(b.Rows, 1);
			Matrix z = new Matrix(b.Rows, 1);
			for (int i = 0; i < Rows; i++)
			{
				z[i] = b[i];
				for (int j = 0; j < i; j++)
					z[i] -= L[i, j] * z[j];
			}
			for (int i = Rows - 1; i >= 0; i--)
			{
				x[i] = z[i];
				for (int j = i + 1; j < Rows; j++)
					x[i] -= U[i, j] * x[j];
				x[i] /= U[i, i];
			}
			return x;
		}

		public double Determinant()
		{
			if (Rows != Columns)
				throw new Exception("Can't get determinant from not square matrix");

			RecalculateLU();
			double res = 1;
			for (int i = 0; i < Rows; i++)
				res *= L[i, i] * U[i, i];
			return res;
		}

		private void RecalculateLU()
		{
			if (Rows != Columns)
				throw new Exception("Can't get LU from not square matrix");
			L = new Matrix(Rows, Columns);
			U = new Matrix(Rows, Columns);
			for (int j = 0; j < U.Columns; j++)
				U[0, j] = this[0, j];
			for (int j = 0; j < U.Rows; j++)
				if (U[0, 0] != 0)
					L[j, 0] = this[j, 0] / U[0, 0];
				else
					L[j, 0] = 0;

			for (int i = 1; i < Rows; i++)
			{
				for (int j = i; j < U.Columns; j++)
				{
					U[i, j] = this[i, j];
					for (int k = 0; k < i; k++)
						U[i, j] -= L[i, k] * U[k, j];
				}

				for (int j = i; j < U.Rows; j++)
				{
					L[j, i] = this[j, i];
					for (int k = 0; k < i; k++)
						L[j, i] -= L[j, k] * U[k, i];
					if (U[i, i] != 0)
						L[j, i] /= U[i, i];
					else
						L[j, i] = 0;
				}
			}
		}
	}
}
