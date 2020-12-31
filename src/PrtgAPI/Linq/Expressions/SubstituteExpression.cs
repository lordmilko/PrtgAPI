using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    /// <summary>
    /// Represents the substitution of the original body of a <see cref="LambdaExpression"/> before being parsed by a <see cref="PropertyEvaluator{T}"/>.
    /// </summary>
    class SubstituteExpression : ExpressionEx
    {
        public override Type Type => Original.Type;

        public override bool CanReduce => true;

        /// <summary>
        /// Returns the original body of the <see cref="LambdaExpression"/>.
        /// </summary>
        /// <returns>The original body of the <see cref="LambdaExpression"/>.</returns>
        public override Expression Reduce() => Original;

        public Expression Original { get; }

        public Expression Replacement { get; }

        public SubstituteExpression(Expression original, Expression replacement) : base(ExpressionTypeEx.Substitute)
        {
            Original = original;
            Replacement = replacement;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"SubstExpr({Replacement})";

        public static Expression Strip(Expression expr)
        {
            var substitute = expr as SubstituteExpression;

            if (substitute != null)
                return Strip(substitute.Replacement);

            return expr;
        }

        public Expression Update(Expression replacement)
        {
            if (replacement != Replacement)
                return new SubstituteExpression(Original, replacement);

            return this;
        }
    }
}