using NumericMethods.Core.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NumericMethods.Core.PartialDiffEquation.Parabolic
{
	public class ParabolicImplicitFiniteDifference
	{
		private readonly double[,] _grid;

		private readonly ParabolicBoundaryConditions _conditions;
		private readonly FiniteDifferenceParams _params;

		public ParabolicImplicitFiniteDifference(ParabolicBoundaryConditions conditions, FiniteDifferenceParams @params)
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
				_grid[i, 0] = _conditions.InitialCondition(x, 0);
			}
		}

		public double[,] Solve(double[] coefs, Func<double, double, double> f)
		{
			var a = coefs[0];
			var b = coefs[1];
			var c = coefs[2];

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			var matrix = new Matrix(_params.SpaceStepCount, _params.SpaceStepCount);

			for (int i = 0; i < _params.SpaceStepCount - 1; i++)
			{
				matrix[i, i] = 2 * a / (h * h) + 1 / tau - c;
				matrix[i, i + 1] = -(a / (h * h) + b / (2 * h));
				matrix[i + 1, i] = - (a / (h * h) - b / (2* h));
			}

			for (int k = 1; k < _params.TimeStepCount; k++)
			{
				var d = SetBorderGrid(k, matrix, coefs, f);

				var newLayer = matrix.SolveTridiagonal(d);

				for(int i = 0; i < _params.SpaceStepCount; i++)
				{
					_grid[i, k] = newLayer[i];
				}
			}

			return _grid.Clone() as double[,];
		}

		private double[] SetBorderGrid(int k, Matrix matrix, double[] coefs, Func<double, double, double> f)
		{
			var a = coefs[0];
			var b = coefs[1];
			var c = coefs[2];

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			var alpha = _conditions.FirstConditionParameters[0];
			var betta = _conditions.FirstConditionParameters[1];

			var gamma = _conditions.SecondConditionParameters[0];
			var delta = _conditions.SecondConditionParameters[1];

			var N = _params.SpaceStepCount - 1;

			var d = Enumerable
					.Range(0, _params.SpaceStepCount)
					.Select(i => _grid[i, k - 1] / tau + f(GetSpaceCoordinate(i), GetTimeCoordinate(k)))
					.ToArray();

			d[0] = Math.Abs(alpha) > 1E-3
				? h / tau * _grid[0, k - 1]
					+ -_conditions.FirstCondition(0, GetTimeCoordinate(k)) * (2 * a - b * h) / alpha
				: _conditions.FirstCondition(0, GetTimeCoordinate(k)) / betta;
			d[^1] = Math.Abs(gamma) > 1E-3
				? h / tau * _grid[N, k - 1]
					+ _conditions.SecondCondition(0, GetTimeCoordinate(k)) * (2 * a + b * h) / gamma
				: _conditions.SecondCondition(0, GetTimeCoordinate(k)) / delta;

			matrix[0, 0] = Math.Abs(alpha) > 1E-3
				? 2 * a / h + h / tau - c * h - betta / alpha * (2 * a - b * h)
				: 1;
			matrix[0, 1] = Math.Abs(alpha) > 1E-3
				? -2 * a / h
				: 0;

			matrix[N, N - 1] = Math.Abs(gamma) > 1E-3
				? -2 * a / h
				: 0;
			matrix[N, N] = Math.Abs(gamma) > 1E-3
				? 2 * a / h + h / tau - c * h + delta / gamma * (2 * a + b * h)
				: 1;

			return d;
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
