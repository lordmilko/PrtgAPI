using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Enables PRTG Progress for the current PowerShell Session.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Enable, "PrtgProgress")]
    public class EnablePrtgProgress : PSCmdlet
    {
        /// <summary>
        /// Performs record-by-record processing for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            PrtgSessionState.EnableProgress = true;
        }
    }
}
