using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "Channel")]
    public class GetChannel : PrtgObjectCmdlet<Channel>
    {
        [Parameter(ValueFromPipeline = true)]
        public Sensor Sensor { get; set; }

        [Parameter(Position = 0, ValueFromPipeline = true)]
        public int? SensorId { get; set; }

        protected override List<Channel> GetRecords()
        {
            if (Sensor == null && SensorId == null)
                throw new ArgumentException("Please specify either a Sensor or a SensorId");
            
            if (Sensor != null)
            {
                SensorId = Sensor.Id;
            }

            return client.GetChannels(SensorId.Value).OrderBy(c => c.Id).ToList();
        }
    }
}
