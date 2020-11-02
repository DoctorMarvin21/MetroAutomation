using System;
using System.Collections.Generic;

namespace MetroAutomation.ExpressionEvaluation
{
    internal sealed class Operator : Symbol
    {
        public static readonly Operator Addition = new Operator(1, NodeOperationType.Sum, false);
        public static readonly Operator Subtraction = new Operator(1, NodeOperationType.Substract, false);
        public static readonly Operator Multiplication = new Operator(2, NodeOperationType.Multiply, false);
        public static readonly Operator Division = new Operator(2, NodeOperationType.Divide, false);
        public static readonly Operator UnaryMinus = new Operator(2, NodeOperationType.Negate, true);
        public static readonly Operator Power = new Operator(3, NodeOperationType.Power, false);
        public static readonly Operator Log = new Operator(3, NodeOperationType.Log, false);
        public static readonly Operator SqareRoot = new Operator(3, NodeOperationType.Sqrt, true);
        public static readonly Operator Abs = new Operator(3, NodeOperationType.Abs, true);
        public static readonly Operator Lg10 = new Operator(3, NodeOperationType.Lg10, true);
        public static readonly Operator Ln = new Operator(3, NodeOperationType.Ln, true);
        public static readonly Operator Sin = new Operator(3, NodeOperationType.Sin, true);
        public static readonly Operator Cos = new Operator(3, NodeOperationType.Cos, true);
        public static readonly Operator Tan = new Operator(3, NodeOperationType.Tan, true);

        private static readonly Dictionary<char, Operator> Operations = new Dictionary<char, Operator>
        {
            { '+', Addition },
            { '-', Subtraction },
            { '*', Multiplication },
            { '/', Division },
            { '^', Power },
            { '_', Log }
        };

        private static readonly Dictionary<string, Operator> Functions = new Dictionary<string, Operator>
        {
            { "sqrt", SqareRoot },
            { "sin", Sin },
            { "cos", Cos },
            { "tan", Tan },
            { "abs", Abs },
            { "lg", Lg10 },
            { "ln", Ln }
        };

        private Operator(int precedence, NodeOperationType operationType, bool isUnary)
        {
            Precedence = precedence;
            OperationType = operationType;
            NumberOfOperands = isUnary ? 1 : 2;
        }

        public int NumberOfOperands { get; private set; }

        public int Precedence { get; private set; }

        public NodeOperationType OperationType
        {
            get;
            private set;
        }

        public static explicit operator Operator(char operation)
        {
            if (Operations.TryGetValue(operation, out Operator result))
            {
                return result;
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public static explicit operator Operator(string function)
        {
            if (Functions.TryGetValue(function, out Operator result))
            {
                return result;
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public static bool IsDefined(char operation)
        {
            return Operations.ContainsKey(operation);
        }

        public static bool IsDefinedFunction(string function)
        {
            return Functions.ContainsKey(function);
        }

        public NodeOperation Apply(params NodeOperation[] expressions)
        {
            if (expressions.Length == 1)
            {
                return Apply(expressions[0]);
            }
            else if (expressions.Length == 2)
            {
                return Apply(expressions[0], expressions[1]);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private NodeOperation Apply(NodeOperation node)
        {
            return new NodeOperation(node, OperationType);
        }

        private NodeOperation Apply(NodeOperation left, NodeOperation right)
        {
            return new NodeOperation(left, right, OperationType);
        }
    }

    internal class Parentheses : Symbol
    {
        public static readonly Parentheses Left = new Parentheses();
        public static readonly Parentheses Right = new Parentheses();

        private Parentheses()
        {
        }
    }

    internal class Symbol
    {
    }
}
