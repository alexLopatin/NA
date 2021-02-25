using System;

namespace NumericMethods.Core.PartialDiffEquation.Elliptical
{
	public class EllipticalBoundaryConditions
	{
		public double[,] ConditionParameters { get; set; } = new double[4, 2];
		public Func<double, double, double>[] InitialConditions { get; set; } = new Func<double, double, double>[4];
	}
}
