using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class TakeTests : BaseQueryTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Take_ThenCount_NoPredicate()
        {
            ExecuteNow(q => q.Take(2).Count(), "count=2", r => Assert.AreEqual(2, r), UrlFlag.Columns);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Take_Smaller_ThenBigger()
        {
            ExecuteNow(q => q.Take(2).Take(3).Count(), "count=2", r => Assert.AreEqual(2, r), UrlFlag.Columns);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Take_Bigger_ThenSmaller()
        {
            ExecuteNow(q => q.Take(3).Take(2).Count(), "count=2", r => Assert.AreEqual(2, r), UrlFlag.Columns);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Take_ThreeTimes()
        {
            ExecuteNow(q => q.Take(3).Take(1).Take(2).Count(), "count=1", r => Assert.AreEqual(1, r), UrlFlag.Columns);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Take_ThenWhere()
        {
            ExecuteNow(q => q.Take(2).Where(s => s.Id == 4001).Count(), "count=2", r => Assert.AreEqual(1, r), UrlFlag.Columns);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Take_ThenCount_WithPredicate()
        {
            ExecuteNow(q => q.Take(2).Count(s => s.Id == 4001), "count=2", r => Assert.AreEqual(1, r), UrlFlag.Columns);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Where_ThenTake_LegalPredicate()
        {
            ExecuteNow(q => q.Where(s => s.Id == 4001).Take(2).Count(), "count=2&filter_objid=4001", r => Assert.AreEqual(1, r), UrlFlag.Columns);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Where_ThenTake_IllLegalPredicate_And()
        {
            ExecuteNow(q => q.Where(s => s.Id == 4001 && s.Id == 4002).Take(2).Count(), "filter_objid=4001", r => Assert.AreEqual(0, r));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Where_ThenTake_IllLegalPredicate_Or()
        {
            ExecuteNow(
                q => q.Where(s => s.Id == 4001 || s.ParentId == 4002).Take(2).Count(),
                new[] {"filter_objid=4001", "filter_parentid=4002"},
                r => Assert.AreEqual(1, r)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Take_Until_Matched()
        {
            var client = GetUntilMatchedClient();

            var response = client.Item1.QuerySensors(s => s.Name.Contains("Ye") && s.Name == "Yes").Take(2).ToList();

            Assert.AreEqual(2, response.Count);

            client.Item2.AssertFinished();
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Take_Until_Matched_Manual()
        {
            var client = GetUntilMatchedClient(1);

            var parameters = new SensorParameters
            {
                PageSize = 1,
                SearchFilters = new List<SearchFilter> {new SearchFilter(Property.Name, FilterOperator.Contains, "Ye")}
            };

            var response = client.Item1.StreamSensors(parameters, true).Where(s => s.Name == "Yes").Take(2).ToList();

            Assert.AreEqual(2, response.Count);

            client.Item2.AssertFinished();
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Take_Until_Matched_TakeIterator()
        {
            var items = new List<BaseItem>();
            items.AddRange(GetPage("Yes", 1));
            items.AddRange(GetPage("YeNo", 1));
            items.AddRange(GetPage("YeNo", 1));
            items.AddRange(GetPage("Yes", 1));
            items.AddRange(GetPage("YeNo", 1));
            items.AddRange(GetPage("Yes", 1));

            var urls = new[]
            {
                UnitRequest.Sensors("count=2&filter_name=@sub(Ye)", UrlFlag.Columns),          //Yes, YeNo
                UnitRequest.Sensors("count=0&filter_name=@sub(Ye)", null),                     //Count
                UnitRequest.Sensors("count=1&filter_name=@sub(Ye)&start=2", UrlFlag.Columns),  //YeNo
                UnitRequest.Sensors("count=3&filter_name=@sub(Ye)&start=3", UrlFlag.Columns) //Yes
            };

            var itemOverride = new Dictionary<Content, BaseItem[]>
            {
                [Content.Sensors] = items.ToArray()
            };

            Execute(c =>
            {
                Func<SensorParameters, Func<int>, IEnumerable<Sensor>> streamer = (p, c1) => c.ObjectEngine.StreamObjects<Sensor, SensorParameters>(p, true, c1);

                var parameters = new SensorParameters
                {
                    SearchFilters = new List<SearchFilter> { new SearchFilter(Property.Name, FilterOperator.Contains, "Ye") }
                };

                var iterator = new TakeIterator<Sensor, SensorParameters>(
                    2,
                    parameters,
                    streamer,
                    () => c.GetTotalObjects(parameters.Content, parameters.SearchFilters?.ToArray()),
                    r => r.Where(s => s.Name == "Yes")
                );

                var response = iterator.ToList();
                Assert.AreEqual(2, response.Count);

            }, urls, itemOverride: itemOverride);
        }

        private Tuple<PrtgClient, AddressValidatorResponse> GetUntilMatchedClient(int pageSize = 500)
        {
            var items = new List<BaseItem>();
            items.AddRange(GetPage("Yes", pageSize));
            items.AddRange(GetPage("YeNo", pageSize));
            items.AddRange(GetPage("YeNo", pageSize));
            items.AddRange(GetPage("Yes", pageSize));
            items.AddRange(GetPage("YeNo", pageSize));
            items.AddRange(GetPage("Yes", pageSize));

            var urls = new object[]
            {
                UnitRequest.Sensors($"count={pageSize}&filter_name=@sub(Ye)", UrlFlag.Columns),                      //Yes
                UnitRequest.Sensors($"count={pageSize}&filter_name=@sub(Ye)&start={pageSize * 1}", UrlFlag.Columns), //YeNo
                UnitRequest.Sensors($"count={pageSize}&filter_name=@sub(Ye)&start={pageSize * 2}", UrlFlag.Columns), //YeNo
                UnitRequest.Sensors($"count={pageSize}&filter_name=@sub(Ye)&start={pageSize * 3}", UrlFlag.Columns)  //Yes
            };

#pragma warning disable 618
            var response = new AddressValidatorResponse(urls)
            {
                ItemOverride = new Dictionary<Content, BaseItem[]>
                {
                    [Content.Sensors] = items.ToArray()
                }
            };
#pragma warning restore 618

            var client = Initialize_Client(response);

            return Tuple.Create(client, response);
        }

        private List<SensorItem> GetPage(string target, int pageSize = 500)
        {
            var result = Enumerable.Range(0, pageSize).Select(i => new SensorItem(name: "YeNo")).ToList();

            if (result.Count > 4)
                result[3].Name = target;
            else
                result[0].Name = target;

            return result;
        }
    }
}
