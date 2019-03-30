using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    public class Test
    {
    }

    //todo: implement parallel test execution support

    public class TestContextEx
    {
        public Queue<Test> Tests { get; set; }
    }

    [TestClass]
    public class BasePrtgClientTest
    {
        public static ServerManager ServerManager = new ServerManager(() => new BasePrtgClientTest().client);

        private PrtgClient cachedClient;
        private PrtgClient cachedReadOnlyClient;

        protected PrtgClient client
        {
            get
            {
                if (cachedClient == null)
                    cachedClient = new PrtgClient(Settings.ServerWithProto, Settings.UserName, Settings.Password);

                return cachedClient;
            }
        }

        protected PrtgClient readOnlyClient
        {
            get
            {
                if (cachedReadOnlyClient == null)
                    cachedReadOnlyClient = new PrtgClient(Settings.ServerWithProto, Settings.ReadOnlyUserName, Settings.ReadOnlyPassword);

                return cachedReadOnlyClient;
            }
        }

        public TestContext TestContext { get; set; }

        public TestContextEx TestContextEx { get; set; }

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

        [TestInitialize]
        public void TestInitialize()
        {
            ServerManager.WaitForSensors(Status.None);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (AssertEx.HadFailure) //We've already logged that we failed; no need to log it again
            {
                try
                {
                    var groups = client.GetSensors().OrderBy(s => s.Id).GroupBy(s => s.Status);

                    Logger.LogTestDetail("Sensor statuses:");

                    foreach (var group in groups)
                    {
                        Logger.LogTestDetail($"    {group.Key}: " + string.Join(",", group));
                    }
                }
                catch
                {
                }

                AssertEx.HadFailure = false;
            }
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

            AssertEx.IsTrue(results.Any(), "Response did not contain any results");
        }

        protected void ReturnsJustObjectsOfType<T>(Func<Property, object, List<T>> method, int parentId, int expectedCount, BaseType baseType) where T : SensorOrDeviceOrGroupOrProbe
        {
            var results = method(Property.ParentId, parentId);

            AssertEx.AreEqual(expectedCount, results.Count, "Expected number of results was incorrect");
            var different = results.Where(r => r.BaseType != baseType).ToList();
            AssertEx.IsTrue(!different.Any(), $"One or more objects had a type other than {baseType}: {string.Join(", ", different)}");
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
