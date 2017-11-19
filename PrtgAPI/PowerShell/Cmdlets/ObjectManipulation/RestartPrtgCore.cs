using System;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Threading;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Restarts the PRTG Core Service of a PRTG Network Monitor server.</para>
    /// 
    /// <para type="description">The Restart-PrtgCore cmdlet restarts the PRTG Core Service of a PRTG Network Monitor Server.
    /// Upon restarting the service, all monitoring will cease functioning and all users will be completely disconnected.
    /// If PRTG is in a cluster setup, this cmdlet will only restart the PRTG Core Service of the server PrtgAPI is connected to.</para>
    /// 
    /// <para type="description">When executed, Restart-PrtgCore will prompt to confirm you wish to restart the PRTG Core Service. To override this prompt, the -<see cref="Force"/>
    /// parameter can be specified.By default, Restart-PrtgCore will one hour for the PRTG Core Service to completely restart and come back online. If you do not wish
    /// to wait at all, this can be overridden by specifying -<see cref="Wait" />:$false. You may additionally specify a custom timeout duration (in seconds)
    /// via the -<see cref="Timeout"/>  parameter. If Restart-PrtgCore times out waiting for the PRTG Core Service to restart, a <see cref="TimeoutException"/> 
    /// will be thrown. <para/>
    /// Extreme caution should be used when using Restart-PrtgCore. While smaller PRTG installs can restart in a matter of minutes,
    /// servers containing in excess of 10,000 sensors can take over half an hour to restart.</para>
    /// 
    /// <example>
    ///     <code>C:\> Restart-PrtgCore</code>
    ///     <para>Restart the PRTG Core Service. The cmdlet will wait up to 60 minutes for the service to restart.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Restart-PrtgCore -Wait:$false</code>
    ///     <para>Restart the PRTG Core Service, without waiting for the service to restart before ending the cmdlet.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Restart-PrtgCore -Timeout 600</code>
    ///     <para>Restart the PRTG Core Service, waiting 10 minutes (600 seconds) for the service to restart.</para>
    /// </example>
    /// 
    /// <para type="link">Restart-Probe</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Restart, "PrtgCore", SupportsShouldProcess = true)]
    public class RestartPrtgCore : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">Forces the PRTG Core Service to be restarted without displaying a confirmation prompt.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// <para type="description">Wait for the PRTG Core Service to restart before ending the cmdlet. By default this value is true.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Wait { get; set; } = SwitchParameter.Present;

        /// <summary>
        /// <para type="description">Duration (in seconds) to wait for the PRTG Core Service to restart. Default value is 3600 (1 hour). If <see cref="Wait"/> is false, this parameter will have no effect.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Duration (in seconds) to wait for the PRTG Core Service to restart. Default value is 3600 (1 hour). If -Wait is false, this parameter will have no effect.")]
        public int Timeout { get; set; } = 3600;

        private DateTime restartTime;

        private int secondsRemaining = 150;
        private int secondsElapsed;

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess("PRTG Core Service"))
            {
                bool yes = false;
                bool no = false;
                if (Force || ShouldContinue("Are you sure you want to restart the PRTG Core Service? All users will be disconnected while restart is in progress", "WARNING!", true, ref yes, ref no))
                {
                    restartTime = DateTime.Now;

                    client.RestartCore();

                    if (Wait)
                    {
                        WaitForRestart();
                    }
                }
            }
        }

        private void WaitForRestart()
        {
            var record = new ProgressRecord(1, "Restart PRTG Core", "Restarting PRTG Core")
            {
                CurrentOperation = "Waiting for PRTG Core Server to shutdown"
            };

            RestartStage stage = RestartStage.Shutdown;

            var originalRetries = client.RetryCount;

            try
            {
                client.RetryCount = 0;

                while (stage != RestartStage.Completed)
                {
                    stage = ProbeServer();

                    switch (stage)
                    {
                        case RestartStage.Shutdown:
                            break;
                        case RestartStage.Restart:
                            record.CurrentOperation = "Waiting for PRTG Core Service to restart";
                            break;
                        case RestartStage.Startup:
                            record.CurrentOperation = "Waiting for PRTG Core Server to initialize";
                            break;
                    }

                    if (stage == RestartStage.Completed)
                        break;

                    record.PercentComplete = (int) ((double) stage/3*100);
                    record.SecondsRemaining = secondsRemaining;

                    for (int i = 0; i < 5; i++)
                    {
                        WriteProgress(record);

                        secondsRemaining--;
                        secondsElapsed++;

                        if (secondsRemaining < 0)
                            secondsRemaining = 30;

                        record.SecondsRemaining = secondsRemaining;

                        if (secondsElapsed > Timeout)
                            throw new TimeoutException($"Timed out waiting for PRTG Core Service to restart");

#if !DEBUG
                        Thread.Sleep(1000);
#endif
                    }

                    if (Stopping)
                        break;
                }
            }
            finally
            {
                client.RetryCount = originalRetries;
            }
        }

        private RestartStage ProbeServer()
        {
            RestartStage stage;

            try
            {
                var logs = client.GetLogs(null, endDate: restartTime, status: LogStatus.SystemStart);

                stage = logs.Any() ? RestartStage.Completed : RestartStage.Shutdown;
            }
            catch (Exception ex) when (ex is WebException || ex is TimeoutException)
            {
                stage = RestartStage.Restart;
            }
            catch (HttpRequestException)
            {
                stage = RestartStage.Startup;
            }

            return stage;
        }

        internal enum RestartStage
        {
            Shutdown = 1,
            Restart = 2,
            Startup = 3,
            Completed
        }
    }
}
