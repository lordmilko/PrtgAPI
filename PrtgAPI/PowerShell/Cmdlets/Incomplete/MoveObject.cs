using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets.Incomplete
{
    /// <summary>
    /// <para type="synopsis">Move a device or group within the PRTG Object Tree.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Move, "Object")]
    public class MoveObject : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The device to move to another group or probe.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Device")]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">The group to move to another group or probe.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Group")]
        public Group Group { get; set; }

        /// <summary>
        /// <para type="description">The group or probe to move the object to. This cannot be the Root PRTG Group.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public int DestinationId { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            switch (ParameterSetName)
            {
                case "Device":
                    client.MoveObject(Device.Id, DestinationId);
                    break;
                case "Group":
                    client.MoveObject(Group.Id, DestinationId);
                    break;
            }
        }
    }
}
