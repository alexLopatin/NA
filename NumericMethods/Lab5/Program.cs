using System;
using System.Collections.Generic;
using System.Linq;
using NumericMethods.Core;

namespace Lab5
{
	class FiniteDifference
	{
		private readonly double[,] _grid;

		private readonly int _timeStepCount;
		private readonly int _spaceStepCount;

		private BoundaryConditionsFirstDegree _conditions;

		public FiniteDifference(BoundaryConditionsFirstDegree conditions, int timeStepCount, int spaceStepCount)
		{
			_timeStepCount = timeStepCount;
			_spaceStepCount = spaceStepCount;
			_grid = new double[spaceStepCount, timeStepCount];
			InitializeGrid();
		}

		private void InitializeGrid()
		{
			for (int i = 0; i < _timeStepCount; i++)
			{
				_grid[0, i] = _conditions.LeftCondition[i];
				_grid[_spaceStepCount - 1, i] = _conditions.RightCondition[i];
			}

			for (int i = 0; i < _spaceStepCount; i++)
			{
				_grid[i, 0] = _conditions.LowerCondition[i];
			}
		}

		public double[,] Solve(double a)
		{
			double sigma = a * a * _timeStepCount / (_spaceStepCount * _spaceStepCount);

			for(int k = 1; k < _timeStepCount; k++)
			{
				for(int j = 1; j < _spaceStepCount - 1; j++)
				{
					_grid[j, k] = sigma * _grid[j + 1, k - 1] + (1 - 2 * sigma) * _grid[j, k - 1] + sigma * _grid[j - 1, k - 1];
				}
			}

			return _grid.Clone() as double[,];
		}
	}

	class BoundaryConditionsFirstDegree
	{
		public IReadOnlyList<double> LeftCondition { get; set; }
		public IReadOnlyList<double> RightCondition { get; set; }
		public IReadOnlyList<double> LowerCondition { get; set; }
	}

	static class ErrorCalculator
	{
		public static double[,] FindGridError(double[,] grid, double timeStepCount, double spaceStepCount, double left, double right, double time, Func<double, double, double> exactSolution)
		{
			var errors = grid.Clone() as double[,];

			for (int i = 0; i < spaceStepCount; i++)
			{
				for (int j = 0; j < timeStepCount; j++)
				{
					var x = (right - left) / spaceStepCount * i + left;
					var t = time / timeStepCount * j;
					errors[i, j] = Math.Abs(grid[i, j] - exactSolution(x, t));
				}
			}

			return errors;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var timeStepCount = 10;
			var spaceStepCount = 50;

			var left = 0.0d;
			var right = Math.PI;

			var time = 10d;

			var conditions = new BoundaryConditionsFirstDegree()
			{
				LeftCondition = Enumerable.Range(0, timeStepCount)
					.Select(x => Math.Exp(-0.5d * time / timeStepCount * x))
					.ToList(),
				RightCondition = Enumerable.Range(0, timeStepCount)
					.Select(x => - Math.Exp(-0.5d * time / timeStepCount * x))
					.ToList(),
				LowerCondition = Enumerable.Range(0, spaceStepCount)
					.Select(x => Math.Sin((right - left) / spaceStepCount * x + left))
					.ToList()
			};
			var finiteDiff = new FiniteDifference(conditions, timeStepCount, spaceStepCount);

			var result = finiteDiff.Solve(1.0d);
			var errors = ErrorCalculator.FindGridError(result, timeStepCount, spaceStepCount, left, right, time, (x, t) => Math.Exp(-0.5 * t) * Math.Sin(x));

			Console.WriteLine("Hello World!");
		}
	}
}
