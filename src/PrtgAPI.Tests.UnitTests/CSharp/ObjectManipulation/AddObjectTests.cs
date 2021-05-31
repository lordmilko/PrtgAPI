using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Request;
using PrtgAPI.Targets;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class AddObjectTests : BaseTest
    {
        #region AddSensor

        [UnitTest]
        [TestMethod]
        public void AddSensor_CanExecute_v14() => AddSensor(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task AddSensor_CanExecuteAsync_v14() => await AddSensorAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void AddSensor_CanExecute_v18() => AddSensor(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task AddSensor_CanExecuteAsync_v18() => await AddSensorAsync(RequestVersion.v18_1);

        private void AddSensor(RequestVersion version)
        {
            var builder = new StringBuilder();

            builder.Append("https://prtg.example.com/addsensor5.htm?name_=XML+Custom+EXE%2FScript+Sensor&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds");
            builder.Append("&errorintervalsdown_=1&tags_=xmlexesensor&exefile_=test.ps1%7Ctest.ps1%7C%7C&exeparams_=&environment_=0");
            builder.Append("&usewindowsauthentication_=0&mutexname_=&timeout_=60&writeresult_=0&sensortype=exexml&id=1001&");

            var auth = version == RequestVersion.v18_1 ? "tmpid=2" : "username=username&passhash=12345678";

            builder.Append(auth);

            var urls = new List<string>
            {
                ////We don't request status since we already injected our version
                UnitRequest.BeginAddSensorQuery(1001, "exexml"),
                builder.ToString()
            };

            if (version == RequestVersion.v18_1)
                urls.Add(UnitRequest.AddSensorProgress(1001, 2, true));

            var parameters = new ExeXmlSensorParameters("test.ps1");

            Execute(
                c => c.AddSensor(1001, parameters, false),
                urls.ToArray(),
                version: version
            );
        }

        private async Task AddSensorAsync(RequestVersion version)
        {
            var builder = new StringBuilder();

            builder.Append("https://prtg.example.com/addsensor5.htm?name_=XML+Custom+EXE%2FScript+Sensor&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds");
            builder.Append("&errorintervalsdown_=1&tags_=xmlexesensor&exefile_=test.ps1%7Ctest.ps1%7C%7C&exeparams_=&environment_=0");
            builder.Append("&usewindowsauthentication_=0&mutexname_=&timeout_=60&writeresult_=0&sensortype=exexml&id=1001&");

            var auth = version == RequestVersion.v18_1 ? "tmpid=2" : "username=username&passhash=12345678";

            builder.Append(auth);

            var urls = new List<string>
            {
                ////We don't request status since we already injected our version
                UnitRequest.BeginAddSensorQuery(1001, "exexml"),
                builder.ToString()
            };

            if (version == RequestVersion.v18_1)
                urls.Add(UnitRequest.AddSensorProgress(1001, 2, true));

            var parameters = new ExeXmlSensorParameters("test.ps1");

            await ExecuteAsync(
                async c => await c.AddSensorAsync(1001, parameters, false),
                urls.ToArray(),
                version: version
            );
        }

        #region Excessive

        [UnitTest]
        [TestMethod]
        public void AddSensor_AddsExcessiveItems_v14() => AddSensorWithExcessiveItems(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task AddSensor_AddsExcessiveItemsAsync_v14() => await AddSensorWithExcessiveItemsAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void AddSensor_AddsExcessiveItems_v18() => AddSensorWithExcessiveItems(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task AddSensor_AddsExcessiveItemsAsync_v18() => await AddSensorWithExcessiveItemsAsync(RequestVersion.v18_1);

        private void AddSensorWithExcessiveItems(RequestVersion version)
        {
            var servicesClient = Initialize_Client(new WmiServiceTargetResponse());
            var services = servicesClient.Targets.GetWmiServices(1001);

            Assert.IsTrue(services.Count > 30);

            var client = GetAddExcessiveSensorClient(services, version);

            var parameters = new WmiServiceSensorParameters(services);

            client.Item1.AddSensor(1001, parameters, false);

            client.Item2.AssertFinished();
        }

        private async Task AddSensorWithExcessiveItemsAsync(RequestVersion version)
        {
            var servicesClient = Initialize_Client(new WmiServiceTargetResponse());
            var services = await servicesClient.Targets.GetWmiServicesAsync(1001);

            Assert.IsTrue(services.Count > 30);

            var client = GetAddExcessiveSensorClient(services, version);

            var parameters = new WmiServiceSensorParameters(services);

            await client.Item1.AddSensorAsync(1001, parameters, false);

            client.Item2.AssertFinished();
        }

        private Tuple<PrtgClient, AddressValidatorResponse> GetAddExcessiveSensorClient(List<WmiServiceTarget> services, RequestVersion version)
        {
            var formats = services.Select(s => "service__check=" + WebUtility.UrlEncode(((ISerializable)s).GetSerializedFormat())).ToList();

            var urls = new List<string>();

            //We don't request status since we already injected our version
            urls.Add(UnitRequest.BeginAddSensorQuery(1001, "wmiservice"));

            for (int i = 0; i < formats.Count; i += 30)
            {
                var thisRequest = formats.Skip(i).Take(30);

                var str = string.Join("&", thisRequest);

                var auth = version == RequestVersion.v18_1 ? "tmpid=2" : "username=username&passhash=12345678";

                var url = $"https://prtg.example.com/addsensor5.htm?name_=Service&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=wmiservicesensor+servicesensor&restart_=0&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&{str}&id=1001&{auth}";

                urls.Add(url);

                if (i == 0 && version == RequestVersion.v18_1)
                {
                    urls.Add(UnitRequest.AddSensorProgress(1001, 2, true));
                }
            }

#pragma warning disable 618
            var response = new AddressValidatorResponse(urls.ToArray(), true);

            var client = Initialize_Client(response, version);
#pragma warning restore 618

            return Tuple.Create(client, response);
        }

        #endregion

        [UnitTest]
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

        [UnitTest]
        [TestMethod]
        public void AddSensor_Throws_WhenMissingRequiredValue_WithEmptyList()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            var parameters = new WmiServiceSensorParameters(new List<SensorTarget<WmiServiceTarget>>().Cast<WmiServiceTarget>().ToList());

            AssertEx.Throws<InvalidOperationException>(() => client.AddSensor(1001, parameters, false), "Property 'Services' requires a value");
        }

        [UnitTest]
        [TestMethod]
        public void AddSensor_Throws_MissingSensorName()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            AssertEx.Throws<ArgumentException>(() => client.AddSensor(1001, new ExeXmlSensorParameters("test.ps1", null)), "An object name cannot be null.");
        }

        [UnitTest]
        [TestMethod]
        public void AddSensor_Throws_MissingSensorType()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            AssertEx.Throws<ArgumentException>(() => client.AddSensor(1001, new RawSensorParameters("sensorName", null)), "SensorType cannot be null or empty.");
        }

        [UnitTest]
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

        [UnitTest]
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

        [UnitTest]
        [TestMethod]
        public void AddSensor_CleansLeadingSpaces()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(true) {LeadingSpace = true});

            var sensors = client.AddSensor(1001, new ExeXmlSensorParameters("test.ps1"));

            Assert.AreEqual(2, sensors.Count);
            Assert.AreEqual("Volume IO _Total0", sensors[0].Name);
            Assert.AreEqual("Volume IO _Total1", sensors[1].Name);
        }

        [UnitTest]
        [TestMethod]
        public async Task AddSensor_CleansLeadingSpacesAsync()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(true) { LeadingSpace = true });

            var sensors = await client.AddSensorAsync(1001, new ExeXmlSensorParameters("test.ps1"));

            Assert.AreEqual(2, sensors.Count);
            Assert.AreEqual("Volume IO _Total0", sensors[0].Name);
            Assert.AreEqual("Volume IO _Total1", sensors[1].Name);
        }

        [UnitTest]
        [TestMethod]
        public void AddSensor_SensorQueryTarget_IgnoresTarget()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.Status(),
                UnitRequest.BeginAddSensorQuery(1001, "snmplibrary"),
                UnitRequest.AddSensor("name_=test&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&sensortype=snmplibrary&id=1001")
            }));

            var parameters = new RawSensorParameters("test", "snmplibrary");

            client.AddSensor(1001, parameters, false);
        }

        [UnitTest]
        [TestMethod]
        public void AddSensor_SensorQueryParameters_SynthesizesParameters()
        {
            var client = Initialize_Client(new SensorQueryTargetParametersValidatorResponse(new[]
            {
                UnitRequest.Status(),
                UnitRequest.BeginAddSensorQuery(1001, "oracletablespace"),
                UnitRequest.ContinueAddSensorQuery(2055, 7, "database_=XE&sid_type_=0&prefix_=0"), //Response hardcodes 2055, however normally this will in fact match
                UnitRequest.AddSensor("name_=test&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&database=XE&sid_type=0&prefix=0&sensortype=oracletablespace&id=1001")
            }));

            var parameters = new RawSensorParameters("test", "oracletablespace")
            {
                ["database"] = "XE",
                ["sid_type"] = 0,
                ["prefix"] = 0
            };

            client.AddSensor(1001, parameters, false);
        }

        [UnitTest]
        [TestMethod]
        public void AddSensor_SensorQueryParameters_Throws_WhenSynthesizedParametersAreMissing()
        {
            var client = Initialize_Client(new SensorQueryTargetParametersValidatorResponse(new[]
            {
                UnitRequest.Status(),
                UnitRequest.BeginAddSensorQuery(1001, "oracletablespace"),
            }));

            var parameters = new RawSensorParameters("test", "oracletablespace")
            {
                ["sid_type"] = 0,
                ["prefix"] = 0
            };

            AssertEx.Throws<InvalidOperationException>(
                () => client.AddSensor(1001, parameters, false),
                "Failed to process request for sensor type 'oracletablespace': sensor query target parameters did not include mandatory parameter 'database_'."
            );
        }

        #endregion
        #region AddDevice

        [UnitTest]
        [TestMethod]
        public void AddDevice_CanExecute()
        {
            var url = "adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=0&discoveryschedule_=0&id=1001";

            var parameters = new NewDeviceParameters("device", "host");

            Execute(
                c => c.AddDevice(1001, parameters, false),
                url
            );
        }

        [UnitTest]
        [TestMethod]
        public void AddDevice_Light_CanExecute()
        {
            var url = "adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=1&discoveryschedule_=0&id=1001";

            Execute(
                c => c.AddDevice(1001, "device", "host", AutoDiscoveryMode.Automatic, false),
                url
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task AddDevice_CanExecuteAsync()
        {
            var url = "adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=0&discoveryschedule_=0&id=1001";

            var parameters = new NewDeviceParameters("device", "host");

            await ExecuteAsync(
                async c => await c.AddDeviceAsync(1001, parameters, false),
                url
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task AddDevice_Light_CanExecuteAsync()
        {
            var url = "adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=1&discoveryschedule_=0&id=1001";

            await ExecuteAsync(
                async c => await c.AddDeviceAsync(1001, "device", "host", AutoDiscoveryMode.Automatic, false),
                url
            );
        }

        [UnitTest]
        [TestMethod]
        public void AddDevice_WithTemplates_CanExecute()
        {
            var templateClient = Initialize_Client(new MultiTypeResponse());
            var templates = templateClient.GetDeviceTemplates().Take(2).ToList();

            var builder = new StringBuilder();
            builder.Append("adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=2&discoveryschedule_=0&devicetemplate_=1&");
            builder.Append("devicetemplate__check=Cisco+ADSL.odt%7CADSL%7C%7C&");
            builder.Append("devicetemplate__check=Cloudwatch.odt%7CAmazon+Cloudwatch%7C%7C&");
            builder.Append("id=1001");

            var parameters = new NewDeviceParameters("device", "host")
            {
                AutoDiscoveryMode = AutoDiscoveryMode.AutomaticTemplate,
                DeviceTemplates = templates
            };

            Execute(
                c => c.AddDevice(1001, parameters, false),
                builder.ToString()
            );
        }

        [UnitTest]
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
                "Property 'DeviceTemplates' requires a value when property 'AutoDiscoveryMode' is value 'AutomaticTemplate', however the value was null, empty or whitespace."
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task AddDevice_WithTemplates_CanExecuteAsync()
        {
            var templateClient = Initialize_Client(new MultiTypeResponse());
            var templates = (await templateClient.GetDeviceTemplatesAsync()).Take(2).ToList();

            var builder = new StringBuilder();
            builder.Append("adddevice2.htm?name_=device&host_=host&ipversion_=0&discoverytype_=2&discoveryschedule_=0&devicetemplate_=1&");
            builder.Append("devicetemplate__check=Cisco+ADSL.odt%7CADSL%7C%7C&");
            builder.Append("devicetemplate__check=Cloudwatch.odt%7CAmazon+Cloudwatch%7C%7C&");
            builder.Append("id=1001");

            var parameters = new NewDeviceParameters("device", "host")
            {
                AutoDiscoveryMode = AutoDiscoveryMode.AutomaticTemplate,
                DeviceTemplates = templates
            };

            await ExecuteAsync(
                async c => await c.AddDeviceAsync(1001, parameters, false),
                builder.ToString()
            );
        }

        [UnitTest]
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
                "Property 'DeviceTemplates' requires a value when property 'AutoDiscoveryMode' is value 'AutomaticTemplate', however the value was null, empty or whitespace."
            );
        }

        [UnitTest]
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

        [UnitTest]
        [TestMethod]
        public void AddDevice_Throws_ResolvingMultiple()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse());

            var str = "Could not uniquely identify created Device: multiple new objects ('Probe Device2' (ID: 1002), 'Probe Device3' (ID: 1003)) were found";

            AssertEx.Throws<ObjectResolutionException>(() => client.AddDevice(1001, "newDevice", "localhost"), str);
        }

        [UnitTest]
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

        [UnitTest]
        [TestMethod]
        public async Task AddDevice_Throws_ResolvingMultipleAsync()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse());

            var str = "Could not uniquely identify created Device: multiple new objects ('Probe Device2' (ID: 1002), 'Probe Device3' (ID: 1003)) were found";

            await AssertEx.ThrowsAsync<ObjectResolutionException>(async () => await client.AddDeviceAsync(1001, "newDevice", "localhost"), str);
        }

        [UnitTest]
        [TestMethod]
        public void DeviceTemplate_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var templates = client.GetDeviceTemplates(300);

            AssertEx.AllPropertiesRetrieveValues(templates);
        }

        [UnitTest]
        [TestMethod]
        public async Task DeviceTemplate_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var templates = await client.GetDeviceTemplatesAsync(300);

            AssertEx.AllPropertiesRetrieveValues(templates);
        }

        #endregion
        #region AddGroup

        [UnitTest]
        [TestMethod]
        public void AddGroup_CanExecute()
        {
            var parameters = new NewGroupParameters("group");

            Execute(
                c => c.AddGroup(1001, parameters, false),
                "addgroup2.htm?name_=group&id=1001"
            );
        }

        [UnitTest]
        [TestMethod]
        public void AddGroup_Light_CanExecute()
        {
            Execute(
                c => c.AddGroup(1001, "group", false),
                "addgroup2.htm?name_=group&id=1001"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task AddGroup_CanExecuteAsync()
        {
            var parameters = new NewGroupParameters("group");

            await ExecuteAsync(
                async c => await c.AddGroupAsync(1001, parameters, false),
                "addgroup2.htm?name_=group&id=1001"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task AddGroup_Light_CanExecuteAsync()
        {
            await ExecuteAsync(
                async c => await c.AddGroupAsync(1001, "group", false),
                "addgroup2.htm?name_=group&id=1001"
            );
        }

        [UnitTest]
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

        [UnitTest]
        [TestMethod]
        public void AddGroup_Throws_ResolvingMultiple()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse());

            AssertEx.Throws<ObjectResolutionException>(() => client.AddGroup(1001, "newGroup"), "Could not uniquely identify created Group");
        }

        [UnitTest]
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

        [UnitTest]
        [TestMethod]
        public async Task AddGroup_Throws_ResolvingMultipleAsync()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse());

            await AssertEx.ThrowsAsync<ObjectResolutionException>(async () => await client.AddGroupAsync(1001, "newDevice"), "Could not uniquely identify created Group");
        }

        #endregion

        [UnitTest]
        [TestMethod]
        public void AddObject_Throws_FailingToResolve()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            AssertEx.Throws<ObjectResolutionException>(() => client.AddGroup(1001, "newDevice"), "Could not resolve object: PRTG is taking too long to create the object");
        }

        [UnitTest]
        [TestMethod]
        public async Task AddObject_Throws_FailingToResolveAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<ObjectResolutionException>(async () => await client.AddGroupAsync(1001, "newDevice"), "Could not resolve object: PRTG is taking too long to create the object");
        }

        [UnitTest]
        [TestMethod]
        public void AddObject_Throws_FailingToResolve_EnhancedError_Sensor()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var parameters = new ExeXmlSensorParameters("test.ps1");
            AssertEx.Throws<ObjectResolutionException>(() => client.AddSensor(1001, parameters), "DynamicType = true");
        }
    }
}
