using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieve groups from a PRTG Server.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Group")]
    public class GetGroup : PrtgTableCmdlet<Group, GroupParameters>
    {
        /// <summary>
        /// The parent group to retrieve groups for.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Group Group { get; set; }

        /// <summary>
        /// The probe to retrieve groups for.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Probe Probe { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetGroup"/> class.
        /// </summary>
        public GetGroup() : base(Content.Groups, null)
        {
        }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (Probe != null)
                AddPipelineFilter(Property.Probe, Probe.Name);
            else if (Group != null)
                AddPipelineFilter(Property.ParentId, Group.Id);

            base.ProcessRecordEx();
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving groups from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        protected override GroupParameters CreateParameters() => new GroupParameters();
    }
}
