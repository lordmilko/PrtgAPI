using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Enable, "PrtgProgress")]
    public class EnablePrtgProgress : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            PrtgSessionState.EnableProgress = true;
        }
    }
}
