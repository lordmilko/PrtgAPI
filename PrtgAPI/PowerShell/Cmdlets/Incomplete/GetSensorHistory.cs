using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Request;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "SensorHistory")]
    public class GetSensorHistory : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "Default", ValueFromPipeline = true)]
        public Sensor Sensor { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Manual")]
        public int Id { get; set; }

        [Parameter(Mandatory = false)]
        public DateTime? StartDate { get; set; }

        [Parameter(Mandatory = false)]
        public DateTime? EndDate { get; set; }

        protected override void ProcessRecordEx()
        {
            //todo: need to figure out how to average?

            if (ParameterSetName == "Default")
                Id = Sensor.Id;

            var response = client.GetSensorHistory(Id, StartDate, EndDate);

            var list = new List<PSObject>();

            foreach (var date in response)
            {
                var obj = new PSObject();

                obj.Properties.Add(new PSNoteProperty("DateTime", date.DateTime));
                obj.Properties.Add(new PSNoteProperty("SensorId", date.SensorId));

                foreach (var channel in date.Values)
                {
                    obj.Properties.Add(new PSNoteProperty(channel.Name, channel.Value));
                }

                list.Add(obj);
            }

            WriteObject(list, true);
        }
    }
}
