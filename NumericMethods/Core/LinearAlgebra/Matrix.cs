using System;
using System.Globalization;

namespace NumericMethods.Core.LinearAlgebra
{
	public partial class Matrix
	{
		public int Rows { get; private set; }
		public int Columns { get; private set; }

		private readonly double[][] _matrix;

		public Matrix(string rawString)
		{
			var rows = rawString.Split("\n");
			Rows = rows.Length;
			var columns = rows[0].Split(" ");
			Columns = columns.Length;
			_matrix = new double[Rows][];

			for (int i = 0; i < Rows; i++)
			{
				_matrix[i] = new double[Columns];
			}

			for (int i = 0; i < Rows; i++)
			{
				columns = rows[i].Split(" ");

				for (int j = 0; j < Columns; j++)
				{
					_matrix[i][j] = double.Parse(columns[j], CultureInfo.InvariantCulture);
				}
			}
		}

		public Matrix(Matrix[] columns)
		{
			Rows = columns[0].Rows;
			Columns = columns.Length;
			_matrix = new double[Rows][];

			for (int i = 0; i < Rows; i++)
				_matrix[i] = new double[Columns];

			for (int i = 0; i < Rows; i++)
				for (int j = 0; j < Columns; j++)
					this[i, j] = columns[j][i];
		}

		public Matrix(string[] lineString)
		{
			var rows = lineString;
			Rows = rows.Length;
			var columns = rows[0].Split(" ");
			Columns = columns.Length;
			_matrix = new double[Rows][];

			for (int i = 0; i < Rows; i++)
			{
				_matrix[i] = new double[Columns];
			}

			for (int i = 0; i < Rows; i++)
			{
				columns = rows[i].Split(" ");
				for (int j = 0; j < Columns; j++)
					_matrix[i][j] = double.Parse(columns[j], CultureInfo.InvariantCulture);
			}
		}
		
		public Matrix(Matrix other)
		{
			Rows = other.Rows;
			Columns = other.Columns;
			_matrix = new double[Rows][];
			for (int i = 0; i < Rows; i++)
				_matrix[i] = new double[Columns];

			for (int i = 0; i < Rows; i++)
				for (int j = 0; j < Columns; j++)
					_matrix[i][j] = other[i, j];

		}

		public Matrix(double[][] arr)
		{
			_matrix = arr;
			Rows = arr.Length;
			Columns = arr[0].Length;
		}

		public Matrix(int rows, int columns)
		{
			Rows = rows;
			Columns = columns;
			_matrix = new double[Rows][];
			for (int i = 0; i < Rows; i++)
				_matrix[i] = new double[Columns];
		}

		public Matrix(double[] vector)
		{
			Rows = vector.Length;
			Columns = 1;
			_matrix = new double[Rows][];
			for (int i = 0; i < Rows; i++)
			{
				_matrix[i] = new double[1];
				_matrix[i][0] = vector[i];
			}
		}

		public double[] ToVectorArray()
		{
			if (Columns == 1)
			{
				double[] column = new double[Rows];
				for (int i = 0; i < Rows; i++)
					column[i] = _matrix[i][0];

				return column;
			}
			else if (Rows == 1)
				return (double[])_matrix[0].Clone();
			else
				throw new Exception("Can't cast matrix to vector array");
		}
	}
}