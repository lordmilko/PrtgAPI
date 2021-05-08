using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Targets;
using PrtgAPI.Tests.UnitTests.ObjectData;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.Serialization;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;
using PrtgAPI.Tree;
using PrtgAPI.Tree.Progress;

namespace PrtgAPI.Tests.UnitTests.Infrastructure
{
    [TestClass]
    public class PrtgClientTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void PrtgClient_Constructor_RetrievesPassHash()
        {
            var webClient = new MockWebClient(new PassHashResponse());

            var client = new PrtgClient("prtg.example.com", "username", "password", AuthMode.Password, webClient);

            Assert.IsTrue(client.PassHash == "12345678");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_Constructor_CantRetrievePassHash()
        {
            var webClient = new MockWebClient(new PassHashResponse("PRTG Network Monitor is starting"));

            AssertEx.Throws<PrtgRequestException>(
                () => new PrtgClient("prtg.example.com", "username", "password", AuthMode.Password, webClient),
                "Could not retrieve PassHash from PRTG Server."
            );
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_Constructor_ServerCannotBeNull()
        {
            AssertEx.Throws<ArgumentNullException>(() => new PrtgClient(null, "username", "password"), $"Value cannot be null.{Environment.NewLine}Parameter name: server");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_Constructor_UsernameCannotBeNull()
        {
            AssertEx.Throws<ArgumentNullException>(() => new PrtgClient("prtg.example.com", null, "password"), $"Value cannot be null.{Environment.NewLine}Parameter name: username");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_Constructor_PasswordCannotBeNull()
        {
            AssertEx.Throws<ArgumentNullException>(() => new PrtgClient("prtg.example.com", "username", null), $"Value cannot be null.{Environment.NewLine}Parameter name: password");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_Constructor_RemovesTrailingServerSlash()
        {
            var response = new AddressValidatorResponse(new[] {"https://prtg.example.com/api/getpasshash.htm?password=password&username=username"}, true, new PassHashResponse());

            var client = new PrtgClient("prtg.example.com/", "username", "password", AuthMode.Password, new MockWebClient(response));

            Assert.AreEqual("prtg.example.com", client.Server);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_RetriesWhileStreaming()
        {
            var response = new SensorResponse(Enumerable.Repeat(new SensorItem(), 1001).ToArray());

            var client = new PrtgClient("prtg.example.com", "username", "passhash", AuthMode.PassHash, new MockRetryWebClient(response, false));

            var retriesToMake = 3;
            var retriesMade = 0;

            client.RetryCount = retriesToMake;

            object objLock = new object();

            client.RetryRequest += (sender, args) =>
            {
                lock (objLock)
                {
                    retriesMade++;
                }
            };

            AssertEx.Throws<WebException>(() => client.StreamSensors().ToList(), string.Empty);
            Assert.AreEqual(retriesToMake * 2, retriesMade, "An incorrect number of retries were made.");
        }

        private int prtgClientRetriesNormally;

        [UnitTest]
        [TestMethod]
        public void PrtgClient_RetriesNormally()
        {
            prtgClientRetriesNormally = 0;

            var response = new SensorResponse(Enumerable.Repeat(new SensorItem(), 1001).ToArray());

            var client = new PrtgClient("prtg.example.com", "username", "passhash", AuthMode.PassHash, new MockRetryWebClient(response, true));

            var retriesToMake = 3;

            client.RetryCount = retriesToMake;

            client.RetryRequest += OnClientOnRetryRequest;

            AssertEx.Throws<WebException>(() => client.GetSensors().ToList(), string.Empty);
            Assert.AreEqual(retriesToMake, prtgClientRetriesNormally, "An incorrect number of retries were made.");

            client.RetryRequest -= OnClientOnRetryRequest;

            AssertEx.Throws<WebException>(() => client.GetSensors().ToList(), string.Empty);
            Assert.AreEqual(retriesToMake, prtgClientRetriesNormally, "Retry handler was called after it was removed");
        }

        private void OnClientOnRetryRequest(object sender, RetryRequestEventArgs args)
        {
            prtgClientRetriesNormally++;
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_SyncAsync_Counterparts()
        {
            var methods = typeof (PrtgClient).GetMethods().ToList();

            var skipStartsWith = new List<string>
            {
                "Stream",
                "Watch",
                "Query",
                "get_",
                "set_",
                "add_",
                "remove_"
            };

            var skipFull = new List<string>
            {
                "ToString",
                "Equals",
                "GetHashCode",
                "GetType"
            };

            var syncMethods = methods.Where(m => !m.Name.EndsWith("Async") && !m.Name.EndsWith("Lazy") && !skipStartsWith.Any(m.Name.StartsWith) && !skipFull.Any(m.Name.StartsWith)).ToList();

            var methodFullNames = methods.Select(m =>
            {
                var str = m.ToString();

                return str.Substring(str.IndexOf(' ') + 1);
            }).ToList();

            var missingAsync = new List<MethodInfo>();

            foreach (var s in syncMethods)
            {
                var str = s.ToString();
                var fullName = str.Substring(str.IndexOf(' ') + 1);

                var asyncName = fullName.Replace($"{s.Name}(", $"{s.Name}Async(");

                if (!methodFullNames.Contains(asyncName))
                {
                    //Maybe the async method only exists with a CancellationToken parameter
                    var asyncWithToken = asyncName.Replace(")", ", System.Threading.CancellationToken)");

                    if (!methodFullNames.Contains(asyncWithToken))
                        missingAsync.Add(s);
                }
            }

            Assert.AreEqual(0, missingAsync.Count, $"Async counterparts of the following methods are missing: {string.Join(", ", missingAsync.Select(m => ReflectionExtensions.GetInternalProperty(m, "FullName").ToString().Substring("PrtgAPI.PrtgClient.".Length)))}");
        }

        [TestMethod]
        [UnitTest(TestCategory.SkipCoverage)]
        public void PrtgClient_StreamsSerial_WhenRequestingOver20000Items()
        {
            var count = 20001;

            var response = new SensorResponse(Enumerable.Repeat(new SensorItem(), count).ToArray());

            var client = new PrtgClient("prtg.example.com", "username", "passhash", AuthMode.PassHash, new MockRetryWebClient(response, false));

            var messageFound = false;

            client.LogVerbose += (e, o) =>
            {
                if (o.Message == "Switching to serial stream mode as over 20000 objects were detected")
                    messageFound = true;
            };

            var sensors = client.StreamSensors().ToList();

            Assert.IsTrue(messageFound, "Request did not stream serially");

            Assert.AreEqual(count, sensors.Count);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_ThrowsWithBadRequest()
        {
            var xml = new XElement("prtg",
                new XElement("version", "1.2.3.4"),
                new XElement("error", "Some of the selected objects could not be deleted.")
            );

            ExecuteFailedRequest(HttpStatusCode.BadRequest, xml.ToString(), null, "Some of the selected objects could not be deleted.");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_ThrowsWithErrorUrl()
        {
            var address = "http://prtg.example.com/error.htm?errormsg=Something+bad+happened";

            ExecuteFailedRequest(HttpStatusCode.OK, string.Empty, address, "Something bad happened");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_ThrowsWithHtml()
        {
            var html = new XElement("div",
                new XAttribute("class", "errormsg"),
                new XElement("p", "PRTG Network Monitor has discovered a problem. Your last request could not be processed properly."),
                new XElement("h3", "Error message: Sorry, the selected object cannot be used here."),
                new XElement("small",
                    new XAttribute("style", "padding:5px;text-align:left"),
                    "Url: /controls/objectdata.htm<br>Params: id=1&objecttype=probe&username=prtgadmin&passhash=***&")
            );

            ExecuteFailedRequest(
                HttpStatusCode.OK,
                html.ToString(SaveOptions.DisableFormatting),
                null,
                "PRTG Network Monitor has discovered a problem. Your last request could not be processed properly. Error message: Sorry, the selected object cannot be used here.",
                c => c.GetSensorProperties(1001)
            );
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_ParsesInvalidXml_List()
        {
            var xml = "<sensors totalcount=\"1\"><item><property><value>1\0</value></property></item></sensors>";

            var client = Initialize_Client(new BasicResponse(xml.ToString()));

            var result = client.ObjectEngine.GetObjects<DummyElement<DummyElementRoot>>(new SensorParameters());

            Assert.AreEqual(true, result.Single().Property.Value);
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_ParsesInvalidXml_ListAsync()
        {
            var xml = "<sensors totalcount=\"1\"><item><property><value>1\0</value></property></item></sensors>";

            var client = Initialize_Client(new BasicResponse(xml.ToString()));

            var result = await client.ObjectEngine.GetObjectsAsync<DummyElement<DummyElementRoot>>(new SensorParameters());

            Assert.AreEqual(true, result.Single().Property.Value);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_ParsesInvalidXml_Object()
        {
            var xml = "<property><value>\01</value></property>";

            var client = Initialize_Client(new BasicResponse(xml.ToString()));

            var result = client.ObjectEngine.GetObject<DummyElementRoot>(new SensorParameters());

            Assert.AreEqual(true, result.Value);
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_ParsesInvalidXml_ObjectAsync()
        {
            var xml = "<property><value>\01</value></property>";

            var client = Initialize_Client(new BasicResponse(xml.ToString()));

            var result = await client.ObjectEngine.GetObjectAsync<DummyElementRoot>(new SensorParameters());

            Assert.AreEqual(true, result.Value);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_ParsesInvalidXml_XDocument()
        {
            var xml = "<property><value>\01</value></property>";

            var client = Initialize_Client(new BasicResponse(xml.ToString()));

            var result = client.ObjectEngine.GetObjectsXml(new SensorParameters());

            Assert.AreEqual("<property><value>1</value></property>", result.ToString(SaveOptions.DisableFormatting));
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_ParsesInvalidXml_XDocumentAsync()
        {
            var xml = "<property><value>\01</value></property>";

            var client = Initialize_Client(new BasicResponse(xml.ToString()));

            var result = await client.ObjectEngine.GetObjectsXmlAsync(new SensorParameters());

            Assert.AreEqual("<property><value>1</value></property>", result.ToString(SaveOptions.DisableFormatting));
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_ParsesInvalidXml_RetainsDirty()
        {
            var xml = "<property><value>\01</value></property>";

            var client = Initialize_Client(new BasicResponse(xml.ToString()));

            var validator = new EventValidator(client, new[]
            {
                //First - retry a dirty response
                UnitRequest.Objects(),
                "XmlSerializer encountered exception ''.', hexadecimal value 0x00, is an invalid character. Line 1, position 18.' while processing request. Retrying request and flagging engine as dirty.",
                UnitRequest.Objects(),

                //Second - engine should be already marked as dirty
                UnitRequest.Objects()
            });

            validator.MoveNext(3);
            var result = client.ObjectEngine.GetObject<DummyElementRoot>(new PrtgObjectParameters());

            validator.MoveNext();
            result = client.ObjectEngine.GetObject<DummyElementRoot>(new PrtgObjectParameters());

            Assert.IsTrue(validator.Finished, "Did not process all requests");
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_ParsesInvalidXml_RetainsDirtyAsync()
        {
            var xml = "<property><value>\01</value></property>";

            var client = Initialize_Client(new BasicResponse(xml.ToString()));

            var validator = new EventValidator(client, new[]
            {
                //First - retry a dirty response
                UnitRequest.Objects(),
                "XmlSerializer encountered exception ''.', hexadecimal value 0x00, is an invalid character. Line 1, position 18.' while processing request. Retrying request and flagging engine as dirty.",
                UnitRequest.Objects(),

                //Second - engine should be already marked as dirty
                UnitRequest.Objects()
            });

            validator.MoveNext(3);
            var result = await client.ObjectEngine.GetObjectAsync<DummyElementRoot>(new PrtgObjectParameters());

            validator.MoveNext();
            result = await client.ObjectEngine.GetObjectAsync<DummyElementRoot>(new PrtgObjectParameters());

            Assert.IsTrue(validator.Finished, "Did not process all requests");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_ParsesInvalidXml_Name()
        {
            var xml = "<property><value>\01</value></property>";

            var client = Initialize_Client(new BasicResponse(xml.ToString()));

            var result = client.ObjectEngine.GetObject<DummyElementRoot>(new SensorParameters());

            Assert.AreEqual(true, result.Value);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_ParsesInvalidXml_Value()
        {
            var xml = "<property\0><value>1</value></property>";

            var client = Initialize_Client(new BasicResponse(xml.ToString()));

            var result = client.ObjectEngine.GetObjectsXml(new SensorParameters());

            Assert.AreEqual("<property><value>1</value></property>", result.ToString(SaveOptions.DisableFormatting));
        }

        private int prtgClientLogVerboseHit;

        [UnitTest]
        [TestMethod]
        public void PrtgClient_LogVerbose()
        {
            prtgClientLogVerboseHit = 0;

            var client = new PrtgClient("prtg.example.com", "username", "1234567890", AuthMode.PassHash, new MockWebClient(new SensorResponse(new SensorItem())));

            client.LogVerbose += LogVerboseHandler;

            var sensors = client.GetSensors();

            Assert.AreEqual(prtgClientLogVerboseHit, 1, "Verbose was not called");

            client.LogVerbose -= LogVerboseHandler;

            sensors = client.GetSensors();

            Assert.AreEqual(prtgClientLogVerboseHit, 1, "Verbose was called after it was removed");
        }

        private void LogVerboseHandler(object sender, LogVerboseEventArgs e)
        {
            prtgClientLogVerboseHit++;
        }

        private void ExecuteFailedRequest(HttpStatusCode statusCode, string xml, string address, string expectedError, Action<PrtgClient> action = null)
        {
            if (action == null)
                action = c => c.GetSensors();

            var response = new FailedRequestResponse(statusCode, xml, address);

            var client = new PrtgClient("prtg.example.com", "username", "1234567890", AuthMode.PassHash, new MockWebClient(response));

            AssertEx.Throws<PrtgRequestException>(() => action(client), $"PRTG was unable to complete the request. The server responded with the following error: {expectedError}");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_HandlesTimeoutSocketException()
        {
            AssertEx.Throws<TimeoutException>(() => ExecuteSocketException(SocketError.TimedOut), "Connection timed out while communicating with remote server");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_RedirectsHttpUrlToHttps()
        {
            var httpServer = "http://prtg.example.com";
            var httpsServer = "https://prtg.example.com";

            var client = new PrtgClient(httpServer, "username", "password", AuthMode.PassHash, new MockWebClient(new HttpToHttpsResponse()));

            Assert.AreEqual(httpServer, client.Server);

            client.GetSensors();

            Assert.AreEqual(httpsServer, client.Server);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_DoesntModifyHttpsToHttps()
        {
            var httpsServer = "https://prtg.example.com";

            var client = new PrtgClient(httpsServer, "username", "password", AuthMode.PassHash, new MockWebClient(new HttpToHttpsResponse()));

            Assert.AreEqual(httpsServer, client.Server);

            client.GetSensors();

            Assert.AreEqual(httpsServer, client.Server);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_SplitsRequests_BatchingOver1500Items()
        {
            var range = Enumerable.Range(1, 2000).ToArray();

            var server = "prtg.example.com";
            var username = "username";
            var passhash = "12345678";

            string[] urls =
            {
                $"https://{server}/api/pause.htm?id={string.Join(",", Enumerable.Range(1, 1500))}&action=0&username={username}&passhash={passhash}",
                $"https://{server}/api/pause.htm?id={string.Join(",", Enumerable.Range(1501, 500))}&action=0&username={username}&passhash={passhash}"
            };

            Execute(
                c => c.PauseObject(range),
                urls
            );
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_HandlesConnectionRefusedSocketException()
        {
            AssertEx.Throws<WebException>(() => ExecuteSocketException(SocketError.ConnectionRefused), "Server rejected HTTPS connection");
        }

        private void ExecuteSocketException(SocketError error)
        {
#if NETFRAMEWORK
            var ctor = typeof(SocketException).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[]
            {
                typeof (int), typeof (EndPoint)
            }, null);

            var ex = (SocketException)ctor.Invoke(new object[]
            {
                (int) error, new IPEndPoint(new IPAddress(0x2414188f), 80)
            });
#else
            var ctor = typeof(SocketException).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[]
            {
                typeof(SocketError)
            }, null);

            var ex = (SocketException) ctor.Invoke(new object[]
            {
                error
            });
#endif

            var response = new ExceptionResponse(ex);
            var webClient = new MockWebClient(response);

            var client = new PrtgClient("prtg.example.com", "username", "1234567890", AuthMode.PassHash, webClient);

            client.GetSensors();
        }

        #region Either

        [UnitTest]
        [TestMethod]
        public void PrtgClient_Either_Object()
        {
            var urls = new[]
            {
                UnitRequest.Sensors("filter_objid=4000"),
                UnitRequest.Channels(4000),
                UnitRequest.ChannelProperties(4000, 1)
            };

            Execute(c =>
            {
                var sensor = c.GetSensor(4000);
                var channel = c.GetChannel(sensor, 1);
            }, urls);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_Either_Id()
        {
            var urls = new[]
            {
                UnitRequest.Channels(4000),
                UnitRequest.ChannelProperties(4000, 1)
            };

            Execute(
                c => c.GetChannel(4000, 1),
                urls
            );
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_Either_Null_Throws()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            AssertEx.Throws<ArgumentNullException>(
                () => client.GetChannel(null, 1),
                "Value of type 'Sensor' cannot be null."
            );
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_Either_Default_Throws()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            AssertEx.Throws<InvalidOperationException>(
                () => client.GetChannel(default(Either<Sensor, int>), 1),
                "Value of type 'Either<Sensor, Int32>' was not properly initialized. Value must specify a 'Left' (Sensor) or 'Right' (Int32) value."
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_Either_Default_ThrowsAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.GetChannelAsync(default(Either<Sensor, int>), 1),
                "Value of type 'Either<Sensor, Int32>' was not properly initialized. Value must specify a 'Left' (Sensor) or 'Right' (Int32) value."
            );
        }

        #endregion
        #region Execute With Log Response

        [UnitTest]
        [TestMethod]
        public void PrtgClient_AllMethodsExecute_WithLogResponse()
        {
            var methods = GetMethods(false);

            var client = GetDefaultClient();

            foreach(var method in methods)
            {
                var c = GetCustomClient(method, client);
                client.LogLevel = LogLevel.All;
                var m = GetCustomMethod(method);
                var p = GetParameters(m);

                m.Invoke(c, p);
            }
        }

        [UnitTest]
        [TestMethod]
        public void PrtgClient_AllMethodsExecute_WithoutLogResponse()
        {
            var methods = GetMethods(false);

            var client = GetDefaultClient();

            foreach (var method in methods)
            {
                var c = GetCustomClient(method, client);
                client.LogLevel = LogLevel.None;
                var m = GetCustomMethod(method);
                var p = GetParameters(method);

                m.Invoke(c, p);
            }
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_AllMethodsExecute_WithLogResponseAsync()
        {
            var methods = GetMethods(true);

            var client = GetDefaultClient();

            foreach (var method in methods)
            {
                var c = GetCustomClient(method, client);
                client.LogLevel = LogLevel.All;
                var m = GetCustomMethod(method);
                var p = GetParameters(method);

                await (Task)m.Invoke(c, p);
            }
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_AllMethodsExecute_WithoutLogResponseAsync()
        {
            var methods = GetMethods(true);

            var client = GetDefaultClient();

            foreach (var method in methods)
            {
                var c = GetCustomClient(method, client);
                client.LogLevel = LogLevel.None;
                var m = GetCustomMethod(method);
                var p = GetParameters(method);

                await (Task)m.Invoke(c, p);
            }
        }

        private List<MethodInfo> GetMethods(bool allowAsync)
        {
            var illegalPrefixes = new[]
            {
                "Watch",
                "get_",
                "set_",
                "add_",
                "remove_"
            };

            var illegalNames = new[]
            {
                "Equals",
                "GetHashCode",
                "GetGroupProperties",
                "GetProbeProperties",
                "GetGroupPropertiesAsync",
                "GetProbePropertiesAsync"
            };

            var methods = typeof(PrtgClient).GetMethods().Where(m => illegalPrefixes.All(p => !m.Name.StartsWith(p)) && illegalNames.All(n => n != m.Name));

            methods = methods.Where(m => m.Name.EndsWith("Async") == allowAsync);

            return methods.ToList();
        }

        internal static MethodInfo GetCustomMethod(MethodInfo method)
        {
            if (method.ContainsGenericParameters)
            {
                if (method.Name.StartsWith("GetSystemInfo"))
                    return method.MakeGenericMethod(typeof(DeviceProcessInfo));
                else
                    return method.MakeGenericMethod(typeof(string));
            }

            return method;
        }

        internal static PrtgClient GetDefaultClient(bool switchContext = false)
        {
            var client = Initialize_Client(new MultiTypeResponse
            {
                CountOverride = new Dictionary<Content, int>
                {
                    [Content.Sensors] = 1,
                    [Content.Devices] = 1,
                    [Content.Groups] = 1,
                    [Content.Probes] = 1,
                    [Content.Notifications] = 1,
                    [Content.Schedules] = 1
                }
            }, switchContext);

            return client;
        }

        private PrtgClient GetCustomClient(MethodInfo method, PrtgClient defaultClient)
        {
            switch (method.Name)
            {
                case "GetNotificationTriggers":
                case "GetNotificationTriggersAsync":
                    return Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger()));
            }

            if (method.Name.StartsWith("GetTree"))
            {
                return Initialize_Client(new MultiTypeResponse
                {
                    ItemOverride = new Dictionary<Content, BaseItem[]>
                    {
                        [Content.Groups] = new[] {new GroupItem(objid: "0")}
                    },
                    CountOverride = new Dictionary<Content, int>
                    {
                        [Content.Probes] = 0
                    }
                });
            }

            return defaultClient;
        }

        internal static object[] GetParameters(MethodBase method)
        {
            var parameters = method.GetParameters();
            return parameters.Select(p => GetParameterObject(method, p)).ToArray();
        }

        private static object GetParameterObject(MethodBase method, ParameterInfo parameter)
        {
            var realClient = Initialize_Client(new MultiTypeResponse());

            var t = parameter.ParameterType;

            if (t == typeof(string))
            {
                if (parameter.Name == "sensorType")
                {
                    if (method.Name == "GetDynamicSensorParameters" || method.Name == "GetDynamicSensorParametersAsync")
                        return "snmplibrary";

                    return "exexml";
                }
                if (parameter.Name == "objectType")
                    return "device";
                if (parameter.Name == "property")
                    return "name_";
                if (parameter.Name == "name")
                {
                    if (method.Name == "GetChannel" || method.Name == "GetChannelAsync")
                        return "Percent Available Memory";
                }

                return "test";
            }
            if (t == typeof(int) || t == typeof(int?))
                return 1;
            if (t == typeof(int[]))
                return new[] { 1 };
            if (t == typeof(CustomParameter[]))
                return new[] { new CustomParameter("name_", "test") };
            if (t == typeof(SystemCacheType))
                return SystemCacheType.General;
            if (t == typeof(ConfigFileType))
                return ConfigFileType.General;
            if (t == typeof(bool))
                return false;
            if (t == typeof(Func<ProbeRestartProgress, bool>))
            {
                Func<ProbeRestartProgress, bool> f = p => true;
                return f;
            }
            if (t == typeof(Func<ProbeRestartProgress[], bool>))
            {
                Func<ProbeRestartProgress[], bool> f = p => true;
                return f;
            }
            if (t == typeof(Func<RestartCoreStage, bool>))
            {
                Func<RestartCoreStage, bool> f = p => true;
                return f;
            }
            if (t == typeof(CancellationToken))
                return CancellationToken.None;
            if (t == typeof(ObjectProperty))
                return ObjectProperty.Name;
            if (t == typeof(ChannelProperty))
                return ChannelProperty.LowerWarningLimit;
            if (t == typeof(object) && parameter.Name == "value")
            {
                if (method.Name.StartsWith("SetTriggerProperty"))
                    return TriggerSensorState.Down;

                return "1";
            }
            if (t == typeof(Position))
                return Position.Up;
            if (t == typeof(ProbeApproval))
                return ProbeApproval.Allow;
            if (t == typeof(SensorOrDeviceOrGroupOrProbe))
            {
                return new Sensor
                {
                    Name = "test",
                    Id = 1001,
                    Position = 3
                };
            }
            if (t == typeof(NewSensorParameters))
                return new ExeXmlSensorParameters("test.ps1");
            if (t == typeof(NewDeviceParameters))
                return new NewDeviceParameters("test");
            if (t == typeof(NewGroupParameters))
                return new NewGroupParameters("test");
            if (t == typeof(TriggerParameters))
                return new ChangeTriggerParameters(1001);
            if (t == typeof(LogParameters))
                return new LogParameters(1001);
            if (t == typeof(Func<int, bool>))
            {
                Func<int, bool> f = p => true;
                return f;
            }
            if (t == typeof(DeviceTemplate[]))
                return new[] { new DeviceTemplate("test|test") };
            if (t == typeof(Channel))
                return realClient.GetChannel(4000, 1);
            if (t == typeof(IEnumerable<Channel>))
                return realClient.GetChannels(4000).ToArray();
            if (t == typeof(NotificationTrigger))
                return realClient.GetNotificationTriggers(0).First();
            if (t == typeof(IEnumerable<NotificationTrigger>))
                return realClient.GetNotificationTriggers(0).Where(tr => !tr.Inherited).ToArray();
            if (t == typeof(PropertyParameter[]))
                return new[] { new PropertyParameter(ObjectProperty.Name, "test") };
            if (t == typeof(ChannelParameter[]))
                return new[] { new ChannelParameter(ChannelProperty.LowerWarningLimit, 1) };
            if (t == typeof(TriggerParameter[]))
                return new[] { new TriggerParameter(TriggerProperty.Latency, 40) };
            if (t == typeof(ObjectType))
                return ObjectType.Device;
            if (t == typeof(RecordAge))
                return RecordAge.LastMonth;
            if (t == typeof(LogStatus[]))
                return new[] { LogStatus.Connected };
            if (t == typeof(Expression<Func<Sensor, bool>>))
            {
                Expression<Func<Sensor, bool>> f = l => true;
                return f;
            }
            if (t == typeof(Expression<Func<Device, bool>>))
            {
                Expression<Func<Device, bool>> f = l => true;
                return f;
            }
            if (t == typeof(Expression<Func<Group, bool>>))
            {
                Expression<Func<Group, bool>> f = l => true;
                return f;
            }
            if (t == typeof(Expression<Func<Probe, bool>>))
            {
                Expression<Func<Probe, bool>> f = l => true;
                return f;
            }
            if (t == typeof(Expression<Func<Log, bool>>))
            {
                Expression<Func<Log, bool>> f = l => true;
                return f;
            }
            if (t == typeof(Property))
                return Property.Name;
            if (t == typeof(SearchFilter[]))
                return new[] { new SearchFilter(Property.Name, "test") };
            if (t == typeof(DateTime?))
                return DateTime.Now;
            if (t == typeof(SystemInfoType))
                return SystemInfoType.Processes;
            if (t == typeof(SystemInfoType[]))
                return new[] { SystemInfoType.Processes };
            if (t == typeof(Content))
                return Content.Sensors;
            if (t == typeof(AutoDiscoveryMode))
                return AutoDiscoveryMode.Automatic;
            if (t == typeof(SensorParameters))
                return new SensorParameters();
            if (t == typeof(DeviceParameters))
                return new DeviceParameters();
            if (t == typeof(GroupParameters))
                return new GroupParameters();
            if (t == typeof(ProbeParameters))
                return new ProbeParameters();
            if (t == typeof(PrtgObjectParameters))
                return new PrtgObjectParameters();
            if (t == typeof(FilterOperator))
                return FilterOperator.Contains;
            if (t == typeof(Status[]))
                return new[] { Status.Up };

            //Parameter specific
            if (t == typeof(ExeFileTarget))
                return (ExeFileTarget) "test.ps1";
            if (t == typeof(ISensorQueryTargetParameters))
                return new SensorQueryTarget("APC UPS.oidlib");
            if (t == typeof(IEnumerable<string>))
                return Enumerable.Empty<string>();
            if (t.IsEnum)
                return Enum.GetValues(t).Cast<object>().First();
            if (t == typeof(object) && parameter.Name == "objectType")
                return ObjectType.Device;
            if (t == typeof(WmiServiceTarget))
                return Initialize_Client(new MultiTypeResponse()).Targets.GetWmiServices(1001).First();
            if (t == typeof(Either<IPrtgObject, int>))
            {
                if (method.Name.StartsWith("SetTriggerProperty"))
                    return new Either<IPrtgObject, int>(0);

                return new Either<IPrtgObject, int>(1001);
            }
            if (t == typeof(Either<PrtgObject, int>))
                return new Either<PrtgObject, int>(new Group {Id = 0});
            if (t == typeof(Either<Sensor, int>))
                return new Either<Sensor, int>(1001);
            if (t == typeof(Either<Device, int>))
                return new Either<Device, int>(1001);
            if (t == typeof(Either<Group, int>))
                return new Either<Group, int>(1001);
            if (t == typeof(Either<Probe, int>))
                return new Either<Probe, int>(1001);
            if (t == typeof(Either<GroupOrProbe, int>))
                return new Either<GroupOrProbe, int>(1001);
            if (t == typeof(Either<DeviceOrGroupOrProbe, int>))
                return new Either<DeviceOrGroupOrProbe, int>(1001);
            if (t == typeof(PrtgObject))
                return new Group {Id = 0};
            if (t == typeof(ITreeProgressCallback))
                return new DummyTreeProgressCallback();
            if (t == typeof(Dictionary<string, string>))
                return new Dictionary<string, string>();
            if (parameter.Name == "versionSpecific")
                return null;
            if (t == typeof(FlagEnum<TreeParseOption>?))
                return new FlagEnum<TreeParseOption>(TreeParseOption.Common);
            if (t == typeof(Func<DateTime, DateTime>))
            {
                Func<DateTime, DateTime> f = d => d;
                return f;
            }

            throw new NotImplementedException($"Don't know how to create instance for parameter {parameter}");
        }

        private object GetTypeObject(Type t)
        {
            if (t == typeof(DeviceTemplate))
                return new DeviceTemplate("a|b");
            if (t == typeof(string))
                return "test";
            if (t == typeof(WmiServiceTarget))
                return Initialize_Client(new MultiTypeResponse()).Targets.GetWmiServices(1001).First();
            if (t == typeof(GenericSensorTarget[]))
                return new GenericSensorTarget[] { };
            if (t == typeof(CustomParameter))
                return new CustomParameter("test", "value");
            if (t == typeof(SearchFilter))
                return new SearchFilter(Property.Name, "test");
            if (t == typeof(Property))
                return Property.Name;

            throw new NotImplementedException($"Don't know how to create instance for type {t}");
        }

        #endregion
        #region Execute With Null Arguments

        [UnitTest]
        [TestMethod]
        public void PrtgClient_AllMethodsExecute_WithNullArgumentValidations()
        {
            WithMethodParameters(false, null, (c, method, @params, p, i) =>
            {
                var nullArgument = TestReflectionUtilities.GetDefault(p.ParameterType);

                if (nullArgument == null)
                {
                    var args = GetParametersWithNull(nullArgument, @params, method, i);

                    try
                    {
                        method.Invoke(c, args);

                        if (CanIgnoreNullParameter(method, p))
                            return;

                        Assert.Fail($"Expected an ArgumentNullException to be thrown for parameter '{p.Name}' on method {method}, however no exception was thrown.");
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (IsMethod(method, "GetDynamicSensorParameters") && p.Name == "queryParameters")
                            return;

                        Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentNullException), $"Parameter '{p.Name}' did not throw the right exception on method '{method}': {ex.InnerException.StackTrace}");
                    }
                }
            });
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_AllMethodsExecute_WithNullArgumentValidationsAsync()
        {
            await WithMethodParametersAsync(true, null, async (c, method, @params, p, i) =>
            {
                var nullArgument = TestReflectionUtilities.GetDefault(p.ParameterType);

                if (nullArgument == null)
                {
                    var args = GetParametersWithNull(nullArgument, @params, method, i);

                    try
                    {
                        await (Task)method.Invoke(c, args);

                        if (CanIgnoreNullParameter(method, p))
                            return;

                        Assert.Fail($"Expected an ArgumentNullException to be thrown for parameter '{p.Name}' on method {method}, however no exception was thrown.");
                    }
                    catch (Exception ex)
                    {
                        if (IsMethod(method, "GetDynamicSensorParameters") && p.Name == "queryParameters")
                            return;

                        Assert.IsInstanceOfType(ex, typeof(ArgumentNullException), $"Parameter '{p.Name}' did not throw the right exception on method '{method}': {ex.StackTrace}");
                    }
                }
            });
        }

        private object[] GetParametersWithNull(object nullArgument, ParameterInfo[] parameters, MethodInfo method, int nullIndex)
        {
            var args = parameters.Select((v, j) =>
            {
                if (nullIndex == j)
                    return nullArgument;

                return GetParameterObject(method, v);
            }).ToArray();

            return args;
        }

        private bool CanIgnoreNullParameter(MethodInfo method, ParameterInfo parameter)
        {
            if (method.Name.StartsWith("Query") && parameter.Name == "predicate")
                return true;

            if ((IsMethod(method, "GetLogs") || method.Name == "StreamLogs") && (parameter.Name == "startDate" || parameter.Name == "count"))
                return true;

            if (parameter.HasDefaultValue && parameter.DefaultValue == null)
                return true;

            //Params shouldn't throw
            if (parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
                return true;

            if (IsMethod(method, "SetObjectProperty") || IsMethod(method, "SetChannelProperty") || IsMethod(method, "SetObjectPropertyRaw") || IsMethod(method, "SetTriggerProperty") && parameter.Name == "value")
                return true;

            var allowed = new Dictionary<string, object>
            {
                ["AutoDiscoverAsync"] = "templates",
                ["GetLogsAsync"] = new[] { "status", "endDate" },
                ["GetSensorsAsync"] = "statuses",
                ["GetTotalObjectsAsync"] = "filters",
                ["RefreshSystemInfoAsync"] = "types"
            };

            object v;

            if (allowed.TryGetValue(method.Name, out v) && v.ToString() == parameter.Name || ((string[])v).Contains(parameter.Name))
                return true;

            return false;
        }

        #endregion
        #region Execute With Null List Elements

        [UnitTest]
        [TestMethod]
        public void PrtgClient_AllMethodsExecute_WithNullListElements()
        {
            WithMethodParameters(
                false,
                GetEnumerableParameterFilter,
                (c, method, @params, p, i) =>
                {
                    if (IsValueTypeArray(p))
                        return;

                    var args = GetParametersWithNullElement(@params, p, method);

                    try
                    {
                        method.Invoke(c, args);
                    }
                    catch (TargetInvocationException ex)
                    {
                        Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentException), $"Parameter '{p.Name}' did not throw the right exception on method '{method}': {ex.InnerException.StackTrace}");
                    }
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_AllMethodsExecute_WithNullListElementsAsync()
        {
            await WithMethodParametersAsync(
                true,
                GetEnumerableParameterFilter,
                async (c, method, @params, p, i) =>
                {
                    if (IsValueTypeArray(p))
                        return;

                    var args = GetParametersWithNullElement(@params, p, method);

                    try
                    {
                        await (Task)method.Invoke(c, args);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            );
        }

        private object[] GetParametersWithNullElement(ParameterInfo[] parameters, ParameterInfo currentParameter, MethodInfo method)
        {
            var args = parameters.Select((p) =>
            {
                if (p == currentParameter)
                {
                    if (p.ParameterType.IsInterface)
                    {
                        var underlying = p.ParameterType.GetGenericArguments()[0];

                        var array = underlying.MakeArrayType();

                        return Activator.CreateInstance(array, 1);
                    }
                    else
                        return Activator.CreateInstance(p.ParameterType, 1);
                }

                return GetParameterObject(method, p);
            }).ToArray();

            return args;
        }

        private bool IsValueTypeArray(ParameterInfo parameter) => parameter.ParameterType.IsArray && parameter.ParameterType.GetElementType().IsValueType;

        #endregion
        #region Execute With Empty Lists

        [UnitTest]
        [TestMethod]
        public void PrtgClient_AllMethodsExecute_WithEmptyListValidations()
        {
            WithMethodParameters(
                false,
                GetEnumerableParameterFilter,
                (c, method, @params, p, i) =>
                {
                    var args = GetParametersWithEmptyListElement(@params, p, method);

                    try
                    {
                        method.Invoke(c, args);

                        if (p.GetCustomAttribute<ParamArrayAttribute>() != null || p.HasDefaultValue)
                            return;

                        Assert.Fail($"Expected an ArgumentException to be thrown for parameter '{p.Name}' on method {method}, however no exception was thrown.");
                    }
                    catch (TargetInvocationException ex)
                    {
                        Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentException), $"Parameter '{p.Name}' did not throw the right exception on method '{method}': {ex.InnerException.StackTrace}");
                    }
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_AllMethodsExecute_WithEmptyListValidationsAsync()
        {
            await WithMethodParametersAsync(
                true,
                GetEnumerableParameterFilter,
                async (c, method, @params, p, i) =>
                {
                    var args = GetParametersWithEmptyListElement(@params, p, method);

                    try
                    {
                        await (Task)method.Invoke(c, args);

                        if (p.GetCustomAttribute<ParamArrayAttribute>() != null || p.HasDefaultValue)
                            return;

                        var allowed = new Dictionary<string, object>
                        {
                            ["AutoDiscoverAsync"] = "templates",
                            ["GetSensorsAsync"] = new[] { "filters", "statuses" },
                            ["GetDevicesAsync"] = "filters",
                            ["GetGroupsAsync"] = "filters",
                            ["GetProbesAsync"] = "filters",
                            ["GetNotificationActionsAsync"] = "filters",
                            ["GetSchedulesAsync"] = "filters",
                            ["GetObjectsAsync"] = "filters",
                            ["GetLogsAsync"] = "status"
                        };

                        object v;

                        if (allowed.TryGetValue(method.Name, out v) && v.ToString() == p.Name || ((string[])v).Contains(p.Name))
                            return;
                    }
                    catch (Exception ex)
                    {
                        Assert.IsInstanceOfType(ex, typeof(ArgumentException), $"Parameter '{p.Name}' did not throw the right exception on method '{method}': {ex.StackTrace}");
                    }
                }
            );
        }

        private object[] GetParametersWithEmptyListElement(ParameterInfo[] parameters, ParameterInfo currentParameter, MethodInfo method)
        {
            var args = parameters.Select((p) =>
            {
                if (p == currentParameter)
                {
                    if (p.ParameterType.IsInterface)
                    {
                        var underlying = p.ParameterType.GetGenericArguments()[0];

                        var array = underlying.MakeArrayType();

                        return Activator.CreateInstance(array, 0);
                    }
                    else
                        return Activator.CreateInstance(p.ParameterType, 0);
                }

                return GetParameterObject(method, p);
            }).ToArray();

            return args;
        }

        #endregion
        #region Execute With Injected Null List Elements

        [UnitTest]
        [TestMethod]
        public void PrtgClient_AllMethodsExecute_WithInjectedNullArrayElements()
        {
            WithMethodParameters(
                false,
                p => typeof(IParameters).IsAssignableFrom(p.ParameterType),
                (c, method, @params, p, i) =>
                {
                    Action<Type> action = instanceType =>
                    {
                        foreach (var property in GetEnumerableParameterProperties(instanceType))
                        {
                            object[] args;

                            if (!GetInjectedNullArrayArgs(property, method, @params, p, instanceType, out args))
                                continue;

                            try
                            {
                                method.Invoke(c, args);
                            }
                            catch (TargetInvocationException ex)
                            {
                                Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidOperationException), $"Parameter '{p.Name}' did not throw the right exception on method '{method}': {ex.InnerException.StackTrace}");
                            }
                        }
                    };

                    if (IsBaseTypeParameter(p))
                    {
                        foreach (var type in DerivedParameterTypes(p))
                        {
                            action(type);
                        }
                    }
                    else
                    {
                        action(p.ParameterType);
                    }
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_AllMethodsExecute_WithInjectedNullArrayElementsAsync()
        {
            await WithMethodParametersAsync(
                true,
                p => typeof(IParameters).IsAssignableFrom(p.ParameterType),
                async (c, method, @params, p, i) =>
                {
                    Func<Type, Task> action = async instanceType =>
                    {
                        foreach (var property in GetEnumerableParameterProperties(instanceType))
                        {
                            object[] args;

                            if (!GetInjectedNullArrayArgs(property, method, @params, p, instanceType, out args))
                                continue;

                            try
                            {
                                await (Task)method.Invoke(c, args);
                            }
                            catch (InvalidOperationException)
                            {
                            }
                        }
                    };

                    if (IsBaseTypeParameter(p))
                    {
                        foreach (var type in DerivedParameterTypes(p))
                        {
                            await action(type);
                        }
                    }
                    else
                    {
                        await action(p.ParameterType);
                    }
                }
            );
        }

        private bool GetInjectedNullArrayArgs(PropertyCache paramsInstanceProperty, MethodInfo method, ParameterInfo[] @params, ParameterInfo p, Type instanceType, out object[] args)
        {
            Type underlying;

            if (!TryGetUnderlyingEnumerableType(paramsInstanceProperty.Property.PropertyType, out underlying))
            {
                args = null;
                return false;
            }

            //Create an instance of our IParameters type
            var instance = ParameterTests.GetParametersInstance(instanceType);

            //Create a set of arguments for the method, including our IParameters instance at its appropriate position
            args = GetParametersWithInjectedNullArrayElement(@params, p, method, instance);

            //Create an array for the current enumerable IParameters property we're processing containing
            //a single valid element. Assign the value to the property, then do a bait and switch, replacing
            //the value with null after it's already been inserted.
            InjectNullArrayArg(paramsInstanceProperty, underlying, instance);

            return true;
        }

        private void InjectNullArrayArg(PropertyCache paramsInstanceProperty, Type underlying, object instance)
        {
            var t = paramsInstanceProperty.Property.PropertyType;

            //Create a new collection capable of holding a single element
            var collection = Activator.CreateInstance(t, 1);

            //Figure out what sort of collection it is, and how to interface with its members

            var list = collection as IList;

            if (list != null)
            {
                if (t.IsArray)
                {
                    list[0] = GetTypeObject(underlying);
                    paramsInstanceProperty.SetValue(instance, list);
                    list[0] = null;
                }
                else
                {
                    list.Add(GetTypeObject(underlying));
                    paramsInstanceProperty.SetValue(instance, list);
                    list[0] = null;
                }
            }
            else
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var genericArgs = t.GetGenericArguments();

                    var key = GetTypeObject(genericArgs[0]);
                    var value = GetTypeObject(genericArgs[1]);

                    ((IDictionary) collection).Add(key, value);

                    paramsInstanceProperty.SetValue(instance, collection);

                    ((IDictionary)collection)[key] = null;
                }
                else
                    throw new NotImplementedException($"Don't know what sort of collection '{t}' is");
            }
        }

        private bool TryGetUnderlyingEnumerableType(Type t, out Type underlying)
        {
            //Is the property we're looking at on this IParmeters instance some type of array? If so, record its underlying type
            //so we can create a dummy instance of it
            if (t.IsArray)
            {
                if (t.GetElementType().IsValueType)
                {
                    underlying = null;
                    return false;
                }

                underlying = t.GetElementType();
            }
            else
            {
                if (t.IsGenericType)
                {
                    underlying = t.GetGenericArguments()[0];

                    if (underlying.IsValueType)
                    {
                        underlying = null;
                        return false;
                    }
                }
                else
                    throw new NotImplementedException($"Don't know what sort of collection property of type '{t}' is");
            }

            return true;
        }

        private IEnumerable<PropertyCache> GetEnumerableParameterProperties(Type t) => ReflectionExtensions.GetNormalProperties(t)
            .Where(p2 => typeof(IEnumerable).IsAssignableFrom(p2.Property.PropertyType) && p2.Property.PropertyType != typeof(string));

        private object[] GetParametersWithInjectedNullArrayElement(ParameterInfo[] parameters, ParameterInfo currentParameter, MethodInfo method, object instance)
        {
            var args = parameters.Select((p) =>
            {
                if (p == currentParameter)
                {
                    return instance;
                }

                return GetParameterObject(method, p);
            }).ToArray();

            return args;
        }

        #endregion
        #region Execute With Null IParameter Properties

        [UnitTest]
        [TestMethod]
        public void PrtgClient_AllMethodsExecute_ParametersWithNullProperties()
        {
            WithMethodParameters(
                false,
                p => typeof(IParameters).IsAssignableFrom(p.ParameterType),
                (c, method, @params, p, i) =>
                {
                    if (IsBaseTypeParameter(p))
                    {
                        foreach (var type in DerivedParameterTypes(p))
                        {
                            var args = GetParametersWithNullParameterProperties(@params, p, method, type);

                            method.Invoke(c, args);
                        }
                    }
                    else
                    {
                        var args = GetParametersWithNullParameterProperties(@params, p, method);

                        method.Invoke(c, args);
                    }
                });
        }

        [UnitTest]
        [TestMethod]
        public async Task PrtgClient_AllMethodsExecute_ParametersWithNullPropertiesAsync()
        {
            await WithMethodParametersAsync(
                true,
                p => typeof(IParameters).IsAssignableFrom(p.ParameterType),
                async (c, method, @params, p, i) =>
                {
                    if (p.ParameterType == typeof(TriggerParameters) || p.ParameterType == typeof(NewSensorParameters))
                    {
                        foreach (var type in p.ParameterType.Assembly.GetTypes()
                            .Where(t => p.ParameterType.IsAssignableFrom(t) && t != p.ParameterType && !t.IsAbstract))
                        {
                            var args = GetParametersWithNullParameterProperties(@params, p, method, type);

                            await (Task)method.Invoke(c, args);
                        }
                    }
                    else
                    {
                        var args = GetParametersWithNullParameterProperties(@params, p, method);

                        await (Task)method.Invoke(c, args);
                    }
                });
        }

        private object[] GetParametersWithNullParameterProperties(ParameterInfo[] parameters, ParameterInfo currentParameter, MethodInfo method, Type alternateType = null)
        {
            var args = parameters.Select((p) =>
            {
                if (p == currentParameter)
                {
                    var t = alternateType ?? p.ParameterType;

                    var instance = ParameterTests.GetParametersInstance(t);

                    foreach (var prop in ReflectionExtensions.GetNormalProperties(t))
                    {
                        if (prop.Property.PropertyType.IsValueType)
                            continue;

                        try
                        {
                            prop.SetValue(instance, null);
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }

                return GetParameterObject(method, p);
            }).ToArray();

            return args;
        }

        #endregion
        #region Execute With* Helpers

        private bool IsBaseTypeParameter(ParameterInfo p) => p.ParameterType == typeof(TriggerParameters) || p.ParameterType == typeof(NewSensorParameters);

        private IEnumerable<Type> DerivedParameterTypes(ParameterInfo p) => p.ParameterType.Assembly.GetTypes()
            .Where(t => p.ParameterType.IsAssignableFrom(t) && t != p.ParameterType && !t.IsAbstract);

        private void WithMethodParameters(bool allowAsync, Func<ParameterInfo, bool> filter,
            Action<PrtgClient, MethodInfo, ParameterInfo[], ParameterInfo, int> action)
        {
            var methods = GetMethods(allowAsync).OrderBy(m => m.Name).ToList();

            var client = GetDefaultClient();

            foreach (var method in methods)
            {
                var c = GetCustomClient(method, client);
                var m = GetCustomMethod(method);
                var parameters = m.GetParameters();

                var filteredParams = parameters;

                if (filter != null)
                    filteredParams = parameters.Where(filter).ToArray();

                for (var i = 0; i < filteredParams.Length; i++)
                {
                    action(c, m, parameters, filteredParams[i], i);
                }
            }
        }

        private async Task WithMethodParametersAsync(bool allowAsync, Func<ParameterInfo, bool> filter,
            Func<PrtgClient, MethodInfo, ParameterInfo[], ParameterInfo, int, Task> action)
        {
            var methods = GetMethods(allowAsync).OrderBy(m => m.Name).ToList();

            var client = GetDefaultClient();

            foreach (var method in methods)
            {
                var c = GetCustomClient(method, client);
                var m = GetCustomMethod(method);
                var parameters = m.GetParameters();

                var filteredParams = parameters;

                if (filter != null)
                    filteredParams = parameters.Where(filter).ToArray();

                for (var i = 0; i < filteredParams.Length; i++)
                {
                    await action(c, m, parameters, filteredParams[i], i);
                }
            }
        }

        private bool GetEnumerableParameterFilter(ParameterInfo info) => typeof(IEnumerable).IsAssignableFrom(info.ParameterType) && info.ParameterType != typeof(string);

        private bool IsMethod(MethodInfo method, string name)
        {
            return method.Name == name || method.Name == $"{name}Async";
        }

        #endregion
    }

    internal class DummyTreeProgressCallback : ITreeProgressCallback
    {
        public DepthManager DepthManager { get; }
        public void OnLevelBegin(ITreeValue parent, PrtgNodeType parentType, int depth)
        {
        }

        public void OnLevelWidthKnown(ITreeValue parent, PrtgNodeType parentType, int width)
        {
        }

        public void OnProcessValue(ITreeValue value)
        {
        }

        public void OnProcessType(PrtgNodeType type, int index, int total)
        {
        }
    }
}
