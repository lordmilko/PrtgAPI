using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Identifies subexpressions that can be pre-evaluated locally and do not have a dependency on data returned from PRTG.
    /// </summary>
    class LocalExpressionAnalyzer : ExpressionVisitor
    {
        private Func<Expression, bool> isLocalExpression;
        private HashSet<Expression> localExpressions;

        private bool cannotBeEvaluated;

        public LocalExpressionAnalyzer(Func<Expression, bool> isLocalExpression)
        {
            this.isLocalExpression = isLocalExpression;
        }

        public HashSet<Expression> GetLocalExpressions(Expression expression)
        {
            localExpressions = new HashSet<Expression>();

            Visit(expression);

            return localExpressions;
        }

        public override Expression Visit(Expression node)
        {
            if (node != null)
            {
                var oldCannotBeEvaluated = cannotBeEvaluated;
                cannotBeEvaluated = false;

                base.Visit(node);

                //If my child already identified he can't be evaluated locally, I (the parent) can't be evaluated locally either.
                //That way, we maintain the relationship all the way from the root down to the deepest nodes that can't be evaluated
                //locally
                if (!cannotBeEvaluated)
                {
                    if (isLocalExpression(node))
                        localExpressions.Add(node);
                    else
                        cannotBeEvaluated = true;
                }

                cannotBeEvaluated |= oldCannotBeEvaluated;
            }

            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            //We have to visit MemberInitExpressions manually, as otherwise node.NewExpression will be passed to Visit, causing it to be
            //added to the candidate list

            var newExpr = (NewExpression) VisitNew(node.NewExpression);
            var bindings = Visit(node.Bindings, VisitMemberBinding);

            if (newExpr != node.NewExpression || bindings != node.Bindings)
                return Expression.MemberInit(newExpr, bindings);

            return node;
        }
    }
}
