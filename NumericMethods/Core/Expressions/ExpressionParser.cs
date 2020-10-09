using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NumericMethods.Core.Expressions
{
	class ExpressionParser
	{
		string Expression;
		int curIndex = 0;
		const string EndOfExpression = "?EOE";
		List<Function> Functions;
		List<Variable> Constants;
		List<Variable> Variables;
		Dictionary<char, int> operators = new Dictionary<char, int>()
		{
			{ '~', 3 },
			{ '+', 1 },
			{ '-', 1 },
			{ '*', 2 },
			{ '/', 2 },
			{ '^', 3 },
			{ '!', 3 }
		};
		public ExpressionParser(string expression, List<Function> functions, List<Variable> constants, List<Variable> variables)
		{
			Expression = expression.Replace("!", "!1 ");
			Functions = functions;
			Constants = constants;
			Variables = variables;
			for (int i = 0; i < Expression.Length; i++)
				if (!(char.IsLetterOrDigit(Expression[i])
					|| char.IsWhiteSpace(Expression[i])
					|| IsComma(Expression[i])
					|| IsBracket(Expression[i])
					|| IsOperator(Expression[i])
					|| char.IsPunctuation(Expression[i])))
					throw new Exception("Bad expression format: only allowed a-z, A-Z, 0-9, commas, dots, operators and brackets");
		}
		bool IsOperator(char c)
		{
			return (c == '+' || c == '-' || c == '*' || c == '/' || c == '^' || c == '!' || c == '~');
		}
		bool IsBracket(char c)
		{
			return (c == '(' || c == ')');
		}
		bool IsComma(char c)
		{
			return (c == ',');
		}
		bool IsFunction(string token)
		{
			return Functions.Exists(p => p.Name == token);
		}
		bool IsVariable(string token)
		{
			return Variables.Exists(p => p.Name == token);
		}
		bool IsConstant(string token)
		{
			return Constants.Exists(p => p.Name == token);
		}
		public string ReadNext()
		{
			while (curIndex != Expression.Length && Expression[curIndex] == ' ')
				curIndex++;
			if (curIndex == Expression.Length)
				return EndOfExpression;
			int length = 0;
			if (IsOperator(Expression[curIndex]) || IsBracket(Expression[curIndex]) || IsComma(Expression[curIndex]))
				return Expression[curIndex++].ToString();
			for (; curIndex < Expression.Length && (Char.IsLetterOrDigit(Expression[curIndex]) || Expression[curIndex] == '.'); curIndex++)
				length++;
			return Expression.Substring(curIndex - length, length);
		}
		public Queue Build()
		{
			Queue queue = new Queue();
			Stack stack = new Stack();
			string prevToken = "";
			string token = "";
			while ((token = ReadNext()) != EndOfExpression)
			{
				if (token == "-" && ((!Double.TryParse(prevToken, out _) && prevToken != ")" && !IsVariable(prevToken)) || prevToken == ""))
					token = "~";
				if (Double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
					queue.Enqueue(token);
				if (IsVariable(token))
					queue.Enqueue(Variables.Find(p => p.Name == token));
				if (IsConstant(token))
					queue.Enqueue(Constants.Find(p => p.Name == token));
				if (IsFunction(token))
					stack.Push(Functions.Find(p => p.Name == token));
				if (IsComma(token[0]))
					while (stack.Count > 0 && stack.Peek() as string != "(")
						queue.Enqueue(stack.Pop());
				if (IsOperator(token[0]))
				{
					while (stack.Count > 0
						&& IsOperator(((string)stack.Peek())[0])
						&& operators[((string)stack.Peek())[0]] >= operators[token[0]])
						queue.Enqueue(stack.Pop());
					stack.Push(token);
				}
				if (token[0] == '(')
					stack.Push(token);
				if (token[0] == ')')
				{
					while (stack.Count > 0 && stack.Peek() as string != "(")
						queue.Enqueue(stack.Pop());
					if (stack.Count == 0 || stack.Peek() as string != "(")
						throw new Exception("Bad expression format: missed bracket");
					if (stack.Count > 0)
						stack.Pop();
					if (stack.Count > 0 && stack.Peek() is Function)
						queue.Enqueue(stack.Pop());
				}
				prevToken = token;
			}
			while (stack.Count > 0)
				if (stack.Peek() as string == "(")
					throw new Exception("Bad expression format: missed bracket");
				else
					queue.Enqueue(stack.Pop());
			return queue;
		}
	}
}
