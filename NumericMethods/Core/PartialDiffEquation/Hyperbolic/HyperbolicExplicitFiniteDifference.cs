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
			Func<int, int, double> f = (x, t) => Math.Cos(GetTimeCoordinate(t)) * Math.Exp(2 * GetSpaceCoordinate(x));


			for (int i = 0; i <= _params.SpaceStepCount; i++)
			{
				var x = GetSpaceCoordinate(i);
				//_grid[i, 0] = p1(x, 0);
				_grid[i, 0] = f(i, 0);

				switch (_conditions.InitialApproximation)
				{
					case InitialApproximationType.FirstDegree:
						_grid[i, 1] = p1(x, 0) + p2(x, 0) * tau;
						break;
					case InitialApproximationType.SecondDegree:
						//_grid[i, 1] = p1(x, 0) + p2(x, 0) * tau - Math.Sin(x) * tau * tau / 2;
						_grid[i, 1] = f(i, 1);
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

			var sigma = a * tau * tau / (h * h);

			if (sigma > 1.0d)
			{
				throw new ArgumentException("σ должен быть меньше 1.0");
			}

			for (int k = 2; k <= _params.TimeStepCount; k++)
			{
				for (int j = 1; j < _params.SpaceStepCount; j++)
				{
					_grid[j, k] = (_grid[j + 1, k - 1] * (a / (h * h) + b / (2 * h))
						+ _grid[j, k - 1] * (-2 * a / (h * h) + 2 / (tau * tau) + c)
						+ _grid[j - 1, k - 1] * (a / (h * h) - b / (2 * h))
						+ _grid[j, k - 2] * (-1 / (tau * tau) + g / (2 * tau))
						+ f(GetSpaceCoordinate(j), GetTimeCoordinate(k - 1)))
							/ (1 / (tau * tau) + g / (2 * tau));
				}

				SetBorderGrid(timeCoefs, coefs, f, k);
			}

			return _grid.Clone() as double[,];
		}

		private void SetBorderGrid(double[] timeCoefs, double[] coefs, Func<double, double, double> f, int k)
		{
			var d = timeCoefs[0];
			var a = coefs[0];
			var b = coefs[1];
			var c = coefs[2];

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			var alpha = _conditions.FirstConditionParameters[0];
			var betta = _conditions.FirstConditionParameters[1];

			var gamma = _conditions.SecondConditionParameters[0];
			var delta = _conditions.SecondConditionParameters[1];

			var f0 = _conditions.FirstCondition(0, GetTimeCoordinate(k));
			var fn = _conditions.SecondCondition(0, GetTimeCoordinate(k));

			switch (_conditions.BoundaryApproximation)
			{
				case BoundaryApproximationType.FirstDegreeTwoPoints:
					_grid[0, k] = -(alpha / h) / (betta - alpha / h) * _grid[1, k] +
						f0 / (betta - alpha / h);
					_grid[_params.SpaceStepCount, k] = (gamma / h) / (delta + gamma / h) * _grid[_params.SpaceStepCount - 1, k] +
						fn / (delta + gamma / h);
					break;
				case BoundaryApproximationType.SecondDegreeThreePoints:
					_grid[0, k] = 1 / (- 3 * alpha / (2 * h) + betta) *
						(f0 + alpha / (2 * h) * (_grid[2, k] - 4 * _grid[1, k]));
					_grid[_params.SpaceStepCount, k] = 1 / (3 * gamma / (2 * h) + delta) *
						(fn + gamma / (2 * h) * (4 * _grid[_params.SpaceStepCount - 1, k] - _grid[_params.SpaceStepCount - 2, k]));
					break;
				case BoundaryApproximationType.SecondDegreeTwoPoints:
					var lam0 = (-2 * a / h - h / (tau * tau) - d * h / tau + c * h) / (2 * a - b * h);
					var ro0 = (2 * a * _grid[1, k] / h
						- h / (tau * tau) * (_grid[0, k - 2] - 2 * _grid[0, k - 1])
						+ d * h * _grid[0, k - 1] / tau
						+ f(0, GetTimeCoordinate(k)) * h)
						/ (2 * a - b * h);

					var lam1 = (2 * a / h + h / (tau * tau) + d * h / tau - c * h) / (2 * a + b * h);
					var ro1 = (-2 * a * _grid[_params.SpaceStepCount - 1, k] / h
						+ h / (tau * tau) * (_grid[_params.SpaceStepCount, k - 2] - 2 * _grid[_params.SpaceStepCount, k - 1])
						- d * h * _grid[_params.SpaceStepCount, k - 1] / tau
						+ f(GetSpaceCoordinate(_params.SpaceStepCount), GetTimeCoordinate(k)) * h)
						/ (2 * a + b * h);

					_grid[0, k] = (f0 - alpha * ro0) / (alpha * lam0 + betta);
					_grid[_params.SpaceStepCount, k] = (fn - gamma * ro1) / (gamma * lam1 + delta);
					break;
			}
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
