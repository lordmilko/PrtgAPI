using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Clones a group within PRTG.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Group", SupportsShouldProcess = true)]
    public class CloneGroup : CloneSensorOrGroup<Group>
    {
        /// <summary>
        /// <para type="description">The group to clone.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Group Group { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"{Group.Name} (ID: {Group.Id}, Destination: {DestinationId})"))
            {
                ProcessRecordEx(Group.Id, Group.Name, id => client.GetGroups(Property.Id, id));
            }
        }
    }
}