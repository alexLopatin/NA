using System;

namespace NumericMethods.Core.PartialDiffEquation.Splitting
{
	public class SplittingBoundaryConditions
	{
		public double[,] ConditionParameters { get; set; } = new double[4, 2];
		public Func<double, double, double>[] InitialConditions { get; set; } = new Func<double, double, double>[4];

		public Func<double, double, double, double> ZeroTimeCondition { get; set; }
	}
}
