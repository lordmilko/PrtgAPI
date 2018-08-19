using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    static class VisitHelpers
    {
        internal static LambdaExpression GetLambda(Expression expr)
        {
            //When you pass a Func<T> to a Queryable method, it gets transformed into an Expression<Func<T>>.
            //We unwrap all outer Expression nodes to retrieve the underlying lambda expression

            while (expr.NodeType == ExpressionType.Quote)
                expr = ((UnaryExpression)expr).Operand;

            if (expr.NodeType == ExpressionType.Constant)
                return ((ConstantExpression)expr).Value as LambdaExpression;

            return expr as LambdaExpression;
        }
    }
}
