using System;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    class Record
    {
        public DateTime DateTime { get; set; }
        public string ChannelName { get; set; }
        public int ChannelId { get; set; }
        public string Value { get; set; } 
    }

    [Cmdlet(VerbsCommon.Get, "ObjectHistory")]
    public class GetObjectHistory : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "Default", ValueFromPipeline = true)]
        public PrtgObject Object { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Manual")]
        public int Id { get; set; }

        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == "Default")
                Id = Object.Id;

            var response = client.GetObjectHistory(Id);

            WriteObject(response, true);

            return;

            //i think there are three types of history
            //1. the history thing that has a channel id (content=values)
            //2. the modification history of an object
            //3. the stuff from the historic data tab

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
