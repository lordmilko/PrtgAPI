using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.PowerShell
{
    [Cmdlet(VerbsCommon.Remove, "Object", SupportsShouldProcess = true)]
    public class RemoveObject : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            if(ShouldProcess($"'{Object.Name}' (ID: {Object.Id})"))
            {
                if(Force.IsPresent || ShouldContinue($"Are you sure you want to delete {Object.BaseType.ToString().ToLower()} '{Object.Name}' (ID: {Object.Id})", "WARNING!"))
                    client.Delete(Object.Id.Value);
            }
        }
    }
}
