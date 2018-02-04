using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public abstract class StreamableObjectTests<TObject, TItem, TResponse> : ObjectTests<TObject, TItem, TResponse>
        where TResponse : IWebResponse
        where TObject : ObjectTable
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
            Func<PrtgClient, PrtgClient, List<Func<Property, object, object>>> propertyValue,
            Func<PrtgClient, PrtgClient, List<Func<Property, FilterOperator, string, object>>> propertyOperatorValue,
            Func<PrtgClient, PrtgClient, List<Func<SearchFilter[], object>>> searchFilters,
            Action<PrtgClient, PrtgClient> other = null
        )
        {
            var synchronousClient = Initialize_Client_WithItems(GetItem());
            var asynchronousClient = Initialize_Client_WithItems(GetItem());
            
            var propertyValueFunctions = propertyValue(synchronousClient, asynchronousClient);
            var propertyOperatorValueFunctions = propertyOperatorValue(synchronousClient, asynchronousClient);
            var searchFilterFunctions = searchFilters(synchronousClient, asynchronousClient);

            if (propertyValueFunctions.Count != 2)
                throw new NotImplementedException();
            if (propertyOperatorValueFunctions.Count != 2)
                throw new NotImplementedException();
            if (searchFilterFunctions.Count != 2)
                throw new NotImplementedException();

            RunFunctions<List<TObject>>(propertyValueFunctions[0], propertyOperatorValueFunctions[0], searchFilterFunctions[0]);
            RunFunctionsAsync<List<TObject>>(propertyValueFunctions[1], propertyOperatorValueFunctions[1], searchFilterFunctions[1]);

            other?.Invoke(synchronousClient, asynchronousClient);
        }

        protected void Object_GetObjectsOverloads_Stream_CanExecute(
            Func<PrtgClient, Func<bool, object>> regularValue,
            Func<PrtgClient, Func<Property, object, object>> propertyValue,
            Func<PrtgClient, Func<Property, FilterOperator, string, object>> propertyOperatorValue,
            Func<PrtgClient, Func<SearchFilter[], object>> searchFilter,
            Action<PrtgClient> other = null
        )
        {
            //OpenCover doesn't deal with Streaming cmdlets very well. As such, we execute these in a separate method
            //we can potentially exclude from coverage

            var streamClient = Initialize_Client_WithItems(Enumerable.Range(0, 755).Select(i => GetItem()).ToArray());

            var regularFunction = regularValue(streamClient);
            var propertyValueFunction = propertyValue(streamClient);
            var propertyOperatorValueFunction = propertyOperatorValue(streamClient);
            var searchFilterFunction = searchFilter(streamClient);

            CheckResult<IEnumerable<TObject>>(regularFunction(false));
            RunFunctions<IEnumerable<TObject>>(propertyValueFunction, propertyOperatorValueFunction, searchFilterFunction);

            other?.Invoke(streamClient);
        }

        protected void Object_SerialStreamObjects<TParam>(Func<PrtgClient, Func<bool, IEnumerable<TObject>>> getObjects1, Func<PrtgClient, Func<TParam, bool, IEnumerable<TObject>>> getObjects2, TParam parameters) where TParam : TableParameters<TObject>
        {
            var count = 755;

            var streamClient = Initialize_Client_WithItems(Enumerable.Range(0, count).Select(i => GetItem()).ToArray());

            var items1 = getObjects1(streamClient)(true).ToList();
            Assert.AreEqual(count, items1.Count);

            var items2 = getObjects2(streamClient)(parameters, true).ToList();
            Assert.AreEqual(count, items2.Count);
        }

        private void RunFunctions<T>(Func<Property, object, object> f, Func<Property, FilterOperator, string, object> g, Func<SearchFilter[], object> h) where T : IEnumerable
        {
            CheckResult<T>(f(testProperty, testValue));
            CheckResult<T>(g(testProperty, testOperator, testValue));
            CheckResult<T>(h(testFilters));
        }

        private void RunFunctionsAsync<T>(Func<Property, object, object> f, Func<Property, FilterOperator, string, object> g, Func<SearchFilter[], object> h) where T : IEnumerable
        {
            CheckResult<T>(((Task<List<TObject>>)f(testProperty, testValue)).Result);
            CheckResult<T>(((Task<List<TObject>>)g(testProperty, testOperator, testValue)).Result);
            CheckResult<T>(((Task<List<TObject>>)h(testFilters)).Result);
        }

        protected void CheckResult<T>(object result) where T : IEnumerable
        {
            Assert.IsTrue(((IEnumerable<TObject>) result).Any());
        }

        protected abstract IEnumerable<TObject> Stream(PrtgClient client);
    }
}
