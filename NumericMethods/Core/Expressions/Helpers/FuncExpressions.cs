using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.Expressions.Helpers
{
	//супер пупер говно хелпер
	public static class FuncExpressions
	{
		public static Func<double> GetZeroDimensional(this Expression expression)
		{
			return () => expression.GetValue();
		}

		public static Func<double, double> GetOneDimensional(this Expression expression)
		{
			return (x) =>
			{
				expression.Variables[0].Value = x;

				return expression.GetValue();
			};
		}

		public static Func<double, double, double> GetTwoDimensional(this Expression expression)
		{
			return (x, y) =>
			{
				expression.Variables[0].Value = x;
				expression.Variables[1].Value = y;

				return expression.GetValue();
			};
		}

		public static Func<double, double, double, double> GetThreeDimensional(this Expression expression)
		{
			return (x, y, z) =>
			{
				expression.Variables[0].Value = x;
				expression.Variables[1].Value = y;
				expression.Variables[2].Value = z;

				return expression.GetValue();
			};
		}
	}
}
