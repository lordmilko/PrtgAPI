using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    //PRTG does not support batching move requests

    /// <summary>
    /// <para type="synopsis">Moves a device or group within the PRTG Object Tree.</para>
    /// 
    /// <para type="description">The Move-Object cmdlet allows you to move a device or group to another group or probe within PRTG.
    /// Any device or group can be moved to any other group or probe, with the exception of special objects such as the "Probe Device"
    /// object under each probe, as well as the Root group (ID: 0).</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Device dc-1 | Move-Object 5678</code>
    ///     <para>Move all devices named dc-1 to under the object with ID 5678</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Organization#moving-1">Online version:</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Move, "Object", SupportsShouldProcess = true)]
    public class MoveObject : PrtgPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">The device to move to another group or probe.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Device)]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">The group to move to another group or probe.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Group)]
        public Group Group { get; set; }

        /// <summary>
        /// <para type="description">The group or probe to move the object to. This cannot be the Root PRTG Group.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public int DestinationId { get; set; }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            switch (ParameterSetName)
            {
                case ParameterSet.Device:
                    ExecuteOperation(Device);
                    break;
                case ParameterSet.Group:
                    ExecuteOperation(Group);
                    break;
                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }
        }

        private void ExecuteOperation(SensorOrDeviceOrGroupOrProbe obj)
        {
            if (ShouldProcess($"'{obj.Name}' (ID: {obj.Id}) (Destination ID: {DestinationId})"))
            {
                ExecuteOperation(() => client.MoveObject(obj.Id, DestinationId), $"Moving {obj.BaseType.ToString().ToLower()} {obj.Name} (ID: {obj.Id}) to object ID {DestinationId}");
            }
        }

        internal override string ProgressActivity => "Moving PRTG Objects";

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => (object) Group ?? Device;
    }
}
