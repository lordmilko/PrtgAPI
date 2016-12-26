using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet("Refresh", "Object")]
    public class RefreshObject : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        protected override void ProcessRecord()
        {
            client.CheckNow(Object.Id);
        }
    }
}
