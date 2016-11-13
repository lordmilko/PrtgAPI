using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Attributes;

namespace PrtgAPI.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "Sensor")]
    public class GetSensor : PrtgTableCmdlet<Sensor>
    {
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Device Device { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Probe Probe { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Group Group { get; set; }

        protected override void ProcessRecord()
        {
            if (Device != null)
                SetPipelineFilter(Property.ParentId, Device.Id);
            else if (Group != null)
                SetPipelineFilter(Property.Group, Group.Name);
            else if (Probe != null)
                SetPipelineFilter(Property.Probe, Probe.Name);

            base.ProcessRecord();
        }

        protected override List<Sensor> GetRecords()
        {
            return client.GetSensors();
        }

        protected override List<Sensor> GetRecords(SearchFilter[] filter)
        {
            return client.GetSensors(filter);
        }

        //implement support for doing stuff like get-device|get-sensor etc, and have that supported on all our types, e.g get-probe|get-device
        //i think the way we do that is we have our sensor have a device property that can take a value from the pipeline
        
        //test if property.parent or some other property can let a sensor detect its parent probe by id
    }
}
