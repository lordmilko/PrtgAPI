using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class BasePrtgClientTest
    {
        private static string PrtgConfig => $"\\\\{Settings.Server}\\c$\\ProgramData\\Paessler\\PRTG Network Monitor\\PRTG Configuration.dat";
        public static string PrtgConfigBackup => $"\\\\{Settings.Server}\\c$\\Users\\{Settings.WindowsUsername}\\AppData\\Local\\Temp\\PRTG Configuration.dat";

        private static bool initialized = false;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            initialized = false;

            Logger.Log($"Pinging {Settings.Server}");

            var sender = new Ping();
            var reply = sender.Send(Settings.Server);

            if (reply.Status != IPStatus.Success)
            {
                Assert2.Fail("Ping responded with" + reply.Status.ToString());
            }

            Logger.Log("Connecting to local server");

            Impersonator.ExecuteAction(RemoteInit);

            Logger.Log("Refreshing CI device");

            try
            {
                new BasePrtgClientTest().client.RefreshObject(Settings.Device);
            }
            catch(Exception ex)
            {
                Logger.Log($"Exception occurred while refreshing device: {ex.Message}", true);
                AssemblyCleanup();
                throw;
            }

            Logger.Log("Ready for tests");
        }

        private static void RemoteInit()
        {
            Logger.Log("Retrieving service details");

            var coreService = new ServiceController("PRTGCoreService", Settings.Server);
            var probeService = new ServiceController("PRTGProbeService", Settings.Server);

            if (coreService.Status != ServiceControllerStatus.Running)
                StartService(coreService, "Core Service");

            if (probeService.Status != ServiceControllerStatus.Running)
                StartService(probeService, "Probe Service");

            RepairState();

            if (Settings.ResetAfterTests)
            {
                if (File.Exists(PrtgConfigBackup))
                {
                    Logger.Log("Restoring PRTG Config leftover from previous aborted test");
                    Logger.LogTest("Stopping PRTGCoreService");
                    coreService.Stop();
                    coreService.WaitForStatus(ServiceControllerStatus.Stopped);

                    Logger.LogTest("Copying PRTG Config");
                    File.Copy(PrtgConfigBackup, PrtgConfig, true);
                    File.Delete(PrtgConfigBackup);

                    Logger.LogTest("Starting PRTGCoreService");
                    coreService.Start();
                    coreService.WaitForStatus(ServiceControllerStatus.Running);

                    Logger.LogTest("Sleeping for 30 seconds while PRTG starts up");
                    Thread.Sleep(30 * 1000);
                }

                Logger.Log("Backing up PRTG Config");

                File.Copy(PrtgConfig, PrtgConfigBackup);

                initialized = true;
            }
        }

        private static void StartService(ServiceController service, string friendlyName)
        {
            if (!File.Exists(PrtgConfigBackup))
            {
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    Logger.LogTest($"{service.ServiceName} is not running. Starting service");
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                    Logger.LogTest("Sleeping for 30 seconds while PRTG service starts");
                    Thread.Sleep(30 * 1000);
                }
                else
                    Assert2.Fail($"{friendlyName} is not running. Service status is {service.Status}", true);
            }
            else
                Logger.LogTest($"{service.ServiceName} is not running. Aborting restart as found leftover PRTG Config");
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            if (!initialized)
            {
                Logger.Log("Did not initialize properly; not cleaning up");
                return;
            }
                
            Logger.Log("Cleaning up after tests");

            if (Settings.ResetAfterTests)
            {
                Logger.Log("Connecting to server");

                Impersonator.ExecuteAction(() => RemoteCleanup(true));
            }
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

        private static void RemoteCleanup(bool deleteConfig)
        {
            RepairState();

            Logger.Log("Retrieving service details");

            var controller = new ServiceController("PRTGCoreService", Settings.Server);

            Logger.Log("Stopping service");

            controller.Stop();
            controller.WaitForStatus(ServiceControllerStatus.Stopped);

            Logger.Log("Restoring config");

            try
            {
                File.Copy(PrtgConfigBackup, PrtgConfig, true);

                if(deleteConfig)
                    File.Delete(PrtgConfigBackup);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, true);
                throw;
            }

            Logger.Log("Starting service");

            controller.Start();
            controller.WaitForStatus(ServiceControllerStatus.Running);

            Logger.Log("Finished");
        }

        private static void RepairState()
        {
            var client = new BasePrtgClientTest().client;

            if (client.GetProbes(Property.Id, Settings.Probe).First().Name != Settings.ProbeName)
            {
                Logger.Log("Restoring probe name");
                client.SetObjectProperty(Settings.Probe, ObjectProperty.Name, Settings.ProbeName);
            }
        }

        protected static void RepairConfig()
        {
            Logger.Log("Repairing config");

            Impersonator.ExecuteAction(() => RemoteCleanup(false));

            Logger.Log("Sleeping for 60 seconds while service starts up");
            Thread.Sleep(60 * 1000);
        }

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void ValidateSettings()
        {
            Logger.LogTest($"Running test '{TestContext.TestName}'");

            var properties = typeof (Settings).GetFields();

            //todo: this part should be in a separate function that is then called from the powershell init.ps1 in startup

            foreach (var property in properties)
            {
                var value = property.GetValue(null);

                Assert2.IsTrue(value != null && value.ToString() != "-1", $"Setting '{property.Name}' must be initialized before running tests. Please specify a value in file Settings.cs");
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
            Thread.Sleep(30000);
        }
    }
}
