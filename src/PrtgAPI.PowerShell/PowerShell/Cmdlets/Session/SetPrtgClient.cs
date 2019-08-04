using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Modifies settings on the current session's <see cref="PrtgClient"/>.</para>
    /// 
    /// <para type="description">The Set-PrtgClient cmdlet modifies settings present on the current session's <see cref="PrtgClient"/>.
    /// Typically these settings are specified when establishing a PRTG session with Connect-PrtgServer. This cmdlet allows you
    /// to modify these settings after the session has already been created, such as when you've connected to a GoPrtg Server.</para>
    /// 
    /// <example>
    ///     <code>C:\> Set-PrtgClient -RetryCount 5 -LogLevel Trace,Response</code>
    ///     <para>Update the RetryCount and LogLevel on the current session's PrtgClient.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Getting-Started#session-management-1">Online version:</para>
    /// <para type="link">Connect-PrtgServer</para>
    /// <para type="link">Get-PrtgClient</para>
    /// <para type="link">Connect-GoPrtgServer</para>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "PrtgClient")]
    public class SetPrtgClient : PrtgCmdlet, IPrtgPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">The number of times to retry a request that times out while communicating with PRTG.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int? RetryCount { get; set; }

        /// <summary>
        /// <para type="description">The base delay (in seconds) between retrying a timed out request. Each successive failure of a given request will wait an additional multiple of this value.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int? RetryDelay { get; set; }

        /// <summary>
        /// <para type="description">The type of events to log when -Verbose is specified.</para> 
        /// </summary>
        [Parameter(Mandatory = false)]
        public LogLevel[] LogLevel { get; set; }

        /// <summary>
        /// <para type="description">Enable or disable PowerShell Progress when piping between cmdlets. By default, if Connect-PrtgServer is being called from within a script or the PowerShell ISE this value is false. Otherwise, true.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Progress { get; set; }

        /// <summary>
        /// <para type="description">Ignore any SSL validation when communicating with PRTG. Modifying this property
        /// will regenerate the current session's <see cref="PrtgClient"/>.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter IgnoreSSL { get; set; }

        /// <summary>
        /// <para type="description">Specifies whether to return the <see cref="PrtgClient"/> that will be used by this session after processing all parameters.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// <para type="description">Returns the <see cref="PrtgClient"/> that was created or manipulated by this cmdlet.</para>
        /// </summary>
        public object PassThruObject => PrtgSessionState.Client;

        /// <summary>
        /// Writes the current <see cref="PassThruObject"/> to the pipeline if <see cref="PassThru"/> is specified.
        /// </summary>
        public void WritePassThru()
        {
            if (PassThru)
                WriteObject(PassThruObject);
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (IgnoreSSL)
            {
                var oldClient = PrtgSessionState.Client;
                PrtgSessionState.Client = new PrtgClient(oldClient.Server, oldClient.UserName, oldClient.PassHash,
                    AuthMode.PassHash, true)
                {
                    RetryCount = oldClient.RetryCount,
                    RetryDelay = oldClient.RetryDelay,
                    LogLevel = oldClient.LogLevel
                };
            }

            if (RetryCount != null)
                PrtgSessionState.Client.RetryCount = RetryCount.Value;

            if (RetryDelay != null)
                PrtgSessionState.Client.RetryDelay = RetryDelay.Value;

            if (LogLevel != null)
            {
                LogLevel level = PrtgAPI.LogLevel.None;

                foreach (var l in LogLevel)
                    level |= l;

                PrtgSessionState.Client.LogLevel = level;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Progress)))
                PrtgSessionState.EnableProgress = Progress;

            WritePassThru();
        }
    }
}
