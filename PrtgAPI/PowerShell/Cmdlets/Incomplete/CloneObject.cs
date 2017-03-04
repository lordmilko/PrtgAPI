using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    //we should output the object id created and have it have a column. newobjectid or something better
    //implement supportsshouldprocess

    //input object, new name, target location

    //todo: is it possible to get a list of all auto discover templates and then auto discover USING one of them

    /// <summary>
    /// <para type="synopsis">Clone a device within PRTG.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Device", SupportsShouldProcess = true)]
    public class CloneDevice : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The device to clone.</para>
        /// </summary>
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">The name to rename the cloned object to.</para>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The ID of the group or probe the device should be cloned to.</para>
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>
        /// <para type="description">The hostname or IP Address to set on the cloned device.</para>
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            client.CloneObject(Device.Id, Name, Hostname ?? Device.Host, DestinationId);
        }
    }

    /// <summary>
    /// <para type="synopsis">Clone a sensor within PRTG.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Sensor", SupportsShouldProcess = true)]
    public class CloneSensor : CloneSensorOrGroup
    {
        /// <summary>
        /// <para type="description">The sensor to clone.</para>
        /// </summary>
        public Sensor Sensor { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx() => ProcessRecordEx(Sensor.Id);
    }

    /// <summary>
    /// <para type="synopsis">Clone a group within PRTG.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Group", SupportsShouldProcess = true)]
    public class CloneGroup : CloneSensorOrGroup
    {
        /// <summary>
        /// <para type="description">The group to clone.</para>
        /// </summary>
        public Group Group { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx() => ProcessRecordEx(Group.Id);
    }

    /// <summary>
    /// Clone a sensor or group within PRTG.
    /// </summary>
    public abstract class CloneSensorOrGroup : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The name to rename the cloned object to.</para>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The ID of the device or group that will hold the cloned object.</para>
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        /// <param name="objectId">The ID of the object to clone.</param>
        protected void ProcessRecordEx(int objectId)
        {
            client.CloneObject(objectId, Name, DestinationId);
        }
    }
}
