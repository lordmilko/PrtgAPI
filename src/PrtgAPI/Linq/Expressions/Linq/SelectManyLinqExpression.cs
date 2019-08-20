using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    class SelectManyLinqExpression : SelectionLinqExpression
    {
        public LambdaExpression CollectionSelector => collectionSelector;
        public LambdaExpression ResultSelector { get; }

        public SelectManyLinqExpression(MethodCallExpression node, LinqInitializer init) : base(node, init, ExpressionTypeEx.SelectManyLinq)
        {
            if (node.Arguments.Count == 3)
            {
                var rawResultSelector = GetLambda(node.Arguments[2]);

                ResultSelector = init.Evaluator(Source, rawResultSelector);
            }
        }

        public SelectManyLinqExpression(MethodCallExpression node, Expression source, LambdaExpression selector, LambdaExpression resultSelector) :
            base(node, source, selector, ExpressionTypeEx.SelectManyLinq)
        {
            ResultSelector = resultSelector;
        }

        public override Expression Accept(LinqExpressionVisitor visitor)
        {
            return visitor.VisitSelectMany(this);
        }

        protected override bool IsOriginalMethod()
        {
            if (ResultSelector == null)
                return base.IsOriginalMethod();

            return base.IsOriginalMethod() && Method.Arguments[2] == ResultSelector;
        }

        public override Expression Reduce(Expression source)
        {
            return Reduce(source, CollectionSelector, ResultSelector);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (ResultSelector == null)
                return $"{Source}.SelectManyExpr({CollectionSelector})";

            return $"{Source}.SelectManyExpr({CollectionSelector}, {ResultSelector})";
        }
    }
}
