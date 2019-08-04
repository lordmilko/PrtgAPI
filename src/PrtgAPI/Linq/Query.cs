using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace PrtgAPI.Linq
{
    [ExcludeFromCodeCoverage]
    class Query<T> : IOrderedQueryable<T>
    {
        public Query(QueryProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            Provider = provider;
            Expression = Expression.Constant(this);
        }

        public Query(QueryProvider provider, Expression expression)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
                throw new ArgumentException($"Expression of type {expression.Type} is not assignable from {nameof(IQueryable<T>)}.", nameof(expression));

            Provider = provider;
            Expression = expression;
        }

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)Provider.Execute(Expression)).GetEnumerator();

        #endregion
        #region IEnumerable<T>

        public IEnumerator<T> GetEnumerator() =>
            ((IEnumerable<T>) Provider.Execute(Expression)).GetEnumerator();

        #endregion
        #region IQueryable

        public Expression Expression { get; }

        public Type ElementType => typeof(T);

        public IQueryProvider Provider { get; }

        #endregion

        public override string ToString()
        {
            if (Expression.NodeType == ExpressionType.Constant && ((ConstantExpression) Expression).Value == this)
                return $"Query({typeof(T)})";

            return Expression.ToString();
        }
    }
}
