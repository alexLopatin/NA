using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.PartialDiffEquation
{
	public class FiniteDifference
	{
		private readonly double[,] _grid;

		private readonly BoundaryConditionsThirdDegree _conditions;
		private readonly FiniteDifferenceParams _params;

		public FiniteDifference(BoundaryConditionsThirdDegree conditions, FiniteDifferenceParams @params)
		{
			_params = @params;
			_conditions = conditions;
			_grid = new double[_params.SpaceStepCount, _params.TimeStepCount];
			InitializeGrid();
		}

		private double GetSpaceCoordinate(int i)
		{
			return (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount * i + _params.SpaceBoundLeft;
		}

		private double GetTimeCoordinate(int i)
		{
			return _params.TimeLimit / _params.TimeStepCount * i;
		}

		private void InitializeGrid()
		{
			for (int i = 0; i < _params.SpaceStepCount; i++)
			{
				var x = GetSpaceCoordinate(i);
				_grid[i, 0] = _conditions.InitialFunc(x, 0);
			}
		}

		public double[,] Solve(double[] coefs, Func<double, double, double> f)
		{
			var a = coefs[0];
			var b = coefs[1];
			var c = coefs[2];

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			double sigma = a * tau / (h * h);

			if (sigma > 0.5d)
			{
				throw new ArgumentException("σ должен быть меньше 0.5");
			}

			var alpha = _conditions.FirstConditionParameters[0];
			var betta = _conditions.FirstConditionParameters[1];

			var gamma = _conditions.SecondConditionParameters[0];
			var delta = _conditions.SecondConditionParameters[1];

			for (int k = 1; k < _params.TimeStepCount; k++)
			{
				for (int j = 1; j < _params.SpaceStepCount - 1; j++)
				{
					_grid[j, k] = sigma * _grid[j + 1, k - 1]
						+ (1 - 2 * sigma) * _grid[j, k - 1]
						+ sigma * _grid[j - 1, k - 1]
						+ b * (_grid[j + 1, k - 1] - _grid[j, k - 1]) / h * tau
						+ c * _grid[j, k - 1] * tau
						+ f(GetSpaceCoordinate(j), GetTimeCoordinate(k - 1)) * tau;
				}

				_grid[0, k] = -(alpha / h) / (betta - alpha / h) * _grid[1, k] +
					_conditions.FirstFunc(0, GetTimeCoordinate(k)) / (betta - alpha / h);
				_grid[_params.SpaceStepCount - 1, k] = (gamma / h) / (delta + gamma / h) * _grid[_params.SpaceStepCount - 2, k] +
					_conditions.SecondFunc(0, GetTimeCoordinate(k)) / (delta + gamma / h);
			}

			return _grid.Clone() as double[,];
		}

		public double[,] FindError(Func<double, double, double> u)
		{
			var errors = _grid.Clone() as double[,];

			for (int k = 0; k < _params.TimeStepCount; k++)
			{
				for (int j = 0; j < _params.SpaceStepCount; j++)
				{
					errors[j, k] = Math.Abs(_grid[j, k] - u(GetSpaceCoordinate(j), GetTimeCoordinate(k)));
				}
			}

			return errors;
		}
	}
}
