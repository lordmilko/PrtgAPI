using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieves the current session's <see cref="PrtgClient"/> 
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "PrtgClient")]
    public class GetPrtgClient : PSCmdlet
    {
        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            WriteObject(PrtgSessionState.Client);
        }
    }
}
