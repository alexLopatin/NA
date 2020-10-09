using NumericMethods.Core.Expressions.Standard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NumericMethods.Core.Expressions
{
	public class Expression
	{
		Queue queue;
		public ExpressionTree expTree;
		public List<Variable> Variables;

		public Expression() { }

		public Expression(Expression expr)
		{
			Variables = new List<Variable>(expr.Variables);
			queue = new Queue(expr.queue);
			ToExpressionTree();
		}

		public void FromString(string textExp, List<Variable> variables)
		{
			queue = new ExpressionParser(textExp,
				StandardFunctions.Get(),
				StandardConstants.Get(),
				variables
				).Build();
			Variables = variables;
			ToExpressionTree();
		}

		public bool IsOperator(object obj)
		{
			if (obj is string)
			{
				char c = ((string)obj)[0];
				return (c == '+' || c == '-' || c == '*' || c == '/' || c == '^' || c == '!');
			}
			return false;
		}

		public double GetValue() => expTree.GetValue();

		public void Simplify() => expTree.Simplify();

		private void ToExpressionTree()
		{
			expTree = new ExpressionTree();
			int cur = 0;
			Stack s = new Stack();
			ArrayList listQueue = new ArrayList(queue);
			while (cur < listQueue.Count)
			{
				if (listQueue[cur] is string && (string)listQueue[cur] == "~")
				{
					cur++;
					s.Push(new ExpressionNode("~", s.Pop()));
				}
				else if (IsOperator(listQueue[cur]))
				{
					char op = ((string)listQueue[cur++])[0];
					object b = s.Pop();
					object a = s.Pop();
					switch (op)
					{
						case '+':
							s.Push(new ExpressionNode("+", a, b));
							break;
						case '-':
							s.Push(new ExpressionNode("-", a, b));
							break;
						case '*':
							s.Push(new ExpressionNode("*", a, b));
							break;
						case '/':
							s.Push(new ExpressionNode("/", a, b));
							break;
						case '^':
							s.Push(new ExpressionNode("^", a, b));
							break;
						case '!':
							s.Push(new ExpressionNode("!", a));
							break;
					}
				}
				else if (listQueue[cur] is Function)
				{
					Function func = (Function)listQueue[cur++];
					ArrayList arguments = new ArrayList();
					for (int i = 0; i < func.CountOfArguments; i++)
						arguments.Add(s.Pop());
					s.Push(new ExpressionNode(func, arguments));
				}
				else if (listQueue[cur] is Variable)
					s.Push(listQueue[cur++]);
				else
					s.Push(Double.Parse((string)listQueue[cur++], CultureInfo.InvariantCulture));
			}
			expTree.head = s.Peek();
		}

		public override string ToString() => expTree.ToString();
	}
}
