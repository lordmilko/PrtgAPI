using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    class SelectLinqExpression : SelectionLinqExpression
    {
        public LambdaExpression Selector => collectionSelector;

        public SelectLinqExpression(MethodCallExpression node, LinqInitializer init) : base(node, init, ExpressionTypeEx.SelectLinq)
        {
        }

        public SelectLinqExpression(MethodCallExpression node, Expression source, LambdaExpression selector) : base(node, source, selector, ExpressionTypeEx.SelectLinq)
        {
        }

        public override Expression Accept(LinqExpressionVisitor visitor)
        {
            return visitor.VisitSelect(this);
        }

        /// <summary>
        /// Reduces this expression to its underlying <see cref="MethodCallExpression"/> using a specified source (that has potentially already been reduced).
        /// </summary>
        /// <param name="source">The TSource of the method.</param>
        /// <returns>A <see cref="MethodCallExpression"/> representing a call to Queryable.Select</returns>
        public override Expression Reduce(Expression source)
        {
            return Reduce(source, Selector);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"{Source}.SelectExpr({Selector})";
        }
    }
}
