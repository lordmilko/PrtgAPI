using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    class OrderByLinqExpression : LinqExpression
    {
        public LambdaExpression KeySelector { get; }

        public Expression Comparer { get; set; }

        public SortDirection SortDirection { get; }

        public OrderByLinqExpression(MethodCallExpression node, SortDirection direction, LinqInitializer init) : base(node, init, ExpressionTypeEx.OrderByLinq)
        {
            var rawSelector = GetLambda(node.Arguments[1]);

            KeySelector = init.Evaluator(Source, rawSelector);

            SortDirection = direction;

            if (node.Arguments.Count == 3)
                Comparer = node.Arguments[2];
        }

        public OrderByLinqExpression(MethodCallExpression node, Expression source, LambdaExpression keySelector, Expression comparer, SortDirection direction) : base(node, source, ExpressionTypeEx.OrderByLinq)
        {
            KeySelector = keySelector;
            Comparer = comparer;
            SortDirection = direction;
        }

        public override Expression Accept(LinqExpressionVisitor visitor)
        {
            return visitor.VisitOrderBy(this);
        }

        protected override bool IsOriginalMethod()
        {
            return Method.Arguments[1] == KeySelector;
        }

        /// <summary>
        /// Reduces this expression to its underlying <see cref="MethodCallExpression"/> using a specified source (that has potentially already been reduced).
        /// </summary>
        /// <param name="source">The TSource of the method.</param>
        /// <returns>A <see cref="MethodCallExpression"/> representing a call to Queryable.OrderBy / Queryable.OrderByDescending</returns>
        public override Expression Reduce(Expression source)
        {
            return Reduce(source, KeySelector, Comparer);
        }

        public override string ToString()
        {
            var func = SortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";

            if (Comparer == null)
                return $"{Source}.{func}Expr({KeySelector})";

            return $"{Source}.{func}Expr({KeySelector}, {Comparer})";
        }
    }
}
