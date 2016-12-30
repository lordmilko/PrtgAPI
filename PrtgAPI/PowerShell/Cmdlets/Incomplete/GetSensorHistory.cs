using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell.Cmdlets.Incomplete
{
    class Record
    {
        public DateTime DateTime { get; set; }
        public string ChannelName { get; set; }
        public int ChannelId { get; set; }
        public string Value { get; set; } 
    }

    [Cmdlet(VerbsCommon.Get, "SensorHistory")]
    class GetSensorHistory : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            throw new NotImplementedException();
            var list = new List<Record>();

            list.Add(new Record
            {
                DateTime = DateTime.Now,
                ChannelName = "Percent Available Memory",
                ChannelId = 1,
                Value = "100%"
            });

            WriteObject(list, true);
        }
    }
}
