using System;

namespace NumericMethods.Core.PartialDiffEquation.Hyperbolic
{
	public class HyperbolicExplicitFiniteDifference
	{
		private readonly double[,] _grid;

		private readonly HyperbolicBoundaryConditions _conditions;
		private readonly FiniteDifferenceParams _params;

		public HyperbolicExplicitFiniteDifference(HyperbolicBoundaryConditions conditions, FiniteDifferenceParams @params)
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

		public double[,] Solve(double[] coefs, Func<double, double, double> f)
		{
			var a = coefs[0];
			var b = coefs[1];
			var c = coefs[2];

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			double sigma = a * tau * tau / (h * h);

			if (sigma > 1.0d)
			{
				throw new ArgumentException("σ должен быть меньше 1.0");
			}

			for (int k = 2; k <= _params.TimeStepCount; k++)
			{
				for (int j = 1; j < _params.SpaceStepCount; j++)
				{
					_grid[j, k] =  _grid[j + 1, k - 1] * (sigma + b * tau * tau / (2 * h))
						+ _grid[j, k - 1] * (-2 * sigma + 2 + c * tau * tau)
						+ _grid[j - 1, k - 1] * (sigma - b * tau * tau / (2 * h))
						- _grid[j, k - 2]
						+ f(GetSpaceCoordinate(j), GetTimeCoordinate(k - 1)) * tau * tau;
				}

				SetBorderGrid(k);
			}

			return _grid.Clone() as double[,];
		}

		private void SetBorderGrid(int k)
		{
			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			var alpha = _conditions.FirstConditionParameters[0];
			var betta = _conditions.FirstConditionParameters[1];

			var gamma = _conditions.SecondConditionParameters[0];
			var delta = _conditions.SecondConditionParameters[1];

			_grid[0, k] = -(alpha / h) / (betta - alpha / h) * _grid[1, k] +
				_conditions.FirstCondition(0, GetTimeCoordinate(k)) / (betta - alpha / h);
			_grid[0, k] = _conditions.FirstCondition(0, GetTimeCoordinate(k));
			_grid[_params.SpaceStepCount, k] = (gamma / h) / (delta + gamma / h) * _grid[_params.SpaceStepCount - 2, k] +
				_conditions.SecondCondition(0, GetTimeCoordinate(k)) / (delta + gamma / h);
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
