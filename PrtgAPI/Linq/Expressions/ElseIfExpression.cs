using System;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions
{
    class ElseIfExpression : ExpressionEx
    {
        public ConditionalExpression[] If { get; }

        public Expression Else { get; }

        public override Type Type => Reduce().Type;

        public ElseIfExpression(ConditionalExpression[] @if, Expression @else) : base(ExpressionTypeEx.ElseIf)
        {
            If = @if;
            Else = @else;
        }

        public override bool CanReduce => true;

        public override Expression Reduce()
        {
            var elseBlock = Else;

            for (var i = If.Length - 1; i >= 0; i--)
            {
                elseBlock = IfThenElse(
                    If[i].Test,
                    If[i].IfTrue,
                    elseBlock
                );
            }

            return elseBlock;
        }
    }
}
