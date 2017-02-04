using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class BasePrtgClientTest
    {
        private static string PrtgConfig => $"\\\\{Settings.Server}\\c$\\ProgramData\\Paessler\\PRTG Network Monitor\\PRTG Configuration.dat";
        public static string PrtgConfigBackup => $"\\\\{Settings.Server}\\c$\\Users\\{Settings.WindowsUsername}\\AppData\\Local\\Temp\\PRTG Configuration.dat";

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            var sender = new Ping();
            var reply = sender.Send(Settings.Server);

            if (reply.Status != IPStatus.Success)
            {
                Assert.Fail("Ping responded with" + reply.Status.ToString());
            }

            Impersonator.ExecuteAction(() =>
            {
                var coreService = new ServiceController("PRTGCoreService", Settings.Server);
                var probeService = new ServiceController("PRTGProbeService", Settings.Server);

                if (coreService.Status != ServiceControllerStatus.Running)
                    Assert.Fail("Core Service is not running");

                if (probeService.Status != ServiceControllerStatus.Running)
                    Assert.Fail("Probe Service is not running");

                if (Settings.ResetAfterTests)
                    File.Copy(PrtgConfig, PrtgConfigBackup);
            });

            new BasePrtgClientTest().client.CheckNow(Settings.Device);
        }

        [AssemblyCleanup]
        public static void AssembyCleanup()
        {
            if (Settings.ResetAfterTests)
            {
                Impersonator.ExecuteAction(() =>
                {
                    var controller = new ServiceController("PRTGCoreService", Settings.Server);

                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped);

                    File.Copy(PrtgConfigBackup, PrtgConfig, true);
                    File.Delete(PrtgConfigBackup);

                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running);
                });
            }
        }

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void ValidateSettings()
        {
            var properties = typeof (Settings).GetFields();

            foreach (var property in properties)
            {
                var value = property.GetValue(null);

                Assert.IsTrue(value != null && value.ToString() != "-1", $"Setting '{property.Name}' must be initialized before running tests. Please specify a value in file Settings.cs");
            }
        }

        protected PrtgClient client
        {
            get
            {
                if (Settings.Protocol == null)
                    throw new Exception("Please specify either HTTP or HTTPS in file Settings.cs");

                return new PrtgClient(Settings.ServerWithProto, Settings.Username, Settings.Password);
            }
        }

        protected void HasAnyResults<T>(Func<List<T>> method)
        {
            var results = method();

            Assert.IsTrue(results.Any());
        }

        protected void ReturnsJustObjectsOfType<T>(Func<Property, object, List<T>> method, int parentId, int expectedCount, BaseType baseType) where T : SensorOrDeviceOrGroupOrProbe
        {
            var results = method(Property.ParentId, parentId);

            Assert.AreEqual(expectedCount, results.Count, "Expected number of results");
            Assert.IsTrue(results.Count(r => r.BaseType != baseType) == 0);
        }

        protected void CheckAndSleep(int objectId)
        {
            client.CheckNow(objectId);
            Thread.Sleep(30000);
        }
    }
}
