using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Threading;

namespace PrtgAPI.Tests.IntegrationTests
{
    /// <summary>
    /// Manages connections to a PRTG Continuous Integration Server, including service, state and configuration mangement.
    /// </summary>
    public class ServerManager
    {
        private static string PrtgConfigFolder = $"\\\\{Settings.Server}\\c$\\ProgramData\\Paessler\\PRTG Network Monitor";

        private static string PrtgConfig => $"{PrtgConfigFolder}\\PRTG Configuration.dat";

        public static string PrtgConfigBackup => $"\\\\{Settings.Server}\\c$\\Users\\{Settings.WindowsUserName}\\AppData\\Local\\Temp\\PRTG Configuration.dat";

        internal static string PrtgSystemInformationDatabase = $"{PrtgConfigFolder}\\System Information Database";

        public bool Initialized { get; set; }

        private Func<PrtgClient> getClient;

        internal static bool IsEnglish(PrtgClient client, bool defaultValue = true)
        {
            string language;

            if (client.GetObjectPropertiesRaw(810).TryGetValue("languagefile", out language))
            {
                return language == "english.lng";
            }

            return defaultValue;
        }

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
                AssertEx.Fail("Ping responded with " + reply.Status);
            }
        }

        public void WaitForObjects()
        {
            WaitForSensors();
            WaitForProbes();

            RepairState();
        }

        private void WaitForSensors()
        {
            WaitForSensors(Status.Unknown, Status.None);

            try
            {
                WaitForSensors(Status.Down);
                WaitForSensor(Settings.WarningSensor, Status.Warning);
            }
            catch
            {
                RepairConfig();
                WaitForObjects();
            }
        }

        public void WaitForSensor(int id, Status desiredStatus)
        {
            var sensor = Client.GetSensor(id);

            if (sensor.Status == desiredStatus)
            {
                Logger.LogTest($"Sensor with ID {id} was already status '{desiredStatus}'");
                return;
            }

            Logger.LogTest($"Waiting for sensor with ID {id} to become '{desiredStatus}'");

            for(var i = 0; i < 10; i++)
            {
                Logger.LogTestDetail("Refreshing sensor and sleeping 5 seconds");
                Client.RefreshObject(id);
                Thread.Sleep(5000);

                sensor = Client.GetSensor(id);

                if (sensor.Status == desiredStatus)
                    return;
            }

            AssertEx.Fail($"Sensor ID {id} failed to return to status '{desiredStatus}'");
        }

        internal void WaitForSensors(params Status[] status)
        {
            List<Sensor> sensors;

            var attempts = 15;

            var downSensor = Client.GetSensor(Settings.DownSensor);

            if (downSensor.Status == Status.DownAcknowledged)
            {
                Logger.LogTest("Down sensor is still acknowledged. Pausing sensor");
                Client.PauseObject(Settings.DownSensor);
                Client.RefreshObject(Settings.DownSensor);

                Logger.LogTest("Unpausing sensor");
                Client.ResumeObject(Settings.DownSensor);
                Logger.LogTest("Waiting for sensor to go down");
                WaitForSensors(Status.DownAcknowledged);
                Logger.LogTest("Sensor successfully went down");
            }

            do
            {
                sensors = Client.GetSensors(status);

                if (sensors.Count > 1)
                {
                    Logger.LogTest($"Sensors are still initializing ({sensors.Count}x " + string.Join("/", status)  + "). Sleeping for 30 seconds");

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

            var coreService = GetCoreService();

            Logger.Log("Stopping service");

            coreService.Stop();
            WaitForStop(coreService);

            Logger.Log($"Restoring config. Delete config backup: {deleteConfig}");

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

            coreService.Start();
            coreService.WaitForStatus(ServiceControllerStatus.Running);

            if (probeNameNeedsRepairing)
            {
                Logger.Log("Previous repair indicated probe name still needs renaming. Waiting 60 seconds for PRTG to start");

                Thread.Sleep(60000);

                RepairState();
            }
            else
            {
                Logger.Log("Restarting probe service");
                var probeService = GetProbeService();
                probeService.Stop();
                WaitForStop(probeService);

                if (probeService.Status != ServiceControllerStatus.Stopped)
                    Thread.Sleep(5000);

                probeService.Start();
                probeService.WaitForStatus(ServiceControllerStatus.Running);
            }

            Logger.Log("Finished");
        }

        private void RestorePrtgConfig(bool deleteConfig)
        {
            if (!Initialized)
            {
                Logger.Log("Cannot restore PRTG config as test never initialized");
                return;
            }

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

        private static void KillService(string service)
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

        public void StartServices(bool restoreConfig = true)
        {
            Impersonator.ExecuteAction(() =>
            {
                var coreService = GetCoreService();
                var probeService = GetProbeService();

                if (coreService.Status != ServiceControllerStatus.Running)
                    StartService(coreService, "Core Service", restoreConfig);

                if (probeService.Status != ServiceControllerStatus.Running)
                    StartService(probeService, "Probe Service", restoreConfig);
            });
        }

        private void StartService(ServiceController service, string friendlyName, bool restoreConfig = true)
        {
            if (File.Exists(PrtgConfigBackup) && restoreConfig)
            {
                Logger.LogTest($"Restoring leftover PRTG config");
                RestorePrtgConfig(true);
            }

            if (service.Status == ServiceControllerStatus.StopPending || service.Status == ServiceControllerStatus.StartPending)
            {
                var op = service.Status == ServiceControllerStatus.StopPending ? "stopping" : "starting";

                Logger.LogTest($"{friendlyName} is stuck {op}. Killing service");
                KillService(service.ServiceName);

                service.Refresh();
            }

            if (service.Status == ServiceControllerStatus.Stopped)
            {
                Logger.LogTest($"{service.ServiceName} is not running. Starting service");
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);

                Logger.LogTest("Sleeping for 30 seconds while PRTG service starts");
                Thread.Sleep(30 * 1000);
            }
            else
            {
                var message = $"{friendlyName} did not stop after killing service. Service status is {service.Status}";

                if (service.Status == ServiceControllerStatus.StartPending)
                {
                    Logger.LogTest("Sleeping for 30 seconds while PRTG service starts");
                    Thread.Sleep(30 * 1000);

                    if (service.Status == ServiceControllerStatus.StartPending)
                    {
                        AssertEx.Fail($"Service did not leave status {service.Status}. Giving up");
                    }
                }
                else
                    AssertEx.Fail(message);
            }
        }

        public void RepairState()
        {
            try
            {
                Logger.Log("Checking probe health");
                Logger.LogTest("Validating probe can be retrieved by ID");

                if (Client.GetProbes(Property.Id, Settings.Probe).First().Name != Settings.ProbeName)
                {
                    Logger.LogTestDetail("Probe name was incorrect. Requires rename");
                    RestoreProbeName();
                }
                else
                {
                    probeNameNeedsRepairing = false;

                    Logger.LogTest("Validating probe can be retrieved by name");

                    if (Client.GetProbes(Property.Name, Settings.ProbeName).Count == 0)
                    {
                        Logger.LogTestDetail("Probe could not be retrieved by name; previous rename may have failed. Re-attempting rename");
                        RestoreProbeName();

                        Logger.LogTestDetail("Sleeping for 30 seconds so name change can properly apply");
                        Thread.Sleep(30000);
                    }
                }

                Logger.Log("State was successfully repaired");
            }
            catch (Exception ex)
            {
                //If we're not English, lets assume this is the issue
                if (ex.Message.Contains("probe is not connected") || !IsEnglish(Client))
                {
                    Logger.Log("Cannot repair state as probe is not connected yet. Sleeping for 5 seconds");
                    Thread.Sleep(5000);

                    RepairState();
                }
                else
                {
                    Logger.Log(ex.Message, true);
                    throw;
                }
            }
        }

        private void RestoreProbeName()
        {
            Logger.Log("Restoring probe name");
            Client.RenameObject(Settings.Probe, Settings.ProbeName);
            Client.SetObjectProperty(Settings.Probe, ObjectProperty.Name, Settings.ProbeName);

            var probe = Client.GetProbes(Property.Name, Settings.ProbeName);

            if (probe.Count == 0)
            {
                Logger.Log("Probe name didn't stick. Restoring probe name again");
                Client.RenameObject(Settings.Probe, Settings.ProbeName);
                Client.SetObjectProperty(Settings.Probe, ObjectProperty.Name, Settings.ProbeName);

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

                AssertEx.IsTrue(value != null && value.ToString() != "-1", $"Setting '{property.Name}' must be initialized before running tests. Please specify a value in file Settings.cs");
            }
        }

        #endregion
    }
}
