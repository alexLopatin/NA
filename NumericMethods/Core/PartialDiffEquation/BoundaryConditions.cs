using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.PartialDiffEquation
{
	public class BoundaryConditions
	{
		public double[] FirstConditionParameters { get; set; } = Array.Empty<double>();
		public double[] SecondConditionParameters { get; set; } = Array.Empty<double>();

		public Func<double, double, double> FirstCondition { get; set; }
		public Func<double, double, double> SecondCondition { get; set; }

		public Func<double, double, double> InitialCondition { get; set; }
	}
}
