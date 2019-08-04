using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData.Query
{
    public class BaseQueryTest : BasePrtgClientTest
    {
        protected void ExecuteFilter(System.Linq.Expressions.Expression<Func<Sensor, bool>> predicate, Action<List<Sensor>> validator)
        {
            ExecuteQuery(q => q.Where(predicate), validator);
        }

        protected void ExecuteQuery(Func<IQueryable<Sensor>, IQueryable<Sensor>> func, Action<List<Sensor>> validator)
        {
            var result = func(client.QuerySensors()).ToList();

            validator(result);
        }

        protected void ExecuteClient<TResult>(Func<PrtgClient, TResult> action, Action<TResult> validator)
        {
            var result = action(client);

            validator(result);
        }

        protected void ExecuteNullable<TResult, TProp>(
            Func<IQueryable<Sensor>, IQueryable<TResult>> queryFunc,
            Func<List<TResult>> normal,
            Func<TResult, TProp> nullValidator,
            Action<List<TResult>> queryResultValidator)
        {
            var preNormalResults = normal();

            foreach (var result in preNormalResults)
            {
                Assert.AreEqual(null, nullValidator(result), $"Property in object {result} in pre-validator was not null");
            }

            var queryResults = queryFunc(client.QuerySensors()).ToList();

            foreach (var result in queryResults)
            {
                Assert.AreEqual(null, nullValidator(result), $"Property in object {result} in query validator was not null");
            }

            queryResultValidator(queryResults);

            var postNormalResults = normal();

            foreach (var result in postNormalResults)
            {
                Assert.AreEqual(null, nullValidator(result), $"Property in object {result} in post-validator was not null");
            }
        }

        protected void Execute<TResult>(Func<IQueryable<Sensor>, IQueryable<TResult>> func, Action<List<TResult>> validator)
        {
            var result = func(client.QuerySensors());

            validator(result.ToList());
        }

        protected void ExecuteNow<TResult>(Func<IQueryable<Sensor>, TResult> func, Action<TResult> validator)
        {
            var result = func(client.QuerySensors());

            validator(result);
        }

        protected void HasAllTests(Type unitTestClass)
        {
            var expected = TestHelpers.GetTests(unitTestClass).Select(m => $"Data_{m.Name}").ToList();

            TestHelpers.Assert_TestClassHasMethods(GetType(), expected);
        }
    }
}
