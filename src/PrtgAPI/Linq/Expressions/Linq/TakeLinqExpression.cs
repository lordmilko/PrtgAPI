using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    class TakeLinqExpression : LinqExpression, ICountLimitLinqExpression
    {
        public ConstantExpression Count { get; }

        public TakeLinqExpression(MethodCallExpression node, LinqInitializer init) : base(node, init, ExpressionTypeEx.TakeLinq)
        {
            Count = (ConstantExpression) node.Arguments[1];
        }

        public TakeLinqExpression(MethodCallExpression node, Expression source, ConstantExpression count) : base(node, source, ExpressionTypeEx.TakeLinq)
        {
            Count = count;
        }

        public override Expression Accept(LinqExpressionVisitor visitor)
        {
            return visitor.VisitTake(this);
        }

        protected override bool IsOriginalMethod()
        {
            return Method.Arguments[1] == Count;
        }

        public override Expression Reduce(Expression source)
        {
            return Reduce(source, Count);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"{Source}.TakeExpr({Count})";
        }
    }
}
