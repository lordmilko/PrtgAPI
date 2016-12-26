using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Invalidates a <see cref="PrtgClient"/> previously created with <see cref="ConnectPrtgServer"/>.
    /// </summary>
    [Cmdlet(VerbsCommunications.Disconnect, "PrtgServer")]
    public class DisconnectPrtgServer : PSCmdlet
    {
        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            PrtgSessionState.Client = null;
        }
    }
}
