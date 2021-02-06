using NumericMethods.Core.Numerics;
using System;

namespace NumericMethods.Core.PartialDiffEquation.Hyperbolic
{
	public class HyperbolicExplicitFiniteDifference
	{
		private readonly double[,] _grid;

		private readonly HyperbolicBoundaryConditions _conditions;
		private readonly HyperbolicFiniteDifferenceParams _params;
		private readonly HyperbolicEquationParams _equation;

		public HyperbolicExplicitFiniteDifference(HyperbolicBoundaryConditions conditions, HyperbolicEquationParams equation, HyperbolicFiniteDifferenceParams @params)
		{
			_params = @params;
			_conditions = conditions;
			_equation = equation;

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
			var d = _equation.d;
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

			var tau = _params.TimeLimit / _params.TimeStepCount;
			Func<double, double> p1 = (x) => _conditions.InitialCondition(x, 0);
			Func<double, double> p2 = (x) => _conditions.DerivativeCondition(x, 0);
			var firstDerivative = Derivative.FindByFunction(p1, 1, _params.SpaceBoundLeft, _params.SpaceBoundRight, _params.SpaceStepCount);
			var secondDerivative = Derivative.FindByFunction(p1, 2, _params.SpaceBoundLeft, _params.SpaceBoundRight, _params.SpaceStepCount);

			for (int i = 0; i <= _params.SpaceStepCount; i++)
			{
				var x = GetSpaceCoordinate(i);
				_grid[i, 0] = p1(x);

				switch (_params.InitialApproximation)
				{
					case InitialApproximationType.FirstDegree:
						_grid[i, 1] = p1(x) + p2(x) * tau;
						break;
					case InitialApproximationType.SecondDegree:
						_grid[i, 1] = p1(x) + p2(x) * tau
							+ tau * tau / 2 * (a * secondDerivative[i] + b * firstDerivative[i] + c * p1(x) + _equation.f(x, 0) - d * p2(x));
						break;
				}
			}
		}

		public double[,] Solve()
		{
			var g = _equation.d;
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

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
						+ _equation.f(GetSpaceCoordinate(j), GetTimeCoordinate(k - 1)))
							/ (1 / (tau * tau) + g / (2 * tau));
				}

				SetBorderGrid(k);
			}

			return _grid.Clone() as double[,];
		}

		private void SetBorderGrid(int k)
		{
			var d = _equation.d;
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			var alpha = _conditions.FirstConditionParameters[0];
			var betta = _conditions.FirstConditionParameters[1];

			var gamma = _conditions.SecondConditionParameters[0];
			var delta = _conditions.SecondConditionParameters[1];

			var f0 = _conditions.FirstCondition(0, GetTimeCoordinate(k));
			var fn = _conditions.SecondCondition(0, GetTimeCoordinate(k));

			switch (_params.BoundaryApproximation)
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
						+ _equation.f(0, GetTimeCoordinate(k)) * h)
						/ (2 * a - b * h);

					var lam1 = (2 * a / h + h / (tau * tau) + d * h / tau - c * h) / (2 * a + b * h);
					var ro1 = (-2 * a * _grid[_params.SpaceStepCount - 1, k] / h
						+ h / (tau * tau) * (_grid[_params.SpaceStepCount, k - 2] - 2 * _grid[_params.SpaceStepCount, k - 1])
						- d * h * _grid[_params.SpaceStepCount, k - 1] / tau
						+ _equation.f(GetSpaceCoordinate(_params.SpaceStepCount), GetTimeCoordinate(k)) * h)
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
