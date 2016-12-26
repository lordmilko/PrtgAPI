using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Cmdlets.Incomplete
{
    [Cmdlet(VerbsCommon.New, "TriggerParameter")]
    public class NewTriggerParameter : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            //WriteObject(new StateTriggerParameters())
        }
    }
}
