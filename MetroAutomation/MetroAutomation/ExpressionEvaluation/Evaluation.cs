using System;

namespace MetroAutomation.ExpressionEvaluation
{
    internal static class Evaluation
    {
        public static double EvaluateDoubleBinary(double left, double right, NodeOperationType operationType)
        {
            switch (operationType)
            {
                case NodeOperationType.Sum:
                    {
                        return left + right;
                    }
                case NodeOperationType.Substract:
                    {
                        return left - right;
                    }
                case NodeOperationType.Multiply:
                    {
                        return left * right;
                    }
                case NodeOperationType.Divide:
                    {
                        return left / right;
                    }
                case NodeOperationType.Power:
                    {
                        return Math.Pow(left, right);
                    }
                case NodeOperationType.Log:
                    {
                        return Math.Log(left, right);
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }

        public static double EvaluateDoubleUnary(double value, NodeOperationType operationType)
        {
            switch (operationType)
            {
                case NodeOperationType.Negate:
                    {
                        return -value;
                    }
                case NodeOperationType.Sqrt:
                    {
                        return Math.Sqrt(value);
                    }
                case NodeOperationType.Abs:
                    {
                        return Math.Abs(value);
                    }
                case NodeOperationType.Lg10:
                    {
                        return Math.Log10(value);
                    }
                case NodeOperationType.Ln:
                    {
                        return Math.Log(value);
                    }
                case NodeOperationType.Sin:
                    {
                        return Math.Sin(value);
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }
    }
}
