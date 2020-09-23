using System;
using System.Collections.Generic;
using System.Linq;
using NumericMethods.Core;
using NumericMethods.Core.PartialDiffEquation;

namespace Lab5
{
	class Program
	{
		static void Main(string[] args)
		{
			var conditions = new BoundaryConditionsThirdDegree()
			{
				FirstConditionParameters = new[] { .0d, 1.0d },
				SecondConditionParameters = new[] { .0d, 1.0d },
				InitialFunc = (x, t) => Math.Sin(2 * Math.PI * x),
				FirstFunc = (x, t) => 0,
				SecondFunc = (x, t) => 0
			};

			var @params = new FiniteDifferenceParams()
			{
				SpaceBoundLeft = 0,
				SpaceBoundRight = 1,
				TimeLimit = 1d,
				SpaceStepCount = 50,
				TimeStepCount = 25000
			};

			var finiteDiff = new FiniteDifference(conditions, @params);

			var result = finiteDiff.SolveImplicit(new[] { 1.0d, .0d, .0d }, (x, t) => 0);
			var errors = finiteDiff.FindError((x, t) => Math.Exp(-t * 4 * Math.PI * Math.PI) * Math.Sin(2 * Math.PI * x));

			var maxError = 0.0d;

			for(int i = 0; i < errors.GetLength(0); i++)
			{
				for (int j = 0; j < errors.GetLength(1); j++)
				{
					maxError = Math.Max(maxError, errors[i, j]);
				}
			}

			Console.WriteLine($"Max error: {maxError}");
		}
	}
}
