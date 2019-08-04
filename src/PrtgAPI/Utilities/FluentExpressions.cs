using System.Linq.Expressions;

namespace PrtgAPI.Utilities
{
    internal static class FluentExpressions
    {
        internal static BinaryExpression AndAlso(this Expression left, Expression right)
        {
            return Expression.AndAlso(left, right);
        }

        internal static BinaryExpression Assign(this Expression left, Expression right)
        {
            return Expression.Assign(left, right);
        }

        internal static MethodCallExpression Call(this Expression instance, string methodName)
        {
            return Expression.Call(instance, methodName, null);
        }

        internal static BinaryExpression Equal(this Expression left, Expression right)
        {
            return Expression.Equal(left, right);
        }

        internal static ConditionalExpression IfThen(this Expression test, Expression ifTrue)
        {
            return Expression.IfThen(test, ifTrue);
        }

        internal static UnaryExpression Not(this Expression expression)
        {
            return Expression.Not(expression);
        }

        internal static BinaryExpression NotEqual(this Expression left, Expression right)
        {
            return Expression.NotEqual(left, right);
        }

        internal static UnaryExpression Throw(this Expression value)
        {
            return Expression.Throw(value);
        }
    }
}
