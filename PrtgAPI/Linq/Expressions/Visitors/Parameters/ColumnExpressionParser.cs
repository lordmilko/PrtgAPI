using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors.Parameters
{
    /// <summary>
    /// Identifies a list of expressions that can be used with <see cref="Parameter.Columns"/>, replacing processed
    /// <see cref="LinqExpression"/>s with their underlying <see cref="MethodCallExpression"/>.
    /// </summary>
    class ColumnExpressionParser : LinqExpressionVisitor
    {
        public List<PropertyExpression> Members { get; } = new List<PropertyExpression>();

        public bool HasSelect { get; private set; }

        protected internal override Expression VisitSelect(SelectLinqExpression node)
        {
            HasSelect = true;

            var reducedSource = Visit(node.Source);

            Visit(node.Selector);

            return node.Reduce(reducedSource);
        }

        protected internal override Expression VisitSelectMany(SelectManyLinqExpression node)
        {
            HasSelect = true;

            var reducedSource = Visit(node.Source);

            Visit(node.CollectionSelector);
            Visit(node.ResultSelector);

            return node.Reduce(reducedSource);
        }

        protected override Expression VisitProperty(PropertyExpression node)
        {
            //If we don't have this property already and this isn't a dummy expression
            if (Members.All(m => node.PropertyInfo.Name != m.PropertyInfo.Name) && node.Expression != null)
                Members.Add(node);

            return node;
        }
    }
}
