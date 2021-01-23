using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.PartialDiffEquation
{
	public class FiniteDifferenceParams
	{
		public double SpaceBoundLeft { get; set; }
		public double SpaceBoundRight { get; set; }
		public double TimeLimit { get; set; }

		public int SpaceStepCount { get; set; }
		public int TimeStepCount { get; set; }

		public BoundaryApproximationType ApproximationType { get; set; }
	}
}
