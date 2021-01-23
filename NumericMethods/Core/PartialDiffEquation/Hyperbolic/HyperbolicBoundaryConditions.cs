using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.PartialDiffEquation.Hyperbolic
{
	public class HyperbolicBoundaryConditions : BoundaryConditions
	{
		public Func<double, double, double> DerivativeCondition { get; set; }
		public InitialApproximationType InitialApproximation { get; set; }
	}
}
