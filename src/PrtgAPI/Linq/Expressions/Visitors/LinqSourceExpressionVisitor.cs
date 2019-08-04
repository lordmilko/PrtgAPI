using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Represents a visitor or rewriter for LINQ expression tree sources.<para/>
    /// This type does not visit the <see cref="LambdaExpression"/> of a specified <see cref="LinqExpression"/>.
    /// In order to visit additional expression members, override the corresponding visit method.
    /// </summary>
    [ExcludeFromCodeCoverage]
    class LinqSourceExpressionVisitor : LinqExpressionVisitor
    {
        protected internal override Expression VisitAny(AnyLinqExpression node)
        {
            var source = Visit(node.Source);

            if (source != node.Source)
                return new AnyLinqExpression(node.Method, source, node.Predicate);

            return node;
        }

        protected internal override Expression VisitCount(CountLinqExpression node)
        {
            var source = Visit(node.Source);

            if (source != node.Source)
                return new CountLinqExpression(node.Method, source, node.Predicate);

            return node;
        }

        protected internal override Expression VisitFirst(FirstLinqExpression node)
        {
            var source = Visit(node.Source);

            if (source != node.Source)
                return new FirstLinqExpression(node.Method, source, node.Predicate);

            return node;
        }

        protected internal override Expression VisitLast(LastLinqExpression node)
        {
            var source = Visit(node.Source);

            if (source != node.Source)
                return new LastLinqExpression(node.Method, source, node.Predicate);

            return node;
        }

        protected internal override Expression VisitOrderBy(OrderByLinqExpression node)
        {
            var source = Visit(node.Source);

            if (source != node.Source)
                return new OrderByLinqExpression(node.Method, source, node.KeySelector, node.Comparer, node.SortDirection);

            return node;
        }

        protected internal override Expression VisitSkip(SkipLinqExpression node)
        {
            var source = Visit(node.Source);

            if (source != node.Source)
                return new SkipLinqExpression(node.Method, source, node.Count);

            return node;
        }

        protected internal override Expression VisitTake(TakeLinqExpression node)
        {
            var source = Visit(node.Source);

            if (source != node.Source)
                return new TakeLinqExpression(node.Method, source, node.Count);

            return node;
        }

        protected internal override Expression VisitSelect(SelectLinqExpression node)
        {
            var source = Visit(node.Source);

            if (source != node.Source)
                return new SelectLinqExpression(node.Method, source, node.Selector);

            return node;
        }

        protected internal override Expression VisitSelectMany(SelectManyLinqExpression node)
        {
            var source = Visit(node.Source);

            if (source != node.Source)
                return new SelectManyLinqExpression(node.Method, source, node.CollectionSelector, node.ResultSelector);

            return node;
        }

        protected internal override Expression VisitWhere(WhereLinqExpression node)
        {
            var source = Visit(node.Source);

            if (source != node.Source)
                return new WhereLinqExpression(node.Method, source, node.Predicate);

            return node;
        }
    }
}
