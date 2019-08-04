using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.IntegrationTests.ObjectData.Query;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class StreamTests : BasePrtgClientTest
    {
        #region Serial

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_WithStartOffset_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            StreamLogsSerial(parameters, expected);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_WithStartOffset_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.PageSize = 50;

            StreamLogsSerial(parameters, expected);
        }

        #region Start

        //todo: what to do about count being 0 when requesting page count?

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Start_SinglePage()
        {
            var sensors = client.GetSensors();

            var start = 3;

            //Count (30) > (Total (30) - Start (3))
            //Result: request 27 instead
            StreamSerial(new SensorParameters
            {
                Start = start
            }, sensors.Skip(start).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Start_MultiplePages()
        {
            var sensors = client.GetSensors();

            var start = 8;

            //Count (30) > (Total (30) - Start (8))
            //Result: false. Request 500 as normal

            //Increment page: want 500, but can only get 2
            StreamSerial(
                new SensorParameters
                {
                    Start = start,
                    PageSize = 5
                },
                sensors.Skip(start).ToList()
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Start_AndStartOffset_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var start = 3;

            parameters.Start = start;

            StreamLogsSerial(parameters, expected.Skip(start - 1).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Start_AndStartOffset_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var start = 13;

            parameters.Start = start;
            parameters.PageSize = 5;

            StreamLogsSerial(parameters, expected.Skip(start - 1).ToList());
        }

        #endregion
        #region Count

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_SinglePage()
        {
            var sensors = client.GetSensors();

            var count = 10;

            Assert.IsTrue(count < Settings.SensorsInTestServer);

            //Total Objects is not known, so we ask for Count many
            StreamSerial(new SensorParameters
            {
                Count = count
            }, sensors.Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_MultiplePages()
        {
            var sensors = client.GetSensors();

            var count = 13;

            Assert.IsTrue(count < Settings.SensorsInTestServer);

            //Total Objects is not known, so we ask for Count many
            StreamSerial(new SensorParameters
            {
                Count = count,
                PageSize = 5
            }, sensors.Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_LessThanAvailable_SinglePage()
        {
            var sensors = client.GetSensors();

            var count = 10;

            Assert.IsTrue(count < sensors.Count);

            StreamSerial(new SensorParameters
            {
                Count = count
            }, sensors.Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_LessThanAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            var count = 17;

            Assert.IsTrue(count < sensors.Count);

            StreamSerial(new SensorParameters
            {
                Count = count,
                PageSize = 4
            }, sensors.Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_EqualsAvailable_SinglePage()
        {
            var sensors = client.GetSensors();

            StreamSerial(new SensorParameters
            {
                Count = Settings.SensorsInTestServer
            }, sensors);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_EqualsAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            StreamSerial(new SensorParameters
            {
                Count = Settings.SensorsInTestServer,
                PageSize = 7
            }, sensors);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_MoreThanAvailable_SinglePage()
        {
            var sensors = client.GetSensors();

            StreamSerial(new SensorParameters
            {
                Count = Settings.SensorsInTestServer * 2,
            }, sensors);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_MoreThanAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            StreamSerial(new SensorParameters
            {
                Count = Settings.SensorsInTestServer * 2,
                PageSize = 5
            }, sensors);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_AndStartOffset_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.Count = 10;

            StreamLogsSerial(parameters, expected.Take(10).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_AndStartOffset_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.PageSize = 5;
            parameters.Count = 10;

            StreamLogsSerial(parameters, expected.Take(10).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_AndStartOffset_LessThanAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var count = 100;

            Assert.IsTrue(count < expected.Count);

            parameters.Count = count;

            StreamLogsSerial(parameters, expected.Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_AndStartOffset_LessThanAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var count = 100;

            Assert.IsTrue(count < expected.Count);

            parameters.Count = count;
            parameters.PageSize = 30;

            StreamLogsSerial(parameters, expected.Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_AndStartOffset_EqualsAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.Count = expected.Count;

            StreamLogsSerial(parameters, expected.ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_AndStartOffset_EqualsAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.Count = expected.Count;
            parameters.PageSize = 507;

            StreamLogsSerial(parameters, expected.ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_AndStartOffset_MoreThanAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.Count = expected.Count * 2;

            StreamLogsSerial(parameters, expected.ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_Count_AndStartOffset_MoreThanAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.Count = expected.Count;
            parameters.PageSize = 507;

            StreamLogsSerial(parameters, expected.ToList());
        }

        #endregion
        #region StartAndCount

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_SinglePage()
        {
            var sensors = client.GetSensors();

            var start = 3;
            var count = 10;

            StreamSerial(new SensorParameters
            {
                Start = start,
                Count = count
            }, sensors.Skip(start).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_MultiplePages()
        {
            var sensors = client.GetSensors();

            var start = 3;
            var count = 10;

            StreamSerial(new SensorParameters
            {
                Start = start,
                Count = count,
                PageSize = 3
            }, sensors.Skip(start).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_CountLessThanAvailable_SinglePage()
        {
            var sensors = client.GetSensors();

            var start = 3;
            var count = 10;

            Assert.IsTrue(count < sensors.Count);

            StreamSerial(new SensorParameters
            {
                Start = start,
                Count = count
            }, sensors.Skip(start).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_CountLessThanAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            var start = 3;
            var count = 10;

            Assert.IsTrue(count < sensors.Count);

            StreamSerial(new SensorParameters
            {
                Start = start,
                Count = count,
                PageSize = 3
            }, sensors.Skip(start).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_CountEqualsAvailable_SinglePage()
        {
            var sensors = client.GetSensors();

            var start = 3;
            var count = sensors.Count;

            StreamSerial(new SensorParameters
            {
                Start = start,
                Count = count
            }, sensors.Skip(start).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_CountEqualsAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            var start = 7;
            var count = sensors.Count;

            StreamSerial(new SensorParameters
            {
                Start = start,
                Count = count,
                PageSize = 5
            }, sensors.Skip(start).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_CountMoreThanAvailable_SinglePage()
        {
            var sensors = client.GetSensors();

            var start = 3;
            var count = sensors.Count * 2;

            StreamSerial(new SensorParameters
            {
                Start = start,
                Count = count
            }, sensors.Skip(start).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_CountMoreThanAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            var start = 7;
            var count = sensors.Count * 2;

            StreamSerial(new SensorParameters
            {
                Start = start,
                Count = count,
                PageSize = 5
            }, sensors.Skip(start).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_AndStartOffset_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var count = 10;
            var start = 4;

            parameters.Count = count;
            parameters.Start = start;

            StreamLogsSerial(parameters, expected.Skip(start - 1).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_AndStartOffset_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var count = 10;
            var start = 8;

            parameters.Count = count;
            parameters.Start = start;
            parameters.PageSize = 5;

            StreamLogsSerial(parameters, expected.Skip(start - 1).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_AndStartOffset_CountLessThanAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var count = 10;
            var start = 4;

            parameters.Count = count;
            parameters.Start = start;

            Assert.IsTrue(parameters.Count < expected.Count);

            StreamLogsSerial(parameters, expected.Skip(start - 1).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_AndStartOffset_CountLessThanAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var count = 10;
            var start = 8;

            parameters.Count = count;
            parameters.Start = start;
            parameters.PageSize = 5;

            Assert.IsTrue(parameters.Count < expected.Count);

            StreamLogsSerial(parameters, expected.Skip(start - 1).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_AndStartOffset_CountEqualsAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var start = 4;

            parameters.Count = expected.Count;
            parameters.Start = start;

            StreamLogsSerial(parameters, expected.Skip(start - 1).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_AndStartOffset_CountEqualsAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var start = 8;

            parameters.Count = expected.Count;
            parameters.Start = start;
            parameters.PageSize = 50;

            StreamLogsSerial(parameters, expected.Skip(start - 1).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_AndStartOffset_CountMoreThanAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var start = 4;

            parameters.Count = expected.Count * 2;
            parameters.Start = start;

            StreamLogsSerial(parameters, expected.Skip(start - 1).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Serially_StartAndCount_AndStartOffset_CountMoreThanAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var start = 8;

            parameters.Count = expected.Count * 2;
            parameters.Start = start;
            parameters.PageSize = 50;

            StreamLogsSerial(parameters, expected.Skip(start - 1).ToList());
        }

        #endregion
        #endregion
        #region Parallel

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_WithStartOffset_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            StreamLogs(parameters, expected);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_WithStartOffset_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.PageSize = 50;

            StreamLogs(parameters, expected);
        }

        #region Start

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Start_SinglePage()
        {
            var sensors = client.GetSensors();

            var start = 3;

            Stream(new SensorParameters
            {
                Start = start
            }, sensors.Skip(start).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Start_MultiplePages()
        {
            var sensors = client.GetSensors();

            var start = 7;

            Stream(new SensorParameters
            {
                Start = start,
                PageSize = 5
            }, sensors.Skip(start).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Start_AndStartOffset_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.Today, null);

            var expected = client.GetLogs(parameters);

            var start = 3;

            parameters.Start = start;

            StreamLogs(parameters, expected.Skip(start - 1).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Start_AndStartOffset_MultiplePages()
        {
            FilterTests.Retry(retry =>
            {
                var parameters = new LogParameters(Settings.UpSensor, RecordAge.Today, null);

                var expected = client.GetLogs(parameters);

                var start = 130;

                parameters.Start = start;
                parameters.PageSize = 50;

                StreamLogs(parameters, expected.Skip(start - 1).ToList(), retry);
            });
        }

        #endregion
        #region Count

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_SinglePage()
        {
            var sensors = client.GetSensors();

            var count = 900;

            //Total Objects is retrieved, which is lower than Count so Count is ignored
            Stream(new SensorParameters
            {
                Count = count
            }, sensors.Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_MultiplePages()
        {
            var sensors = client.GetSensors();

            var count = 900;

            Stream(new SensorParameters
            {
                Count = count,
                PageSize = 5
            }, sensors.Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_LessThanAvailable_SinglePage()
        {
            var sensors = client.GetSensors();

            var count = 3;

            Stream(new SensorParameters
            {
                Count = count
            }, sensors.Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_LessThanAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            var count = 13;

            Stream(
                new SensorParameters
                {
                    Count = count,
                    PageSize = 5
                },
                sensors.Take(count).ToList()
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_EqualsAvailable_SinglePage()
        {
            var sensors = client.GetSensors();

            Stream(new SensorParameters
            {
                Count = Settings.SensorsInTestServer
            }, sensors);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_EqualsAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            Stream(
                new SensorParameters
                {
                    Count = sensors.Count,
                    PageSize = 5
                },
                sensors
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_MoreThanAvailable_SinglePage()
        {
            var sensors = client.GetSensors();

            var count = 900;

            Stream(new SensorParameters
            {
                Count = count
            }, sensors);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_MoreThanAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            var count = 900;

            Stream(new SensorParameters
            {
                Count = count,
                PageSize = 5
            }, sensors);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_AndStartOffset_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.Count = 10;

            StreamLogs(parameters, expected.Take(10).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_AndStartOffset_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.PageSize = 5;
            parameters.Count = 10;

            StreamLogs(parameters, expected.Take(10).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_AndStartOffset_LessThanAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var count = 100;

            Assert.IsTrue(count < expected.Count);

            parameters.Count = count;

            StreamLogs(parameters, expected.Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_AndStartOffset_LessThanAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var count = 100;

            Assert.IsTrue(count < expected.Count);

            parameters.Count = count;
            parameters.PageSize = 30;

            StreamLogs(parameters, expected.Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_AndStartOffset_EqualsAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.Count = expected.Count;

            StreamLogs(parameters, expected.ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_AndStartOffset_EqualsAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.Count = expected.Count;
            parameters.PageSize = 507;

            StreamLogs(parameters, expected.ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_AndStartOffset_MoreThanAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.Count = expected.Count * 2;

            StreamLogsSerial(parameters, expected.ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_Count_AndStartOffset_MoreThanAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            parameters.Count = expected.Count;
            parameters.PageSize = 507;

            StreamLogsSerial(parameters, expected.ToList());
        }

        #endregion
        #region StartAndCount

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_SinglePage()
        {
            var sensors = client.GetSensors();

            var start = 3;
            var count = 10;

            Stream(new SensorParameters
            {
                Start = start,
                Count = count
            }, sensors.Skip(start).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_MultiplePages()
        {
            var sensors = client.GetSensors();

            var start = 3;
            var count = 10;

            Stream(new SensorParameters
            {
                Start = start,
                Count = count,
                PageSize = 3
            }, sensors.Skip(start).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_CountLessThanAvailable_SinglePage()
        {
            var sensors = client.GetSensors();

            var count = 5;
            var start = 2;

            Stream(new SensorParameters
            {
                Count = count,
                Start = start
            }, sensors.Skip(start).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_CountLessThanAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            var count = 16;
            var start = 10;

            Stream(
                new SensorParameters
                {
                    Count = count,
                    Start = start,
                    PageSize = 3
                },
                sensors.Skip(start).Take(count).ToList()
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_CountEqualsAvailable_SinglePage()
        {
            var sensors = client.GetSensors();

            var start = 2;

            Stream(new SensorParameters
            {
                Count = sensors.Count,
                Start = start
            }, sensors.Skip(start).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_CountEqualsAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            var count = Settings.SensorsInTestServer;
            var start = 7;

            Stream(
                new SensorParameters
                {
                    Count = count,
                    Start = start,
                    PageSize = 5
                },
                sensors.Skip(start).ToList()
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_CountMoreThanAvailable_SinglePage()
        {
            var sensors = client.GetSensors();
            var start = 1;

            Stream(new SensorParameters
            {
                Count = 900,
                Start = start
            }, sensors.Skip(start).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_CountMoreThanAvailable_MultiplePages()
        {
            var sensors = client.GetSensors();

            var start = 7;
            var count = sensors.Count * 2;

            Stream(new SensorParameters
            {
                Start = start,
                Count = count,
                PageSize = 5
            }, sensors.Skip(start).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_AndStartOffset_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var start = 4;

            parameters.Count = expected.Count;
            parameters.Start = start;

            StreamLogs(parameters, expected.Skip(start - 1).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_AndStartOffset_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var count = 10;
            var start = 8;

            parameters.Count = count;
            parameters.Start = start;
            parameters.PageSize = 5;

            StreamLogs(parameters, expected.Skip(start - 1).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_AndStartOffset_CountLessThanAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var count = 10;
            var start = 4;

            parameters.Count = count;
            parameters.Start = start;

            Assert.IsTrue(parameters.Count < expected.Count);

            StreamLogs(parameters, expected.Skip(start - 1).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_AndStartOffset_CountLessThanAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var count = 10;
            var start = 8;

            parameters.Count = count;
            parameters.Start = start;
            parameters.PageSize = 5;

            Assert.IsTrue(parameters.Count < expected.Count);

            StreamLogs(parameters, expected.Skip(start - 1).Take(count).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_AndStartOffset_CountEqualsAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var start = 4;

            parameters.Count = expected.Count;
            parameters.Start = start;

            StreamLogs(parameters, expected.Skip(start - 1).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_AndStartOffset_CountEqualsAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var start = 8;

            parameters.Count = expected.Count;
            parameters.Start = start;
            parameters.PageSize = 50;

            StreamLogs(parameters, expected.Skip(start - 1).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_AndStartOffset_CountMoreThanAvailable_SinglePage()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var start = 4;

            parameters.Count = expected.Count * 2;
            parameters.Start = start;

            StreamLogs(parameters, expected.Skip(start - 1).ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Stream_Parallel_StartAndCount_AndStartOffset_CountMoreThanAvailable_MultiplePages()
        {
            var parameters = new LogParameters(Settings.UpSensor, RecordAge.All, null);

            var expected = client.GetLogs(parameters);

            var start = 8;

            parameters.Count = expected.Count * 2;
            parameters.Start = start;
            parameters.PageSize = 50;

            StreamLogs(parameters, expected.Skip(start - 1).ToList());
        }

        #endregion
        #endregion

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_HasAllStreamTests()
        {
            var expected = TestHelpers.GetTests(typeof(UnitTests.ObjectData.StreamTests)).Where(m => m.Name != "Stream_HasAllTests").Select(m => $"Data_{m.Name}").ToList();

            TestHelpers.Assert_TestClassHasMethods(GetType(), expected);
        }

        private void Stream(SensorParameters parameters, List<Sensor> expected)
        {
            var result = client.StreamSensors(parameters).OrderBy(s => s.Id).ToList();

            AssertEx.AreEqualLists(expected, result, new PrtgObjectComparer(), "Lists were not the same");
        }

        private void StreamSerial(SensorParameters parameters, List<Sensor> expected)
        {
            var result = client.StreamSensors(parameters, true).ToList();

            AssertEx.AreEqualLists(expected, result, new PrtgObjectComparer(), "Lists were not the same");
        }

        private void StreamLogs(LogParameters parameters, List<Log> expected, bool retry = false)
        {
            var result = client.StreamLogs(parameters).ToList().OrderBy(d => d.DateTime).ThenBy(d => d.Status).ToList();
            expected = expected.OrderBy(d => d.DateTime).ThenBy(d => d.Status).ToList();

            AssertEx.AreEqualLists(expected, result, PrtgAPIHelpers.LogEqualityComparer(), "Lists were not the same");
        }

        private void StreamLogsSerial(LogParameters parameters, List<Log> expected)
        {
            var result = client.StreamLogs(parameters, true).ToList();

            AssertEx.AreEqualLists(expected, result, PrtgAPIHelpers.LogEqualityComparer(), "Lists were not the same");
        }
    }
}
