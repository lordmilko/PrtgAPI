using System;
using System.Linq.Expressions;
using PrtgAPI.Reflection;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Constructs an <see cref="Expression"/> tree suitable for executing client side, stripping out any
    /// expressions that may only be applicable server side (or should not be executed twice)
    /// </summary>
    class ClientTreeBuilder : LinqExpressionVisitor
    {
        public static Expression Parse(Expression expr)
        {
            var builder = new ClientTreeBuilder();

            var result = builder.Visit(expr);

            return result;
        }

        protected internal override Expression VisitSkip(SkipLinqExpression node)
        {
            return Visit(node.Source);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var obj = Visit(node.Object);
            var args = Visit(node.Arguments, Visit);

            if (obj != node.Object || args != node.Arguments)
                node = Expression.Call(obj, node.Method, args);

            Expression final;

            switch (node.Method.Name)
            {
                case FilterMethod.Contains:
                case FilterMethod.Equals:
                    final = node.Object == null ? node : ExpressionHelpers.BoolIfNotNull(node.Object, node);
                    break;
                case FilterMethod.ToString:
                    final = node.Object == null ? node : ExpressionHelpers.NullConditional(node.Object, node);
                    break;
                default:
                    final = CatchNullReference(node);
                    break;
            }

            return final;
        }

        protected Expression CatchNullReference(MethodCallExpression expr)
        {
            if (expr.Object == null)
                return expr;

            if (!expr.Object.Type.IsValueType || expr.Object.Type.IsNullable())
            {
                var @null = Expression.Constant(null, expr.Object.Type);
                var isNull = Expression.Equal(@null, expr.Object);
                var exception = Expression.Constant(new NullReferenceException($"Object reference not set to an instance of an object calling method '{expr.Method.Name}' in expression '{ConditionUnwrapper.Unwrap(expr)}'. Consider using a ternary expression to specify conditional access."));
                var @throw = Expression.Throw(exception);
                var block = Expression.Block(@throw, Expression.Default(expr.Type));
                var condition = Expression.Condition(isNull, block, expr);

                return condition;
            }
            else
                return expr;
        }

        protected override Expression VisitExtension(Expression node)
        {
            var substitute = node as SubstituteExpression;

            if (substitute != null)
            {
                var original = Visit(substitute.Original);
                var replacement = Visit(substitute.Replacement);

                if (original != substitute.Original || replacement != substitute.Replacement)
                    node = new SubstituteExpression(original, replacement);

                return node;
            }

            return base.VisitExtension(node);
        }
    }
}
