using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    abstract class BaseExpressionSearcher : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            var substExpr = node as SubstituteExpression;

            if (substExpr != null)
                return substExpr.Update(Visit(substExpr.Replacement));

            if (node.CanReduce)
                return base.VisitExtension(node);

            return node;
        }
    }
}