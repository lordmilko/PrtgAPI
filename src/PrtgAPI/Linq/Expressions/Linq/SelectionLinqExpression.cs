using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions
{
    abstract class SelectionLinqExpression : LinqExpression
    {
        protected LambdaExpression collectionSelector;

        protected SelectionLinqExpression(MethodCallExpression node, LinqInitializer init, ExpressionTypeEx type) : base(node, init, type)
        {
            var rawSelector = GetLambda(node.Arguments[1]);

            collectionSelector = init.Evaluator(Source, rawSelector);
        }

        protected SelectionLinqExpression(MethodCallExpression node, Expression source, LambdaExpression selector, ExpressionTypeEx type) : base(node, source, type)
        {
            collectionSelector = selector;
        }

        protected override bool IsOriginalMethod()
        {
            return Method.Arguments[1] == collectionSelector;
        }

        public override bool CanUse(ConsecutiveCallManager manager)
        {
            return !manager.HasUnknownMethod();
        }
    }
}
