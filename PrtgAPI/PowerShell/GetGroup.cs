using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "Group")]
    public class GetGroup : PrtgObjectCmdlet<Group>
    {
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Group Group { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Probe Probe { get; set; }

        protected override void ProcessRecord()
        {
            if (Probe != null)
                SetPipelineFilter(Property.Probe, Probe.Name);
            else if (Group != null)
                SetPipelineFilter(Property.ParentId, Group.Id);

            base.ProcessRecord();
        }

        protected override List<Group> GetRecords()
        {
            return client.GetGroups();
        }

        protected override List<Group> GetRecords(params SearchFilter[] filter)
        {
            return client.GetGroups(filter);
        }
    }
}
