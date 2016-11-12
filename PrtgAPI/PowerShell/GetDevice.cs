using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "Device")]
    public class GetDevice : PrtgObjectCmdlet<Device>
    {
        protected override List<Device> GetRecords()
        {
            return client.GetDevices();
        }

        protected override List<Device> GetRecords(params SearchFilter[] filter)
        {
            return client.GetDevices(filter);
        }
    }
}
