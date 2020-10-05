using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.LinearAlgebra
{
	public partial class Matrix
	{
		public double[] Row(int i)
		{
			return (double[])_matrix[i].Clone();
		}
		public double[] Column(int i)
		{
			double[] column = new double[Rows];
			for (int j = 0; j < Rows; j++)
				column[j] = this[j, i];
			return column;
		}

		public double Magnitude()
		{
			double res = 0;
			for (int i = 0; i < Rows; i++)
				for (int j = 0; j < Columns; j++)
					res += this[i, j] * this[i, j];
			return Math.Sqrt(res);
		}

		public static Matrix Identity(int size)
		{
			Matrix matrix = new Matrix(size, size);
			for (int i = 0; i < size; i++)
				matrix[i, i] = 1;
			return matrix;
		}

		public Matrix GetMinorSubMatrix(int row, int column)
		{
			Matrix result = new Matrix(Rows - 1, Columns - 1);
			for (int i = 0; i < Rows; i++)
				for (int j = 0; j < Columns; j++)
					if (i < row)
					{
						if (j < column)
							result[i, j] = this[i, j];
						if (j > column)
							result[i, j - 1] = this[i, j];
					}
					else if (i > row)
					{
						if (j < column)
							result[i - 1, j] = this[i, j];
						if (j > column)
							result[i - 1, j - 1] = this[i, j];
					}
			return result;
		}
		public void Log(string matrixName)
		{
			Console.WriteLine("--------------------");
			Console.WriteLine(matrixName);
			Console.WriteLine(ToString());
		}

		public double Trace()
		{
			if (Rows != Columns)
				throw new Exception("Can't get trace from not square matrix");
			double res = 0;
			for (int i = 0; i < Rows; i++)
				res += this[i, i];
			return res;
		}

		public double Norm()
		{
			double res = 0;
			for (int i = 0; i < Rows; i++)
			{
				double cur = 0;
				for (int j = 0; j < Columns; j++)
					cur += Math.Abs(this[i, j]);
				if (cur > res)
					res = cur;
			}
			return res;
		}

		public Matrix LowTriangal()
		{
			Matrix res = new Matrix(Rows, Columns);
			for (int i = 0; i < Rows; i++)
				for (int j = 0; j < i; j++)
					res[i, j] = this[i, j];
			return res;
		}
		public Matrix UpTriangal()
		{
			Matrix res = new Matrix(Rows, Columns);
			for (int i = 0; i < Rows; i++)
				for (int j = i; j < Columns; j++)
					res[i, j] = this[i, j];
			return res;
		}

		public Matrix Inverse()
		{
			if (Rows != Columns)
				throw new Exception("Can't inverse not square matrix");
			Matrix[] res = new Matrix[Columns];
			RecalculateLU();
			for (int i = 0; i < Columns; i++)
			{
				Matrix curVector = new Matrix(Columns, 1);
				curVector[i] = 1;
				res[i] = Solve(curVector);
			}

			return new Matrix(res);
		}

		public Matrix Transpose()
		{
			Matrix transposed = new Matrix(Columns, Rows);
			for (int i = 0; i < Columns; i++)
				for (int j = 0; j < Rows; j++)
					transposed[i, j] = this[j, i];
			return transposed;
		}

		public (int, int) FindMax(Func<int, int, bool> predicate)
		{
			(int, int) maxIndex = (0, 0);
			double maxValue = double.NegativeInfinity;

			for (int i = 0; i < Rows; i++)
				for (int j = 0; j < Columns; j++)
					if (predicate(i, j) && maxValue < Math.Abs(this[i, j]))
						(maxIndex, maxValue) = ((i, j), Math.Abs(this[i, j]));

			return maxIndex;
		}

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();

			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Columns; j++)
				{
					stringBuilder.Append(Math.Round(this[i, j], 2).ToString("0.00") + " ");
				}

				stringBuilder.Append(System.Environment.NewLine);
			}

			return stringBuilder.ToString();
		}

		public static implicit operator double(Matrix matrix)
		{
			if (matrix.Columns != 1 || matrix.Rows != 1)
				throw new Exception("Can't cast " + matrix.Rows.ToString() + "x" + matrix.Columns.ToString() + " Matrix to a single number");
			return matrix[0, 0];
		}
	}
}
