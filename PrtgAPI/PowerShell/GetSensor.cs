using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Attributes;

namespace PrtgAPI.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "Sensor")]
    public class GetSensor : PrtgObjectCmdlet<Sensor>
    {
        protected override List<Sensor> GetRecords()
        {
            return client.GetSensors();
        }

        protected override List<Sensor> GetRecords(params SearchFilter[] filter)
        {
            return client.GetSensors(filter);
        }
    }
}
