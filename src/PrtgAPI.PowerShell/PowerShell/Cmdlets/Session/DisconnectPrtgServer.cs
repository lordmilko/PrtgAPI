using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Invalidates a <see cref="PrtgClient"/> previously created with Connect-PrtgServer.</para>
    /// 
    /// <para type="description">The Disconnect-PrtgServer cmdlet invalidates a PrtgClient previously created
    /// with Connect-PrtgServer. As PRTG uses a stateless REST API, it is not necessary to call
    /// Disconnect-PrtgServer when you have finished making requests unless you are developing a script,
    /// in which case Connect-PrtgServer will fail to create a PrtgClient if a PRTG connection is already active
    /// in the current session. This can be circumvented by specifying the -Force parameter when establishing
    /// the connection. For more information, see Connect-PrtgServer.</para>
    /// <para type="description">If Disconnect-PrtgServer is called when you are not connected to a PRTG Server, this cmdlet does nothing.</para>
    /// 
    /// <example>
    ///     <code>C:\> Disconnect-PrtgServer</code>
    ///     <para>Disconnect from the current PRTG Server</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Getting-Started#session-management-1">Online version:</para>
    /// <para type="link">Connect-PrtgServer</para>
    /// <para type="link">Get-PrtgClient</para>
    /// </summary>
    [Cmdlet(VerbsCommunications.Disconnect, "PrtgServer")]
    public class DisconnectPrtgServer : PSCmdlet
    {
        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            PrtgSessionState.Client = null;
        }
    }
}
