using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions
{
    abstract class ConditionLinqExpression : LinqExpression
    {
        public LambdaExpression Predicate { get; }

        protected ConditionLinqExpression(MethodCallExpression node, LinqInitializer init, ExpressionTypeEx type) : base(node, init, type)
        {
            if (node.Arguments.Count > 1)
            {
                var rawPredicate = GetLambda(node.Arguments[1]);

                Predicate = init.Evaluator(Source, rawPredicate);
            }
        }

        protected ConditionLinqExpression(MethodCallExpression node, Expression source, LambdaExpression predicate, ExpressionTypeEx type) : base(node, source, type)
        {
            Predicate = predicate;
        }

        /// <summary>
        /// Reduces this expression to its underlying <see cref="MethodCallExpression"/> using a specified source (that has potentially already been reduced).
        /// </summary>
        /// <param name="source">The TSource of the method.</param>
        /// <returns>A <see cref="MethodCallExpression"/> representing a call to the original Queryable</returns>
        public override Expression Reduce(Expression source)
        {
            //Even if expressions without predicates aren't valid, they are valid as far as trying to
            //abort and go back to normal goes
            return Reduce(source, Predicate);
        }

        protected override bool IsOriginalMethod()
        {
            if (Method.Arguments.Count > 1)
                return Method.Arguments[1] == Predicate;

            return Predicate == null;
        }

        public override bool CanUse(ConsecutiveCallManager manager)
        {
            //Cannot call a query if this query has a predicate and we were preceeded by a condition of another type,
            //Skip(), Take() or Unsupported()
            return Predicate != null && !manager.HasOtherConditions(GetType()) && !manager.HasCountOffset(GetType()) && base.CanUse(manager);
        }
    }
}