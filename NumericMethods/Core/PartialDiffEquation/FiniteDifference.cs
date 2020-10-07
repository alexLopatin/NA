using NumericMethods.Core.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public double[,] SolveExplicit(double[] coefs, Func<double, double, double> f)
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

				SetBorderGrid(coefs, k);
			}

			return _grid.Clone() as double[,];
		}

		public double[,] SolveImplicit(double[] coefs, Func<double, double, double> f)
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

			double sigma = a * tau / (h * h);

			Matrix matrix = new Matrix(_params.SpaceStepCount, _params.SpaceStepCount);

			for (int i = 0; i < _params.SpaceStepCount - 1; i++)
			{
				matrix[i, i] = -1 - 2 * sigma;
				matrix[i, i + 1] = sigma;
				matrix[i + 1, i] = sigma;
			}

			var N = _params.SpaceStepCount - 1;

			matrix[0, 0] = betta - alpha / h;
			matrix[0, 1] = alpha / h;
			matrix[N, N - 1] = -gamma / h;
			matrix[N, N] = delta + gamma / h;


			for (int k = 1; k < _params.TimeStepCount; k++)
			{
				var d = Enumerable
					.Range(0, _params.SpaceStepCount)
					.Select(i => - _grid[i, k - 1])
					.ToArray();

				d[0] = _conditions.FirstFunc(0, GetTimeCoordinate(k)) / (betta - alpha / h);
				d[^1] = _conditions.SecondFunc(0, GetTimeCoordinate(k)) / (delta + gamma / h);

				var newLayer = matrix.SolveTridiagonal(d);

				for(int i = 0; i < _params.SpaceStepCount; i++)
				{
					_grid[i, k] = newLayer[i];
				}
			}

			return _grid.Clone() as double[,];
		}

		private void SetBorderGrid(double[] coefs, int k)
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

			var u = _grid;
			var N = _params.SpaceStepCount - 1;

			switch (_params.Approximation)
			{
				case Approximation.ThreeDotsSecondDegree:
					var p = betta - 3 * alpha / (2 * h);
					var g = 3 * gamma / 2 / h + delta;

					_grid[0, k] = _conditions.FirstFunc(0, GetTimeCoordinate(k)) / p
						- (4 * u[1, k] - u[2, k]) * alpha / (2 * h * p);

					_grid[N, k] = _conditions.SecondFunc(0, GetTimeCoordinate(k)) / g
						+ (4 * u[N - 1, k] - u[N - 2, k]) * gamma / (2 * h * g);

					break;
				case Approximation.TwoDotsFirstDegree:
					_grid[0, k] = -(alpha / h) / (betta - alpha / h) * _grid[1, k] +
						_conditions.FirstFunc(0, GetTimeCoordinate(k)) / (betta - alpha / h);

					_grid[N, k] = (gamma / h) / (delta + gamma / h) * _grid[N - 1, k] +
						_conditions.SecondFunc(0, GetTimeCoordinate(k)) / (delta + gamma / h);

					break;
				case Approximation.TwoDotsSecondDegree:
					p = 2 * a - b * h;
					g = -2 * a * alpha / (h * p) - h * alpha / (p * tau) + (c * h * alpha) / p + betta;
					_grid[0, k] = (_conditions.FirstFunc(0, GetTimeCoordinate(k))
						- alpha * 2 * a * u[1, k] / (h * p)
						- h * alpha * u[0, k - 1] / (p * tau)) / g;

					p = 2 * a + b * h;
					g = 2 * a * gamma / (h * p) + h * gamma / (p * tau) - (c * h * gamma) / p + delta;
					_grid[N, k] = (_conditions.SecondFunc(0, GetTimeCoordinate(k))
						+ gamma * 2 * a * u[N - 1, k] / (h * p)
						+ h * gamma * u[N, k - 1] / (p * tau)) / g;

					break;
			}
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
