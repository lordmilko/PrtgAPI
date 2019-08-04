using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class StreamTests : BaseTest
    {
        #region Serial

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_WithStartOffset_SinglePage()
        {
            StreamLogsSerial(
                new LogParameters(null),
                "count=500&start=1",
                false
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_WithStartOffset_MultiplePages()
        {
            StreamLogsSerial(
                new LogParameters(null) { PageSize = 250 },
                "count=250&start=1",
                false
            );
        }

        #region Start

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Start_SinglePage()
        {
            //Count (10) > (Total (10) - Start (3))
            //Result: request 7 instead
            StreamSerial(new SensorParameters
            {
                Start = 3
            }, "count=7&start=3", true, 10);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Start_MultiplePages()
        {
            //Count (500) > (Total (1005) - Start (503))
            //Result: false. Request 500 as normal

            //Increment page: want 500, but can only get 2
            StreamSerial(
                new SensorParameters { Start = 503 },
                new[] { "count=500&start=503", "count=2&start=1003" },
                true,
                1005
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Start_AndStartOffset_SinglePage()
        {
            StreamLogsSerial(new LogParameters(null)
            {
                Start = 3
            }, "count=8&start=3", true);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Start_AndStartOffset_MultiplePages()
        {
            StreamLogsSerial(
                new LogParameters(null) { Start = 7, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=4&start=17" },
                true,
                20
            );
        }

        #endregion
        #region Count

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_SinglePage()
        {
            //Total Objects is not known, so we ask for Count many
            StreamSerial(new SensorParameters
            {
                Count = 10
            }, "count=10", false, 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_MultiplePages()
        {
            StreamSerial(
                new SensorParameters { Count = 10, PageSize = 5 },
                new[] { "count=5", "count=5&start=5" },
                false,
                20
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_LessThanAvailable_SinglePage()
        {
            StreamSerial(new SensorParameters
            {
                Count = 10
            }, "count=10", false, 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_LessThanAvailable_MultiplePages()
        {
            StreamSerial(
                new SensorParameters { Count = 10, PageSize = 5 },
                new[] { "count=5", "count=5&start=5" },
                false,
                20
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_EqualsAvailable_SinglePage()
        {
            var count = 20;

            StreamSerial(new SensorParameters
            {
                Count = count
            }, "count=20", false, count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_EqualsAvailable_MultiplePages()
        {
            var count = 20;

            StreamSerial(
                new SensorParameters { Count = count, PageSize = 5 },
                new[] { "count=5", "count=5&start=5", "count=5&start=10", "count=5&start=15" },
                false,
                count
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_MoreThanAvailable_SinglePage()
        {
            StreamSerial(new SensorParameters
            {
                Count = 30
            }, "count=30", false, 30);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_MoreThanAvailable_MultiplePages()
        {
            StreamSerial(
                new SensorParameters { Count = 30, PageSize = 5 },
                new[] { "count=5", "count=5&start=5", "count=5&start=10", "count=5&start=15" },
                false,
                20
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_AndStartOffset_SinglePage()
        {
            StreamLogsSerial(new LogParameters(null)
            {
                Count = 10
            }, "count=10&start=1", false, 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_AndStartOffset_MultiplePages()
        {
            StreamLogsSerial(
                new LogParameters(null) { Count = 10, PageSize = 5 },
                new[] { "count=5&start=1", "count=5&start=6" },
                false,
                20
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_AndStartOffset_LessThanAvailable_SinglePage()
        {
            StreamLogsSerial(new LogParameters(null)
            {
                Count = 10
            }, "count=10&start=1", false, 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_AndStartOffset_LessThanAvailable_MultiplePages()
        {
            StreamLogsSerial(
                new LogParameters(null) { Count = 10, PageSize = 5 },
                new[] { "count=5&start=1", "count=5&start=6" },
                false,
                20
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_AndStartOffset_EqualsAvailable_SinglePage()
        {
            var count = 20;

            StreamLogsSerial(new LogParameters(null)
            {
                Count = count
            }, "count=20&start=1", false, count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_AndStartOffset_EqualsAvailable_MultiplePages()
        {
            var count = 20;

            StreamLogsSerial(
                new LogParameters(null) { Count = count, PageSize = 5 },
                new[] { "count=5&start=1", "count=5&start=6", "count=5&start=11", "count=5&start=16" },
                false,
                count
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_AndStartOffset_MoreThanAvailable_SinglePage()
        {
            StreamLogsSerial(new LogParameters(null)
            {
                Count = 30
            }, "count=30&start=1", false, 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_Count_AndStartOffset_MoreThanAvailable_MultiplePages()
        {
            StreamLogsSerial(
                new LogParameters(null) { Count = 30, PageSize = 5 },
                new[] { "count=5&start=1", "count=5&start=6", "count=5&start=11", "count=5&start=16" },
                false,
                20
            );
        }

        #endregion
        #region StartAndCount

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_SinglePage()
        {
            StreamSerial(new SensorParameters
            {
                Start = 3,
                Count = 5
            }, "count=5&start=3", true, 10);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_MultiplePages()
        {
            StreamSerial(
                new SensorParameters { Start = 7, Count = 13, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=3&start=17" },
                true,
                30
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_CountLessThanAvailable_SinglePage()
        {
            StreamSerial(new SensorParameters
            {
                Start = 3,
                Count = 5
            }, "count=5&start=3", true, 10);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_CountLessThanAvailable_MultiplePages()
        {
            StreamSerial(
                new SensorParameters { Start = 7, Count = 13, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=3&start=17" },
                true,
                30
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_CountEqualsAvailable_SinglePage()
        {
            var count = 10;

            StreamSerial(new SensorParameters
            {
                Start = 3,
                Count = count
            }, "count=7&start=3", true, count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_CountEqualsAvailable_MultiplePages()
        {
            var count = 25;

            StreamSerial(
                new SensorParameters { Start = 7, Count = count, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=5&start=17", "count=3&start=22" },
                true,
                count
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_CountMoreThanAvailable_SinglePage()
        {
            StreamSerial(new SensorParameters
            {
                Start = 3,
                Count = 30
            }, "count=17&start=3", true, 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_CountMoreThanAvailable_MultiplePages()
        {
            StreamSerial(
                new SensorParameters { Start = 7, Count = 40, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=5&start=17", "count=3&start=22" },
                true,
                25
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_AndStartOffset_SinglePage()
        {
            StreamLogsSerial(new LogParameters(null)
            {
                Start = 3,
                Count = 5
            }, "count=5&start=3", true, 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_AndStartOffset_MultiplePages()
        {
            StreamLogsSerial(new LogParameters(null)
            {
                Start = 7,
                Count = 11
            }, "count=11&start=7", true, 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_AndStartOffset_CountLessThanAvailable_SinglePage()
        {
            StreamLogsSerial(new LogParameters(null)
            {
                Start = 3,
                Count = 5
            }, "count=5&start=3", true, 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_AndStartOffset_CountLessThanAvailable_MultiplePages()
        {
            StreamLogsSerial(
                new LogParameters(null) { Start = 7, Count = 11, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=1&start=17" },
                true,
                20
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_AndStartOffset_CountEqualsAvailable_SinglePage()
        {
            var count = 20;

            StreamLogsSerial(new LogParameters(null)
            {
                Start = 3,
                Count = count
            }, "count=18&start=3", true, count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_AndStartOffset_CountEqualsAvailable_MultiplePages()
        {
            var count = 25;

            StreamLogsSerial(
                new LogParameters(null) { Start = 7, Count = count, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=5&start=17", "count=4&start=22" },
                true,
                count
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_AndStartOffset_CountMoreThanAvailable_SinglePage()
        {
            StreamLogsSerial(new LogParameters(null)
            {
                Start = 3,
                Count = 30
            }, "count=18&start=3", true, 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Serially_StartAndCount_AndStartOffset_CountMoreThanAvailable_MultiplePages()
        {
            StreamLogsSerial(
                new LogParameters(null) { Start = 7, Count = 40, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=5&start=17", "count=4&start=22" },
                true,
                25
            );
        }

        #endregion
        #endregion
        #region Parallel

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_WithStartOffset_SinglePage()
        {
            StreamLogs(
                new LogParameters(null),
                "count=10&start=1"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_WithStartOffset_MultiplePages()
        {
            StreamLogs(
                new LogParameters(null) { PageSize = 250 },
                "count=10&start=1"
            );
        }

        #region Start

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Start_SinglePage()
        {
            Stream(new SensorParameters
            {
                Start = 3
            }, "count=7&start=3", 10);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Start_MultiplePages()
        {
            Stream(
                new SensorParameters { Start = 503 },
                new[] { "count=500&start=503", "count=2&start=1003" },
                1005
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Start_AndStartOffset_SinglePage()
        {
            StreamLogs(new LogParameters(null)
            {
                Start = 3
            }, "count=8&start=3");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Start_AndStartOffset_MultiplePages()
        {
            StreamLogs(
                new LogParameters(null) { Start = 7, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=4&start=17" },
                20
            );
        }

        #endregion
        #region Count

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_SinglePage()
        {
            //Total Objects is retrieved, which is lower than Count so Count is ignored
            Stream(new SensorParameters
            {
                Count = 10
            }, "count=2", 2);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_MultiplePages()
        {
            StreamLogs(
                new LogParameters(null) { Count = 10, PageSize = 5 },
                new[] { "count=5&start=1", "count=5&start=6" },
                20
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_LessThanAvailable_SinglePage()
        {
            Stream(new SensorParameters
            {
                Count = 3
            }, "count=3", 10);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_LessThanAvailable_MultiplePages()
        {
            Stream(
                new SensorParameters { Count = 502 },
                new[] { "count=500", "count=2&start=500" },
                1001
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_EqualsAvailable_SinglePage()
        {
            Stream(new SensorParameters
            {
                Count = 10
            }, "count=10", 10);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_EqualsAvailable_MultiplePages()
        {
            Stream(
                new SensorParameters { Count = 1001 },
                new[] { "count=500", "count=500&start=500", "count=1&start=1000" },
                1001
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_MoreThanAvailable_SinglePage()
        {
            Stream(new SensorParameters
            {
                Count = 10
            }, "count=2", 2);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_MoreThanAvailable_MultiplePages()
        {
            Stream(
                new SensorParameters { Count = 30, PageSize = 5 },
                new[] { "count=5", "count=5&start=5", "count=5&start=10", "count=5&start=15" },
                20
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_AndStartOffset_SinglePage()
        {
            StreamLogs(new LogParameters(null)
            {
                Count = 10
            }, "count=10&start=1", 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_AndStartOffset_MultiplePages()
        {
            StreamLogs(
                new LogParameters(null) { Count = 10, PageSize = 5 },
                new[] { "count=5&start=1", "count=5&start=6" },
                20
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_AndStartOffset_LessThanAvailable_SinglePage()
        {
            StreamLogs(new LogParameters(null)
            {
                Count = 10
            }, "count=10&start=1", 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_AndStartOffset_LessThanAvailable_MultiplePages()
        {
            StreamLogs(
                new LogParameters(null) { Count = 10, PageSize = 5 },
                new[] { "count=5&start=1", "count=5&start=6" },
                20
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_AndStartOffset_EqualsAvailable_SinglePage()
        {
            var count = 20;

            StreamLogs(new LogParameters(null)
            {
                Count = count
            }, "count=20&start=1", count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_AndStartOffset_EqualsAvailable_MultiplePages()
        {
            var count = 20;

            StreamLogs(
                new LogParameters(null) { Count = count, PageSize = 5 },
                new[] { "count=5&start=1", "count=5&start=6", "count=5&start=11", "count=5&start=16" },
                count
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_AndStartOffset_MoreThanAvailable_SinglePage()
        {
            StreamLogs(new LogParameters(null)
            {
                Count = 30
            }, "count=20&start=1", 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_Count_AndStartOffset_MoreThanAvailable_MultiplePages()
        {
            StreamLogs(
                new LogParameters(null) { Count = 30, PageSize = 5 },
                new[] { "count=5&start=1", "count=5&start=6", "count=5&start=11", "count=5&start=16" },
                20
            );
        }

        #endregion
        #region StartAndCount

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_SinglePage()
        {
            Stream(new SensorParameters
            {
                Start = 3,
                Count = 5
            }, "count=5&start=3", 10);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_MultiplePages()
        {
            Stream(
                new SensorParameters { Start = 7, Count = 13, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=3&start=17" },
                30
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_CountLessThanAvailable_SinglePage()
        {
            Stream(new SensorParameters
            {
                Count = 5,
                Start = 2
            }, "count=5&start=2", 10);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_CountLessThanAvailable_MultiplePages()
        {
            Stream(
                new SensorParameters { Count = 1003, Start = 1002 },
                new[] { "count=500&start=1002", "count=500&start=1502", "count=3&start=2002" },
                3001
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_CountEqualsAvailable_SinglePage()
        {
            Stream(new SensorParameters
            {
                Count = 10,
                Start = 2
            }, "count=8&start=2", 10);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_CountEqualsAvailable_MultiplePages()
        {
            Stream(
                new SensorParameters { Count = 3001, Start = 1002 },
                new[] { "count=500&start=1002", "count=500&start=1502", "count=500&start=2002", "count=499&start=2502" },
                3001
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_CountMoreThanAvailable_SinglePage()
        {
            Stream(new SensorParameters
            {
                Count = 20,
                Start = 1
            }, "count=9&start=1", 10);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_CountMoreThanAvailable_MultiplePages()
        {
            Stream(
                 new SensorParameters { Start = 7, Count = 40, PageSize = 5 },
                 new[] { "count=5&start=7", "count=5&start=12", "count=5&start=17", "count=3&start=22" },
                 25
             );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_AndStartOffset_SinglePage()
        {
            StreamLogs(new LogParameters(null)
            {
                Start = 3,
                Count = 5
            }, "count=5&start=3", 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_AndStartOffset_MultiplePages()
        {
            StreamLogs(new LogParameters(null)
            {
                Start = 7,
                Count = 11
            }, "count=11&start=7", 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_AndStartOffset_CountLessThanAvailable_SinglePage()
        {
            StreamLogs(new LogParameters(null)
            {
                Start = 3,
                Count = 5
            }, "count=5&start=3", 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_AndStartOffset_CountLessThanAvailable_MultiplePages()
        {
            StreamLogs(
                new LogParameters(null) { Start = 7, Count = 11, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=1&start=17" },
                20
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_AndStartOffset_CountEqualsAvailable_SinglePage()
        {
            var count = 20;

            StreamLogs(new LogParameters(null)
            {
                Start = 3,
                Count = count
            }, "count=18&start=3", count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_AndStartOffset_CountEqualsAvailable_MultiplePages()
        {
            var count = 25;

            StreamLogs(
                new LogParameters(null) { Start = 7, Count = count, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=5&start=17", "count=4&start=22" },
                count
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_AndStartOffset_CountMoreThanAvailable_SinglePage()
        {
            StreamLogs(new LogParameters(null)
            {
                Start = 3,
                Count = 30
            }, "count=18&start=3", 20);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_Parallel_StartAndCount_AndStartOffset_CountMoreThanAvailable_MultiplePages()
        {
            StreamLogs(
                new LogParameters(null) { Start = 7, Count = 40, PageSize = 5 },
                new[] { "count=5&start=7", "count=5&start=12", "count=5&start=17", "count=4&start=22" },
                25
            );
        }

        #endregion
        #endregion

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Stream_HasAllTests()
        {
            var types = new[] { "Serially", "Parallel" };
            var parameters = new[] { "Start", "Count", "StartAndCount" };
            var offsetModifiers = new[] { string.Empty, "AndStartOffset" };
            var counts = new[] { "LessThanAvailable", "EqualsAvailable", "MoreThanAvailable" };
            var pages = new[] { "SinglePage", "MultiplePages" };

            var list = new List<string>();

            foreach(var type in types)
            {
                foreach(var parameter in parameters)
                {
                    foreach(var modifier in offsetModifiers)
                    {
                        foreach(var page in pages)
                        {
                            list.Add($"Stream_{type}_{parameter}_{(modifier != string.Empty ? $"{modifier}_" : "")}{page}");
                        }

                        if (parameter.Contains("Count"))
                        {
                            foreach(var count in counts)
                            {
                                foreach(var page in pages)
                                {
                                    list.Add($"Stream_{type}_{parameter}_{(modifier != string.Empty ? $"{modifier}_" : "")}{(parameter == "Count" ? "" : "Count")}{count}_{page}");
                                }
                            }
                        }
                    }
                }
            }

            foreach(var test in list)
            {
                System.Diagnostics.Debug.WriteLine($"[TestMethod]{Environment.NewLine}public void " + test + $"(){Environment.NewLine}{{{Environment.NewLine}throw new NotImplementedException(){Environment.NewLine}}}{Environment.NewLine}");
            }

            TestHelpers.Assert_TestClassHasMethods(GetType(), list);
        }

        private void Stream(SensorParameters parameters, string address, int? count = null)
        {
            Stream(parameters, new[] { address }, count);
        }

        private void Stream(SensorParameters parameters, string[] address, int? count = null)
        {
            var url = new List<string>();
            url.Add(UnitRequest.SensorCount);
            url.AddRange(address.Select(a => UnitRequest.Sensors(a, UrlFlag.Columns)));

            Dictionary<Content, int> countOverride = null;

            if (count != null)
            {
                countOverride = new Dictionary<Content, int>();
                countOverride[Content.Sensors] = count.Value;
            }

            Execute(
                c => c.StreamSensors(parameters).ToList(),
                url.ToArray(),
                countOverride
            );
        }

        private void StreamSerial(SensorParameters parameters, string address, bool requestCount = false, int? count = null)
        {
            StreamSerial(parameters, new[] { address }, requestCount, count);
        }

        private void StreamSerial(SensorParameters parameters, string[] address, bool requestCount = false, int? count = null)
        {
            var url = new List<string>();

            if (requestCount)
                url.Add(UnitRequest.SensorCount);

            url.AddRange(address.Select(a => UnitRequest.Sensors(a, UrlFlag.Columns)));

            Dictionary<Content, int> countOverride = null;

            if (count != null)
            {
                countOverride = new Dictionary<Content, int>();
                countOverride[Content.Sensors] = count.Value;
            }

            Execute(
                c => c.StreamSensors(parameters, true).ToList(),
                url.ToArray(),
                countOverride
            );
        }

        private void StreamLogsSerial(LogParameters parameters, string address, bool requestCount, int count = 10)
        {
            StreamLogsSerial(parameters, new[] { address }, requestCount, count);
        }

        private void StreamLogsSerial(LogParameters parameters, string[] address, bool requestCount, int count = 10)
        {
            var url = new List<string>();

            if (requestCount)
                url.Add(UnitRequest.Logs($"count=1&columns=objid,name", null));

            url.AddRange(address.Select(a => UnitRequest.Logs(a, UrlFlag.Columns)));

            Execute(
                c => c.StreamLogs(parameters, true).ToList(),
                url.ToArray(),
                new Dictionary<Content, int>
                {
                    [Content.Logs] = count
                }
            );
        }

        private void StreamLogs(LogParameters parameters, string address, int count = 10)
        {
            StreamLogs(parameters, new[] { address }, count);
        }

        private void StreamLogs(LogParameters parameters, string[] address, int count = 10)
        {
            var url = new List<string>();
            url.Add(UnitRequest.Logs($"count=1&columns=objid,name", null));
            url.AddRange(address.Select(a => UnitRequest.Logs(a, UrlFlag.Columns)));

            Execute(
                c => c.StreamLogs(parameters).ToList(),
                url.ToArray(),
                new Dictionary<Content, int>
                {
                    [Content.Logs] = count
                }
            );
        }
    }
}
