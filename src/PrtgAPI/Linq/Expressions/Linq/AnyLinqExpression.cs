using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    /// <summary>
    /// Represents a <see cref="Queryable.Any{TSource}(IQueryable{TSource})"/> or <see cref="Queryable.Any{TSource}(IQueryable{TSource}, Expression{System.Func{TSource, bool}})"/> method call.
    /// </summary>
    class AnyLinqExpression : ConditionLinqExpression
    {
        public AnyLinqExpression(MethodCallExpression node, LinqInitializer init) : base(node, init, ExpressionTypeEx.AnyLinq)
        {
        }

        public AnyLinqExpression(MethodCallExpression node, Expression source, LambdaExpression predicate) : base(node, source, predicate, ExpressionTypeEx.AnyLinq)
        {
        }

        public override Expression Accept(LinqExpressionVisitor visitor)
        {
            return visitor.VisitAny(this);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"{Source}.AnyExpr({Predicate})";
        }
    }
}
