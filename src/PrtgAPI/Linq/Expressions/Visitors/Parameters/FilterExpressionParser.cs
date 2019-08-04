using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors.Parameters
{
    /// <summary>
    /// Identifies a list of expressions that can be used to generate a <see cref="SearchFilter"/>., replacing processed
    /// <see cref="LinqExpression"/>s with their underlying <see cref="MethodCallExpression"/>.
    /// </summary>
    class FilterExpressionParser : PropertyExpressionVisitor
    {
        /// <summary>
        /// The legal conditions to filter on in the initial API request.
        /// </summary>
        public List<Expression> Conditions { get; } = new List<Expression>();

        protected internal override Expression VisitAny(AnyLinqExpression node)
        {
            //If Any is not valid in the given context (e.g. Where(cond).Any(cond)) our AnyLinqExpression
            //will have already been reverted back to a MethodCallExpression, and won't be processed here
            return VisitCondition(node);
        }

        protected internal override Expression VisitCount(CountLinqExpression node) => VisitCondition(node);
        protected internal override Expression VisitFirst(FirstLinqExpression node) => VisitCondition(node);
        protected internal override Expression VisitLast(LastLinqExpression node) => VisitCondition(node);
        protected internal override Expression VisitWhere(WhereLinqExpression node) => VisitCondition(node);

        private Expression VisitCondition(ConditionLinqExpression node)
        {
            var reducedSource = Visit(node.Source);

            var expander = new ConditionExpander();

            var predicate = expander.Visit(node.Predicate.Body);

            var wrapperPredicate = Expression.MakeBinary(ExpressionType.Equal, predicate, Expression.Constant(true));

            Visit(wrapperPredicate);

            return node.Reduce(reducedSource);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not)
                return UnaryFixer.Fix(node);

            return base.VisitUnary(node);
        }

        /// <summary>
        /// Identifies conditions to include from a <see cref="WhereLinqExpression"/>.
        /// </summary>
        /// <param name="node">The binary expression to analyze.</param>
        /// <returns>The original binary expression.</returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    Visit(node.Left);
                    Visit(node.Right);
                    return node;
                default:
                    var types = ExpressionSearcher.GetTypes(node);

                    if (types.Contains(ExpressionType.AndAlso) || types.Contains(ExpressionType.OrElse))
                    {
                        Visit(node.Left);
                        Visit(node.Right);
                        return node;
                    }
                    else
                    {
                        var visited = UnaryFixer.Fix(node);

                        Conditions.Add(visited);

                        return node;
                    }
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                case FilterMethod.Contains:
                    Conditions.Add(node);
                    break;
                case FilterMethod.Equals:
                    Conditions.Add(node);
                    break;
                default:
                    //Visit the source, but not the lambda
                    if (ExpressionHelpers.IsQueryMethod(node) && node.Arguments.Count > 0)
                    {
                        var source = Visit(node.Arguments[0]);

                        if (source != node.Arguments[0])
                        {
                            var args = new List<Expression> {source};
                            args.AddRange(node.Arguments.Skip(1));

                            return Expression.Call(node.Object, node.Method, args);
                        }
                    }

                    return base.VisitMethodCall(node);
            }

            return node;
        }
    }
}
