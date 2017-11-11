using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves configuration, status and version details from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-PrtgStatus cmdlet retrieves status details from a PRTG Server, including
    /// details regarding sensor types, license and version details, cluster config and user access rights.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-PrtgStatus</code>
    ///     <para>Retrieve the current system status.</para>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "PrtgStatus")]
    public class GetPrtgStatus : PrtgCmdlet
    {
        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            WriteObject(client.GetStatus());
        }
    }
}
