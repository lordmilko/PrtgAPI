using System.Collections.Generic;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    class ExpressionTypeSearcher : BaseExpressionSearcher
    {
        internal HashSet<ExpressionType> types = new HashSet<ExpressionType>();

        public override Expression Visit(Expression node)
        {
            if (node != null)
                types.Add(node.NodeType);

            return base.Visit(node);
        }
    }
}