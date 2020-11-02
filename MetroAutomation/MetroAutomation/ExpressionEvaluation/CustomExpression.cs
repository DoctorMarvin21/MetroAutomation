using System;
using System.Linq.Expressions;

namespace MetroAutomation.ExpressionEvaluation
{
    internal static class CustomExpression
    {
        public static Expression Abs(Expression x)
        {
            return Expression.Call(typeof(CustomActions).GetMethod(nameof(CustomActions.Abs)), x);
        }

        public static Expression Log(Expression left, Expression right)
        {
            return Expression.Call(typeof(CustomActions).GetMethod(nameof(CustomActions.Log)), left, right);
        }

        public static Expression Lg10(Expression x)
        {
            return Expression.Call(typeof(CustomActions).GetMethod(nameof(CustomActions.Lg10)), x);
        }

        public static Expression Ln(Expression x)
        {
            return Expression.Call(typeof(CustomActions).GetMethod(nameof(CustomActions.Ln)), x);
        }

        public static Expression Sin(Expression x)
        {
            return Expression.Call(typeof(CustomActions).GetMethod(nameof(CustomActions.Sin)), x);
        }

        public static Expression Cos(Expression x)
        {
            return Expression.Call(typeof(CustomActions).GetMethod(nameof(CustomActions.Cos)), x);
        }

        public static Expression Tan(Expression x)
        {
            return Expression.Call(typeof(CustomActions).GetMethod(nameof(CustomActions.Tan)), x);
        }
    }

    internal static class CustomActions
    {
        public static double Abs(double x)
        {
            return Math.Abs(x);
        }

        public static double Sin(double x)
        {
            return Math.Sin(x);
        }

        public static double Cos(double x)
        {
            return Math.Cos(x);
        }

        public static double Tan(double x)
        {
            return Math.Tan(x);
        }

        public static double Log(double x, double newBase)
        {
            return Math.Log(x, newBase);
        }

        public static double Lg10(double x)
        {
            return Math.Log10(x);
        }

        public static double Ln(double x)
        {
            return Math.Log(x);
        }
    }
}
