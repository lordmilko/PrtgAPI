using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Disable, "PrtgProgress")]
    public class DisablePrtgProgress : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            PrtgSessionState.EnableProgress = false;
        }
    }
}
