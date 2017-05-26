using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Disable, "PrtgProgress")]
    public class DisablePrtgProgress : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            PrtgSessionState.DisableProgress = true;
        }
    }
}
