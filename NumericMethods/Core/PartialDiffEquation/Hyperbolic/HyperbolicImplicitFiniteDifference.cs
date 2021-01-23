using NumericMethods.Core.LinearAlgebra;
using System;
using System.Linq;

namespace NumericMethods.Core.PartialDiffEquation.Hyperbolic
{
	public class HyperbolicImplicitFiniteDifference
	{
		private readonly double[,] _grid;

		private readonly HyperbolicBoundaryConditions _conditions;
		private readonly FiniteDifferenceParams _params;

		public HyperbolicImplicitFiniteDifference(HyperbolicBoundaryConditions conditions, FiniteDifferenceParams @params)
		{
			_params = @params;
			_conditions = conditions;
			_grid = new double[_params.SpaceStepCount + 1, _params.TimeStepCount + 1];
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
			var tau = _params.TimeLimit / _params.TimeStepCount;
			var p1 = _conditions.InitialCondition;
			var p2 = _conditions.DerivativeCondition;

			for (int i = 0; i <= _params.SpaceStepCount; i++)
			{
				var x = GetSpaceCoordinate(i);
				_grid[i, 0] = p1(x, 0);

				switch(_conditions.InitialApproximation)
				{
					case InitialApproximationType.FirstDegree:
						_grid[i, 1] = p1(x, 0) + p2(x, 0) * tau;
						break;
					case InitialApproximationType.SecondDegree:
						_grid[i, 1] = p1(x, 0) + p2(x, 0) * tau;
						break;
				}
			}
		}

		public double[,] Solve(double[] timeCoefs, double[] coefs, Func<double, double, double> f)
		{
			var g = timeCoefs[0];
			var a = coefs[0];
			var b = coefs[1];
			var c = coefs[2];

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			var matrix = new Matrix(_params.SpaceStepCount + 1, _params.SpaceStepCount + 1);

			for (int i = 0; i < _params.SpaceStepCount; i++)
			{
				matrix[i, i] = 2 * a / (h * h) + 1 / (tau * tau) + g / (2 * tau) - c;
				matrix[i, i + 1] = -(a / (h * h) + b / (2 * h));
				matrix[i + 1, i] = -(a / (h * h) - b / (2 * h));
			}

			for (int k = 2; k <= _params.TimeStepCount; k++)
			{
				var d = SetBorderGrid(k, matrix, timeCoefs, coefs, f);

				var newLayer = matrix.SolveTridiagonal(d);

				for (int i = 0; i <= _params.SpaceStepCount; i++)
				{
					_grid[i, k] = newLayer[i];
				}
			}

			return _grid.Clone() as double[,];
		}

		private double[] SetBorderGrid(int k, Matrix matrix, double[] timeCoefs, double[] coefs, Func<double, double, double> f)
		{
			var g = timeCoefs[0];
			var a = coefs[0];
			var b = coefs[1];
			var c = coefs[2];

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			var alpha = _conditions.FirstConditionParameters[0];
			var betta = _conditions.FirstConditionParameters[1];

			var gamma = _conditions.SecondConditionParameters[0];
			var delta = _conditions.SecondConditionParameters[1];

			var N = _params.SpaceStepCount;

			var d = Enumerable
					.Range(0, _params.SpaceStepCount + 1)
					.Select(i => (2 * _grid[i, k - 1] - _grid[i, k - 2]) / (tau * tau) + g * _grid[i, k - 2] / (2* tau) + f(GetSpaceCoordinate(i), GetTimeCoordinate(k)))
					.ToArray();

			d[0] = _conditions.FirstCondition(0, GetTimeCoordinate(k));
			d[^1] = _conditions.SecondCondition(0, GetTimeCoordinate(k));

			matrix[0, 0] = betta - alpha / h;
			matrix[0, 1] = alpha / h;

			matrix[N, N - 1] = - gamma / h;
			matrix[N, N] = delta + gamma / h;

			return d;
		}

		public double[,] FindError(Func<double, double, double> u)
		{
			var errors = _grid.Clone() as double[,];

			for (int k = 0; k <= _params.TimeStepCount; k++)
			{
				for (int j = 0; j <= _params.SpaceStepCount; j++)
				{
					errors[j, k] = Math.Abs(_grid[j, k] - u(GetSpaceCoordinate(j), GetTimeCoordinate(k)));
				}
			}

			return errors;
		}
	}
}
