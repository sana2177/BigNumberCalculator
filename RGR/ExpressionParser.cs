using System;
using System.Collections.Generic;

namespace RGR
{
    public static class ExpressionParser
    {
        private static Dictionary<string, int> precedence = new Dictionary<string, int>
        {
            {"+", 1}, {"-", 1}, {"*", 2}, {"/", 2}, {"sin", 3}, {"cos", 3}, {"tan", 3}
        };

        private static Dictionary<string, Func<BigNumber, BigNumber>> functions = new Dictionary<string, Func<BigNumber, BigNumber>>();

        static ExpressionParser()
        {
            functions.Add("sin", x => new BigNumber(Math.Sin(double.Parse(x.ToString())).ToString()));
            functions.Add("cos", x => new BigNumber(Math.Cos(double.Parse(x.ToString())).ToString()));
            functions.Add("tan", x => new BigNumber(Math.Tan(double.Parse(x.ToString())).ToString()));
        }

        public static BigNumber Evaluate(string expression)
        {
            expression = expression.Replace(" ", "");
            var output = new Stack<BigNumber>();
            var operators = new Stack<string>();

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (char.IsDigit(c) || c == '.')
                {
                    string number = "";
                    while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                    {
                        number += expression[i];
                        i++;
                    }
                    i--;
                    output.Push(new BigNumber(number));
                }
                else if (c == '+' || c == '-' || c == '*' || c == '/')
                {
                    string op = c.ToString();
                    while (operators.Count > 0 && precedence.ContainsKey(operators.Peek()) &&
                           precedence[operators.Peek()] >= precedence[op])
                    {
                        ApplyOperator(output, operators.Pop());
                    }
                    operators.Push(op);
                }
                else if (char.IsLetter(c))
                {
                    string func = "";
                    while (i < expression.Length && char.IsLetter(expression[i]))
                    {
                        func += expression[i];
                        i++;
                    }
                    i--;
                    operators.Push(func);
                }
                else if (c == '(')
                {
                    operators.Push("(");
                }
                else if (c == ')')
                {
                    while (operators.Peek() != "(")
                    {
                        ApplyOperator(output, operators.Pop());
                    }
                    operators.Pop();

                    if (operators.Count > 0 && precedence.ContainsKey(operators.Peek()) && precedence[operators.Peek()] == 3)
                    {
                        ApplyFunction(output, operators.Pop());
                    }
                }
            }

            while (operators.Count > 0)
            {
                ApplyOperator(output, operators.Pop());
            }

            return output.Pop();
        }

        private static void ApplyOperator(Stack<BigNumber> stack, string op)
        {
            var b = stack.Pop();
            var a = stack.Pop();
            BigNumber result = null;

            switch (op)
            {
                case "+":
                    result = a + b;
                    break;
                case "-":
                    result = a - b;
                    break;
                case "*":
                    result = a * b;
                    break;
                case "/":
                    result = a / b;
                    break;
                default:
                    throw new Exception($"Unknown operator: {op}");
            }
            stack.Push(result);
        }

        private static void ApplyFunction(Stack<BigNumber> stack, string func)
        {
            var a = stack.Pop();
            stack.Push(functions[func](a));
        }
    }
}