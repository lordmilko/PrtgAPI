using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PrtgAPI.Linq.Expressions;

namespace PrtgAPI.Linq
{
    abstract class QueryProvider : IQueryProvider
    {
        #region IQueryProvider

        public IQueryable<T> CreateQuery<T>(Expression expression) => new Query<T>(this, expression);

        [ExcludeFromCodeCoverage]
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.GetElementType();
            
            try
            {
                return (IQueryable) Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), this, expression);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public T Execute<T>(Expression expression) => (T) Execute(expression);

        object IQueryProvider.Execute(Expression expression) => Execute(expression);

        #endregion

        public abstract object Execute(Expression expression);
    }
}
