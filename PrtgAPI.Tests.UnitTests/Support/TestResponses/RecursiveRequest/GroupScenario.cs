using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    abstract class GroupScenario
    {
        protected int requestNum;
        protected ProbeNode probe;

        public IWebResponse GetResponse(string address, string function)
        {
            var components = UrlHelpers.CrackUrl(address);
            Content content = components["content"].DescriptionToEnum<Content>();
            requestNum++;
            return GetResponse(address, content);
        }

        protected abstract IWebResponse GetResponse(string address, Content content);

        protected Exception UnknownRequest(string address)
        {
            return new NotImplementedException($"Don't know how to handle request #{requestNum}: {address}");
        }

        protected SensorResponse GetSensorResponse(List<SensorNode> sensors)
        {
            if (sensors == null)
                sensors = new List<SensorNode>();

            return new SensorResponse(sensors.Select(s => s.GetTestItem()).ToArray());
        }

        protected DeviceResponse GetDeviceResponse(List<DeviceNode> devices)
        {
            if (devices == null)
                devices = new List<DeviceNode>();

            return new DeviceResponse(devices.Select(s => s.GetTestItem()).ToArray());
        }

        protected GroupResponse GetGroupResponse(List<GroupNode> groups)
        {
            if (groups == null)
                groups = new List<GroupNode>();

            return new GroupResponse(groups.Select(s => s.GetTestItem()).ToArray());
        }

        protected void AssertSensorRequest(string address, Content content, string request)
        {
            Assert.AreEqual(Content.Sensors, content);
            Assert.AreEqual(TestHelpers.RequestSensor("count=*&" + request, UrlFlag.Columns), address);
        }

        protected void AssertDeviceRequest(string address, Content content, string request)
        {
            Assert.AreEqual(Content.Devices, content);
            Assert.AreEqual(TestHelpers.RequestDevice("count=*&" + request, UrlFlag.Columns), address);
        }

        protected void AssertGroupRequest(string address, Content content, string request, UrlFlag? flags = null)
        {
            Assert.AreEqual(Content.Groups, content);

            if (flags == null)
                Assert.AreEqual(TestHelpers.RequestGroup("count=*&" + request, UrlFlag.Columns), address);
            else
                Assert.AreEqual(TestHelpers.RequestGroup(request, flags), address);
        }
    }
}