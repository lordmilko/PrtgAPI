using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.IntegrationTests.ObjectData.Query;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class LogTests : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Data_Log_GetLogs_HasAnyResults()
        {
            HasAnyResults(() => client.GetLogs());
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Log_GetLogs_SpecifiesStartDate()
        {
            var count = 6000;

            var logs = client.GetLogs(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1).AddHours(-6), count);

            var first = logs.First();
            var last = logs.Last();

            AssertEx.AreEqual(DateTime.Now.AddDays(-1).Date, first.DateTime.Date, "Start date was incorrect");
            AssertEx.IsTrue(last.DateTime < DateTime.Now.AddDays(-1).AddHours(-5), "Logs didn't appear to go back far enough");
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Log_GetLogs_SpecifiesEndDate()
        {
            var logs = client.GetLogs(null, DateTime.Now.AddDays(-1), null);

            var first = logs.First();
            var last = logs.Last();

            AssertEx.AreEqual(DateTime.Now.Date, first.DateTime.Date, "Start date was incorrect");
            AssertEx.AreEqual(DateTime.Now.AddDays(-1).Date, last.DateTime.Date, "End date was incorrect");
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Log_GetLogs_FiltersByStatus()
        {
            var logs = client.GetLogs(status: LogStatus.Up);

            foreach (var log in logs)
            {
                AssertEx.AreEqual(log.Status, LogStatus.Up, $"Response included an item that was not {LogStatus.Up}");
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_StreamLogs_SerialVsNonSerial_Normal()
        {
            FilterTests.Retry(retry =>
            {
                var normal = OrderLogs(client.StreamLogs(RecordAge.All, 3000)).ToList();
                var serial = OrderLogs(client.StreamLogs(RecordAge.All, 3000, true)).ToList();

                AssertLogStreamsEqual(normal, serial, retry);
            });
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_StreamLogs_SerialVsNonSerial_StartOffset()
        {
            FilterTests.Retry(retry =>
            {
                var parameters = new LogParameters(null, RecordAge.All, 3000)
                {
                    Start = 502
                };

                Logger.LogTestDetail("Retrieving normal logs");
                var normalNoSkip = OrderLogs(client.StreamLogs(RecordAge.All, 3000, true)).ToList();

                Logger.LogTestDetail("Retrieving parallel logs");
                var normal = OrderLogs(client.StreamLogs(parameters)).ToList();

                Logger.LogTestDetail("Retrieving serial logs");
                var serial = OrderLogs(client.StreamLogs(parameters, true)).ToList();

                AssertLogStreamsEqual(normal, serial, true);

                //Sometimes it might fail due to the order of entries at a given datetime being nondeterministic
                try
                {
                    Assert.IsTrue(PrtgAPIHelpers.LogEqualityComparer().Equals(normalNoSkip[501], serial.First()));
                }
                catch(AssertFailedException)
                {
                    var time = serial.First().DateTime;

                    var matchingTime = normalNoSkip.Where(l => l.DateTime == time).ToList();

                    Assert.IsTrue(matchingTime.Any(l => PrtgAPIHelpers.LogEqualityComparer().Equals(l, serial.First())));
                }
            });
        }

        private void AssertLogStreamsEqual(List<Log> normal, List<Log> serial, bool retry = false)
        {
            var comparer = PrtgAPIHelpers.LogEqualityComparer();

            AssertStreamsEqual(
                normal, serial, comparer,
                e => OrderLogs(e),
                g => OrderLogs(g),
                retry
            );
        }

        private void AssertStreamsEqual<T>(List<T> normal, List<T> serial, IEqualityComparer<T> comparer, Func<IEnumerable<T>, IOrderedEnumerable<T>> order, Func<IEnumerable<IGrouping<T, T>>, IOrderedEnumerable<IGrouping<T, T>>> orderGroups, bool retry = false)
        {
            if (retry)
                Assert.AreEqual(normal.Count, serial.Count);

            AssertEx.AreEqual(normal.Count, serial.Count, "Normal and serial did not have the same count");

            var normalGroups = normal.GroupBy(l => l, comparer).ToList();
            var serialGroups = serial.GroupBy(l => l, comparer).ToList();

            var singleNormal = normalGroups.Where(g => g.Count() == 1).ToList();
            var singleSerial = serialGroups.Where(g => g.Count() == 1).ToList();

            var flattenedNormal = order(singleNormal.SelectMany(g => g)).ToList();
            var flattenedSerial = order(singleSerial.SelectMany(g => g)).ToList();

            AssertEx.AreEqualLists(flattenedNormal, flattenedSerial, comparer, "flattenedNormal and flattenedSerial were not equal", retry);

            var multipleNormal = orderGroups(normalGroups.Where(g => g.Count() > 1)).ToList();
            var multipleSerial = orderGroups(serialGroups.Where(g => g.Count() > 1)).ToList();

            AssertEx.AreEqual(multipleNormal.Count, multipleSerial.Count, "Did not have same number of logs with multiple occurrences");

            for (var i = 0; i < multipleNormal.Count; i++)
            {
                AssertEx.AreEqualLists(multipleNormal[i].ToList(), multipleSerial[i].ToList(), comparer, $"Lists on key {multipleNormal[i].Key} were not equal");
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_StreamLogs_WithCorrectPageSize()
        {
            FilterTests.Retry(retry =>
            {
                Stream_WithCorrectPageSize(
                    () => client.GetLogs(RecordAge.Today, 15),
                    () => client.StreamLogs(RecordAge.Today, 15, true),
                    p => client.GetLogs(p),
                    PrtgAPIHelpers.LogEqualityComparer(),
                    new LogParameters(null, RecordAge.Today, 5),
                    1,
                    retry
                );
            });
        }

        internal static void Stream_WithCorrectPageSize<TObject, TParam>(
            Func<List<TObject>> getObjects,
            Func<IEnumerable<TObject>> streamObjects,
            Func<TParam, List<TObject>> getManualObjects,
            IEqualityComparer<TObject> comparer,
            TParam parameters, int? start,
            bool retry = false) where TParam : PageableParameters
        {
            var normalObjects = getObjects();
            var streamedObjects = streamObjects().ToList();

            Assert.AreEqual(start, parameters.Start);

            var firstManualObjects = getManualObjects(parameters);
            parameters.Page++;
            var secondManualObjects = getManualObjects(parameters);
            parameters.Page++;
            var thirdManualObjects = getManualObjects(parameters);

            var allManualObjects = new List<TObject>();
            allManualObjects.AddRange(firstManualObjects);
            allManualObjects.AddRange(secondManualObjects);
            allManualObjects.AddRange(thirdManualObjects);

            AssertEx.AreEqualLists(normalObjects, streamedObjects, comparer, "Normal logs were not equal to streamed logs", retry);
            AssertEx.AreEqualLists(normalObjects, allManualObjects, comparer, "Normal logs were not equal to manual logs", retry);

            AssertEx.AllListElementsUnique(normalObjects, comparer);
            AssertEx.AllListElementsUnique(streamedObjects, comparer);
            AssertEx.AllListElementsUnique(allManualObjects, comparer);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_StreamLogs_WithIncorrectPageSize()
        {
            FilterTests.Retry(retry =>
            {
                var correctParameters = new LogParameters(null, RecordAge.Today, 15) { PageSize = 5 };
                var automaticParameters = new LogParameters(null, RecordAge.Today, 15) { Start = 0, PageSize = 5 };
                var manualParameters = new LogParameters(null, RecordAge.Today, 5) { Start = 0 };

                //The real logs that exist on the server. This is what all other requests compare against
                var correctLogs = client.GetLogs(correctParameters);

                //What we get when we make the same request with a starting index of 0. We expect GetLogs to return
                //something equivalent to a normal request, but StreamSensors to contain a duplicate at index 4 and 5
                var automaticLogs = client.GetLogs(automaticParameters);
                var automaticStreamLogs = client.StreamLogs(automaticParameters, true).ToList();

                //What we get when we manually increment the pages of a stream. We expect to end up with a list
                //identical to our streamed list
                var firstManualLogs = client.GetLogs(manualParameters);
                manualParameters.Page++;
                var secondManualLogs = client.GetLogs(manualParameters);
                manualParameters.Page++;
                var thirdManualLogs = client.GetLogs(manualParameters);

                var allManualLogs = new List<Log>();
                allManualLogs.AddRange(firstManualLogs);
                allManualLogs.AddRange(secondManualLogs);
                allManualLogs.AddRange(thirdManualLogs);

                var comparer = PrtgAPIHelpers.LogEqualityComparer();

                AssertEx.AreEqualLists(correctLogs, automaticLogs, comparer, "Correct logs were not equal to off by one logs", retry);
                AssertEx.AreEqualLists(automaticStreamLogs, allManualLogs, comparer, "Streamed off by one logs were not equal to manual logs", retry);

                Assert.IsTrue(comparer.Equals(automaticStreamLogs[4], automaticStreamLogs[5]));
                Assert.IsTrue(comparer.Equals(allManualLogs[4], allManualLogs[5]));
                //now check none of the OTHER elements are equal to each other

                var automaticDiff = automaticStreamLogs.Where((l, i) => i != 4 && i != 5).ToList();
                var manualDiff = allManualLogs.Where((l, i) => i != 4 && i != 5).ToList();

                AssertEx.AllListElementsUnique(automaticDiff, comparer);
                AssertEx.AllListElementsUnique(manualDiff, comparer);
            });
        }

        public static IOrderedEnumerable<Log> OrderLogs(IEnumerable<Log> logs)
        {
            return logs.OrderByDescending(l => l.DateTime).ThenBy(l => l.Name).ThenBy(l => l.Status);
        }

        public static IOrderedEnumerable<IGrouping<Log, Log>> OrderLogs(IEnumerable<IGrouping<Log, Log>> logs)
        {
            return logs.OrderByDescending(l => l.Key.DateTime).ThenBy(l => l.Key.Name).ThenBy(l => l.Key.Status);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Log_ReadOnlyUser()
        {
            var logs = readOnlyClient.GetLogs();

            foreach (var log in logs)
                AssertEx.AllPropertiesRetrieveValues(log);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Data_Log_ReadOnlyUserAsync()
        {
            var logs = await readOnlyClient.GetLogsAsync();

            foreach (var log in logs)
                AssertEx.AllPropertiesRetrieveValues(log);
        }
    }
}
