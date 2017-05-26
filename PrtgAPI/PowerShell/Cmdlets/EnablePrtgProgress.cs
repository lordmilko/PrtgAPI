using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Enable, "PrtgProgress")]
    public class EnablePrtgProgress : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            PrtgSessionState.DisableProgress = false;
        }
    }
}
