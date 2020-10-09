using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.Expressions.Standard
{
	class StandardDerivativeFunction
	{
		public ExpressionNode Derivative;
		public object Argument;
		public StandardDerivativeFunction(object argument, ExpressionNode derivative)
			=> (Argument, Derivative) = (argument, derivative);
	}
	static class StandardFunctions
	{
		private static List<Function> functions = new List<Function>()
		{
			new Function("sin", 1, (x) => Math.Sin(x[0])),
			new Function("cos", 1, (x) => Math.Cos(x[0])),
			new Function("tg", 1, (x) => Math.Tan(x[0])),
			new Function("ctg", 1, (x) => 1/Math.Tan(x[0])),
			new Function("ln", 1, (x) => Math.Log(x[0])),
			new Function("log", 2, (x) => Math.Log(x[0], x[1])),
			new Function("max", 2, (x) => Math.Max(x[0], x[1])),
			new Function("min", 2, (x) => Math.Log(x[0], x[1])),
			new Function("abs", 1, (x) => Math.Abs(x[0])),
			new Function("sqrt", 1, (x) => Math.Sqrt(x[0])),
			new Function("arccos", 1, (x) => Math.Acos(x[0])),
			new Function("arcsin", 1, (x) => Math.Asin(x[0])),
			new Function("arctg", 1, (x) => Math.Atan(x[0]))
		};
		public static List<Function> Get() => functions;
		private static Dictionary<string, StandardDerivativeFunction> derivatives;
		public static ExpressionNode GetDerivative(string name, object Argument)
		{
			if (derivatives == null)
			{
				derivatives = new Dictionary<string, StandardDerivativeFunction>();
				{
					object argument = new object();
					ExpressionNode der = new ExpressionNode("/", 1d, argument);
					StandardDerivativeFunction ln = new StandardDerivativeFunction(argument, der);
					derivatives["ln"] = ln;
				}

				{
					object argument = new object();
					Function fSin = functions.Find(p => p.Name == "sin");
					ExpressionNode sin = new ExpressionNode(fSin, new ArrayList() { argument });
					ExpressionNode der = new ExpressionNode("*", -1d, sin);
					StandardDerivativeFunction cos = new StandardDerivativeFunction(argument, der);
					derivatives["cos"] = cos;
				}

				{
					object argument = new object();
					Function fCos = functions.Find(p => p.Name == "cos");
					ExpressionNode cos = new ExpressionNode(fCos, new ArrayList() { argument });
					StandardDerivativeFunction sin = new StandardDerivativeFunction(argument, cos);
					derivatives["sin"] = sin;
				}

				{
					object argument = new object();
					Function fCos = functions.Find(p => p.Name == "cos");
					ExpressionNode cos = new ExpressionNode(fCos, new ArrayList() { argument });
					ExpressionNode delim = new ExpressionNode("*", new ExpressionNode(cos), new ExpressionNode(cos));
					ExpressionNode der = new ExpressionNode("/", 1d, delim);
					StandardDerivativeFunction tg = new StandardDerivativeFunction(argument, der);
					derivatives["tg"] = tg;
				}
				{
					object argument = new object();
					Function fSin = functions.Find(p => p.Name == "sin");
					ExpressionNode sin = new ExpressionNode(fSin, new ArrayList() { argument });
					ExpressionNode delim = new ExpressionNode("*", new ExpressionNode(sin), new ExpressionNode(sin));
					ExpressionNode fraction = new ExpressionNode("/", 1d, delim);
					ExpressionNode der = new ExpressionNode("*", -1d, fraction);
					StandardDerivativeFunction ctg = new StandardDerivativeFunction(argument, der);
					derivatives["ctg"] = ctg;
				}
			}

			ExpressionNode res = new ExpressionNode(derivatives[name].Derivative);
			Change(res, derivatives[name].Argument, Argument);
			return res;
		}
		private static void Change(ExpressionNode node, object argumentToChange, object argument)
		{
			for (int i = 0; i < node.Children.Count; i++)
				if (node.Children[i] is ExpressionNode)
					Change(node.Children[i] as ExpressionNode, argumentToChange, argument);
				else if (node.Children[i] == argumentToChange)
					node.Children[i] = argument;
		}
	}
}
