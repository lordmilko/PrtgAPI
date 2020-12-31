using System;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Linq.Expressions
{
    abstract class LinqExpression : ExpressionEx
    {
        public Expression Source { get; }

        public MethodCallExpression Method { get; }

        protected LinqExpression(MethodCallExpression node, LinqInitializer init, ExpressionTypeEx type) : base(type)
        {
            Method = node;
            Source = init.Visitor.Visit(node.Arguments[0]);
        }

        protected LinqExpression(MethodCallExpression node, Expression source, ExpressionTypeEx type) : base(type)
        {
            Method = node;
            Source = source;
        }

        public override Type Type => Method.Method.ReturnType;

        private bool IsOriginal()
        {
            return Method.Arguments[0] == Source && IsOriginalMethod();
        }

        protected abstract bool IsOriginalMethod();

        public abstract Expression Accept(LinqExpressionVisitor visitor);

        public override bool CanReduce => true;

        /// <summary>
        /// Reduces the expression to its <see cref="MethodCallExpression"/>. If no sub-expressions have changed,
        /// returns the original <see cref="MethodCallExpression"/>.
        /// </summary>
        /// <returns>The <see cref="MethodCallExpression"/> this node encapsulates.</returns>
        public override Expression Reduce()
        {
            if (IsOriginal())
                return Method;

            return Reduce(Source);
        }

        protected Expression Reduce(Expression source, Expression arg1, Expression arg2)
        {
            if (arg2 == null)
                return Reduce(source, arg1);

            return Call(null, Method.Method, source, ExtensionReducer.Reduce(arg1), ExtensionReducer.Reduce(arg2));
        }

        protected Expression Reduce(Expression source, Expression arg1)
        {
            if (arg1 == null)
                return Call(null, Method.Method, source);

            return Call(null, Method.Method, source, ExtensionReducer.Reduce(arg1));
        }

        public abstract Expression Reduce(Expression source);

        protected LambdaExpression GetLambda(Expression expression) => VisitHelpers.GetLambda(expression);

        public virtual bool CanUse(ConsecutiveCallManager manager)
        {
            //Cannot call a query if we were preceeded by Take() or Unsupported()
            return !manager.HasCountLimiter(GetType()) && !manager.HasUnknownMethod();
        }
    }
}