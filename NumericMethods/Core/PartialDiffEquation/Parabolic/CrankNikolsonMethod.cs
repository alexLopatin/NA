using NumericMethods.Core.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NumericMethods.Core.PartialDiffEquation.Parabolic
{
	public class CrankNikolsonMethod
	{
		private readonly ParabolicBoundaryConditions _conditions;
		private readonly FiniteDifferenceParams _params;

		private readonly double _weight;

		private double[,] _grid;

		public CrankNikolsonMethod(
			ParabolicBoundaryConditions conditions,
			FiniteDifferenceParams @params,
			double weight = 0.5d)
		{
			_conditions = conditions;
			_params = @params;
			_weight = weight;

			_grid = new double[_params.SpaceStepCount, _params.TimeStepCount];
			InitializeGrid();
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
				matrix[i, i] = (2 * a / (h * h) - c) * _weight + 1 / tau;
				matrix[i, i + 1] = -(a / (h * h) + b / (2 * h)) * _weight;
				matrix[i + 1, i] = -(a / (h * h) - b / (2 * h)) * _weight;
			}

			for (int k = 1; k < _params.TimeStepCount; k++)
			{
				var d = SetBorderGrid(k, matrix, coefs, f);

				var newLayer = matrix.SolveTridiagonal(d);

				for (int i = 0; i < _params.SpaceStepCount; i++)
				{
					_grid[i, k] = newLayer[i];
				}
			}

			return _grid.Clone() as double[,];
		}

		private double[] SetBorderGrid(int k, Matrix matrix, double[] coefs, Func<double, double, double> f)
		{
			switch (_params.ApproximationType)
			{
				case BoundaryApproximationType.FirstDegreeTwoPoints:
					return ApproximateFirstDegreeTwoPoints(k, matrix, coefs, f);
				case BoundaryApproximationType.SecondDegreeTwoPoints:
					return ApproximateSecondDegreeTwoPoints(k, matrix, coefs, f);
				case BoundaryApproximationType.SecondDegreeThreePoints:
					return ApproximateSecondDegreeTwoPoints(k, matrix, coefs, f);
			}

			throw new NotImplementedException();
		}

		private double[] ApproximateFirstDegreeTwoPoints(int k, Matrix matrix, double[] coefs, Func<double, double, double> f)
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

			var d = new double[_params.SpaceStepCount];

			for (int j = 1; j < N; j++)
			{
				d[j] = _grid[j, k - 1] / tau
					+ f(GetSpaceCoordinate(j), GetTimeCoordinate(k - 1))
					+ GetExplicitPart(coefs, f, j, k);
			}

			d[0] = _conditions.FirstCondition(0, GetTimeCoordinate(k)) / (betta - alpha / h);
			d[N] = _conditions.SecondCondition(0, GetTimeCoordinate(k)) / (delta + gamma / h);

			matrix[0, 0] = 1;
			matrix[0, 1] = (alpha / h) / (betta - alpha / h);

			matrix[N, N - 1] = -(gamma / h) / (delta + gamma / h);
			matrix[N, N] = 1;

			return d;
		}

		private double[] ApproximateSecondDegreeTwoPoints(int k, Matrix matrix, double[] coefs, Func<double, double, double> f)
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

			var d = new double[_params.SpaceStepCount];

			for (int j = 1; j < N; j++)
			{
				d[j] = _grid[j, k - 1] / tau
					+ f(GetSpaceCoordinate(j), GetTimeCoordinate(k - 1))
					+ GetExplicitPart(coefs, f, j, k);
			}

			d[0] = Math.Abs(alpha) > 1E-3
				? h / tau * _grid[0, k - 1]
					+ -_conditions.FirstCondition(0, GetTimeCoordinate(k)) * (2 * a - b * h) / alpha
				: _conditions.FirstCondition(0, GetTimeCoordinate(k)) / betta;
			d[N] = Math.Abs(gamma) > 1E-3
				? h / tau * _grid[N, k - 1]
					+ _conditions.SecondCondition(0, GetTimeCoordinate(k)) * (2 * a + b * h) / gamma
				: _conditions.SecondCondition(0, GetTimeCoordinate(k)) / delta;

			matrix[0, 0] = Math.Abs(alpha) > 1E-3
				? (2 * a / h + h / tau - c * h - betta / alpha * (2 * a - b * h))
				: 1;
			matrix[0, 1] = Math.Abs(alpha) > 1E-3
				? -2 * a / h
				: 0;

			matrix[N, N - 1] = Math.Abs(gamma) > 1E-3
				? -2 * a / h
				: 0;
			matrix[N, N] = Math.Abs(gamma) > 1E-3
				? (2 * a / h + h / tau - c * h + delta / gamma * (2 * a + b * h))
				: 1;

			return d;
		}

		private double GetExplicitPart(double[] coefs, Func<double, double, double> f, int j, int k)
		{
			var a = coefs[0];
			var b = coefs[1];
			var c = coefs[2];

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;

			return ((a / (h * h)) * _grid[j + 1, k - 1]
					+ -2 * (a / (h * h)) * _grid[j, k - 1]
					+ (a / (h * h)) * _grid[j - 1, k - 1]
					+ b * (_grid[j + 1, k - 1] - _grid[j - 1, k - 1]) / (2 * h)
					+ c * _grid[j, k - 1]) * (1 - _weight);
		}

		private double GetSpaceCoordinate(int i)
		{
			return (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount * i + _params.SpaceBoundLeft;
		}

		private double GetTimeCoordinate(int i)
		{
			return _params.TimeLimit / _params.TimeStepCount * i;
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
