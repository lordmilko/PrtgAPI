using System.Linq;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    class MethodReplacer : LinqExpressionVisitor
    {
        public static Expression Replace(Expression expr)
        {
            var replacer = new MethodReplacer();
            return replacer.Visit(expr);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == FilterMethod.Equals && node.Object != null && node.Arguments.Count == 1)
            {
                return Expression.Equal(
                    Expression.Convert(node.Object, typeof(object)),
                    Expression.Convert(node.Arguments.First(), typeof(object))
                );
            }

            if (node.Method.Name == FilterMethod.Equals && node.Method.DeclaringType == typeof(object) &&
                node.Object == null && node.Arguments.Count == 2)
            {
                return Expression.Equal(
                    Expression.Convert(node.Arguments[0], typeof(object)),
                    Expression.Convert(node.Arguments[1], typeof(object))
                );
            }
                
            return base.VisitMethodCall(node);
        }
    }
}
