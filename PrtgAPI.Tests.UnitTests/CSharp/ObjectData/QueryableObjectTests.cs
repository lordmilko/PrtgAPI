using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    public abstract class QueryableObjectTests<TObject, TItem, TResponse> : StreamableObjectTests<TObject, TItem, TResponse>
        where TResponse : IWebResponse
        where TObject : ITableObject, IObject
    {
        protected void Object_GetObjectsOverloads_Query_CanExecute(
            Func<PrtgClient, Func<IQueryable<TObject>>> regularValue,
            Func<PrtgClient, Func<bool, IQueryable<TObject>>> strictValue,
            Func<PrtgClient, Func<Expression<Func<TObject, bool>>, IQueryable<TObject>>> predicateValue,
            Func<PrtgClient, Func<Expression<Func<TObject, bool>>, bool, IQueryable<TObject>>> strictPredicateValue
        )
        {
            var queryClient = Initialize_Client_WithItems(Enumerable.Range(0, 755).Select(i => GetItem()).ToArray());

            var regularFunction = regularValue(queryClient);
            var strictFunction = strictValue(queryClient);
            var predicateFunction = predicateValue(queryClient);
            var strictPredicateFunction = strictPredicateValue(queryClient);

            CheckResult<List<Sensor>>(regularFunction().ToList());
            CheckResult<List<Sensor>>(strictFunction(true).ToList());
            CheckResult<List<Sensor>>(predicateFunction(s => s.Name.Contains("a") || s.Name.Contains("e") || s.Name.Contains("1")).ToList());
            CheckResult<List<Sensor>>(strictPredicateFunction(s => s.Name.Contains("a") || s.Name.Contains("e") || s.Name.Contains("1"), true).ToList());
        }
    }
}
