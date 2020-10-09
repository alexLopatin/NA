using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.Expressions
{
	public class Variable
	{
		public string Name { get; private set; }
		public double Value { get; set; }
		public bool IsParameter;
		public Variable(string name, double value, bool isParameter = false)
		{
			Name = name;
			Value = value;
			IsParameter = isParameter;
		}
		public override string ToString()
		{
			return Name;
		}
	}
}
