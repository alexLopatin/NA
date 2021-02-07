using System;

namespace NumericMethods.Core.PartialDiffEquation.Parabolic
{
	public class ParabolicExplicitFiniteDifference
	{
		private readonly double[,] _grid;

		private readonly ParabolicBoundaryConditions _conditions;
		private readonly ParabolicFiniteDifferenceParams _params;
		private readonly ParabolicEquationParams _equation;

		public ParabolicExplicitFiniteDifference(
			ParabolicBoundaryConditions conditions,
			ParabolicEquationParams equation,
			ParabolicFiniteDifferenceParams @params)
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
			for (int i = 0; i <= _params.SpaceStepCount; i++)
			{
				var x = GetSpaceCoordinate(i);
				_grid[i, 0] = _conditions.InitialCondition(x, 0);
			}
		}

		public double[,] Solve()
		{
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			double sigma = a * tau / (h * h);

			if (sigma > 0.5d)
			{
				throw new ArgumentException("σ должен быть меньше 0.5");
			}

			for (int k = 1; k <= _params.TimeStepCount; k++)
			{
				for (int j = 1; j <= _params.SpaceStepCount - 1; j++)
				{
					_grid[j, k] = sigma * _grid[j + 1, k - 1]
						+ (1 - 2 * sigma) * _grid[j, k - 1]
						+ sigma * _grid[j - 1, k - 1]
						+ b * (_grid[j + 1, k - 1] - _grid[j - 1, k - 1]) / (2 * h) * tau
						+ c * _grid[j, k - 1] * tau
						+ _equation.f(GetSpaceCoordinate(j), GetTimeCoordinate(k)) * tau;
				}

				SetBorderGrid(k);
			}

			return _grid.Clone() as double[,];
		}

		private void SetBorderGrid(int k)
		{
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
					_grid[0, k] = 1 / (-3 * alpha / (2 * h) + betta) *
						(f0 + alpha / (2 * h) * (_grid[2, k] - 4 * _grid[1, k]));
					_grid[_params.SpaceStepCount, k] = 1 / (3 * gamma / (2 * h) + delta) *
						(fn + gamma / (2 * h) * (4 * _grid[_params.SpaceStepCount - 1, k] - _grid[_params.SpaceStepCount - 2, k]));
					break;
				case BoundaryApproximationType.SecondDegreeTwoPoints:
					var lam0 = (-2 * a / h -  h / tau + c * h) / (2 * a - b * h);
					var ro0 = (2 * a * _grid[1, k] / h
						+ h * _grid[0, k - 1] / tau
						+ _equation.f(0, GetTimeCoordinate(k)) * h)
						/ (2 * a - b * h);

					var lam1 = (2 * a / h + h / tau - c * h) / (2 * a + b * h);
					var ro1 = (-2 * a * _grid[_params.SpaceStepCount - 1, k] / h
						- 1 * h * _grid[_params.SpaceStepCount, k - 1] / tau
						+ _equation.f(GetSpaceCoordinate(_params.SpaceStepCount), GetTimeCoordinate(k)) * h)
						/ (2 * a + b * h);

					_grid[0, k] = (f0 - alpha * ro0) / (alpha * lam0 + betta);
					_grid[_params.SpaceStepCount, k] = (fn - gamma * ro1) / (gamma * lam1 + delta);
					break;
			}

			Func<double, double, double> f = (x, t) => Math.Exp(-2 * t) * Math.Sin(x + t);

			//_grid[0, k] = f(0, GetTimeCoordinate(k));
			//_grid[_params.SpaceStepCount, k] = f(GetSpaceCoordinate(_params.SpaceStepCount), GetTimeCoordinate(k));

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
