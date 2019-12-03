using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        [IntegrationTest]
        public void Action_AddSensor_AddsWithRawParameters()
        {
            var parameters = new RawSensorParameters("raw c# sensor", "exexml")
            {
                Priority = Priority.Four,
                InheritTriggers = false,
                InheritInterval = false,
                Interval = ScanningInterval.ThirtySeconds,
                IntervalErrorMode = IntervalErrorMode.TwoWarningsThenDown
            };
            parameters.Parameters.AddRange(
                new List<CustomParameter>
                {
                    new CustomParameter("tags_", "xmlexesensor"),
                    new CustomParameter("exefile_", "test.ps1|test.ps1||"),
                    new CustomParameter("exeparams_", "arg1 arg2 arg3"),
                    new CustomParameter("environment_", 1),
                    new CustomParameter("usewindowsauthentication_", 1),
                    new CustomParameter("mutexname_", "testMutex"),
                    new CustomParameter("timeout_", 70),
                    new CustomParameter("writeresult_", 1),
                }
            );

            AddAndValidateRawParameters(parameters);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddSensor_AddsWithDictionaryParameters()
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
        [IntegrationTest]
        public void Action_AddSensor_AddsWithTypedRawParameters()
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
        [IntegrationTest]
        public void Action_AddSensor_Throws_AddingToASensor()
        {
            AddToInvalidObject(Settings.UpSensor);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddSensor_Throws_AddingToAGroup()
        {
            AddToInvalidObject(Settings.Group);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddSensor_Throws_AddingToAProbe()
        {
            AddToInvalidObject(Settings.Probe);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddSensor_Throws_AddingToANonExistentObject()
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
        [IntegrationTest]
        public void Action_AddSensor_ResolvesSingle()
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
        [IntegrationTest]
        public void Action_AddSensor_ResolvesMultiple()
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
        [IntegrationTest]
        public void Action_AddSensor_DoesntResolve()
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
        [IntegrationTest]
        public async Task Action_AddSensor_ResolvesAsync()
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
        [IntegrationTest]
        public async Task Action_AddSensor_ResolvesMultipleAsync()
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
        [IntegrationTest]
        public async Task Action_AddSensor_DoesntResolveAsync()
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

        [TestMethod]
        [IntegrationTest]
        public void Action_AddSensor_AsReadOnlyUser_NoQueryTarget_Throws()
        {
            var parameters = new ExeXmlSensorParameters("test.ps1");

            AssertEx.Throws<PrtgRequestException>(
                () => readOnlyClient.AddSensor(Settings.Device, parameters),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddSensor_AsReadOnlyUser_WithQueryTarget_Throws()
        {
            var parameters = new RawSensorParameters("test", "snmplibrary");

            AssertEx.Throws<PrtgRequestException>(
                () => readOnlyClient.AddSensor(Settings.Device, parameters),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddSensor_AsReadOnlyUser_WithQueryTargetParameters_Throws()
        {
            var parameters = new RawSensorParameters("test", "oracletablespace");

            AssertEx.Throws<PrtgRequestException>(
                () => readOnlyClient.AddSensor(Settings.Device, parameters),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_AddSensor_AsReadOnlyUser_NoQueryTarget_ThrowsAsync()
        {
            var parameters = new ExeXmlSensorParameters("test.ps1");

            await AssertEx.ThrowsAsync<PrtgRequestException>(
                async () => await readOnlyClient.AddSensorAsync(Settings.Device, parameters),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_AddSensor_AsReadOnlyUser_WithQueryTarget_ThrowsAsync()
        {
            var parameters = new RawSensorParameters("test", "snmplibrary");

            await AssertEx.ThrowsAsync<PrtgRequestException>(
                async () => await readOnlyClient.AddSensorAsync(Settings.Device, parameters),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_AddSensor_AsReadOnlyUser_WithQueryTargetParameters_ThrowsAsync()
        {
            var parameters = new RawSensorParameters("test", "oracletablespace");

            await AssertEx.ThrowsAsync<PrtgRequestException>(
                async () => await readOnlyClient.AddSensorAsync(Settings.Device, parameters),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddSensor_SensorQueryTarget_IgnoresTarget()
        {
            var parameters = new RawSensorParameters("test", "snmplibrary")
            {
                ["library_"] = "C:\\Program Files (x86)\\PRTG Network Monitor\\snmplibs\\Basic Linux Library (UCD-SNMP-MIB).oidlib",
                ["interfacenumber_"] = 1,
                ["interfacenumber__check"] = "1.3.6.1.4.1.2021.2.1.100.1|Basic Linux Library (UCD-SNMP-MIB)|Processes: 1|Processes Error Flag|#|0|0|Processes Error Flag|2|1|0|1|A Error flag to indicate trouble with a process. It goes to 1 if there is an error, 0 if no error.|0|0|0|0||1.3.6.1.4.1.2021.2.1.100|prErrorFlag|1.3.6.1.4.1.2021.2||ASN_INTEGER|0|ASN_INTEGER||Basic Linux Library (UCD-SNMP-MIB)|Processes: #[1.3.6.1.4.1.2021.2.1.1]|100|||||||||||||||||||||||||||||||||||",
                DynamicType = true
            };

            Sensor sensor = null;

            try
            {
                sensor = client.AddSensor(Settings.Device, parameters).First();

                AssertEx.AreEqual("enterprises /", sensor.Name, "Sensor name was not correct");
            }
            finally
            {
                if (sensor != null)
                    client.RemoveObject(sensor.Id);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddSensor_SensorQueryParameters_SynthesizesParameters()
        {
            var parameters = new RawSensorParameters("test", "oracletablespace")
            {
                ["database_"] = "XE",
                ["sid_type_"] = 0,
                ["prefix_"] = 0,
                ["tablespace__check"] = "SYSAUX|SYSAUX|",
                ["tablespace_"] = 1
            };

            Sensor sensor = null;

            try
            {
                sensor = client.AddSensor(Settings.Device, parameters).First();

                AssertEx.AreEqual("SYSAUX", sensor.Name, "Sensor name was not correct");
            }
            finally
            {
                if (sensor != null)
                    client.RemoveObject(sensor.Id);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_AddSensor_SensorQueryTarget_IgnoresTargetAsync()
        {
            var parameters = new RawSensorParameters("test", "snmplibrary")
            {
                ["library_"] = "C:\\Program Files (x86)\\PRTG Network Monitor\\snmplibs\\Basic Linux Library (UCD-SNMP-MIB).oidlib",
                ["interfacenumber_"] = 1,
                ["interfacenumber__check"] = "1.3.6.1.4.1.2021.2.1.100.1|Basic Linux Library (UCD-SNMP-MIB)|Processes: 1|Processes Error Flag|#|0|0|Processes Error Flag|2|1|0|1|A Error flag to indicate trouble with a process. It goes to 1 if there is an error, 0 if no error.|0|0|0|0||1.3.6.1.4.1.2021.2.1.100|prErrorFlag|1.3.6.1.4.1.2021.2||ASN_INTEGER|0|ASN_INTEGER||Basic Linux Library (UCD-SNMP-MIB)|Processes: #[1.3.6.1.4.1.2021.2.1.1]|100|||||||||||||||||||||||||||||||||||",
                DynamicType = true
            };

            Sensor sensor = null;

            try
            {
                sensor = (await client.AddSensorAsync(Settings.Device, parameters)).First();

                AssertEx.AreEqual("enterprises /", sensor.Name, "Sensor name was not correct");
            }
            finally
            {
                if (sensor != null)
                    await client.RemoveObjectAsync(sensor.Id);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_AddSensor_SensorQueryParameters_SynthesizesParametersAsync()
        {
            var parameters = new RawSensorParameters("test", "oracletablespace")
            {
                ["database_"] = "XE",
                ["sid_type_"] = 0,
                ["prefix_"] = 0,
                ["tablespace__check"] = "SYSAUX|SYSAUX|",
                ["tablespace_"] = 1
            };

            Sensor sensor = null;

            try
            {
                sensor = (await client.AddSensorAsync(Settings.Device, parameters)).First();

                AssertEx.AreEqual("SYSAUX", sensor.Name, "Sensor name was not correct");
            }
            finally
            {
                if (sensor != null)
                    await client.RemoveObjectAsync(sensor.Id);
            }
        }

        #endregion
        #region AddDevice
            #region Synchronous

        [TestMethod]
        [IntegrationTest]
        public void Action_AddDevice_AddsWithLightParameters()
        {
            var host = "exch-1";

            AddsWithLightParameters<Device, GroupOrProbe>(
                (p, n, r, t) => client.AddDevice(p, n, host, resolve: r, token: t),
                client.GetDevices,
                device => AssertEx.AreEqual(device.Host, host, "Host was not correct")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddDevice_AddsWithRealParameters()
        {
            AddsWithRealParameters<Device, NewDeviceParameters, GroupOrProbe>(
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
        [IntegrationTest]
        public void Action_AddDevice_Resolves()
        {
            Resolves<Device, NewDeviceParameters, GroupOrProbe>(
                n => new NewDeviceParameters(n, "exch-3"),
                client.AddDevice,
                (o, n, r, t) => client.AddDevice(o, n, "sql-2", resolve: r, token: t),
                client.GetDevices,
                true
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddDevice_DoesntResolve()
        {
            Resolves<Device, NewDeviceParameters, GroupOrProbe>(
                n => new NewDeviceParameters(n, "exch-3"),
                client.AddDevice,
                (o, n, r, t) => client.AddDevice(o, n, "sql-2", resolve: r, token: t),
                client.GetDevices,
                false
            );
        }

            #endregion
            #region Asynchronous

        [TestMethod]
        [IntegrationTest]
        public async Task Action_AddDevice_AddsWithLightParametersAsync()
        {
            await AddsWithLightParametersAsync<Device, GroupOrProbe>(
                async (p, n, r, t) => await client.AddDeviceAsync(p, n, "exc-1", resolve: r, token: t),
                client.GetDevicesAsync,
                null
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_AddDevice_AddsWithRealParametersAsync()
        {
            await AddsWithRealParametersAsync<Device, NewDeviceParameters, GroupOrProbe>(
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
        [IntegrationTest]
        public async Task Action_AddDevice_ResolvesAsync()
        {
            await ResolvesAsync<Device, NewDeviceParameters, GroupOrProbe>(
                n => new NewDeviceParameters(n, "exch-3"),
                client.AddDeviceAsync,
                (p, n, r, t) => client.AddDeviceAsync(p, n, "sql-2", resolve: r, token: t),
                client.GetDevicesAsync,
                true
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_AddDevice_DoesntResolveAsync()
        {
            await ResolvesAsync<Device, NewDeviceParameters, GroupOrProbe>(
                n => new NewDeviceParameters(n, "exch-3"),
                client.AddDeviceAsync,
                (p, n, r, t) => client.AddDeviceAsync(p, n, "sql-2", resolve: r, token: t),
                client.GetDevicesAsync,
                false
            );
        }

            #endregion
        #endregion
        #region AddGroup
            #region Synchronous

        [TestMethod]
        [IntegrationTest]
        public void Action_AddGroup_AddsWithLightParameters()
        {
            AddsWithLightParameters<Group, GroupOrProbe>(
                client.AddGroup,
                client.GetGroups,
                null
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddGroup_AddsWithRealParameters()
        {
            AddsWithRealParameters<Group, NewGroupParameters, GroupOrProbe>(new NewGroupParameters("realParameters"), client.AddGroup, client.GetGroups, null);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddGroup_Resolves()
        {
            Resolves<Group, NewGroupParameters, GroupOrProbe>(
                n => new NewGroupParameters(n),
                client.AddGroup,
                client.AddGroup,
                client.GetGroups,
                true
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_AddGroup_DoesntResolve()
        {
            Resolves<Group, NewGroupParameters, GroupOrProbe>(
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
        [IntegrationTest]
        public async Task Action_AddGroup_AddsWithLightParametersAsync()
        {
            await AddsWithLightParametersAsync<Group, GroupOrProbe>(
                client.AddGroupAsync,
                client.GetGroupsAsync,
                null
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_AddGroup_AddsWithRealParametersAsync()
        {
            await AddsWithRealParametersAsync<Group, NewGroupParameters, GroupOrProbe>(new NewGroupParameters("realParameters"), client.AddGroupAsync, client.GetGroupsAsync, null);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_AddGroup_ResolvesAsync()
        {
            await ResolvesAsync<Group, NewGroupParameters, GroupOrProbe>(
                n => new NewGroupParameters(n),
                client.AddGroupAsync,
                client.AddGroupAsync,
                client.GetGroupsAsync,
                true
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_AddGroup_DoesntResolveAsync()
        {
            await ResolvesAsync<Group, NewGroupParameters, GroupOrProbe>(
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

        private void AddsWithLightParameters<TObject, TParent>(
            Func<Either<TParent, int>, string, bool, CancellationToken, TObject> addObject,
            Func<Property, object, List<TObject>> getObjects,
            Action<TObject> validateAdditional) where TObject : SensorOrDeviceOrGroupOrProbe
        {
            var name = "lightParameters";

            addObject(Settings.Probe, name, true, CancellationToken.None);

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

        private void AddsWithRealParameters<TObject, TParams, TParent>(
            TParams parameters,
            Func<Either<TParent, int>, TParams, bool, CancellationToken, TObject> addObject,
            Func<Property, object, List<TObject>> getObjects,
            Action<TObject, TParams> validateAdditional) where TParams : NewObjectParameters where TObject : SensorOrDeviceOrGroupOrProbe
        {
            addObject(Settings.Probe, parameters, true, CancellationToken.None);

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

        private void Resolves<TObject, TParams, TParent>(
            Func<string, TParams> createParams,
            Func<Either<TParent, int>, TParams, bool, CancellationToken, TObject> addObjectParams,
            Func<Either<TParent, int>, string, bool, CancellationToken, TObject> addObjectLight,
            Func<Property, object, List<TObject>> getObjects,
            bool resolve) where TObject : PrtgObject
        {
            var parametersName = resolve ? "resolveRealParameters" : "doesntResolveRealParameters";
            var parameters = createParams(parametersName);
            var paramsResolvedObj = addObjectParams(Settings.Group, parameters, resolve, CancellationToken.None);
            var paramsManualObj = getObjects(Property.Name, parametersName).Single();

            var lightName = resolve ? "resolveLightParameters" : "doesntResolveLightParameters";
            var lightResolvedObj = addObjectLight(Settings.Group, lightName, resolve, CancellationToken.None);
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

        private async Task AddsWithLightParametersAsync<TObject, TParent>(
            Func<Either<TParent, int>, string, bool, CancellationToken, Task<TObject>> addObject,
            Func<Property, object, Task<List<TObject>>> getObjects,
            Func<TObject, Task> validateAdditional) where TObject : SensorOrDeviceOrGroupOrProbe
        {
            var name = "lightParameters";

            await addObject(Settings.Probe, name, true, CancellationToken.None);

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

        private async Task AddsWithRealParametersAsync<TObject, TParams, TParent>(
            TParams parameters,
            Func<Either<TParent, int>, TParams, bool, CancellationToken, Task<TObject>> addObject,
            Func<Property, object, Task<List<TObject>>> getObjects,
            Func<TObject, TParams, Task> validateAdditional) where TParams : NewObjectParameters where TObject : SensorOrDeviceOrGroupOrProbe
        {
            await addObject(Settings.Probe, parameters, true, CancellationToken.None);

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

        private async Task ResolvesAsync<TObject, TParams, TParent>(
            Func<string, TParams> createParams,
            Func<Either<TParent, int>, TParams, bool, CancellationToken, Task<TObject>> addObjectParams,
            Func<Either<TParent, int>, string, bool, CancellationToken, Task<TObject>> addObjectLight,
            Func<Property, object, Task<List<TObject>>> getObjects,
            bool resolve) where TObject : PrtgObject
        {
            var parametersName = resolve ? "resolveRealParameters" : "doesntResolveRealParameters";
            var parameters = createParams(parametersName);
            var paramsResolvedObj = await addObjectParams(Settings.Group, parameters, resolve, CancellationToken.None);
            var paramsManualObj = (await getObjects(Property.Name, parametersName)).Single();

            var lightName = resolve ? "resolveLightParameters" : "doesntResolveLightParameters";
            var lightResolvedObj = await addObjectLight(Settings.Group, lightName, resolve, CancellationToken.None);
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
