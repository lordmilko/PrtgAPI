using System;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    /// <summary>
    /// Helper class for constructing new <see cref="LinqExpression"/>s.<para/>
    /// Wraps the <see cref="ExpressionVisitor"/> that is trying to construct the <see cref="LinqExpression"/>, as well the
    /// <see cref="PropertyEvaluator{T}"/> evaluator used to construct <see cref="ParameterExpression"/>s.
    /// </summary>
    class LinqInitializer
    {
        public ExpressionVisitor Visitor { get; }

        public Func<Expression, LambdaExpression, LambdaExpression> Evaluator { get; }

        public LinqInitializer(ExpressionVisitor visitor, Func<Expression, LambdaExpression, LambdaExpression> evaluator)
        {
            Visitor = visitor;
            Evaluator = evaluator;
        }
    }
}