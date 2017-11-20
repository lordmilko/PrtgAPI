using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Restarts the PRTG Probe Service of PRTG Network Monitor probes.</para>
    /// 
    /// <para type="description">The Restart-Probe cmdlet restarts the PRTG Probe Service of a specified PRTG Probe. If no probe is specified, Restart-Probe
    /// will restart the PRTG Probe Service of all PRTG Probes in your environment.</para>
    /// <para type="description">When executed, Restart-Probe will prompt to confirm you wish to restart the PRTG Probe Service of each PRTG Probe.
    /// Within this prompt you may respond to each probe individually or answer yes/no to all. To override this prompt completely, the -<see cref="Force"/>
    /// parameter can be specified.</para>
    /// <para type="description">By default, Restart-Probe will wait for one hour for all probes restart and reconnect to PRTG.
    /// If you do not wish to wait at all, this can be overridden by specifying -<see cref="Wait"/>:$false. You may additionally specify a custom
    /// timeout duration (in seconds) via the -<see cref="Timeout"/> parameter. If Restart-Probe times out waiting for a PRTG Probe Service to restart,
    /// a <see cref="TimeoutException"/> will be thrown specifying the number of probes that failed to restart.</para>
    /// 
    /// <example>
    ///     <code>C:\> Restart-Probe</code>
    ///     <para>Restart all probes on a PRTG Server and wait for them to restart.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe *contoso* | Restart-Probe -Timeout 180 -Force</code>
    ///     <para>Restart all probes containing "contoso" in their names, waiting 3 minutes for them to restart, without displaying a confirmation prompt.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>Get-Probe -Id 2004 | Restart-Probe -Wait:$false</code>
    ///     <para>Restart the probe with ID 2004, without waiting for the probe to restart.</para>
    /// </example>
    /// 
    /// <para type="link">Get-Probe</para>
    /// <para type="link">Restart-PrtgCore</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Restart, "Probe", SupportsShouldProcess = true)]
    public class RestartProbe : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The probe to restart. If no probe is specified, all probes will be restarted.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Probe Probe { get; set; }

        /// <summary>
        /// <para type="description">Forces the PRTG Probe Service to be restarted on all specified probes without displaying a confirmation prompt.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// <para type="description">Wait for the PRTG Core Service to restart before ending the cmdlet. By default this value is true.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Wait { get; set; } = SwitchParameter.Present;

        /// <summary>
        /// <para type="description">Duration (in seconds) to wait for the PRTG Probe Service of all probes to restart. Default value is 3600 (1 hour). If <see cref="Wait"/> is false, this parameter will have no effect.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int Timeout { get; set; } = 3600;

        private bool yesToAll;
        private bool noToAll;

        private DateTime? restartTime;

        private int secondsRemaining = 150;
        private int secondsElapsed;

        private List<Probe> probesRestarted = new List<Probe>();

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (Probe == null)
            {
                if (ShouldProcess("All PRTG Probes"))
                {
                    var count = client.GetTotalObjects(Content.ProbeNode);
                    if (Force || yesToAll || ShouldContinue($"Are you want to restart the PRTG Probe Service on all {count} PRTG Probes?", "WARNING", true, ref yesToAll, ref noToAll))
                    {
                        restartTime = DateTime.Now;
                        client.RestartProbe(null);
                    }
                }
            }
            else
            {
                if (ShouldProcess($"'{Probe.Name}' (ID: {Probe.Id})"))
                {
                    if (Force || yesToAll || ShouldContinue($"Are you sure you want to restart the PRTG Probe Service on probe '{Probe.Name}' (ID: {Probe.Id})?", "WARNING", true, ref yesToAll, ref noToAll))
                    {
                        restartTime = DateTime.Now;
                        probesRestarted.Add(Probe);
                        client.RestartProbe(Probe.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Provides a one-time, postprocessing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
            if (Wait && restartTime != null)
            {
                if (Probe == null)
                    GetProgressForAllProbes();
                else
                    GetProgressForRestartedProbes(probesRestarted);
            }
        }

        private void GetProgressForAllProbes()
        {
            var probes = client.GetProbes();

            GetProgressForRestartedProbes(probes);
        }

        private void GetProgressForRestartedProbes(List<Probe> probes)
        {
            List<RestartedProbe> probeStatuses = probes.Select(p => new RestartedProbe(p)).ToList();

            while (probeStatuses.Any(p => p.Reconnected == false))
            {
                //Get all logs relating to probes connecting and disconnecting since we initiated the restarts.
                //If we've already detected all probes have disconnected, no need to include those logs in the response
                var statuses = new List<LogStatus> { LogStatus.Connected };

                if (probeStatuses.Any(p => !p.Disconnected))
                    statuses.Add(LogStatus.Disconnected);

                var logs = client.GetLogs(null, endDate: restartTime, status: statuses.ToArray());

                UpdateProbeStatus(probeStatuses, logs);

                WriteProbeProgress(probeStatuses);
            }
        }

        private void UpdateProbeStatus(List<RestartedProbe> probes, List<Log> logs)
        {
            foreach (var probe in probes)
            {
                if (!probe.Disconnected)
                {
                    //If we got a log saying the probe disconnected, or the probe was already disconnected, flag it as having disconnected
                    if (logs.Any(log => log.Status == LogStatus.Disconnected && log.Id == probe.Id) || probe.Condition == ProbeStatus.Disconnected)
                        probe.Disconnected = true;
                }
                if (probe.Disconnected && !probe.Reconnected) //If it's already disconnected and hasn't reconnected, check its status
                {
                    //If the probe has disconnected and we see it's reconnected, flag it as such. If it was already disconnected though,
                    //it'll never reconnect, so let it through
                    if (logs.Any(log => log.Status == LogStatus.Connected && log.Id == probe.Id) || probe.Condition == ProbeStatus.Disconnected)
                        probe.Reconnected = true;
                }
            }
        }

        private void WriteProbeProgress(List<RestartedProbe> probeStatuses)
        {
            var completed = probeStatuses.Count(p => p.Reconnected) + 1;

            if (completed > probeStatuses.Count)
                completed--;

            var completedPercent = completed / probeStatuses.Count * 100;

            var record = new ProgressRecord(1, "Restart PRTG Probes", $"Restarting all probes {completed}/{probeStatuses.Count}")
            {
                PercentComplete = completedPercent,
                SecondsRemaining = secondsRemaining,
                CurrentOperation = "Waiting for all probes to restart"
            };

            for (int i = 0; i < 5; i++)
            {
                WriteProgress(record);

                secondsRemaining--;
                secondsElapsed++;

                if (secondsRemaining < 0)
                    secondsRemaining = 30;

                record.SecondsRemaining = secondsRemaining;

                if (secondsElapsed > Timeout)
                {
                    var remaining = probeStatuses.Count - completed + 1;

                    var plural = remaining == 1 ? "probe" : "probes";

                    throw new TimeoutException($"Timed out waiting for {remaining} {plural} to restart");
                }

#if DEBUG
                if(!UnitTest())
                    Thread.Sleep(1000);
#else
                Thread.Sleep(1000);
#endif
            }
        }

        class RestartedProbe
        {
            private Probe probe { get; set; }

            public int Id => probe.Id;

            public ProbeStatus Condition => probe.Condition;

            public bool Disconnected { get; set; }

            public bool Reconnected { get; set; }

            public RestartedProbe(Probe probe)
            {
                this.probe = probe;
            }
        }
    }
}
