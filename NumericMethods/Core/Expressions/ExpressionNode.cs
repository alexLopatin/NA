using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.Expressions
{
	class ExpressionNode
	{
		public object Operation;
		public ArrayList Children = new ArrayList();
		public ExpressionNode() { }
		public ExpressionNode(ExpressionNode other)
		{
			Operation = other.Operation;
			for (int i = 0; i < other.Children.Count; i++)
				if (other.Children[i] is ExpressionNode)
					Children.Add(new ExpressionNode(other.Children[i] as ExpressionNode));
				else
					Children.Add(other.Children[i]);
		}
		public ExpressionNode(string operation, object first, object second)
		{
			Operation = operation;
			Children.Add(first);
			Children.Add(second);
		}
		public ExpressionNode(string operation, object first)
		{
			Operation = operation;
			Children.Add(first);
		}
		public ExpressionNode(object operation, ArrayList children)
		{
			Operation = operation;
			Children = children;
		}
		public override string ToString()
		{
			if (Operation is string
				&& (string)Operation == "~")
			{
				if (Children[0] is ExpressionNode)
					return "-(" + Children[0].ToString() + ")";
				else
					return "-" + Children[0].ToString();
			}
			if (Operation is string
				&& (string)Operation == "*")
			{
				string res = "";
				if (Children[0] is ExpressionNode)
				{
					if (((ExpressionNode)Children[0]).Operation is string
					&& (string)((ExpressionNode)Children[0]).Operation == "*")
						res += Children[0].ToString();
					else
						res += "(" + Children[0].ToString() + ")";
				}
				else if (Children[0] is double && (double)Children[0] < 0)
					res = "(" + Children[0].ToString() + ")";
				else
					res = Children[0].ToString();

				res += Operation.ToString();

				if (Children[1] is ExpressionNode)
				{
					if (((ExpressionNode)Children[1]).Operation is string
					&& (string)((ExpressionNode)Children[1]).Operation == "*")
						res += Children[1].ToString();
					else
						res += "(" + Children[1].ToString() + ")";
				}
				else
					res += Children[1].ToString();

				return res;
			}

			if (Operation is string
				&& ((string)Operation == "/" || (string)Operation == "^"))
			{
				string res = "";
				if (Children[0] is ExpressionNode)
					res = "(" + Children[0].ToString() + ")";
				else
					res = Children[0].ToString();

				res += Operation.ToString();

				if (Children[1] is ExpressionNode)
					res += "(" + Children[1].ToString() + ")";
				else
					res += Children[1].ToString();
				return res;
			}

			if (Operation is Function)
				return Operation.ToString() + "(" + Children[0].ToString() + ")";
			else
				return Children[0].ToString() + Operation.ToString() + Children[1].ToString();
		}
		public double Factorial(double f)
		{
			if (f == 0)
				return 1;
			else
				return f * Factorial(f - 1);
		}
		public double GetValue()
		{
			if (Operation is Function)
			{
				Function func = Operation as Function;
				double[] args = new double[func.CountOfArguments];
				for (int i = 0; i < func.CountOfArguments; i++)
					if (Children[i] is ExpressionNode)
						args[i] = ((ExpressionNode)Children[i]).GetValue();
					else if (Children[i] is Variable)
						args[i] = ((Variable)Children[i]).Value;
					else
						args[i] = (double)Children[i];
				return func.GetValue(args);
			}
			if (Operation is string)
			{
				string op = (string)Operation;
				double b = 0;
				double a = 0;
				if (Children[0] is ExpressionNode)
					a = ((ExpressionNode)Children[0]).GetValue();
				else if (Children[0] is Variable)
					a = ((Variable)Children[0]).Value;
				else
					a = (double)Children[0];
				if (op != "!" && op != "~")
				{
					if (Children[1] is ExpressionNode)
						b = ((ExpressionNode)Children[1]).GetValue();
					else if (Children[1] is Variable)
						b = ((Variable)Children[1]).Value;
					else
						b = (double)Children[1];
				}

				double val = 0;
				switch (op)
				{
					case "+":
						val = a + b;
						break;
					case "-":
						val = a - b;
						break;
					case "*":
						val = a * b;
						break;
					case "/":
						val = a / b;
						break;
					case "^":
						val = Math.Pow(a, b);
						break;
					case "!":
						val = Factorial((int)a);
						break;
					case "~":
						val = -a;
						break;
				}
				return val;
			}
			return 0;
		}

		public static ExpressionNode operator +(ExpressionNode a, ExpressionNode b)
			=> new ExpressionNode("+", new ExpressionNode(a), new ExpressionNode(b));
		public static ExpressionNode operator +(Variable a, ExpressionNode b)
			=> new ExpressionNode("+", a, new ExpressionNode(b));
		public static ExpressionNode operator +(ExpressionNode a, Variable b)
			=> new ExpressionNode("+", new ExpressionNode(a), b);
		public static ExpressionNode operator -(ExpressionNode a, ExpressionNode b)
			=> new ExpressionNode("-", new ExpressionNode(a), new ExpressionNode(b));
		public static ExpressionNode operator -(Variable a, ExpressionNode b)
			=> new ExpressionNode("-", a, new ExpressionNode(b));
		public static ExpressionNode operator -(ExpressionNode a, Variable b)
			=> new ExpressionNode("-", new ExpressionNode(a), b);
		public static ExpressionNode operator *(ExpressionNode a, ExpressionNode b)
			=> new ExpressionNode("*", new ExpressionNode(a), new ExpressionNode(b));
		public static ExpressionNode operator *(Variable a, ExpressionNode b)
			=> new ExpressionNode("*", a, new ExpressionNode(b));
		public static ExpressionNode operator *(ExpressionNode a, Variable b)
			=> new ExpressionNode("*", new ExpressionNode(a), b);
		public static ExpressionNode operator /(ExpressionNode a, ExpressionNode b)
			=> new ExpressionNode("/", new ExpressionNode(a), new ExpressionNode(b));
		public static ExpressionNode operator /(Variable a, ExpressionNode b)
			=> new ExpressionNode("/", a, new ExpressionNode(b));
		public static ExpressionNode operator /(ExpressionNode a, Variable b)
			=> new ExpressionNode("/", new ExpressionNode(a), b);
		public static ExpressionNode operator ^(ExpressionNode a, ExpressionNode b)
			=> new ExpressionNode("^", new ExpressionNode(a), new ExpressionNode(b));
		public static ExpressionNode operator ^(Variable a, ExpressionNode b)
			=> new ExpressionNode("^", a, new ExpressionNode(b));
		public static ExpressionNode operator ^(ExpressionNode a, Variable b)
			=> new ExpressionNode("^", new ExpressionNode(a), b);
	}

}
