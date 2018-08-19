using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    class PrtgExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            if (node is PropertyExpression)
                return VisitProperty((PropertyExpression) node);

            var substExpr = node as SubstituteExpression;

            if (substExpr != null)
                return substExpr.Update(Visit(substExpr.Replacement));

            return base.VisitExtension(node);
        }

        protected virtual Expression VisitProperty(PropertyExpression property)
        {
            var expr = Visit(property.Expression);

            if (expr != property.Expression)
                return new PropertyExpression(expr, property.PropertyInfo);

            return property;
        }
    }
}
