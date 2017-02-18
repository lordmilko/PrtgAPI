using System;
using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
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
            /*var list = new List<Record>();

            list.Add(new Record
            {
                DateTime = DateTime.Now,
                ChannelName = "Percent Available Memory",
                ChannelId = 1,
                Value = "100%"
            });

            WriteObject(list, true);
            */
            throw new NotImplementedException();
        }
    }
}
