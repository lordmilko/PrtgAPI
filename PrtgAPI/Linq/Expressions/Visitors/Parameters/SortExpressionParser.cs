using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors.Parameters
{
    /// <summary>
    /// Identifies the expression that specifies the property and direction to <see cref="Parameter.SortBy"/>, replacing processed
    /// <see cref="LinqExpression"/>s with their underlying <see cref="MethodCallExpression"/>.
    /// </summary>
    class SortExpressionParser : PropertyExpressionVisitor
    {
        public PropertyExpression SortProperty { get; private set; }
        public SortDirection SortDirection { get; private set; }

        protected internal override Expression VisitOrderBy(OrderByLinqExpression node)
        {
            var reducedSource = Visit(node.Source);

            Visit(node.KeySelector);

            SortDirection = node.SortDirection;

            return node.Reduce(reducedSource);
        }

        protected override Expression VisitProperty(PropertyExpression node)
        {
            //It's a real node and not a member of a MemberInitExpression / NewExpression
            if (node.CanReduce)
                SortProperty = node;

            return node;
        }
    }
}
