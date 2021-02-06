using System;

namespace NumericMethods.Core.PartialDiffEquation.Hyperbolic
{
	public class HyperbolicBoundaryConditions : BoundaryConditions
	{
		public Func<double, double, double> DerivativeCondition { get; set; }
	}
}
