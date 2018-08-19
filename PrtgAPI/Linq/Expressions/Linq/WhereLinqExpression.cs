using System;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    /// <summary>
    /// Represents a <see cref="Queryable.Where{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})"/> or <see cref="Queryable.Where{TSource}(IQueryable{TSource}, Expression{Func{TSource, int, bool}})"/> method call.
    /// </summary>
    class WhereLinqExpression : ConditionLinqExpression
    {
        public WhereLinqExpression(MethodCallExpression node, LinqInitializer init) : base(node, init, ExpressionTypeEx.WhereLinq)
        {
        }

        public WhereLinqExpression(MethodCallExpression node, Expression source, LambdaExpression predicate) : base(node, source, predicate, ExpressionTypeEx.WhereLinq)
        {
        }

        public override Expression Accept(LinqExpressionVisitor visitor)
        {
            return visitor.VisitWhere(this);
        }

        public override string ToString()
        {
            return $"{Source}.WhereExpr({Predicate})";
        }
    }
}