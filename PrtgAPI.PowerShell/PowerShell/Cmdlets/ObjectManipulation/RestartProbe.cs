using System;
using System.Collections.Generic;
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
    ///     <code>C:\> Get-Probe -Id 2004 | Restart-Probe -Wait:$false</code>
    ///     <para>Restart the probe with ID 2004, without waiting for the probe to restart.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Administrative-Tools#restart-probe-service-1">Online version:</para>
    /// <para type="link">Get-Probe</para>
    /// <para type="link">Restart-PrtgCore</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Restart, "Probe", SupportsShouldProcess = true)]
    public class RestartProbe : PrtgPostProcessCmdlet, IPrtgMultiPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">The probe to restart. If no probe is specified, all probes will be restarted.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Probe Probe { get; set; }

        /// <summary>
        /// <para type="description">Specifies whether to return the original <see cref="IObject"/> that was passed to this cmdlet, allowing the object to be further piped into additional cmdlets.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

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

        internal override string ProgressActivity => "Restart PRTG Probes";

        private bool allProbesRestarted;

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (Probe == null)
            {
                var processMessage = "All PRTG Probes";
                var count = client.GetTotalObjects(Content.Probes);
                var continueMessage = $"Are you want to restart the PRTG Probe Service on all {count} PRTG Probes?";

                Restart(processMessage, continueMessage, null, null);
            }
            else
            {
                var processMessage = $"'{Probe.Name}' (ID: {Probe.Id})";
                var continueMessage = $"Are you sure you want to restart the PRTG Probe Service on probe '{Probe.Name}' (ID: {Probe.Id})?";
                var progressMessage = $"Restarting probe '{Probe.Name}'";

                Restart(processMessage, continueMessage, progressMessage, Probe);
            }
        }

        private void Restart(string processMessage, string continueMessage, string progressMessage, Probe probe)
        {
            if (ShouldProcess(processMessage))
            {
                if (Force || yesToAll || ShouldContinue(continueMessage, "WARNING", true, ref yesToAll, ref noToAll))
                {
                    if (Wait)
                        restartTime = client.GetStatus().DateTime;

                    if (probe != null)
                        probesRestarted.Add(probe);

                    ExecuteOperation(() => client.RestartProbe(probe == null ? null : new[] {probe.Id}, token: CancellationToken), progressMessage, !Wait);
                }
            }
        }

        /// <summary>
        /// Provides an enhanced one-time, postprocessing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessingEx()
        {
            if (Wait && restartTime != null)
            {
                probesRestarted = Probe == null ? client.GetProbes() : probesRestarted;

                client.WaitForProbeRestart(restartTime.Value, probesRestarted, WriteProbeProgress, CancellationToken);

                allProbesRestarted = true;
                WriteMultiPassThru();
            }
        }

        private bool WriteProbeProgress(ProbeRestartProgress[] probeStatuses)
        {
            var completed = probeStatuses.Count(p => p.Reconnected) + 1;

            var complete = false;

            if (completed > probeStatuses.Length)
            {
                completed--;
                complete = true;
            }

            var statusDescription = $"Restarting all probes {completed}/{probeStatuses.Length}";
            var completedPercent = (int) (completed/(double) probeStatuses.Length * 100);
            var currentOperation = "Waiting for all probes to restart";

            DisplayPostProcessProgress(ProgressActivity, statusDescription, completedPercent, secondsRemaining, currentOperation, complete);

            if (complete)
                return false;

            for (int i = 0; i < 5; i++)
            {
                secondsRemaining--;
                secondsElapsed++;

                if (secondsRemaining < 0)
                    secondsRemaining = 30;

                ProgressManager.CurrentRecord.SecondsRemaining = secondsRemaining;

                if (secondsElapsed > Timeout)
                {
                    var remaining = probeStatuses.Length - completed + 1;

                    var plural = remaining == 1 ? "probe" : "probes";

                    throw new TimeoutException($"Timed out waiting for {remaining} {plural} to restart.");
                }

                if (Stopping)
                    return false;

#if DEBUG
                if (!client.UnitTest())
#endif
                Sleep(1000);


                ProgressManager.WriteProgress(true);
            }

            return true;
        }

        /// <summary>
        /// Writes the current <see cref="PassThruObject"/> to the pipeline if <see cref="PassThru"/> is specified.
        /// </summary>
        public void WritePassThru()
        {
            if (PassThru)
            {
                if (Wait && !allProbesRestarted)
                    return;
                
                WriteObject(PassThruObject);
            }
        }

        /// <summary>
        /// Whether this cmdlet will execute its post processing operation.
        /// </summary>
        protected override bool ShouldPostProcess() => Wait;

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public object PassThruObject => Probe;

        /// <summary>
        /// The objects that should be output from the cmdlet.
        /// </summary>
        public List<object> PassThruObjects => probesRestarted.Cast<object>().ToList();

        /// <summary>
        /// Stores the last object that was output from the cmdlet.
        /// </summary>
        public object CurrentMultiPassThru { get; set; }

        /// <summary>
        /// Writes all objects stored in <see cref="PassThruObjects"/> if <see cref="IPrtgPassThruCmdlet.PassThru"/> is specified.
        /// </summary>
        public void WriteMultiPassThru()
        {
            if (PassThru)
            {
                if (Wait && !allProbesRestarted)
                    return;

                foreach (var o in probesRestarted)
                {
                    CurrentMultiPassThru = o;
                    WriteObject(o);
                }
            }
        }
    }
}
