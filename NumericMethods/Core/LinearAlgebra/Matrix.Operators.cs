using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.LinearAlgebra
{
	public partial class Matrix
	{
		public static Matrix operator *(Matrix a, Matrix b)
		{
			if (a.Columns != b.Rows)
				throw new Exception("Multiplication dimensions incorrect");
			Matrix result = new Matrix(a.Rows, b.Columns);
			for (int i = 0; i < result.Rows; i++)
				for (int j = 0; j < result.Columns; j++)
					for (int k = 0; k < a.Columns; k++)
						result[i, j] += a[i, k] * b[k, j];
			return result;
		}

		public static Matrix operator *(double a, Matrix b)
		{
			Matrix result = new Matrix(b);
			for (int i = 0; i < b.Rows; i++)
				for (int j = 0; j < b.Columns; j++)
					result[i, j] *= a;
			return result;
		}

		public static Matrix operator *(Matrix b, double a)
		{
			Matrix result = new Matrix(b);
			for (int i = 0; i < b.Rows; i++)
				for (int j = 0; j < b.Columns; j++)
					result[i, j] *= a;
			return result;
		}

		public static Matrix operator /(double a, Matrix b)
		{
			Matrix result = new Matrix(b);
			for (int i = 0; i < b.Rows; i++)
				for (int j = 0; j < b.Columns; j++)
					result[i, j] /= a;
			return result;
		}

		public static Matrix operator /(Matrix b, double a)
		{
			Matrix result = new Matrix(b);
			for (int i = 0; i < b.Rows; i++)
				for (int j = 0; j < b.Columns; j++)
					result[i, j] /= a;
			return result;
		}

		public static Matrix operator +(Matrix a, Matrix b)
		{
			if (a.Columns != b.Columns || a.Rows != b.Rows)
				throw new Exception("Sum dimensions incorrect");
			Matrix result = new Matrix(a);
			for (int i = 0; i < a.Rows; i++)
				for (int j = 0; j < a.Columns; j++)
					result[i, j] += b[i, j];
			return result;
		}

		public static Matrix operator -(Matrix a, Matrix b)
		{
			if (a.Columns != b.Columns || a.Rows != b.Rows)
				throw new Exception("Sum dimensions incorrect");
			Matrix result = new Matrix(a);
			for (int i = 0; i < a.Rows; i++)
				for (int j = 0; j < a.Columns; j++)
					result[i, j] -= b[i, j];
			return result;
		}

		public double this[int i, int j]
		{
			get { return _matrix[i][j]; }
			set { _matrix[i][j] = value; }
		}

		public double this[int i]
		{
			get
			{
				if (Columns == 1)
					return _matrix[i][0];
				else if (Rows == 1)
					return _matrix[0][i];
				else
					throw new Exception("For matrices use [i, j] indexer instead of [i] for vectors");
			}
			set
			{
				if (Columns == 1)
					_matrix[i][0] = value;
				else if (Rows == 1)
					_matrix[0][i] = value;
				else
					throw new Exception("For matrices use [i, j] indexer instead of [i] for vectors");
			}
		}
	}
}
