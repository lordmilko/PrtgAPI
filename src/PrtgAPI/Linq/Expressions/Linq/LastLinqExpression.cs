using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    class LastLinqExpression : ConditionLinqExpression
    {
        public LastLinqExpression(MethodCallExpression node, LinqInitializer init) : base(node, init, ExpressionTypeEx.LastLinq)
        {
        }

        public LastLinqExpression(MethodCallExpression node, Expression source, LambdaExpression predicate) : base(node, source, predicate, ExpressionTypeEx.LastLinq)
        {
        }

        public override Expression Accept(LinqExpressionVisitor visitor)
        {
            return visitor.VisitLast(this);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"{Source}.LastExpr({Predicate})";
        }
    }
}
