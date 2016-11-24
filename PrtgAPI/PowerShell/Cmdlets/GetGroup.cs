using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieve groups from a PRTG Server.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Group")]
    public class GetGroup : PrtgTableCmdlet<Group>
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
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Probe != null)
                AddPipelineFilter(Property.Probe, Probe.Name);
            else if (Group != null)
                AddPipelineFilter(Property.ParentId, Group.Id);

            base.ProcessRecord();
        }

        /// <summary>
        /// Retrieves all groups from a PRTG Server.
        /// </summary>
        /// <returns>A list of all groups.</returns>
        protected override IEnumerable<Group> GetRecords()
        {
            return client.GetGroups();
        }

        /// <summary>
        /// Retrieves a list of groups from a PRTG Server based on a specified filter.
        /// </summary>
        /// <param name="filter">A list of filters to use to limit search results.</param>
        /// <returns>A list of groups that match the specified search criteria.</returns>
        protected override IEnumerable<Group> GetRecords(params SearchFilter[] filter)
        {
            return client.GetGroups(filter);
        }
    }
}
