using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Represents a visitor capable selectively visiting a <see cref="PropertyExpression"/> of a specified <see cref="LinqExpression"/> type.<para/>
    /// By default, this type will only visit the sources of a <see cref="LinqExpression"/> tree. Override the visitor for the specified <see cref="LinqExpression"/>
    /// type in order to visit its <see cref="PropertyExpression"/>.
    /// </summary>
    class PropertyExpressionVisitor : LinqSourceExpressionVisitor
    {
        protected override Expression VisitProperty(PropertyExpression node)
        {
            return node;
        }
    }
}
