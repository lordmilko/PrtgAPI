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
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">The ID of the group or probe the device should be cloned to.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public int DestinationId { get; set; }

        /// <summary>
        /// <para type="description">The name to rename the cloned object to.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The hostname or IP Address to set on the cloned device.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 2)]
        public string HostName { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"{Device.Name} (ID: {Device.Id}) to destination ID {DestinationId}"))
            {
                var hostnameTouse = HostName ?? Device.Host;
                var id = client.CloneObject(Device.Id, Name, HostName ?? Device.Host, DestinationId);

                var response = new PSObject();
                response.Properties.Add(new PSNoteProperty("ObjectId", id));
                response.Properties.Add(new PSNoteProperty("Name", Name));
                response.Properties.Add(new PSNoteProperty("HostName", hostnameTouse));

                WriteObject(response);
            }
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
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"{Sensor.Name} (ID: {Sensor.Id}, Destination: {DestinationId})"))
            {
                ProcessRecordEx(Sensor.Id, Sensor.Name);
            }
        }
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
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Group Group { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"{Group.Name} (ID: {Group.Id}, Destination: {DestinationId})"))
            {
                ProcessRecordEx(Group.Id, Group.Name);
            }
        }
    }

    /// <summary>
    /// Clone a sensor or group within PRTG.
    /// </summary>
    public abstract class CloneSensorOrGroup : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The ID of the device or group that will hold the cloned object.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public int DestinationId { get; set; }

        /// <summary>
        /// <para type="description">The name to rename the cloned object to.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        /// <param name="objectId">The ID of the object to clone.</param>
        /// <param name="name">The name of the object to clone. If <see cref="Name"/> is not specified, this value will be used.</param>
        protected void ProcessRecordEx(int objectId, string name)
        {
            var nameToUse = Name ?? name;
            var id = client.CloneObject(objectId, nameToUse, DestinationId);

            var response = new PSObject();
            response.Properties.Add(new PSNoteProperty("ObjectId", id));
            response.Properties.Add(new PSNoteProperty("Name", nameToUse));

            WriteObject(response);
        }
    }
}
