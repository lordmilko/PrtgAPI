using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "Probe")]
    class GetProbe : PrtgObjectCmdlet<Probe>
    {
        protected override List<Probe> GetRecords()
        {
            return client.GetProbes();
        }

        protected override List<Probe> GetRecords(params SearchFilter[] filter)
        {
            return client.GetProbes(filter);
        }
    }
}
