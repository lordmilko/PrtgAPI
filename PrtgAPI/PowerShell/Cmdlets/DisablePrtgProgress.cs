using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Disables PRTG Progress for the current PowerShell Session.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Disable, "PrtgProgress")]
    public class DisablePrtgProgress : PSCmdlet
    {
        /// <summary>
        /// Performs record-by-record processing for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            PrtgSessionState.EnableProgress = false;
        }
    }
}
