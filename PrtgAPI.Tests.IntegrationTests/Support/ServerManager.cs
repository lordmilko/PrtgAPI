using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.IntegrationTests
{
    public class ServerManager
    {
        private static string PrtgConfig => $"\\\\{Settings.Server}\\c$\\ProgramData\\Paessler\\PRTG Network Monitor\\PRTG Configuration.dat";

        public static string PrtgConfigBackup => $"\\\\{Settings.Server}\\c$\\Users\\{Settings.WindowsUserName}\\AppData\\Local\\Temp\\PRTG Configuration.dat";

        public bool Initialized { get; set; }

        private Func<PrtgClient> getClient;

        private PrtgClient Client
        {
            get
            {
                try
                {
                    return getClient();
                }
                catch (Exception ex)
                {
                    StartServices();

                    if (ex.Message.Contains("Service Unavailable"))
                    {
                        Logger.Log("PRTG is in the process of starting up. Sleeping for 30 seconds");
                        Thread.Sleep(30000);
                    }
                    else if (ex.Message.Contains("not a socket"))
                    {
                        Logger.Log($"{ex.Message}. Sleeping for 30 seconds");
                        Thread.Sleep(30000);
                    }

                    return getClient();
                }
            }
        }

        private static bool probeNameNeedsRepairing;

        public ServerManager(Func<PrtgClient> client)
        {
            getClient = client;
        }

        #region Startup

        public void Initialize()
        {
            Initialized = false;

            PingServer();

            Logger.Log("Connecting to local server");
            Impersonator.ExecuteAction(RemoteInit);

            Logger.Log("Refreshing CI device");

            try
            {
                Client.RefreshObject(Settings.Device);

                WaitForObjects();
            }
            catch (Exception ex)
            {
                Logger.Log($"Exception occurred while refreshing device: {ex.Message}", true);
                Cleanup();
                throw;
            }

            Logger.Log("Ready for tests");
        }

        private void PingServer()
        {
            Logger.Log($"Pinging {Settings.Server}");

            var sender = new Ping();
            var reply = sender.Send(Settings.Server);

            if (reply.Status != IPStatus.Success)
            {
                Assert2.Fail("Ping responded with " + reply.Status.ToString());
            }
        }

        public void WaitForObjects()
        {
            WaitForSensors();
            WaitForProbes();
        }

        private void WaitForSensors()
        {
            WaitForSensors(Status.Unknown, Status.None);
            WaitForSensors(Status.Down);
        }

        private void WaitForSensors(params Status[] status)
        {
            List<Sensor> sensors;

            var attempts = 15;

            do
            {
                sensors = Client.GetSensors(status);

                if (sensors.Count > 1)
                {
                    Logger.LogTest("Sensors are still initializing. Sleeping for 30 seconds");

                    attempts--;

                    if (attempts == 0)
                    {
                        throw new Exception("Sensors did not initialize");
                    }

                    Thread.Sleep(30000);
                }
            } while (sensors.Count > 1);
        }

        private void WaitForProbes()
        {
            List<Probe> probes;

            var attempts = 15;

            do
            {
                probes = Client.GetProbes(Property.Condition, ProbeStatus.Disconnected);

                if (probes.Count > 0)
                {
                    Logger.LogTest("Probes are still initializing. Sleeping for 30 seconds");

                    attempts--;

                    if (attempts == 0)
                        throw new Exception("Probes did not initialize");

                    Thread.Sleep(30000);
                }
            } while (probes.Count > 0);
        }

        private void RemoteInit()
        {
            Logger.Log("Retrieving service details");

            var coreService = GetCoreService();
            var probeService = GetProbeService();

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

                Initialized = true;
            }
        }

        #endregion
        #region Shutdown

        public void Cleanup()
        {
            if (!Initialized)
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

        private void RemoteCleanup(bool deleteConfig)
        {
            RepairState();

            Logger.Log("Retrieving service details");

            var controller = new ServiceController("PRTGCoreService", Settings.Server);

            Logger.Log("Stopping service");

            controller.Stop();
            WaitForStop(controller);

            Logger.Log("Restoring config");

            try
            {
                RestorePrtgConfig(deleteConfig);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, true);
                throw;
            }

            Logger.Log("Starting service");

            controller.Start();
            controller.WaitForStatus(ServiceControllerStatus.Running);

            if (probeNameNeedsRepairing)
            {
                Logger.Log("Previous repair indicated probe name still needs renaming. Waiting 60 seconds for PRTG to start");

                Thread.Sleep(60000);

                RepairState();
            }

            Logger.Log("Finished");
        }

        private void RestorePrtgConfig(bool deleteConfig)
        {
            File.Copy(PrtgConfigBackup, PrtgConfig, true);

            if (deleteConfig)
                File.Delete(PrtgConfigBackup);
        }

        private void WaitForStop(ServiceController service)
        {
            try
            {
                service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 2, 0));
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                Logger.LogTest($"{service.ServiceName} is taking too long to stop. Terminating service");
                KillService(service.ServiceName);
            }
        }

        private static void KillService(string service) //todo: make it private
        {
            var options = new ConnectionOptions
            {
                Username = Settings.WindowsUserName,
                Password = Settings.WindowsPassword
            };

            Logger.LogTestDetail("Connecting with WMI");
            var scope = new ManagementScope($@"\\{Settings.Server}\root\cimv2", options);
            scope.Connect();

            var serviceQuery = $"SELECT ProcessId FROM Win32_Service WHERE name='{service}'";
            var processQuery = "SELECT * FROM Win32_Process WHERE ProcessId = ";

            Logger.LogTestDetail("Resolving process ID");

            ExecuteQuery(scope, serviceQuery, obj =>
            {
                var pid = Convert.ToInt32(obj["ProcessId"]);

                ExecuteQuery(scope, $"{processQuery}{pid}", process =>
                {
                    Logger.LogTestDetail($"Terminating process with PID {pid}");
                    process.InvokeMethod("Terminate", null);
                });
            });
        }

        private static void ExecuteQuery(ManagementScope scope, string queryStr, Action<ManagementObject> action)
        {
            var query = new SelectQuery(queryStr);

            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                foreach (var obj in searcher.Get())
                {
                    if (obj is ManagementObject)
                        action((ManagementObject)obj);
                }
            }
        }

        #endregion
        #region Shared

        private ServiceController GetCoreService() => new ServiceController("PRTGCoreService", Settings.Server);

        private ServiceController GetProbeService() => new ServiceController("PRTGProbeService", Settings.Server);

        public void StartServices()
        {
            Impersonator.ExecuteAction(() =>
            {
                var coreService = GetCoreService();
                var probeService = GetProbeService();

                if (coreService.Status != ServiceControllerStatus.Running)
                    StartService(coreService, "Core Service");

                if (probeService.Status != ServiceControllerStatus.Running)
                    StartService(probeService, "Probe Service");
            });
        }

        private void StartService(ServiceController service, string friendlyName)
        {
            if (File.Exists(PrtgConfigBackup))
            {
                Logger.LogTest($"Restoring leftover PRTG config");
                RestorePrtgConfig(true);
            }

            if (service.Status == ServiceControllerStatus.StopPending)
            {
                Logger.LogTest($"{friendlyName} is stuck stopping. Killing service");
                KillService(service.ServiceName);

                service.Refresh();
            }

            if (service.Status == ServiceControllerStatus.Stopped)
            {
                Logger.LogTest($"{service.ServiceName} is not running. Starting service", true);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);

                Logger.LogTest("Sleeping for 30 seconds while PRTG service starts");
                Thread.Sleep(30 * 1000);
            }
            else
                Assert2.Fail($"{friendlyName} is not running. Service status is {service.Status}", true);
        }

        public void RepairState()
        {
            try
            {
                if (Client.GetProbes(Property.Id, Settings.Probe).First().Name != Settings.ProbeName)
                {
                    RestoreProbeName();
                }
                else
                {
                    probeNameNeedsRepairing = false;

                    if (Client.GetProbes(Property.Name, Settings.ProbeName).Count == 0)
                    {
                        Logger.Log("Probe could not be retrieved by name; previous rename may have failed. Re-attempting rename");
                        RestoreProbeName();

                        Logger.Log("Sleeping for 30 seconds so name change can properly apply");
                        Thread.Sleep(30000);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, true);
                throw;
            }
        }

        private void RestoreProbeName()
        {
            Logger.Log("Restoring probe name");
            Client.RenameObject(Settings.Probe, Settings.ProbeName);

            var probe = Client.GetProbes(Property.Name, Settings.ProbeName);

            if (probe.Count == 0)
            {
                Logger.Log("Probe name didn't stick. Restoring probe name again");
                Client.RenameObject(Settings.Probe, Settings.ProbeName);

                probe = Client.GetProbes(Property.Name, Settings.ProbeName);

                if (probe.Count == 0)
                {
                    Logger.Log("Probe name still didn't stick. Will try again after service restarts");

                    //todo: what if we're the retry after the service restarts

                    probeNameNeedsRepairing = true;
                }
            }
        }

        public void RepairConfig()
        {
            Logger.Log("Repairing config");

            Impersonator.ExecuteAction(() => RemoteCleanup(false));

            Logger.Log("Sleeping for 60 seconds while service starts up");
            Thread.Sleep(60 * 1000);
        }

        public void ValidateSettings()
        {
            var properties = typeof(Settings).GetFields();

            foreach (var property in properties)
            {
                var value = property.GetValue(null);

                Assert2.IsTrue(value != null && value.ToString() != "-1", $"Setting '{property.Name}' must be initialized before running tests. Please specify a value in file Settings.cs");
            }
        }

        #endregion
    }
}
