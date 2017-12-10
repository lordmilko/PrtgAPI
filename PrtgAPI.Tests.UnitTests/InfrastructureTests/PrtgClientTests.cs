using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests
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
        [ExpectedException(typeof(PrtgRequestException))]
        public void PrtgClient_Constructor_CantRetrievePassHash()
        {
            var webClient = new MockWebClient(new PassHashResponse("PRTG Network Monitor is starting"));

            var client = new PrtgClient("prtg.example.com", "username", "password", AuthMode.Password, webClient);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PrtgClient_Constructor_ServerCannotBeNull()
        {
            var client = new PrtgClient(null, "username", "password");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PrtgClient_Constructor_UsernameCannotBeNull()
        {
            var client = new PrtgClient("prtg.example.com", null, "password");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PrtgClient_Constructor_PasswordCannotBeNull()
        {
            var client = new PrtgClient("prtg.example.com", "username", null);
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

            try
            {
                var sensors = client.StreamSensors().ToList();
                Assert.Fail("StreamSensors did not throw");
            }
            catch (WebException)
            {
            }

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

            try
            {
                var sensors = client.GetSensors().ToList();
                Assert.Fail("GetSensors did not throw");
            }
            catch (WebException)
            {
            }

            Assert.AreEqual(retriesToMake, prtgClientRetriesNormally, "An incorrect number of retries were made.");

            client.RetryRequest -= OnClientOnRetryRequest;

            try
            {
                var moreSensors = client.GetSensors().ToList();
            }
            catch (WebException)
            {
            }

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
                    missingAsync.Add(s);
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

            ExecuteFailedRequest(HttpStatusCode.OK, html.ToString(SaveOptions.DisableFormatting), null, "PRTG Network Monitor has discovered a problem. Your last request could not be processed properly. Error message: Sorry, the selected object cannot be used here.");
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

        private void ExecuteFailedRequest(HttpStatusCode statusCode, string xml, string address, string expectedError)
        {
            var response = new FailedRequestResponse(statusCode, xml, address);

            var client = new PrtgClient("prtg.example.com", "username", "1234567890", AuthMode.PassHash, new MockWebClient(response));

            try
            {
                client.GetSensors();
            }
            catch (PrtgRequestException ex)
            {
                if (ex.Message != $"PRTG was unable to complete the request. The server responded with the following error: {expectedError}")
                    Assert.Fail($"Exception did not contain expected error message. Expected: '{expectedError}'. Received: '{ex.Message}'");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void PrtgClient_HandlesTimeoutSocketException()
        {
            ExecuteSocketException(SocketError.TimedOut);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void PrtgClient_HandlesConnectionRefusedSocketException()
        {
            ExecuteSocketException(SocketError.ConnectionRefused);
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
    }
}
