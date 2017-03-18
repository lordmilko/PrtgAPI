using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public abstract class StreamableObjectTests<TObject, TItem, TResponse> : ObjectTests<TObject, TItem, TResponse>
        where TResponse : IWebResponse
        where TObject : PrtgObject
    {
        private Property testProperty = Property.Name;
        private FilterOperator testOperator = FilterOperator.Contains;
        private string testValue = "test";
        private SearchFilter[] testFilters = new[]
        {
            new SearchFilter(Property.ParentId, 2001),
            new SearchFilter(Property.Probe, FilterOperator.Contains, "contoso")
        };

        protected void Object_CanStream_Ordered_FastestToSlowest()
        {
            var count = 2000;
            var perPage = 500;
            var pages = count / perPage;

            //bug: the issue is our sensorresponse has no way of knowing whether to do a normal response or do a streaming response

            var client = Initialize_Client_WithItems(Enumerable.Range(0, count).Select(i => GetItem()).ToArray());
            var results = Stream(client).Select(i => i.Id).ToList();
            Assert.IsTrue(results.Count == count, $"Expected {count} results but got {results.Count} instead.");

            for (int pageNum = pages; pageNum > 0; pageNum--)
            {
                var r = results.Skip((pages - pageNum) * perPage).Take(perPage).ToList();
                Assert.IsTrue(r.TrueForAll(item => item == pageNum));
            }
        }

        protected void Object_GetObjectsOverloads_CanExecute(
            Func<PrtgClient, PrtgClient, PrtgClient, List<Func<Property, object, object>>> propertyValue,
            Func<PrtgClient, PrtgClient, PrtgClient, List<Func<Property, FilterOperator, string, object>>> propertyOperatorValue,
            Func<PrtgClient, PrtgClient, PrtgClient, List<Func<SearchFilter[], object>>> searchFilters,
            Action<PrtgClient, PrtgClient, PrtgClient> other = null
        )
        {
            var synchronousClient = Initialize_Client_WithItems(GetItem());
            var asynchronousClient = Initialize_Client_WithItems(GetItem());
            var streamClient = Initialize_Client_WithItems(Enumerable.Range(0, 2000).Select(i => GetItem()).ToArray()); ;

            var propertyValueFunctions = propertyValue(synchronousClient, asynchronousClient, streamClient);
            var propertyOperatorValueFunctions = propertyOperatorValue(synchronousClient, asynchronousClient, streamClient);
            var searchFilterFunctions = searchFilters(synchronousClient, asynchronousClient, streamClient);

            if (propertyValueFunctions.Count != 3)
                throw new NotImplementedException();
            if (propertyOperatorValueFunctions.Count != 3)
                throw new NotImplementedException();
            if (searchFilterFunctions.Count != 3)
                throw new NotImplementedException();

            RunFunctions<List<TObject>>(propertyValueFunctions[0], propertyOperatorValueFunctions[0], searchFilterFunctions[0]);
            RunFunctionsAsync<List<TObject>>(propertyValueFunctions[1], propertyOperatorValueFunctions[1], searchFilterFunctions[1]);
            RunFunctions<IEnumerable<TObject>>(propertyValueFunctions[2], propertyOperatorValueFunctions[2], searchFilterFunctions[2]);

            other?.Invoke(synchronousClient, asynchronousClient, streamClient);
        }

        private void RunFunctions<T>(Func<Property, object, object> f, Func<Property, FilterOperator, string, object> g, Func<SearchFilter[], object> h) where T : IEnumerable
        {
            CheckResult<T>(f(testProperty, testValue));
            CheckResult<T>(g(testProperty, testOperator, testValue));
            CheckResult<T>(h(testFilters));
        }

        private async void RunFunctionsAsync<T>(Func<Property, object, object> f, Func<Property, FilterOperator, string, object> g, Func<SearchFilter[], object> h) where T : IEnumerable
        {
            CheckResult<T>(await (Task<List<TObject>>)f(testProperty, testValue));
            CheckResult<T>(await (Task<List<TObject>>)g(testProperty, testOperator, testValue));
            CheckResult<T>(await (Task<List<TObject>>)h(testFilters));
        }

        protected void CheckResult<T>(object result) where T : IEnumerable
        {
            Assert.IsTrue(((IEnumerable<TObject>) result).Any());
        }

        protected abstract IEnumerable<TObject> Stream(PrtgClient client);
    }
}
