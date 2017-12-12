using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class BasePrtgClientTest
    {
        public static ServerManager ServerManager = new ServerManager(() => new BasePrtgClientTest().client);

        protected PrtgClient client => new PrtgClient(Settings.ServerWithProto, Settings.UserName, Settings.Password);

        public TestContext TestContext { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            ServerManager.Initialize();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            ServerManager.Cleanup();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Assert2.HadFailure) //We've already logged that we failed; no need to log it again
                Assert2.HadFailure = false;
            else
            {
                if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
                    Logger.LogTestDetail($"Test completed with outcome '{TestContext.CurrentTestOutcome}'", true);
            }
        }

        [TestInitialize]
        public void ValidateSettings()
        {
            Logger.LogTest($"Running test '{TestContext.TestName}'");

            ServerManager.ValidateSettings();
        }

        protected void HasAnyResults<T>(Func<List<T>> method)
        {
            var results = method();

            Assert2.IsTrue(results.Any(), "Response did not contain any results");
        }

        protected void ReturnsJustObjectsOfType<T>(Func<Property, object, List<T>> method, int parentId, int expectedCount, BaseType baseType) where T : SensorOrDeviceOrGroupOrProbe
        {
            var results = method(Property.ParentId, parentId);

            Assert2.AreEqual(expectedCount, results.Count, "Expected number of results was incorrect");
            var different = results.Where(r => r.BaseType != baseType).ToList();
            Assert2.IsTrue(!different.Any(), $"One or more objects had a type other than {baseType}: {string.Join(", ", different)}");
        }

        protected void CheckAndSleep(int objectId)
        {
            client.RefreshObject(objectId);
            Logger.LogTestDetail("Refreshed object; sleeping for 30 seconds");
            Thread.Sleep(10000);
            client.RefreshObject(objectId);
            Thread.Sleep(10000);
            client.RefreshObject(objectId);
            Thread.Sleep(10000);
            client.RefreshObject(objectId);
        }
    }
}
