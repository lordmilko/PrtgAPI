using System.Collections.Generic;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    class ExpressionParentSearcher : BaseExpressionSearcher
    {
        internal Stack<Expression> parentStack = new Stack<Expression>();

        private Expression child;

        private bool childFound;

        public ExpressionParentSearcher(Expression child)
        {
            this.child = child;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;

            if (!childFound)
            {
                if (node == child)
                {
                    childFound = true;
                    return node;
                }

                parentStack.Push(node);

                node = base.Visit(node);

                if (!childFound)
                    parentStack.Pop();

                return node;
            }

            return node;
        }
    }
}