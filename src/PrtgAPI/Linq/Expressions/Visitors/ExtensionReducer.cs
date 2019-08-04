using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Conditionally reduces <see cref="ExpressionType.Extension"/> <see cref="Expression"/> nodes based
    /// on whether these nodes actually support reduction. If the node cannot be reduced, it is returned as is.
    /// </summary>
    class ExtensionReducer : LinqExpressionVisitor
    {
        public static Expression Reduce(Expression expr)
        {
            return new ExtensionReducer().Visit(expr);
        }

        protected override Expression VisitExtension(Expression node)
        {
            var visited = base.VisitExtension(node);

            if (visited.CanReduce)
                return visited.Reduce();

            return visited;
        }
    }
}
