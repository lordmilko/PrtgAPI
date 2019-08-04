using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    class CountLinqExpression : ConditionLinqExpression
    {
        public CountLinqExpression(MethodCallExpression node, LinqInitializer init) : base(node, init, ExpressionTypeEx.CountLinq)
        {
        }

        public CountLinqExpression(MethodCallExpression node, Expression source, LambdaExpression predicate) : base(node, source, predicate, ExpressionTypeEx.CountLinq)
        {
        }

        public override Expression Accept(LinqExpressionVisitor visitor)
        {
            return visitor.VisitCount(this);
        }

        public override string ToString()
        {
            return $"{Source}.CountExpr({Predicate})";
        }
    }
}