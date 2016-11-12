using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell
{
    [Cmdlet("Disconnect", "PrtgServer")]
    public class DisconnectPrtgServer : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            PrtgSessionState.Client = null;
        }
    }
}
