using System;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors.Parameters
{
    class CountExpressionParser : PropertyExpressionVisitor
    {
        public int? Count { get; private set; }

        public int? Skip { get; private set; }

        public bool ReverseSort { get; }

        protected internal override Expression VisitSkip(SkipLinqExpression node)
        {
            var reducedSource = Visit(node.Source);

            Skip = Convert.ToInt32(node.Count.Value);

            return node.Reduce(reducedSource);
        }

        protected internal override Expression VisitTake(TakeLinqExpression node)
        {
            var reducedSource = Visit(node.Source);

            Count = Convert.ToInt32(node.Count.Value);

            return node.Reduce(reducedSource);
        }
    }
}
