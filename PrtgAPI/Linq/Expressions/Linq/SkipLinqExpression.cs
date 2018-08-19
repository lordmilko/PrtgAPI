using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    class SkipLinqExpression : LinqExpression, ICountOffsetLinqExpression
    {
        public ConstantExpression Count { get; }

        public SkipLinqExpression(MethodCallExpression node, LinqInitializer init) : base(node, init, ExpressionTypeEx.SkipLinq)
        {
            Count = (ConstantExpression)node.Arguments[1];
        }

        public SkipLinqExpression(MethodCallExpression node, Expression source, ConstantExpression count) : base(node, source, ExpressionTypeEx.SkipLinq)
        {
            Count = count;
        }

        public override Expression Accept(LinqExpressionVisitor visitor)
        {
            return visitor.VisitSkip(this);
        }

        protected override bool IsOriginalMethod()
        {
            return Method.Arguments[1] == Count;
        }

        public override Expression Reduce(Expression source)
        {
            return Reduce(source, Count);
        }

        public override string ToString()
        {
            return $"{Source}.SkipExpr({Count})";
        }

        public override bool CanUse(ConsecutiveCallManager manager)
        {
            return !manager.HasCountLimiter(GetType());
        }
    }
}
