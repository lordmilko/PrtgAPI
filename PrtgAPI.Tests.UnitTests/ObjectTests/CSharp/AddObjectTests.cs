using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class AddObjectTests : BaseTest
    {
        [TestMethod]
        public void AddSensor_CanExecute()
        {
            var builder = new StringBuilder();

            builder.Append("addsensor5.htm?name_=XML+Custom+EXE%2fScript+Sensor&priority_=3&inherittriggers_=1&exefile_=test.ps1%7ctest.ps1%7c%7c&");
            builder.Append("tags_=xmlexesensor&exeparams_=&environment_=0&usewindowsauthentication_=0&mutexname_=&timeout_=60&writeresult_=0");
            builder.Append("&intervalgroup=1&interval_=60%7c60+seconds&errorintervalsdown_=1&sensortype=exexml&id=1001");

            var client = Initialize_Client(new AddressValidatorResponse(builder.ToString()));

            var parameters = new ExeXmlSensorParameters("test.ps1");

            client.AddSensor(1001, parameters);
        }

        [TestMethod]
        public async Task AddSensor_CanExecuteAsync()
        {
            var builder = new StringBuilder();

            builder.Append("addsensor5.htm?name_=XML+Custom+EXE%2fScript+Sensor&priority_=3&inherittriggers_=1&exefile_=test.ps1%7ctest.ps1%7c%7c&");
            builder.Append("tags_=xmlexesensor&exeparams_=&environment_=0&usewindowsauthentication_=0&mutexname_=&timeout_=60&writeresult_=0");
            builder.Append("&intervalgroup=1&interval_=60%7c60+seconds&errorintervalsdown_=1&sensortype=exexml&id=1001");

            var client = Initialize_Client(new AddressValidatorResponse(builder.ToString()));

            var parameters = new ExeXmlSensorParameters("test.ps1");

            await client.AddSensorAsync(1001, parameters);
        }

        [TestMethod]
        public void AddSensor_AddsExcessiveItems()
        {
            var servicesClient = Initialize_Client(new WmiServiceTargetResponse());
            var services = servicesClient.Targets.GetWmiServices(1001);

            Assert.IsTrue(services.Count > 30);

            var client = GetAddExcessiveSensorClient(services);

            var parameters = new WmiServiceSensorParameters(services);

            client.AddSensor(1001, parameters);
        }

        [TestMethod]
        public async Task AddSensor_AddsExcessiveItemsAsync()
        {
            var servicesClient = Initialize_Client(new WmiServiceTargetResponse());
            var services = await servicesClient.Targets.GetWmiServicesAsync(1001);

            Assert.IsTrue(services.Count > 30);

            var client = GetAddExcessiveSensorClient(services);

            var parameters = new WmiServiceSensorParameters(services);

            await client.AddSensorAsync(1001, parameters);
        }

        private PrtgClient GetAddExcessiveSensorClient(List<WmiServiceTarget> services)
        {
            var formats = services.Select(s => "service__check=" + HttpUtility.UrlEncode(((IFormattable)s).GetSerializedFormat())).ToList();

            var urls = new List<string>();

            for (int i = 0; i < formats.Count; i += 30)
            {
                var thisRequest = formats.Skip(i).Take(30);

                var str = string.Join("&", thisRequest);

                var url = $"https://prtg.example.com/addsensor5.htm?name_=Service&priority_=3&inherittriggers_=1&tags_=wmiservicesensor+servicesensor&restart_=0&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&{str}&id=1001&username=username&passhash=12345678";

                urls.Add(url);
            }

            var client = Initialize_Client(new AddressValidatorResponse(urls.ToArray(), true));

            return client;
        }

        [TestMethod]
        public void AddSensor_Throws_WhenMissingRequiredValue()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            var parameters = new ExeXmlSensorParameters("test.ps1")
            {
                ExeFile = null
            };

            AssertEx.Throws<InvalidOperationException>(() => client.AddSensor(1001, parameters), "Property 'ExeFile' requires a value");
        }

        [TestMethod]
        public void AddSensor_Throws_WhenMissingRequiredValue_WithEmptyList()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            var parameters = new WmiServiceSensorParameters(new List<SensorTarget<WmiServiceTarget>>().Cast<WmiServiceTarget>().ToList());

            AssertEx.Throws<InvalidOperationException>(() => client.AddSensor(1001, parameters), "Property 'Services' requires a value");
        }

        [TestMethod]
        public void AddSensor_Throws_MissingSensorName()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            AssertEx.Throws<ArgumentException>(() => client.AddSensor(1001, new ExeXmlSensorParameters("test.ps1", null)), "objectName cannot be null or empty");
        }

        [TestMethod]
        public void AddSensor_Throws_MissingSensorType()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            AssertEx.Throws<ArgumentException>(() => client.AddSensor(1001, new RawSensorParameters("sensorName", null)), "sensorType cannot be null or empty");
        }

        [TestMethod]
        public void AddDevice_CanExecute()
        {
            var url = "adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=0&discoveryschedule_=0&id=1001";

            var client = Initialize_Client(new AddressValidatorResponse(url));

            var parameters = new NewDeviceParameters("device", "host");

            client.AddDevice(1001, parameters);
        }

        [TestMethod]
        public async Task AddDevice_CanExecuteAsync()
        {
            var url = "adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=0&discoveryschedule_=0&id=1001";

            var client = Initialize_Client(new AddressValidatorResponse(url));

            var parameters = new NewDeviceParameters("device", "host");

            await client.AddDeviceAsync(1001, parameters);
        }

        [TestMethod]
        public void AddGroup_CanExecute()
        {
            var client = Initialize_Client(new AddressValidatorResponse("addgroup2.htm?name_=group&id=1001"));

            var parameters = new NewGroupParameters("group");

            client.AddGroup(1001, parameters);
        }

        [TestMethod]
        public async Task AddGroup_CanExecuteAsync()
        {
            var client = Initialize_Client(new AddressValidatorResponse("addgroup2.htm?name_=group&id=1001"));

            var parameters = new NewGroupParameters("group");

            await client.AddGroupAsync(1001, parameters);
        }
    }
}
