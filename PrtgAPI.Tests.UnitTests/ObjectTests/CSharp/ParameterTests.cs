using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    class FakeSensorParameters : RawSensorParameters
    {
        public FakeSensorParameters() : base("fake_name", "fake_type")
        {
        }

        public int RestartStage
        {
            get { return (int)GetCustomParameterEnumXml<int>(ObjectProperty.AutoDiscoverySchedule); }
            set { SetCustomParameterEnumXml<int>(ObjectProperty.AutoDiscoverySchedule, value); }
        }
    }

    [TestClass]
    public class ParameterTests
    {
        [TestMethod]
        public void TableParameters_CanSetSortDirection()
        {
            var parameters = new SensorParameters();

            parameters.SortBy = Property.Id;
            Assert.AreEqual(parameters.SortBy, Property.Id, "Retrieve initial value from property");
            Assert.AreEqual(parameters[Parameter.SortBy], Property.Id, "Retrieve initial raw value from indexer");

            parameters.SortDirection = SortDirection.Descending;
            Assert.AreEqual(parameters.SortBy, Property.Id, "Retrieve initial reversed value from property");
            Assert.AreEqual(parameters[Parameter.SortBy], "-objid", "Retrieve initial reversed raw value from indexer");

            parameters.SortBy = Property.Name;
            Assert.AreEqual(parameters.SortBy, Property.Name, "Retrieve new value from property");
            Assert.AreEqual(parameters[Parameter.SortBy], "-name", "Retrieve new reversed raw value from indexer");

            parameters.SortDirection = SortDirection.Ascending;
            Assert.AreEqual(parameters.SortBy, Property.Name, "Retrieve new forwards value from property");
            Assert.AreEqual(parameters[Parameter.SortBy], Property.Name, "Retrieve new forwards raw value from indexer");

            parameters.SortBy = null;
            Assert.AreEqual(parameters.SortBy, null, "Property value is null");
            Assert.AreEqual(parameters[Parameter.SortBy], null, "Raw value is null");

            parameters.SortDirection = SortDirection.Descending;
            parameters.SortBy = Property.Active;

            parameters.SortBy = null;
            Assert.AreEqual(parameters.SortBy, null, "Reversed property value is null");
            Assert.AreEqual(parameters[Parameter.SortBy], null, "Reversed raw value is null");

            parameters[Parameter.SortBy] = "name";
            Assert.AreEqual(parameters.SortBy, Property.Name, "Retrieve property directly set using string");
        }

        [TestMethod]
        public void CustomParameter_ToString_FormatsCorrectly()
        {
            var parameter = new CustomParameter("name", "val");

            Assert.AreEqual("name=val", parameter.ToString());
        }

        [TestMethod]
        public void SensorParameters_Status_CanBeGetAndSet()
        {
            var parameters = new SensorParameters();

            //Test an empty value can be retrieved
            var status = parameters.Status;
            Assert.IsTrue(status == null, "Status was not null");

            //Test a value can be set
            parameters.Status = new[] { Status.Up };
            Assert.IsTrue(parameters.Status.Length == 1 && parameters.Status.First() == Status.Up, "Status was not up");

            //Test a value can be overwritten
            parameters.Status = new[] { Status.Down };
            Assert.IsTrue(parameters.Status.Length == 1 && parameters.Status.First() == Status.Down, "Status was not down");
        }

        #region LogParameters

        [TestMethod]
        public void LogParameters_Date_CanBeGetAndSet()
        {
            var parameters = new LogParameters(null);

            var startDate = parameters.StartDate;
            Assert.IsTrue(startDate == null, "Status was not null");

            var date = DateTime.Now;
            parameters.StartDate = date;
            Assert.IsTrue(parameters.StartDate.ToString() == date.ToString(), $"Status was not {date}");

            var tomorrowStart = DateTime.Now.AddDays(1);
            var tomorrowEnd = DateTime.Now.AddDays(1).AddHours(3);
            parameters.StartDate = tomorrowStart;
            parameters.EndDate = tomorrowEnd;
            Assert.IsTrue(parameters.EndDate.ToString() == tomorrowEnd.ToString(), $"Updated start status was not {date}");
            Assert.IsTrue(parameters.EndDate.ToString() == tomorrowEnd.ToString(), $"Updated end status was not {date}");
        }

        [TestMethod]
        public void LogParameters_SetsRecordAge_InConstructor()
        {
            var parameters = new LogParameters(1001, RecordAge.LastSixMonths);

            Assert.AreEqual(parameters.RecordAge, RecordAge.LastSixMonths);
        }

        [TestMethod]
        public void LogParameters_SetsStartAndEnd_InConstructor()
        {
            var start = DateTime.Now;
            var end = DateTime.Now.AddDays(1);

            var parameters = new LogParameters(null, start, end);

            Assert.AreEqual(start.ToString(), parameters.StartDate.ToString(), "Start was not correct");
            Assert.AreEqual(end.ToString(), parameters.EndDate.ToString(), "End was not correct");
        }

        #endregion
        #region NewSensorParameters

        [TestMethod]
        public void NewSensorParameters_Enum_CanBeSet()
        {
            var parameters = new ExeXmlSensorParameters("test.ps1")
            {
                Priority = Priority.Three
            };

            Assert.AreEqual(Priority.Three, parameters.Priority);
        }

        [TestMethod]
        public void NewSensorParameters_Enum_CanBeSetToNull()
        {
            var parameters = new ExeXmlSensorParameters("test.ps1")
            {
                Priority = null
            };

            Assert.AreEqual(null, parameters.Priority);
        }

        [TestMethod]
        public void NewSensorParameters_Enum_Throws_WhenSetNotEnum()
        {
            var parameters = new FakeSensorParameters();

            AssertEx.Throws<InvalidCastException>(() => parameters.RestartStage = 1, "Unable to cast object of type 'System.Int32' to type 'System.Enum'");
        }

        #endregion

        [TestMethod]
        public void RawSensorParameters_Parameters_InitializesIfNull()
        {
            var parameters = new RawSensorParameters("testName", "sensorType")
            {
                [Parameter.Custom] = null
            };

            Assert.AreEqual(typeof (List<CustomParameter>), parameters.Parameters.GetType());
        }

        [TestMethod]
        public void RawSensorParameters_CanBeUsedAsDictionary()
        {
            var parameters = new RawSensorParameters("testName", "sensorType");

            parameters["customParam"] = 3;
            Assert.AreEqual(3, parameters["customParam"]);

            parameters["customParam_"] = 4;
            Assert.AreEqual(4, parameters["customParam_"]);

            Assert.AreNotEqual(parameters["customParam"], parameters["customParam_"]);

            Assert.IsTrue(parameters.Contains("customParam"));

            parameters["CUSTOMPARAM"] = 5;
            Assert.AreEqual(5, parameters["CUSTOMPARAM"]);
            Assert.AreEqual(5, parameters["customParam"]);

            Assert.IsTrue(parameters.Contains("customParam_"));

            parameters.Remove("customParam_");

            Assert.IsFalse(parameters.Contains("customParam_"));
            AssertEx.Throws<InvalidOperationException>(() =>
            {
                var val = parameters["customParam_"];
            }, "Parameter with name 'customParam_' does not exist.");
        }

        [TestMethod]
        public void SensorHistoryParameters_GetsProperties()
        {
            var start = DateTime.Now.AddHours(-1);
            var parameters = new SensorHistoryParameters(1001, 600, null, null);

            Assert.AreEqual(parameters.StartDate.ToString(), start.ToString());
            Assert.AreEqual(parameters.EndDate.ToString(), start.AddHours(1).ToString());
            Assert.AreEqual(parameters.Average, 600);
        }

        [TestMethod]
        public void SensorHistoryParameters_Throws_WhenAverageIsLessThanZero()
        {
            AssertEx.Throws<ArgumentException>(() => new SensorHistoryParameters(1001, -1, null, null), "Average must be greater than or equal to 0");
        }

        [TestMethod]
        public void NewDeviceParameters_SwapsHostWithIPVersion()
        {
            var parameters = new NewDeviceParameters("device", "dc-1");
            Assert.AreEqual("dc-1", GetCustomParameter(parameters, "host_"));
            Assert.AreEqual("dc-1", parameters.Host);

            parameters.IPVersion = IPVersion.IPv6;
            Assert.AreEqual("dc-1", GetCustomParameter(parameters, "hostv6_"));
            Assert.AreEqual(null, GetCustomParameter(parameters, "host_"));
            Assert.AreEqual("dc-1", parameters.Host);

            parameters.IPVersion = IPVersion.IPv4;
            Assert.AreEqual("dc-1", GetCustomParameter(parameters, "host_"));
            Assert.AreEqual(null, GetCustomParameter(parameters, "hostv6_"));
            Assert.AreEqual("dc-1", parameters.Host);
        }

        [TestMethod]
        public void NewDeviceParameters_AssignsHostToCorrectProperty()
        {
            var parameters = new NewDeviceParameters("device", "dc-1");
            Assert.AreEqual("dc-1", GetCustomParameter(parameters, "host_"));

            parameters.Host = "dc-2";
            Assert.AreEqual("dc-2", GetCustomParameter(parameters, "host_"));

            parameters.IPVersion = IPVersion.IPv6;
            Assert.AreEqual("dc-2", GetCustomParameter(parameters, "hostv6_"));

            parameters.Host = "dc-3";
            Assert.AreEqual("dc-3", GetCustomParameter(parameters, "hostv6_"));
        }

        private string GetCustomParameter(Parameters.Parameters parameters, string name)
        {
            var customParameters = ((List<CustomParameter>) parameters.GetParameters()[Parameter.Custom]);

            var targetParameter = customParameters.FirstOrDefault(p => p.Name == name);

            return targetParameter?.Value?.ToString();
        }

        [TestMethod]
        public void Parameters_ReplacesCounterpart()
        {
            var parameters = new Parameters.Parameters
            {
                [Parameter.Password] = "password",
                [Parameter.PassHash] = "passhash"
            };

            Assert.AreEqual(1, parameters.GetParameters().Keys.Count);
            Assert.AreEqual(parameters[Parameter.PassHash], "passhash");
            Assert.AreEqual(null, parameters[Parameter.Password]);
        }
    }
}
