using System;
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
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.Infrastructure
{
    [TestClass]
    public class PrtgClientTests
    {
        [TestMethod]
        public void PrtgClient_Constructor_RetrievesPassHash()
        {
            var webClient = new MockWebClient(new PassHashResponse());

            var client = new PrtgClient("prtg.example.com", "username", "password", AuthMode.Password, webClient);

            Assert.IsTrue(client.PassHash == "12345678");
        }

        [TestMethod]
        public void PrtgClient_Constructor_CantRetrievePassHash()
        {
            var webClient = new MockWebClient(new PassHashResponse("PRTG Network Monitor is starting"));

            AssertEx.Throws<PrtgRequestException>(
                () => new PrtgClient("prtg.example.com", "username", "password", AuthMode.Password, webClient),
                "Could not retrieve PassHash from PRTG Server."
            );
        }

        [TestMethod]
        public void PrtgClient_Constructor_ServerCannotBeNull()
        {
            AssertEx.Throws<ArgumentNullException>(() => new PrtgClient(null, "username", "password"), "Value cannot be null.\r\nParameter name: server");
        }

        [TestMethod]
        public void PrtgClient_Constructor_UsernameCannotBeNull()
        {
            AssertEx.Throws<ArgumentNullException>(() => new PrtgClient("prtg.example.com", null, "password"), "Value cannot be null.\r\nParameter name: username");
        }

        [TestMethod]
        public void PrtgClient_Constructor_PasswordCannotBeNull()
        {
            AssertEx.Throws<ArgumentNullException>(() => new PrtgClient("prtg.example.com", "username", null), "Value cannot be null.\r\nParameter name: pass");
        }

        [TestMethod]
        public void PrtgClient_RetriesWhileStreaming()
        {
            var response = new SensorResponse(Enumerable.Repeat(new SensorItem(), 1001).ToArray());

            var client = new PrtgClient("prtg.example.com", "username", "passhash", AuthMode.PassHash, new MockRetryWebClient(response, false));

            var retriesToMake = 3;
            var retriesMade = 0;

            client.RetryCount = retriesToMake;

            client.RetryRequest += (sender, args) =>
            {
                retriesMade++;
            };

            AssertEx.Throws<WebException>(() => client.StreamSensors().ToList(), string.Empty);
            Assert.AreEqual(retriesToMake * 2, retriesMade, "An incorrect number of retries were made.");
        }

        private int prtgClientRetriesNormally;

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

            var syncMethods = methods.Where(m => !m.Name.EndsWith("Async") && !skipStartsWith.Any(m.Name.StartsWith) && !skipFull.Any(m.Name.StartsWith)).ToList();

            var methodFullNames = methods.Select(m => m.GetInternalProperty("FullName")).ToList();

            var missingAsync = new List<MethodInfo>();

            foreach (var s in syncMethods)
            {
                var fullName = (string)s.GetInternalProperty("FullName");

                var asyncName = fullName.Replace($"{s.Name}(", $"{s.Name}Async(");

                if (!methodFullNames.Contains(asyncName))
                {
                    //Maybe the async method only exists with a CancellationToken parameter
                    var asyncWithToken = asyncName.Replace(")", ", System.Threading.CancellationToken)");

                    if (!methodFullNames.Contains(asyncWithToken))
                        missingAsync.Add(s);
                }
            }

            Assert.AreEqual(0, missingAsync.Count, $"Async counterparts of the following methods are missing: {string.Join(", ", missingAsync.Select(m => m.GetInternalProperty("FullName").ToString().Substring("PrtgAPI.PrtgClient.".Length)))}");
        }

        [TestMethod]
        [TestCategory("SlowCoverage")]
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

        [TestMethod]
        public void PrtgClient_ThrowsWithBadRequest()
        {
            var xml = new XElement("prtg",
                new XElement("version", "1.2.3.4"),
                new XElement("error", "Some of the selected objects could not be deleted.")
            );

            ExecuteFailedRequest(HttpStatusCode.BadRequest, xml.ToString(), null, "Some of the selected objects could not be deleted.");
        }

        [TestMethod]
        public void PrtgClient_ThrowsWithErrorUrl()
        {
            var address = "http://prtg.example.com/error.htm?errormsg=Something+bad+happened";

            ExecuteFailedRequest(HttpStatusCode.OK, string.Empty, address, "Something bad happened");
        }

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

        [TestMethod]
        public void PrtgClient_ParsesInvalidXml()
        {
            var xml = "<prtg\0>value!</prtg>";

            var xDoc = XDocumentUtilities.SanitizeXml(xml);

            Assert.AreEqual("value!", xDoc.Element("prtg").Value);
        }

        private int prtgClientLogVerboseHit;

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

        [TestMethod]
        public void PrtgClient_HandlesTimeoutSocketException()
        {
            AssertEx.Throws<TimeoutException>(() => ExecuteSocketException(SocketError.TimedOut), "Connection timed out while communicating with remote server");
        }

        [TestMethod]
        public void PrtgClient_SplitsRequests_BatchingOver1500Items()
        {
            var range = Enumerable.Range(1, 2000).ToArray();

            var server = "prtg.example.com";
            var username = "username";
            var passhash = "1234567890";

            string[] urls =
            {
                $"https://{server}/api/pause.htm?id={string.Join(",", Enumerable.Range(1, 1500))}&action=0&username={username}&passhash={passhash}",
                $"https://{server}/api/pause.htm?id={string.Join(",", Enumerable.Range(1501, 500))}&action=0&username={username}&passhash={passhash}"
            };

            var client = new PrtgClient(server, username, passhash, AuthMode.PassHash, new MockWebClient(new AddressValidatorResponse(urls, true)));

            client.PauseObject(range);
        }

        [TestMethod]
        public void PrtgClient_HandlesConnectionRefusedSocketException()
        {
            AssertEx.Throws<WebException>(() => ExecuteSocketException(SocketError.ConnectionRefused), "Server rejected HTTPS connection");
        }

        private void ExecuteSocketException(SocketError error)
        {
            var ctor = typeof(SocketException).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[]
            {
                typeof (int), typeof (EndPoint)
            }, null);

            var ex = (SocketException)ctor.Invoke(new object[]
            {
                (int) error, new IPEndPoint(new IPAddress(0x2414188f), 80)
            });

            var response = new ExceptionResponse(ex);
            var webClient = new MockWebClient(response);

            var client = new PrtgClient("prtg.example.com", "username", "1234567890", AuthMode.PassHash, webClient);

            client.GetSensors();
        }

        #region Execute With Log Response

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
                var p = GetParameters(method);

                m.Invoke(c, p);
            }
        }

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

        private MethodInfo GetCustomMethod(MethodInfo method)
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

        private PrtgClient GetDefaultClient()
        {
            var client = BaseTest.Initialize_Client(new MultiTypeResponse
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
            });

            return client;
        }

        private PrtgClient GetCustomClient(MethodInfo method, PrtgClient defaultClient)
        {
            if (method.Name == "GetNotificationTriggers" || method.Name == "GetNotificationTriggersAsync")
            {
                return BaseTest.Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger()));
            }

            return defaultClient;
        }

        private object[] GetParameters(MethodInfo method)
        {
            var parameters = method.GetParameters();
            return parameters.Select(p => GetParameterObject(method, p)).ToArray();
        }

        private object GetParameterObject(MethodInfo method, ParameterInfo parameter)
        {
            var realClient = BaseTest.Initialize_Client(new MultiTypeResponse());

            var t = parameter.ParameterType;

            if (t == typeof(string))
            {
                if (parameter.Name == "sensorType")
                    return "exexml";
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
                return "1";
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
            if (t == typeof(NotificationTrigger))
                return realClient.GetNotificationTriggers(0).First();
            if (t == typeof(PropertyParameter[]))
                return new[] { new PropertyParameter(ObjectProperty.Name, "test") };
            if (t == typeof(ChannelParameter[]))
                return new[] { new ChannelParameter(ChannelProperty.LowerWarningLimit, 1) };
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

            throw new NotImplementedException($"Don't know how to process parameter {parameter}");
        }

        #endregion
    }
}
