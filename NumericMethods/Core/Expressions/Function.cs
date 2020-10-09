using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.Expressions
{
	class Function
	{
		public string Name { get; private set; }
		public int CountOfArguments { get; private set; }
		private Func<double[], double> Func;
		public Function(string name, int countOfArguments, Func<double[], double> func)
		{
			Name = name;
			CountOfArguments = countOfArguments;
			Func = func;
		}
		public double GetValue(double[] arguments) => Func(arguments);
		public override string ToString()
		{
			return Name;
		}
	}
}
