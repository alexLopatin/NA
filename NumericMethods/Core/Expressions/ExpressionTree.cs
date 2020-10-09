using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.Expressions
{
	public class ExpressionTree
	{
		public object head;
		public void Simplify()
		{
			bool changed = true;
			while (changed)
			{
				changed = false;
				SimplifyCalculations(ref changed);
				SimplifyEquilent(ref changed);
			}
		}
		private void SimplifyCalculations(ref bool changed, ExpressionNode node = null, ExpressionNode parent = null)
		{
			if (!(head is ExpressionNode))
				return;
			if (node == null)
				node = head as ExpressionNode;
			for (int i = 0; i < node.Children.Count; i++)
				if (node.Children[i] is ExpressionNode)
					SimplifyCalculations(ref changed, (ExpressionNode)node.Children[i], node);

			double val = 0;
			if (node.Operation is string)
			{
				if ((string)node.Operation == "~")
				{
					if (node.Children[0] is double)
					{
						val = -(double)node.Children[0];
						if (parent != null)
						{
							int index = parent.Children.IndexOf(node);
							parent.Children[index] = val;
						}
						else
							head = val;
						changed = true;
					}
				}
				else if (node.Children[0] is double && node.Children[1] is double)
				{
					switch (node.Operation)
					{
						case "+":
							val = (double)node.Children[0] + (double)node.Children[1];
							break;
						case "-":
							val = (double)node.Children[0] - (double)node.Children[1];
							break;
						case "*":
							val = (double)node.Children[0] * (double)node.Children[1];
							break;
						case "/":
							val = (double)node.Children[0] / (double)node.Children[1];
							break;
						case "^":
							val = Math.Pow((double)node.Children[0], (double)node.Children[1]);
							break;
					}
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = val;
					}
					else
						head = val;
					changed = true;
				}
			}
			if (node.Operation is Function)
			{
				int countOfTrue = 0;
				Function func = (Function)node.Operation;
				double[] args = new double[node.Children.Count];
				foreach (object operand in node.Children)
					if (operand is double)
						countOfTrue++;
					else if (operand is Variable && !((Variable)operand).IsParameter)
						countOfTrue++;
				if (countOfTrue == node.Children.Count)
				{
					for (int i = 0; i < node.Children.Count; i++)
						if (node.Children[i] is double)
							args[i] = (double)node.Children[i];
						else if (node.Children[i] is Variable)
							args[i] = ((Variable)node.Children[i]).Value;
					val = func.GetValue(args);
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = val;
					}
					else
						head = val;
					changed = true;
				}
			}
		}
		private void SimplifyEquilent(ref bool changed, ExpressionNode node = null, ExpressionNode parent = null)
		{
			if (!(head is ExpressionNode))
				return;
			if (node == null)
				node = head as ExpressionNode;
			for (int i = 0; i < node.Children.Count; i++)
				if (node.Children[i] is ExpressionNode)
					SimplifyEquilent(ref changed, (ExpressionNode)node.Children[i], node);

			// 0 + ...
			{
				if (node.Operation is string
			   && node.Children.Count == 2
			   && node.Children[0] is double
			   && (string)node.Operation == "+"
			   && (double)node.Children[0] == 0)
				{
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = node.Children[1];
					}
					else
						head = node.Children[1];
					changed = true;
					return;
				}
				if (node.Operation is string
					&& node.Children.Count == 2
					&& node.Children[1] is double
					&& (string)node.Operation == "+"
					&& (double)node.Children[1] == 0)
				{
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = node.Children[0];
					}
					else
						head = node.Children[0];
					changed = true;
				}
			}

			// 0 - ...
			{
				if (node.Operation is string
					&& node.Children.Count == 2
					&& node.Children[0] is double
					&& (string)node.Operation == "-"
					&& (double)node.Children[0] == 0)
				{
					node.Operation = "*";
					node.Children[0] = -1d;
					changed = true;
					return;
				}
				if (node.Operation is string
					&& node.Children.Count == 2
					&& node.Children[1] is double
					&& (string)node.Operation == "-"
					&& (double)node.Children[1] == 0)
				{
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = node.Children[0];
					}
					else
						head = node.Children[0];
					changed = true;
				}
			}

			// 1 * ...
			{
				if (node.Operation is string
					&& node.Children.Count == 2
					&& node.Children[0] is double
					&& (string)node.Operation == "*"
					&& (double)node.Children[0] == 1)
				{
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = node.Children[1];
					}
					else
						head = node.Children[1];
					changed = true;
					return;
				}
				if (node.Operation is string
					&& node.Children.Count == 2
					&& node.Children[1] is double
					&& (string)node.Operation == "*"
					&& (double)node.Children[1] == 1)
				{
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = node.Children[0];
					}
					else
						head = node.Children[0];
					changed = true;
				}
			}

			// ... / 1
			{
				if (node.Operation is string
					&& node.Children.Count == 2
					&& node.Children[1] is double
					&& (string)node.Operation == "/"
					&& (double)node.Children[1] == 1)
				{
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = node.Children[0];
					}
					else
						head = node.Children[0];
					changed = true;
					return;
				}
			}

			// 0 * ...
			{
				if (node.Operation is string
					&& node.Children.Count == 2
					&& node.Children[0] is double
					&& (string)node.Operation == "*"
					&& (double)node.Children[0] == 0)
				{
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = 0d;
					}

					else
						head = 0d;
					changed = true;
					return;
				}
				if (node.Operation is string
					&& node.Children.Count == 2
					&& node.Children[1] is double
					&& (string)node.Operation == "*"
					&& (double)node.Children[1] == 0)
				{
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = 0d;
					}
					else
						head = 0d;
					changed = true;
				}
			}

			// ... ^ 0
			{
				if (node.Operation is string
					&& node.Children.Count == 2
					&& node.Children[1] is double
					&& (string)node.Operation == "^"
					&& (double)node.Children[1] == 0)
				{
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = 1d;
					}
					else
						head = 1d;
					changed = true;
					return;
				}
			}

			// ... ^ 0
			{
				if (node.Operation is string
					&& node.Children.Count == 2
					&& node.Children[1] is double
					&& (string)node.Operation == "^"
					&& (double)node.Children[1] == 1d)
				{
					if (parent != null)
					{
						int index = parent.Children.IndexOf(node);
						parent.Children[index] = node.Children[0];
					}
					else
						head = node.Children[0];
					changed = true;
					return;
				}
			}

		}
		public override string ToString()
		{
			return head.ToString();
		}
		public double GetValue()
		{
			if (head is ExpressionNode)
				return ((ExpressionNode)head).GetValue();
			else if (head is Variable)
				return ((Variable)head).Value;
			else
				return (double)head;
		}
	}
}
