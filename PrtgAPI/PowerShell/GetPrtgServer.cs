using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "PrtgServer")]
    public class GetPrtgServer : PrtgCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject(PrtgSessionState.Client);
        }
    }
}
