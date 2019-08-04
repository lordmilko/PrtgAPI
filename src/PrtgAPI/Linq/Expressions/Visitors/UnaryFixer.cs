using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Substitutes <see cref="UnaryExpression"/>s with an equivalent expression more easily parsable,
    /// such as !val -> val == false
    /// </summary>
    class UnaryFixer : PropertyExpressionVisitor
    {
        public static Expression Fix(Expression expr)
        {
            var fixer = new UnaryFixer();

            return fixer.Visit(expr);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not)
                return Visit(Expression.MakeBinary(ExpressionType.Equal, node.Operand, Expression.Constant(false)));

            return base.VisitUnary(node);
        }
    }
}
