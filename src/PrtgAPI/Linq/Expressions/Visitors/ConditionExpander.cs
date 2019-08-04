using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Expands a <see cref="BinaryExpression"/> like ((A) xx (B == v)) to ((A == true) xx ((B == v) == true)).<para/>
    /// Extraneous checks against true will be removed later by <see cref="ConditionReducer"/>.
    /// </summary>
    class ConditionExpander : PropertyExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (ExpressionHelpers.IsBool(node.Left) || ExpressionHelpers.IsBool(node.Right))
                return node;

            //expand something like s.Prop1 in s.Prop1 || s.Prop2 == val
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            var newLeft = GetNew(left);
            var newRight = GetNew(right);

            if (newLeft != node.Left || newRight != node.Right)
                return Expression.MakeBinary(node.NodeType, newLeft, newRight);

            return node;
        }

        private Expression GetNew(Expression expr)
        {
            if (expr.Type == typeof(bool) && !(expr is ConstantExpression || expr is BinaryExpression))
                return Expression.MakeBinary(ExpressionType.Equal, expr, Expression.Constant(true));

            return expr;
        }
    }
}