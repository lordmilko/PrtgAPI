using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;
using PrtgAPI.Tests.UnitTests.ObjectTests.Responses;

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

            var client = new PrtgClient("prtg.example.com", "username", "passhash", AuthMode.PassHash, new MockRetryWebClient(response));

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

                var asyncName = fullName.Replace(s.Name, $"{s.Name}Async");

                if (!methodFullNames.Contains(asyncName))
                    missingAsync.Add(s);
            }

            Assert.AreEqual(0, missingAsync.Count, $"Async counterparts of the following methods are missing: {string.Join(", ", missingAsync.Select(m => m.GetInternalProperty("FullName").ToString().Substring("PrtgAPI.PrtgClient.".Length)))}");
        }
    }
}
