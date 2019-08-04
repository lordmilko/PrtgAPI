using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Represents a visitor or rewriter for LINQ expression trees.
    /// </summary>
    [ExcludeFromCodeCoverage]
    abstract class LinqExpressionVisitor : PrtgExpressionVisitor
    {
        /// <summary>
        /// Visits the children of a <see cref="AnyLinqExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>If any children were modified, a new <see cref="AnyLinqExpression"/>. Otherwise, the original expression.</returns>
        protected internal virtual Expression VisitAny(AnyLinqExpression node)
        {
            //By virtue of the fact we exist we must have a predicate
            var source = Visit(node.Source);
            var predicate = (LambdaExpression) VisitPredicate(node.Predicate);

            if (source != node.Source || predicate != node.Predicate)
                return new AnyLinqExpression(node.Method, source, predicate);

            return node;
        }

        /// <summary>
        /// Visits the children of a <see cref="CountLinqExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>If any children were modified, a new <see cref="CountLinqExpression"/>. Otherwise, the original expression.</returns>
        protected internal virtual Expression VisitCount(CountLinqExpression node)
        {
            //By virtue of the fact we exist we must have a predicate
            var source = Visit(node.Source);
            var predicate = (LambdaExpression) VisitPredicate(node.Predicate);

            if (source != node.Source || predicate != node.Predicate)
                return new CountLinqExpression(node.Method, source, predicate);

            return node;
        }

        /// <summary>
        /// Visits the children of a <see cref="FirstLinqExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>If any children were modified, a new <see cref="FirstLinqExpression"/>. Otherwise, the original expression.</returns>
        protected internal virtual Expression VisitFirst(FirstLinqExpression node)
        {
            var source = Visit(node.Source);
            var predicate = (LambdaExpression) VisitPredicate(node.Predicate);

            if (source != node.Source || predicate != node.Predicate)
                return new FirstLinqExpression(node.Method, source, predicate);

            return node;
        }

        /// <summary>
        /// Visits the children of a <see cref="LastLinqExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>If any children were modified, a new <see cref="LastLinqExpression"/>. Otherwise, the original expression.</returns>
        protected internal virtual Expression VisitLast(LastLinqExpression node)
        {
            var source = Visit(node.Source);
            var predicate = (LambdaExpression) VisitPredicate(node.Predicate);

            if (source != node.Source || predicate != node.Predicate)
                return new LastLinqExpression(node.Method, source, predicate);

            return node;
        }

        /// <summary>
        /// Visits the children of a <see cref="OrderByLinqExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>If any children were modified, a new <see cref="OrderByLinqExpression"/>. Otherwise, the original expression.</returns>
        protected internal virtual Expression VisitOrderBy(OrderByLinqExpression node)
        {
            var source = Visit(node.Source);
            var keySelector = (LambdaExpression)Visit(node.KeySelector);
            var comparer = Visit(node.Comparer);

            if (source != node.Source || keySelector != node.KeySelector || comparer != node.Comparer)
                return new OrderByLinqExpression(node.Method, source, keySelector, comparer, node.SortDirection);

            return node;
        }

        /// <summary>
        /// Visits the children of a <see cref="SelectLinqExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>If any children were modified, a new <see cref="SelectLinqExpression"/>. Otherwise, the original expression.</returns>
        protected internal virtual Expression VisitSelect(SelectLinqExpression node)
        {
            var source = Visit(node.Source);
            var selector = (LambdaExpression) Visit(node.Selector);

            if (source != node.Source || selector != node.Selector)
                return new SelectLinqExpression(node.Method, source, selector);
            
            return node;
        }

        /// <summary>
        /// Visits the children of a <see cref="SelectManyLinqExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>If any children were modified, a new <see cref="SelectManyLinqExpression"/>. Otherwise, the original expression.</returns>
        protected internal virtual Expression VisitSelectMany(SelectManyLinqExpression node)
        {
            var source = Visit(node.Source);
            var selector = (LambdaExpression)Visit(node.CollectionSelector);
            var resultSelector = (LambdaExpression) Visit(node.ResultSelector);

            if (source != node.Source || selector != node.CollectionSelector || resultSelector != node.ResultSelector)
                return new SelectManyLinqExpression(node.Method, source, selector, resultSelector);

            return node;
        }

        /// <summary>
        /// Visits the children of a <see cref="SkipLinqExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>If any children were modified, a new <see cref="SkipLinqExpression"/>. Otherwise, the original expression.</returns>
        protected internal virtual Expression VisitSkip(SkipLinqExpression node)
        {
            var source = Visit(node.Source);
            var count = (ConstantExpression)Visit(node.Count);

            if (source != node.Source || count != node.Count)
                return new SkipLinqExpression(node.Method, source, count);

            return node;
        }

        /// <summary>
        /// Visits the children of a <see cref="TakeLinqExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>If any children were modified, a new <see cref="TakeLinqExpression"/>. Otherwise, the original expression.</returns>
        protected internal virtual Expression VisitTake(TakeLinqExpression node)
        {
            var source = Visit(node.Source);
            var count = (ConstantExpression)Visit(node.Count);

            if (source != node.Source || count != node.Count)
                return new TakeLinqExpression(node.Method, source, count);

            return node;
        }

        /// <summary>
        /// Visits the children of a <see cref="WhereLinqExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>If any children were modified, a new <see cref="WhereLinqExpression"/>. Otherwise, the original expression.</returns>
        protected internal virtual Expression VisitWhere(WhereLinqExpression node)
        {
            var source = Visit(node.Source);
            var predicate = (LambdaExpression) VisitPredicate(node.Predicate);

            if (source != node.Source || predicate != node.Predicate)
                return new WhereLinqExpression(node.Method, source, predicate);

            return node;
        }

        protected virtual Expression VisitPredicate(Expression node)
        {
            return Visit(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node is LinqExpression)
                return ((LinqExpression)node).Accept(this);

            return base.VisitExtension(node);
        }
    }
}