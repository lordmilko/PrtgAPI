using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class AddObjectTests : BaseTest
    {
        #region AddSensor

        [TestMethod]
        public void AddSensor_CanExecute()
        {
            var builder = new StringBuilder();

            builder.Append("addsensor5.htm?name_=XML+Custom+EXE%2fScript+Sensor&exefile_=test.ps1%7ctest.ps1%7c%7c&");
            builder.Append("tags_=xmlexesensor&exeparams_=&environment_=0&usewindowsauthentication_=0&mutexname_=&timeout_=60&writeresult_=0");
            builder.Append("&intervalgroup=1&interval_=60%7c60+seconds&errorintervalsdown_=1&sensortype=exexml&id=1001");

            var client = Initialize_Client(new AddressValidatorResponse(builder.ToString()));

            var parameters = new ExeXmlSensorParameters("test.ps1");

            client.AddSensor(1001, parameters, false);
        }

        [TestMethod]
        public async Task AddSensor_CanExecuteAsync()
        {
            var builder = new StringBuilder();

            builder.Append("addsensor5.htm?name_=XML+Custom+EXE%2fScript+Sensor&exefile_=test.ps1%7ctest.ps1%7c%7c&");
            builder.Append("tags_=xmlexesensor&exeparams_=&environment_=0&usewindowsauthentication_=0&mutexname_=&timeout_=60&writeresult_=0");
            builder.Append("&intervalgroup=1&interval_=60%7c60+seconds&errorintervalsdown_=1&sensortype=exexml&id=1001");

            var client = Initialize_Client(new AddressValidatorResponse(builder.ToString()));

            var parameters = new ExeXmlSensorParameters("test.ps1");

            await client.AddSensorAsync(1001, parameters, false);
        }

        [TestMethod]
        public void AddSensor_AddsExcessiveItems()
        {
            var servicesClient = Initialize_Client(new WmiServiceTargetResponse());
            var services = servicesClient.Targets.GetWmiServices(1001);

            Assert.IsTrue(services.Count > 30);

            var client = GetAddExcessiveSensorClient(services);

            var parameters = new WmiServiceSensorParameters(services);

            client.AddSensor(1001, parameters, false);
        }

        [TestMethod]
        public async Task AddSensor_AddsExcessiveItemsAsync()
        {
            var servicesClient = Initialize_Client(new WmiServiceTargetResponse());
            var services = await servicesClient.Targets.GetWmiServicesAsync(1001);

            Assert.IsTrue(services.Count > 30);

            var client = GetAddExcessiveSensorClient(services);

            var parameters = new WmiServiceSensorParameters(services);

            await client.AddSensorAsync(1001, parameters, false);
        }

        private PrtgClient GetAddExcessiveSensorClient(List<WmiServiceTarget> services)
        {
            var formats = services.Select(s => "service__check=" + HttpUtility.UrlEncode(((IFormattable)s).GetSerializedFormat())).ToList();

            var urls = new List<string>();

            for (int i = 0; i < formats.Count; i += 30)
            {
                var thisRequest = formats.Skip(i).Take(30);

                var str = string.Join("&", thisRequest);

                var url = $"https://prtg.example.com/addsensor5.htm?name_=Service&tags_=wmiservicesensor+servicesensor&restart_=0&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&{str}&id=1001&username=username&passhash=12345678";

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

            AssertEx.Throws<InvalidOperationException>(() => client.AddSensor(1001, parameters, false), "Property 'ExeFile' requires a value");
        }

        [TestMethod]
        public void AddSensor_Throws_WhenMissingRequiredValue_WithEmptyList()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            var parameters = new WmiServiceSensorParameters(new List<SensorTarget<WmiServiceTarget>>().Cast<WmiServiceTarget>().ToList());

            AssertEx.Throws<InvalidOperationException>(() => client.AddSensor(1001, parameters, false), "Property 'Services' requires a value");
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
        public void AddSensor_ResolveScenarios()
        {
            var resolveClient = Initialize_Client(new DiffBasedResolveResponse());
            var sensors = resolveClient.AddSensor(1001, new ExeXmlSensorParameters("test.ps1"));
            Assert.AreEqual(2, sensors.Count);

            var dontResolveClient = Initialize_Client(new DiffBasedResolveResponse());
            var sensor = dontResolveClient.AddSensor(1002, new ExeXmlSensorParameters("test.ps1"), false);

            Assert.AreEqual(null, sensor);
        }

        [TestMethod]
        public async Task AddSensor_ResolveScenariosAsync()
        {
            var resolveMultipleClient = Initialize_Client(new DiffBasedResolveResponse());
            var sensorsMultiple = await resolveMultipleClient.AddSensorAsync(1001, new ExeXmlSensorParameters("test.ps1"));
            Assert.AreEqual(2, sensorsMultiple.Count);

            var resolveSingleClient = Initialize_Client(new DiffBasedResolveResponse(false));
            var sensorsSingle = await resolveSingleClient.AddSensorAsync(1001, new ExeXmlSensorParameters("test.ps1"));
            Assert.AreEqual(1, sensorsSingle.Count);

            var dontResolveClient = Initialize_Client(new DiffBasedResolveResponse());
            var sensor = await dontResolveClient.AddSensorAsync(1002, new ExeXmlSensorParameters("test.ps1"), false);

            Assert.AreEqual(null, sensor);
        }

        [TestMethod]
        public void AddSensor_CleansLeadingSpaces()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(true) {LeadingSpace = true});

            var sensors = client.AddSensor(1001, new ExeXmlSensorParameters("test.ps1"));

            Assert.AreEqual(2, sensors.Count);
            Assert.AreEqual("Volume IO _Total0", sensors[0].Name);
            Assert.AreEqual("Volume IO _Total1", sensors[1].Name);
        }

        [TestMethod]
        public async Task AddSensor_CleansLeadingSpacesAsync()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(true) { LeadingSpace = true });

            var sensors = await client.AddSensorAsync(1001, new ExeXmlSensorParameters("test.ps1"));

            Assert.AreEqual(2, sensors.Count);
            Assert.AreEqual("Volume IO _Total0", sensors[0].Name);
            Assert.AreEqual("Volume IO _Total1", sensors[1].Name);
        }

        #endregion
        #region AddDevice

        [TestMethod]
        public void AddDevice_CanExecute()
        {
            var url = "adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=0&discoveryschedule_=0&id=1001";

            var client = Initialize_Client(new AddressValidatorResponse(url));

            var parameters = new NewDeviceParameters("device", "host");

            client.AddDevice(1001, parameters, false);
        }

        [TestMethod]
        public void AddDevice_Light_CanExecute()
        {
            var url = "adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=1&discoveryschedule_=0&id=1001";

            var client = Initialize_Client(new AddressValidatorResponse(url));

            client.AddDevice(1001, "device", "host", AutoDiscoveryMode.Automatic, false);
        }

        [TestMethod]
        public async Task AddDevice_CanExecuteAsync()
        {
            var url = "adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=0&discoveryschedule_=0&id=1001";

            var client = Initialize_Client(new AddressValidatorResponse(url));

            var parameters = new NewDeviceParameters("device", "host");

            await client.AddDeviceAsync(1001, parameters, false);
        }

        [TestMethod]
        public async Task AddDevice_Light_CanExecuteAsync()
        {
            var url = "adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=1&discoveryschedule_=0&id=1001";

            var client = Initialize_Client(new AddressValidatorResponse(url));

            await client.AddDeviceAsync(1001, "device", "host", AutoDiscoveryMode.Automatic, false);
        }

        [TestMethod]
        public void AddDevice_WithTemplates_CanExecute()
        {
            var templateClient = Initialize_Client(new MultiTypeResponse());
            var templates = templateClient.GetDeviceTemplates().Take(2).ToList();

            var builder = new StringBuilder();
            builder.Append("adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=2&discoveryschedule_=0&devicetemplate_=1&");
            builder.Append("devicetemplate__check=Cisco+ADSL.odt%7cADSL%7c%7c&");
            builder.Append("devicetemplate__check=Cloudwatch.odt%7cAmazon+Cloudwatch%7c%7c&");
            builder.Append("id=1001");

            var client = Initialize_Client(new AddressValidatorResponse(builder.ToString()));

            var parameters = new NewDeviceParameters("device", "host")
            {
                AutoDiscoveryMode = AutoDiscoveryMode.AutomaticTemplate,
                DeviceTemplates = templates
            };

            client.AddDevice(1001, parameters, false);
        }

        [TestMethod]
        public void AddDevice_WithTemplates_Throws_WhenNoTemplatesSpecified()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var parameters = new NewDeviceParameters("device", "host")
            {
                AutoDiscoveryMode = AutoDiscoveryMode.AutomaticTemplate
            };

            AssertEx.Throws<InvalidOperationException>(
                () => client.AddDevice(1001, parameters, false),
                "Property 'DeviceTemplates' requires a value when property 'AutoDiscoveryMode' is value 'AutomaticTemplate', however the value was null or empty."
            );
        }

        [TestMethod]
        public async Task AddDevice_WithTemplates_CanExecuteAsync()
        {
            var templateClient = Initialize_Client(new MultiTypeResponse());
            var templates = (await templateClient.GetDeviceTemplatesAsync()).Take(2).ToList();

            var builder = new StringBuilder();
            builder.Append("adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=2&discoveryschedule_=0&devicetemplate_=1&");
            builder.Append("devicetemplate__check=Cisco+ADSL.odt%7cADSL%7c%7c&");
            builder.Append("devicetemplate__check=Cloudwatch.odt%7cAmazon+Cloudwatch%7c%7c&");
            builder.Append("id=1001");

            var client = Initialize_Client(new AddressValidatorResponse(builder.ToString()));

            var parameters = new NewDeviceParameters("device", "host")
            {
                AutoDiscoveryMode = AutoDiscoveryMode.AutomaticTemplate,
                DeviceTemplates = templates
            };

            await client.AddDeviceAsync(1001, parameters, false);
        }

        [TestMethod]
        public async Task AddDevice_WithTemplates_Throws_WhenNoTemplatesSpecifiedAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var parameters = new NewDeviceParameters("device", "host")
            {
                AutoDiscoveryMode = AutoDiscoveryMode.AutomaticTemplate
            };

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.AddDeviceAsync(1001, parameters, false),
                "Property 'DeviceTemplates' requires a value when property 'AutoDiscoveryMode' is value 'AutomaticTemplate', however the value was null or empty."
            );
        }

        [TestMethod]
        public void AddDevice_ResolveScenarios()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(false));
            var lightDevice = client.AddDevice(1001, "newDevice", "127.0.0.1");
            Assert.AreEqual("Probe Device2", lightDevice.Name);

            var paramsDevice = client.AddDevice(1001, new NewDeviceParameters("newDevice", "127.0.0.1"));
            Assert.AreEqual("Probe Device2", paramsDevice.Name);

            var device = client.AddDevice(1001, "newDevice", "127.0.0.1", resolve: false);
            Assert.AreEqual(null, device);
        }

        [TestMethod]
        public void AddDevice_Throws_ResolvingMultiple()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse());

            var str = "Could not uniquely identify created Device: multiple new objects ('Probe Device2' (ID: 1002), 'Probe Device3' (ID: 1003)) were found";

            AssertEx.Throws<ObjectResolutionException>(() => client.AddDevice(1001, "newDevice", "localhost"), str);
        }

        [TestMethod]
        public async Task AddDevice_ResolveScenariosAsync()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(false));
            var lightDevice = await client.AddDeviceAsync(1001, "newDevice", "127.0.0.1");
            Assert.AreEqual("Probe Device2", lightDevice.Name);

            var paramsDevice = await client.AddDeviceAsync(1001, new NewDeviceParameters("newDevice", "127.0.0.1"));
            Assert.AreEqual("Probe Device2", paramsDevice.Name);

            var device = await client.AddDeviceAsync(1001, "newDevice", "127.0.0.1", resolve: false);
            Assert.AreEqual(null, device);
        }

        [TestMethod]
        public async Task AddDevice_Throws_ResolvingMultipleAsync()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse());

            var str = "Could not uniquely identify created Device: multiple new objects ('Probe Device2' (ID: 1002), 'Probe Device3' (ID: 1003)) were found";

            await AssertEx.ThrowsAsync<ObjectResolutionException>(async () => await client.AddDeviceAsync(1001, "newDevice", "localhost"), str);
        }

        #endregion
        #region AddGroup

        [TestMethod]
        public void AddGroup_CanExecute()
        {
            var client = Initialize_Client(new AddressValidatorResponse("addgroup2.htm?name_=group&id=1001"));

            var parameters = new NewGroupParameters("group");

            client.AddGroup(1001, parameters, false);
        }

        [TestMethod]
        public void AddGroup_Light_CanExecute()
        {
            var client = Initialize_Client(new AddressValidatorResponse("addgroup2.htm?name_=group&id=1001"));

            client.AddGroup(1001, "group", false);
        }

        [TestMethod]
        public async Task AddGroup_CanExecuteAsync()
        {
            var client = Initialize_Client(new AddressValidatorResponse("addgroup2.htm?name_=group&id=1001"));

            var parameters = new NewGroupParameters("group");

            await client.AddGroupAsync(1001, parameters, false);
        }

        [TestMethod]
        public async Task AddGroup_Light_CanExecuteAsync()
        {
            var client = Initialize_Client(new AddressValidatorResponse("addgroup2.htm?name_=group&id=1001"));

            await client.AddGroupAsync(1001, "group", false);
        }

        [TestMethod]
        public void AddGroup_ResolveScenarios()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(false));
            var lightGroup = client.AddGroup(1001, "newGroup");
            Assert.AreEqual("Windows Infrastructure2", lightGroup.Name);

            var paramsGroup = client.AddGroup(1001, new NewGroupParameters("newGroup"));
            Assert.AreEqual("Windows Infrastructure2", paramsGroup.Name);

            var group = client.AddGroup(1001, "newGroup", false);
            Assert.AreEqual(null, group);
        }

        [TestMethod]
        public void AddGroup_Throws_ResolvingMultiple()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse());

            AssertEx.Throws<ObjectResolutionException>(() => client.AddGroup(1001, "newGroup"), "Could not uniquely identify created Group");
        }

        [TestMethod]
        public async Task AddGroup_ResolveScenariosAsync()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(false));
            var lightGroup = await client.AddGroupAsync(1001, "newGroup");
            Assert.AreEqual("Windows Infrastructure2", lightGroup.Name);

            var paramsGroup = await client.AddGroupAsync(1001, new NewGroupParameters("newGroup"));
            Assert.AreEqual("Windows Infrastructure2", paramsGroup.Name);

            var group = await client.AddGroupAsync(1001, "newGroup", false);
            Assert.AreEqual(null, group);
        }

        [TestMethod]
        public async Task AddGroup_Throws_ResolvingMultipleAsync()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse());

            await AssertEx.ThrowsAsync<ObjectResolutionException>(async () => await client.AddGroupAsync(1001, "newDevice"), "Could not uniquely identify created Group");
        }

        #endregion

        [TestMethod]
        public void AddObject_Throws_FailingToResolve()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            AssertEx.Throws<ObjectResolutionException>(() => client.AddGroup(1001, "newDevice"), "Could not resolve object: PRTG is taking too long to create the object");
        }

        [TestMethod]
        public async Task AddObject_Throws_FailingToResolveAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<ObjectResolutionException>(async () => await client.AddGroupAsync(1001, "newDevice"), "Could not resolve object: PRTG is taking too long to create the object");
        }
    }
}
