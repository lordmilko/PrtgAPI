using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Targets;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Infrastructure;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class DynamicSensorParameterTests : BaseTest
    {
        const string exefile = "exefile";

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_CanExecute()
        {
            var parameters = client.GetDynamicSensorParameters(1001, "exexml");

            Assert.AreEqual("exexml", parameters.SensorType);
            Assert.AreEqual("XML Custom EXE/Script Sensor", parameters.Name);
            Assert.AreEqual("Demo Batchfile - Returns static values in four channels.bat", parameters["exefile"].ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DynamicSensorParameters_CanExecuteAsync()
        {
            var parameters = await client.GetDynamicSensorParametersAsync(1001, "exexml");

            Assert.AreEqual("exexml", parameters.SensorType);
            Assert.AreEqual("XML Custom EXE/Script Sensor", parameters.Name);
            Assert.AreEqual("Demo Batchfile - Returns static values in four channels.bat", parameters["exefile"].ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_Indexer_SetTarget_ForTarget()
        {
            var parameters = client.GetDynamicSensorParameters(1001, "exexml");

            Assert.IsTrue(parameters.Targets.ContainsKey(exefile));
            Assert.AreEqual(2, parameters.Targets[exefile].Length, "Did not have expected number of targets");

            var originalTarget = parameters[exefile];
            var newTarget = parameters.Targets[exefile].Last();

            Assert.AreNotEqual(originalTarget.ToString(), newTarget.ToString(), "Original and new target were the same");

            parameters[exefile] = newTarget;

            Assert.AreEqual(newTarget, parameters[exefile], "New target was not installed correctly");

            var url = PrtgRequestMessageTests.CreateUrl(parameters);

            var builder = new StringBuilder();

            builder.Append("name_=XML+Custom+EXE%2FScript+Sensor&");
            builder.Append("priority_=3&");
            builder.Append("inherittriggers=1&");
            builder.Append("intervalgroup=1&");
            builder.Append("interval_=60%7C60+seconds&");
            builder.Append("errorintervalsdown_=1&");
            builder.Append("tags_=xmlexesensor&");
            builder.Append("exefilelabel=&");
            builder.Append("exeparams_=&");
            builder.Append("environment_=0&");
            builder.Append("usewindowsauthentication_=0&");
            builder.Append("mutexname_=&");
            builder.Append("timeout_=60&");
            builder.Append("writeresult_=0&");
            builder.Append("exefile=testScript.bat%7CtestScript.bat%7C%7C&");
            builder.Append("sensortype=exexml");

            var str = builder.ToString();

            Assert.AreEqual(str, url, "URL was incorrect");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_Indexer_SetMultipleTargets_ForTarget()
        {
            var parameters = client.GetDynamicSensorParameters(1001, "exexml");

            Assert.IsTrue(parameters.Targets.ContainsKey(exefile));
            Assert.AreEqual(2, parameters.Targets[exefile].Length, "Did not have expected number of targets");

            parameters[exefile] = parameters.Targets[exefile];

            var targets = (GenericSensorTarget[]) parameters[exefile];

            Assert.AreEqual(2, targets.Length, "Did not retrieve expected number of targets");

            Assert.AreEqual(parameters.Targets[exefile][0], targets[0], "First target was not correct");
            Assert.AreEqual(parameters.Targets[exefile][0], targets[0], "Second target was not correct");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_Index_SetsNormal()
        {
            var parameters = client.GetDynamicSensorParameters(1001, "exexml");

            Assert.AreEqual("0", parameters["environment"]);

            parameters["environment"] = "1";

            Assert.AreEqual("1", parameters["environment"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_Index_SetsTyped()
        {
            var parameters = client.GetDynamicSensorParameters(1001, "exexml");

            var name = "XML Custom EXE/Script Sensor";

            Assert.AreEqual(name, parameters["name"]);
            Assert.AreEqual(name, parameters.Name);

            parameters["name"] = "newName";

            Assert.AreEqual("newName", parameters["name"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_Indexer_Throws_SetMultiple_ForNonTarget()
        {
            var parameters = client.GetDynamicSensorParameters(1001, "exexml");

            parameters["exeparams"] = new[] {1, 2};

            var arr = ((int[]) parameters["exeparams"]);

            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_Indexer_NewProperty_Throws_WhenLocked()
        {
            var name = "FAKE_PARAMETER";

            var parameters = client.GetDynamicSensorParameters(1001, "exexml");

            Assert.IsTrue(parameters.IsLocked(), "Parameters were not locked");

            AssertEx.Throws<InvalidOperationException>(() => { parameters[name] = "test"; }, "Parameter with name 'FAKE_PARAMETER' does not exist. To add new parameters object must first be unlocked.");
            AssertEx.Throws<InvalidOperationException>(() => { var val = parameters[name]; }, "Parameter with name 'FAKE_PARAMETER' does not exist.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_Indexer_NewProperty_Adds_WhenUnlocked()
        {
            var name = "FAKE_PARAMETER";

            var parameters = client.GetDynamicSensorParameters(1001, "exexml");
            parameters.Unlock();

            Assert.IsFalse(parameters.IsLocked(), "Parameters were locked");

            AssertEx.Throws<InvalidOperationException>(() => { var t = parameters[name]; }, "Parameter with name 'FAKE_PARAMETER' does not exist.");
            parameters[name] = "test";
            var val = parameters[name];

            Assert.AreEqual("test", val, "Value was not correct");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_AsDynamic_CanUse_DynamicProperties()
        {
            dynamic dynamic = client.GetDynamicSensorParameters(1001, "exexml");

            Assert.AreEqual("Demo Batchfile - Returns static values in four channels.bat", dynamic.exefile.ToString());
            Assert.IsInstanceOfType(dynamic.exefile, typeof (GenericSensorTarget));

            dynamic.exefile = Enumerable.Last(dynamic.Targets["exefile"]);

            Assert.AreEqual("testScript.bat", dynamic.exefile.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_AsDynamic_CanUse_NormalProperties()
        {
            dynamic dynamic = client.GetDynamicSensorParameters(1001, "exexml");

            Assert.AreEqual("exexml", dynamic.SensorType);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_NewProperty_AsLowerCase_SameAsNormalProperty()
        {
            dynamic dynamic = client.GetDynamicSensorParameters(1001, "exexml");

            dynamic.sensortype = "test";

            Assert.AreEqual("test", dynamic.sensortype);
            Assert.AreEqual("test", dynamic[Parameter.SensorType]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_AsDynamic_Throws_WhenLocked()
        {
            dynamic parameters = client.GetDynamicSensorParameters(1001, "exexml");

            Assert.IsTrue(parameters.IsLocked(), "Parameters were not locked");

            AssertEx.Throws<InvalidOperationException>(() => { parameters.FAKE_PARAMETER = "test"; }, "Parameter with name 'FAKE_PARAMETER' does not exist. To add new parameters object must first be unlocked.");
            AssertEx.Throws<InvalidOperationException>(() => { var val = parameters.FAKE_PARAMETER; }, "Parameter with name 'FAKE_PARAMETER' does not exist.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_AsDynamic_NewProperty_Adds_WhenUnlocked()
        {
            dynamic parameters = client.GetDynamicSensorParameters(1001, "exexml");
            parameters.Unlock();

            Assert.IsFalse(parameters.IsLocked(), "Parameters were locked");

            AssertEx.Throws<InvalidOperationException>(() => { var t = parameters.FAKE_PARAMETER; }, "Parameter with name 'FAKE_PARAMETER' does not exist.");
            parameters.FAKE_PARAMETER = "test";
            var val = parameters.FAKE_PARAMETER;

            Assert.AreEqual("test", val, "Value was not correct");
        }

        [TestMethod]
#if MSTEST2
        [DoNotParallelize]
#endif
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_WithoutPSObjectUtilities_SingleObject()
        {
            TestHelpers.WithPSObjectUtilities(() =>
            {
                var parameters = client.GetDynamicSensorParameters(1001, "exexml");

                var val = true;

                parameters["mutexname"] = val;
                Assert.AreEqual(val, parameters["mutexname"]);

                Assert.IsInstanceOfType(((List<CustomParameter>)(parameters.GetParameters()[Parameter.Custom])).First(p => p.Name == "mutexname_").Value, typeof(SimpleParameterContainerValue));

                var url = PrtgRequestMessageTests.CreateUrl(parameters);

                Assert.IsTrue(url.Contains("mutexname_=True"));
            }, new DefaultPSObjectUtilities());
        }

        [TestMethod]
#if MSTEST2
        [DoNotParallelize]
#endif
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_WithoutPSObjectUtilities_ObjectArray()
        {
            TestHelpers.WithPSObjectUtilities(() =>
            {
                var parameters = client.GetDynamicSensorParameters(1001, "exexml");

                var arr = new[] { 1, 2 };

                parameters["mutexname"] = arr;
                Assert.AreEqual(arr, parameters["mutexname"]);

                Assert.IsInstanceOfType(((List<CustomParameter>)(parameters.GetParameters()[Parameter.Custom])).First(p => p.Name == "mutexname_").Value, typeof(SimpleParameterContainerValue));

                var url = PrtgRequestMessageTests.CreateUrl(parameters);

                Assert.IsTrue(url.Contains("mutexname_=1&mutexname_=2"));
            }, new DefaultPSObjectUtilities());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_SensorQueryTarget_ParsesTarget()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001), //Get initial target

                UnitRequest.SensorTypes(1001), //Verify specified target
                UnitRequest.BeginAddSensorQuery(1001, "snmplibrary_nolist", "APC+UPS.oidlib"),
                UnitRequest.AddSensorProgress(1001, 2),
                UnitRequest.EndAddSensorQuery(1001, 2)
            }));

            var target = client.GetSensorTypes(1001).First(t => t.Id == "snmplibrary").QueryTargets.First();

            client.GetDynamicSensorParameters(1001, "snmplibrary", queryParameters: target);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_SensorQueryTarget_Throws_WhenTargetIsInvalid()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            AssertEx.Throws<InvalidOperationException>(
                () => client.GetDynamicSensorParameters(1001, "snmplibrary", queryParameters: (SensorQueryTarget)"test"),
                "Query target 'test' is not a valid target for sensor type 'snmplibrary' on device ID 1001. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_SensorQueryTarget_Throws_WhenTargetIsNotRequired()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            AssertEx.Throws<InvalidOperationException>(
                () => client.GetDynamicSensorParameters(1001, "ptfadsreplfailurexml", queryParameters: (SensorQueryTarget)"test"),
                "Cannot specify query target 'test' on sensor type 'ptfadsreplfailurexml': type does not support query targets."
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_SensorQueryTarget_Throws_WhenTargetMissing()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            AssertEx.Throws<InvalidOperationException>(
                () => client.GetDynamicSensorParameters(1001, "snmplibrary"),
                "Failed to process query for sensor type 'snmplibrary': a sensor query target is required, however none was specified. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_SensorQueryTarget_Throws_WhenTypeIsInvalid()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.GetDynamicSensorParameters(1001, "potato", queryParameters: (SensorQueryTarget)"test"),
                "Failed to validate query target 'test' on sensor type 'potato': sensor type 'potato' is not valid."
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_SensorQueryTarget_Throws_WhenTypeIsInvalid_NoTargetSpecified()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.GetDynamicSensorParameters(1001, "potato"),
                "Cannot process query for sensor type 'potato': sensor type 'potato' is not valid."
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_SensorQueryParameters_ParsesParameters()
        {
            var client = Initialize_Client(new SensorQueryTargetParametersValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001),
                UnitRequest.BeginAddSensorQuery(1001, "ptfadsreplfailurexml"),
                UnitRequest.ContinueAddSensorQuery(2055, 7, "database_=XE&sid_type_=0&prefix_=0"),
                UnitRequest.AddSensorProgress(1001, 7),
                UnitRequest.EndAddSensorQuery(1001, 7)
            }));

            client.GetDynamicSensorParameters(1001, "ptfadsreplfailurexml", queryParameters: new SensorQueryTargetParameters
            {
                ["database"] = "XE",
                ["sid_type"] = 0,
                ["prefix"] = 0
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicSensorParameters_SensorQueryParameters_Throws_WhenParametersAreMissing()
        {
            var client = Initialize_Client(new SensorQueryTargetParametersValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001),
                UnitRequest.BeginAddSensorQuery(1001, "ptfadsreplfailurexml")
            }));

            AssertEx.Throws<InvalidOperationException>(
                () => client.GetDynamicSensorParameters(1001, "ptfadsreplfailurexml", queryParameters: new SensorQueryTargetParameters()),
                "Failed to process request for sensor type 'oracletablespace': sensor query target parameters did not include mandatory parameters 'database_',"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DynamicSensorParameters_SensorQueryTarget_ParsesTargetAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001), //Get initial target

                UnitRequest.SensorTypes(1001), //Verify specified target
                UnitRequest.BeginAddSensorQuery(1001, "snmplibrary_nolist", "APC+UPS.oidlib"),
                UnitRequest.AddSensorProgress(1001, 2),
                UnitRequest.EndAddSensorQuery(1001, 2)
            }));

            var target = (await client.GetSensorTypesAsync(1001)).First(t => t.Id == "snmplibrary").QueryTargets.First();

            await client.GetDynamicSensorParametersAsync(1001, "snmplibrary", queryParameters: target);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DynamicSensorParameters_SensorQueryTarget_Throws_WhenTargetIsInvalidAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.GetDynamicSensorParametersAsync(1001, "snmplibrary", queryParameters: (SensorQueryTarget)"test"),
                "Query target 'test' is not a valid target for sensor type 'snmplibrary' on device ID 1001. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DynamicSensorParameters_SensorQueryTarget_Throws_WhenTargetIsNotRequiredAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.GetDynamicSensorParametersAsync(1001, "ptfadsreplfailurexml", queryParameters: (SensorQueryTarget)"test"),
                "Cannot specify query target 'test' on sensor type 'ptfadsreplfailurexml': type does not support query targets."
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DynamicSensorParameters_SensorQueryTarget_Throws_WhenTargetMissingAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.GetDynamicSensorParametersAsync(1001, "snmplibrary"),
                "Failed to process query for sensor type 'snmplibrary': a sensor query target is required, however none was specified. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DynamicSensorParameters_SensorQueryTarget_Throws_WhenTypeIsInvalidAsync()
        {
            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.GetDynamicSensorParametersAsync(1001, "potato", queryParameters: (SensorQueryTarget)"test"),
                "Failed to validate query target 'test' on sensor type 'potato': sensor type 'potato' is not valid."
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DynamicSensorParameters_SensorQueryTarget_Throws_WhenTypeIsInvalid_NoTargetSpecifiedAsync()
        {
            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.GetDynamicSensorParametersAsync(1001, "potato"),
                "Cannot process query for sensor type 'potato': sensor type 'potato' is not valid."
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DynamicSensorParameters_SensorQueryParameters_ParsesParametersAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetParametersValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001),
                UnitRequest.BeginAddSensorQuery(1001, "ptfadsreplfailurexml"),
                UnitRequest.ContinueAddSensorQuery(2055, 7, "database_=XE&sid_type_=0&prefix_=0"),
                UnitRequest.AddSensorProgress(1001, 7),
                UnitRequest.EndAddSensorQuery(1001, 7)
            }));

            await client.GetDynamicSensorParametersAsync(1001, "ptfadsreplfailurexml", queryParameters: new SensorQueryTargetParameters
            {
                ["database"] = "XE",
                ["sid_type"] = 0,
                ["prefix"] = 0
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DynamicSensorParameters_SensorQueryParameters_Throws_WhenParametersAreMissingAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetParametersValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001),
                UnitRequest.BeginAddSensorQuery(1001, "ptfadsreplfailurexml")
            }));

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.GetDynamicSensorParametersAsync(1001, "ptfadsreplfailurexml", queryParameters: new SensorQueryTargetParameters()),
                "Failed to process request for sensor type 'oracletablespace': sensor query target parameters did not include mandatory parameters 'database_',"
            );
        }

        private PrtgClient client => Initialize_Client(new ExeFileTargetResponse());
    }
}
