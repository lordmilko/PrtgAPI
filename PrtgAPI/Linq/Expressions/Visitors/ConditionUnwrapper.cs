using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    class ConditionUnwrapper : LinqExpressionVisitor
    {
        public static Expression Unwrap(Expression expr)
        {
            var visitor = new ConditionUnwrapper();
            var result = visitor.Visit(expr);
            return result;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var obj = Visit(node.Object);

            if (obj != null)
            {
                while (obj.NodeType == ExpressionType.Conditional)
                    obj = ((ConditionalExpression)obj).IfFalse;
            }

            if (obj != node.Object)
                node = Expression.Call(obj, node.Method, node.Arguments);

            return node;
        }
    }
}
