using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    /// <summary>
    /// Represents a <see cref="Queryable.First{TSource}(IQueryable{TSource}, Expression{System.Func{TSource, bool}})"/> or <see cref="Queryable.FirstOrDefault{TSource}(IQueryable{TSource}, Expression{System.Func{TSource, bool}})"/> method call.
    /// </summary>
    class FirstLinqExpression : ConditionLinqExpression
    {
        public FirstLinqExpression(MethodCallExpression node, LinqInitializer init) : base(node, init, ExpressionTypeEx.FirstLinq)
        {
        }

        public FirstLinqExpression(MethodCallExpression node, Expression source, LambdaExpression predicate) : base(node, source, predicate, ExpressionTypeEx.FirstLinq)
        {
        }

        public override Expression Accept(LinqExpressionVisitor visitor)
        {
            return visitor.VisitFirst(this);
        }

        public override string ToString()
        {
            return $"{Source}.FirstExpr({Predicate})";
        }
    }
}
