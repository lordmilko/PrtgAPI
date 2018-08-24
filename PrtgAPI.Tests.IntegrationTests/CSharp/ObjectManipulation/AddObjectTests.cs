using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.IntegrationTests.ObjectManipulation.Types;

namespace PrtgAPI.Tests.IntegrationTests.ObjectManipulation
{
    [TestClass]
    public class AddObjectTests : BasePrtgClientTest
    {
        #region AddSensor

        [TestMethod]
        public void AddSensor_AddsWithRawParameters()
        {
            var parameters = new RawSensorParameters("raw c# sensor", "exexml");
            parameters.Parameters.AddRange(
                new List<CustomParameter>
                {
                    new CustomParameter("tags_", "xmlexesensor"),
                    new CustomParameter("priority_", 4),
                    new CustomParameter("inherittriggers_", 0),
                    new CustomParameter("exefile_", "test.ps1|test.ps1||"),
                    new CustomParameter("exeparams_", "arg1 arg2 arg3"),
                    new CustomParameter("environment_", 1),
                    new CustomParameter("usewindowsauthentication_", 1),
                    new CustomParameter("mutexname_", "testMutex"),
                    new CustomParameter("timeout_", 70),
                    new CustomParameter("writeresult_", 1),
                    new CustomParameter("intervalgroup", 0),
                    new CustomParameter("interval_", "30|30 seconds"),
                    new CustomParameter("errorintervalsdown_", 2),
                }
            );

            AddAndValidateRawParameters(parameters);
        }

        [TestMethod]
        public void AddSensor_AddsWithDictionaryParameters()
        {
            var parameters = new RawSensorParameters("raw c# sensor", "exexml")
            {
                Tags = new[] { "xmlexesensor" },
                Priority = Priority.Four,
                InheritInterval = false,
                Interval = ScanningInterval.ThirtySeconds,
                IntervalErrorMode = IntervalErrorMode.TwoWarningsThenDown,
                InheritTriggers = false,
                ["exefile_"] = "test.ps1|test.ps1||",
                ["exeparams_"] = "arg1 arg2 arg3",
                ["environment_"] = 1,
                ["usewindowsauthentication_"] = 1,
                ["mutexname_"] = "testMutex",
                ["timeout_"] = 70,
                ["writeresult_"] = 1
            };

            AddAndValidateRawParameters(parameters);
        }

        [TestMethod]
        public void AddSensor_AddsWithTypedRawParameters()
        {
            var parameters = new ExeXmlRawSensorParameters("raw c# sensor", "exexml", "test.ps1")
            {
                Priority = Priority.Four,
                ExeParameters = "arg1 arg2 arg3",
                SetExeEnvironmentVariables = true,
                UseWindowsAuthentication = true,
                Mutex = "testMutex",
                Timeout = 70,
                DebugMode = DebugMode.WriteToDisk,
                InheritInterval = false,
                Interval = ScanningInterval.ThirtySeconds,
                IntervalErrorMode = IntervalErrorMode.TwoWarningsThenDown,
                InheritTriggers = false
            };

            AddAndValidateRawParameters(parameters);
        }

        private void AddAndValidateRawParameters(RawSensorParameters parameters)
        {
            var sensors = client.GetSensors();

            client.AddSensor(Settings.Device, parameters);

            var newSensors = client.GetSensors();

            AssertEx.IsTrue(newSensors.Count > sensors.Count, "New sensor was not added properly");

            var newSensor = newSensors.Where(s => s.Name == "raw c# sensor").ToList();

            AssertEx.AreEqual(1, newSensor.Count, "A copy of the new sensor already exists");

            try
            {
                var properties = client.GetSensorProperties(newSensor.First().Id);

                AssertEx.AreEqual(properties.Name, "raw c# sensor", "Name was not correct");
                AssertEx.AreEqual(properties.Tags.First(), new[] {"xmlexesensor"}.First(), "Tags was not correct");
                AssertEx.AreEqual(properties.Priority, Priority.Four, "Priority was not correct");
                AssertEx.AreEqual(properties.ExeFile, "test.ps1", "ExeFile was not correct");
                AssertEx.AreEqual(properties.ExeParameters, "arg1 arg2 arg3", "ExeParameters was not correct");
                AssertEx.AreEqual(properties.SetExeEnvironmentVariables, true, "SetExeEnvironmentVariables was not correct");
                AssertEx.AreEqual(properties.UseWindowsAuthentication, true, "UseWindowsAuthentication was not correct");
                AssertEx.AreEqual(properties.Mutex, "testMutex", "Mutex was not correct");
                AssertEx.AreEqual(properties.Timeout, 70, "Timeout was not correct");
                AssertEx.AreEqual(properties.DebugMode, DebugMode.WriteToDisk, "DebugMode was not correct");
                AssertEx.AreEqual(properties.InheritInterval, false, "InheritInterval was not correct");
                AssertEx.AreEqual(properties.Interval.TimeSpan, new TimeSpan(0, 0, 30), "Interval was not correct");
                AssertEx.AreEqual(properties.IntervalErrorMode, IntervalErrorMode.TwoWarningsThenDown, "IntervalErrorMode was not correct");
                AssertEx.AreEqual(newSensor.First().NotificationTypes.InheritTriggers, false, "InheritTriggers was not correct");
            }
            finally
            {
                client.RemoveObject(newSensor.First().Id);
            }
        }

        [TestMethod]
        public void AddSensor_Throws_AddingToASensor()
        {
            AddToInvalidObject(Settings.UpSensor);
        }

        [TestMethod]
        public void AddSensor_Throws_AddingToAGroup()
        {
            AddToInvalidObject(Settings.Group);
        }

        [TestMethod]
        public void AddSensor_Throws_AddingToAProbe()
        {
            AddToInvalidObject(Settings.Probe);
        }

        [TestMethod]
        public void AddSensor_Throws_AddingToANonExistentObject()
        {
            var parameters = new ExeXmlSensorParameters("test.ps1");

            AssertEx.Throws<PrtgRequestException>(
                () => client.AddSensor(9995, parameters),
                "The parent object (i.e. device/group) for your newly created sensor doesn't exist anymore"
            );
        }

        private void AddToInvalidObject(int objectId)
        {
            var parameters = new ExeXmlSensorParameters("test.ps1");

            AssertEx.Throws<PrtgRequestException>(
                () => client.AddSensor(objectId, parameters),
                "The desired object cannot be created here"
            );
        }

        [TestMethod]
        public void AddSensor_ResolvesSingle()
        {
            var name = "resolvesSingle";

            var parameters = new ExeXmlSensorParameters("test.ps1", name);

            var sensor = client.AddSensor(Settings.Device, parameters);

            try
            {
                var manualSensor = client.GetSensors(Property.Name, name).Single();

                AssertEx.AreEqual(manualSensor.Id, sensor.Single().Id, "Resolved sensor ID was incorrect");
            }
            finally
            {
                client.RemoveObject(sensor.Select(s => s.Id).ToArray());
            }
        }

        [TestMethod]
        public void AddSensor_ResolvesMultiple()
        {
            var services = client.Targets.GetWmiServices(Settings.Device).Where(s => s.Name.Contains("PRTG")).ToList();

            var parameters = new WmiServiceSensorParameters(services);

            var sensors = client.AddSensor(Settings.Device, parameters).OrderBy(s => s.Id).ToList();

            try
            {
                var manualSensors = client.GetSensors(
                    new SearchFilter(Property.ParentId, Settings.Device),
                    new SearchFilter(Property.Name, FilterOperator.Contains, "PRTG")
                    ).OrderBy(s => s.Id).ToList();

                AssertEx.AreEqual(2, sensors.Count, "Did not create two sensors");
                AssertEx.AreEqual(2, manualSensors.Count, "Manual search did not find two sensors");

                AssertEx.AreEqual(sensors[0].Id, manualSensors[0].Id, "First sensor did not match");
                AssertEx.AreEqual(sensors[1].Id, manualSensors[1].Id, "Second sensor did not match");
            }
            finally
            {
                client.RemoveObject(sensors.Select(s => s.Id).ToArray());
            }
        }

        [TestMethod]
        public void AddSensor_DoesntResolve()
        {
            var name = "doesntResolveSingle";

            var parameters = new ExeXmlSensorParameters("test.ps1", name);

            var sensor = client.AddSensor(Settings.Device, parameters, false);

            AssertEx.AreEqual(null, sensor, "AddSensor did not return null");

            var manualSensor = client.GetSensors(Property.Name, name);

            try
            {
                AssertEx.AreEqual(1, manualSensor.Count, "Found more than 1 sensor with the specified name");
            }
            finally
            {
                client.RemoveObject(manualSensor.Select(s => s.Id).ToArray());
            }
        }

        [TestMethod]
        public async Task AddSensor_ResolvesAsync()
        {
            var name = "resolvesSingle";

            var parameters = new ExeXmlSensorParameters("test.ps1", name);

            var sensor = await client.AddSensorAsync(Settings.Device, parameters);

            try
            {
                var manualSensor = (await client.GetSensorsAsync(Property.Name, name)).Single();

                AssertEx.AreEqual(manualSensor.Id, sensor.Single().Id, "Resolved sensor ID was incorrect");
            }
            finally
            {
                await client.RemoveObjectAsync(sensor.Select(s => s.Id).ToArray());
            }
        }

        [TestMethod]
        public async Task AddSensor_ResolvesMultipleAsync()
        {
            var services = (await client.Targets.GetWmiServicesAsync(Settings.Device)).Where(s => s.Name.Contains("PRTG")).ToList();

            var parameters = new WmiServiceSensorParameters(services);

            var sensors = (await client.AddSensorAsync(Settings.Device, parameters)).OrderBy(s => s.Id).ToList();

            try
            {
                var manualSensors = (await client.GetSensorsAsync(
                    new SearchFilter(Property.ParentId, Settings.Device),
                    new SearchFilter(Property.Name, FilterOperator.Contains, "PRTG")
                )).OrderBy(s => s.Id).ToList();

                AssertEx.AreEqual(2, sensors.Count, "Did not create two sensors");
                AssertEx.AreEqual(2, manualSensors.Count, "Manual search did not find two sensors");

                AssertEx.AreEqual(sensors[0].Id, manualSensors[0].Id, "First sensor did not match");
                AssertEx.AreEqual(sensors[1].Id, manualSensors[1].Id, "Second sensor did not match");
            }
            finally
            {
                await client.RemoveObjectAsync(sensors.Select(s => s.Id).ToArray());
            }
        }

        [TestMethod]
        public async Task AddSensor_DoesntResolveAsync()
        {
            var name = "doesntResolveSingle";

            var parameters = new ExeXmlSensorParameters("test.ps1", name);

            var sensor = await client.AddSensorAsync(Settings.Device, parameters, false);

            AssertEx.AreEqual(null, sensor, "AddSensor did not return null");

            var manualSensor = await client.GetSensorsAsync(Property.Name, name);

            try
            {
                AssertEx.AreEqual(1, manualSensor.Count, "Found more than 1 sensor with the specified name");
            }
            finally
            {
                await client.RemoveObjectAsync(manualSensor.Select(s => s.Id).ToArray());
            }
        }

        #endregion
        #region AddDevice
            #region Synchronous

        [TestMethod]
        public void AddDevice_AddsWithLightParameters()
        {
            var host = "exch-1";

            AddsWithLightParameters(
                (p, n, r) => client.AddDevice(p, n, host, resolve: r),
                client.GetDevices,
                device => AssertEx.AreEqual(device.Host, host, "Host was not correct")
            );
        }

        [TestMethod]
        public void AddDevice_AddsWithRealParameters()
        {
            AddsWithRealParameters(
                new NewDeviceParameters("realParameters", "exch-2") { AutoDiscoverySchedule = AutoDiscoverySchedule.Hourly },
                client.AddDevice,
                client.GetDevices,
                (device, parameters) =>
                {
                    AssertEx.AreEqual(device.Host, parameters.Host, "Host was not correct");

                    var properties = client.GetDeviceProperties(device.Id);

                    AssertEx.AreEqual(properties.AutoDiscoverySchedule, parameters.AutoDiscoverySchedule, "AutoDiscoverySchedule was not correct");
                }
            );
        }

        [TestMethod]
        public void AddDevice_Resolves()
        {
            Resolves(
                n => new NewDeviceParameters(n, "exch-3"),
                client.AddDevice,
                (o, n, r) => client.AddDevice(o, n, "sql-2", resolve: r),
                client.GetDevices,
                true
            );
        }

        [TestMethod]
        public void AddDevice_DoesntResolve()
        {
            Resolves(
                n => new NewDeviceParameters(n, "exch-3"),
                client.AddDevice,
                (o, n, r) => client.AddDevice(o, n, "sql-2", resolve: r),
                client.GetDevices,
                false
            );
        }

            #endregion
            #region Asynchronous

        [TestMethod]
        public async Task AddDevice_AddsWithLightParametersAsync()
        {
            await AddsWithLightParametersAsync(
                async (p,n,r) => await client.AddDeviceAsync(p, n, "exc-1", resolve: r),
                client.GetDevicesAsync,
                null
            );
        }

        [TestMethod]
        public async Task AddDevice_AddsWithRealParametersAsync()
        {
            await AddsWithRealParametersAsync(
                new NewDeviceParameters("realParameters", "exch-2") { AutoDiscoverySchedule = AutoDiscoverySchedule.Hourly },
                client.AddDeviceAsync,
                client.GetDevicesAsync,
                async (device, parameters) =>
                {
                    AssertEx.AreEqual(device.Host, parameters.Host, "Host was not correct");

                    var properties = await client.GetDevicePropertiesAsync(device.Id);

                    AssertEx.AreEqual(properties.AutoDiscoverySchedule, parameters.AutoDiscoverySchedule, "AutoDiscoverySchedule was not correct");
                }
            );
        }

        [TestMethod]
        public async Task AddDevice_ResolvesAsync()
        {
            await ResolvesAsync(
                n => new NewDeviceParameters(n, "exch-3"),
                client.AddDeviceAsync,
                (p, n, r) => client.AddDeviceAsync(p, n, "sql-2", resolve: r),
                client.GetDevicesAsync,
                true
            );
        }

        [TestMethod]
        public async Task AddDevice_DoesntResolveAsync()
        {
            await ResolvesAsync(
                n => new NewDeviceParameters(n, "exch-3"),
                client.AddDeviceAsync,
                (p, n, r) => client.AddDeviceAsync(p, n, "sql-2", resolve: r),
                client.GetDevicesAsync,
                false
            );
        }

            #endregion
        #endregion
        #region AddGroup
            #region Synchronous

        [TestMethod]
        public void AddGroup_AddsWithLightParameters()
        {
            AddsWithLightParameters(
                client.AddGroup,
                client.GetGroups,
                null
            );
        }

        [TestMethod]
        public void AddGroup_AddsWithRealParameters()
        {
            AddsWithRealParameters(new NewGroupParameters("realParameters"), client.AddGroup, client.GetGroups, null);
        }

        [TestMethod]
        public void AddGroup_Resolves()
        {
            Resolves(
                n => new NewGroupParameters(n),
                client.AddGroup,
                client.AddGroup,
                client.GetGroups,
                true
            );
        }

        [TestMethod]
        public void AddGroup_DoesntResolve()
        {
            Resolves(
                n => new NewGroupParameters(n),
                client.AddGroup,
                client.AddGroup,
                client.GetGroups,
                false
            );
        }

            #endregion
            #region Asynchronous

        [TestMethod]
        public async Task AddGroup_AddsWithLightParametersAsync()
        {
            await AddsWithLightParametersAsync(
                client.AddGroupAsync,
                client.GetGroupsAsync,
                null
            );
        }

        [TestMethod]
        public async Task AddGroup_AddsWithRealParametersAsync()
        {
            await AddsWithRealParametersAsync(new NewGroupParameters("realParameters"), client.AddGroupAsync, client.GetGroupsAsync, null);
        }

        [TestMethod]
        public async Task AddGroup_ResolvesAsync()
        {
            await ResolvesAsync(
                n => new NewGroupParameters(n),
                client.AddGroupAsync,
                client.AddGroupAsync,
                client.GetGroupsAsync,
                true
            );
        }

        [TestMethod]
        public async Task AddGroup_DoesntResolveAsync()
        {
            await ResolvesAsync(
                n => new NewGroupParameters(n),
                client.AddGroupAsync,
                client.AddGroupAsync,
                client.GetGroupsAsync,
                false
            );
        }

            #endregion
        #endregion
        #region Synchronous Helpers

        private void AddsWithLightParameters<TObject>(
            Func<int, string, bool, TObject> addObject,
            Func<Property, object, List<TObject>> getObjects,
            Action<TObject> validateAdditional) where TObject : SensorOrDeviceOrGroupOrProbe
        {
            var name = "lightParameters";

            addObject(Settings.Probe, name, true);

            var obj = getObjects(Property.Name, name).Single();

            try
            {
                AssertEx.AreEqual(obj.Name, name, "Name was not correct");
                AssertEx.AreEqual(obj.ParentId, Settings.Probe, "ParentId was not correct");

                validateAdditional?.Invoke(obj);
            }
            finally
            {
                client.RemoveObject(obj.Id);
            }
        }

        private void AddsWithRealParameters<TObject, TParams>(
            TParams parameters,
            Func<int, TParams, bool, TObject> addObject,
            Func<Property, object, List<TObject>> getObjects,
            Action<TObject, TParams> validateAdditional) where TParams : NewObjectParameters where TObject : SensorOrDeviceOrGroupOrProbe
        {
            addObject(Settings.Probe, parameters, true);

            var obj = getObjects(Property.Name, parameters.Name).Single();

            try
            {
                AssertEx.AreEqual(obj.Name, parameters.Name, "Name was not correct");
                AssertEx.AreEqual(obj.ParentId, Settings.Probe, "ParentId was not correct");

                validateAdditional?.Invoke(obj, parameters);
            }
            finally
            {
                client.RemoveObject(obj.Id);
            }
        }

        private void Resolves<TObject, TParams>(
            Func<string, TParams> createParams,
            Func<int, TParams, bool, TObject> addObjectParams,
            Func<int, string, bool, TObject> addObjectLight,
            Func<Property, object, List<TObject>> getObjects,
            bool resolve) where TObject : PrtgObject
        {
            var parametersName = resolve ? "resolveRealParameters" : "doesntResolveRealParameters";
            var parameters = createParams(parametersName);
            var paramsResolvedObj = addObjectParams(Settings.Group, parameters, resolve);
            var paramsManualObj = getObjects(Property.Name, parametersName).Single();

            var lightName = resolve ? "resolveLightParameters" : "doesntResolveLightParameters";
            var lightResolvedObj = addObjectLight(Settings.Group, lightName, resolve);
            var lightManualObj = getObjects(Property.Name, lightName).Single();

            try
            {
                if (resolve)
                {
                    AssertEx.AreEqual(paramsManualObj.Id, paramsResolvedObj.Id, "Resolved object ID from parameters was not correct");
                    AssertEx.AreEqual(lightManualObj.Id, lightResolvedObj.Id, "Resolved object ID from light parameters was not correct");
                }
                else
                {
                    AssertEx.AreEqual(null, paramsResolvedObj, "AddObject with parameters did not return null");
                    AssertEx.AreEqual(null, lightResolvedObj, "AddObject with light parameters did not return null");
                }
            }
            finally
            {
                client.RemoveObject(paramsManualObj.Id, lightManualObj.Id);
            }
        }

        #endregion
        #region Asynchronous Helpers

        private async Task AddsWithLightParametersAsync<TObject>(
            Func<int, string, bool, Task<TObject>> addObject,
            Func<Property, object, Task<List<TObject>>> getObjects,
            Func<TObject, Task> validateAdditional) where TObject : SensorOrDeviceOrGroupOrProbe
        {
            var name = "lightParameters";

            await addObject(Settings.Probe, name, true);

            var obj = (await getObjects(Property.Name, name)).Single();

            try
            {
                AssertEx.AreEqual(obj.Name, name, "Name was not correct");
                AssertEx.AreEqual(obj.ParentId, Settings.Probe, "ParentId was not correct");

                validateAdditional?.Invoke(obj);
            }
            finally
            {
                await client.RemoveObjectAsync(obj.Id);
            }
        }

        private async Task AddsWithRealParametersAsync<TObject, TParams>(
            TParams parameters,
            Func<int, TParams, bool, Task<TObject>> addObject,
            Func<Property, object, Task<List<TObject>>> getObjects,
            Func<TObject, TParams, Task> validateAdditional) where TParams : NewObjectParameters where TObject : SensorOrDeviceOrGroupOrProbe
        {
            await addObject(Settings.Probe, parameters, true);

            var obj = (await getObjects(Property.Name, parameters.Name)).Single();

            try
            {
                AssertEx.AreEqual(obj.Name, parameters.Name, "Name was not correct");
                AssertEx.AreEqual(obj.ParentId, Settings.Probe, "ParentId was not correct");

                validateAdditional?.Invoke(obj, parameters);
            }
            finally
            {
                await client.RemoveObjectAsync(obj.Id);
            }
        }

        private async Task ResolvesAsync<TObject, TParams>(
            Func<string, TParams> createParams,
            Func<int, TParams, bool, Task<TObject>> addObjectParams,
            Func<int, string, bool, Task<TObject>> addObjectLight,
            Func<Property, object, Task<List<TObject>>> getObjects,
            bool resolve) where TObject : PrtgObject
        {
            var parametersName = resolve ? "resolveRealParameters" : "doesntResolveRealParameters";
            var parameters = createParams(parametersName);
            var paramsResolvedObj = await addObjectParams(Settings.Group, parameters, resolve);
            var paramsManualObj = (await getObjects(Property.Name, parametersName)).Single();

            var lightName = resolve ? "resolveLightParameters" : "doesntResolveLightParameters";
            var lightResolvedObj = await addObjectLight(Settings.Group, lightName, resolve);
            var lightManualObj = (await getObjects(Property.Name, lightName)).Single();

            try
            {
                if (resolve)
                {
                    AssertEx.AreEqual(paramsManualObj.Id, paramsResolvedObj.Id, "Resolved object ID from parameters was not correct");
                    AssertEx.AreEqual(lightManualObj.Id, lightResolvedObj.Id, "Resolved object ID from light parameters was not correct");
                }
                else
                {
                    AssertEx.AreEqual(null, paramsResolvedObj, "AddObject with parameters did not return null");
                    AssertEx.AreEqual(null, lightResolvedObj, "AddObject with light parameters did not return null");
                }
            }
            finally
            {
                await client.RemoveObjectAsync(paramsManualObj.Id, lightManualObj.Id);
            }
        }

        #endregion
    }
}
