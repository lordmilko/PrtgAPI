using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    //we should output the object id created and have it have a column. newobjectid or something better
    //implement supportsshouldprocess

    //input object, new name, target location

    //todo: is it possible to get a list of all auto discover templates and then auto discover USING one of them

    /// <summary>
    /// Clone a device within PRTG.
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Device", SupportsShouldProcess = true)]
    public class CloneDevice : PrtgCmdlet
    {
        /// <summary>
        /// The device to clone.
        /// </summary>
        public Device Device { get; set; }

        /// <summary>
        /// The name to rename the cloned object to.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The ID of the group or probe the device should be cloned to.
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>
        /// The hostname or IP Address to set on the cloned device.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessPrtgRecord()
        {
            client.Clone(Device.Id, Name, Hostname ?? Device.Host, DestinationId);
        }
    }

    /// <summary>
    /// Clone a sensor within PRTG.
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Sensor", SupportsShouldProcess = true)]
    public class CloneSensor : CloneSensorOrGroup
    {
        /// <summary>
        /// The sensor to clone.
        /// </summary>
        public Sensor Sensor { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessPrtgRecord() => ProcessPrtgRecord(Sensor.Id);
    }

    /// <summary>
    /// Clone a group within PRTG.
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Group", SupportsShouldProcess = true)]
    public class CloneGroup : CloneSensorOrGroup
    {
        /// <summary>
        /// The group to clone.
        /// </summary>
        public Group Group { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessPrtgRecord() => ProcessPrtgRecord(Group.Id);
    }

    /// <summary>
    /// Clone a sensor or group within PRTG.
    /// </summary>
    public abstract class CloneSensorOrGroup : PrtgCmdlet
    {
        /// <summary>
        /// The name to rename the cloned object to.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The ID of the device or group that will hold the cloned object.
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        /// <param name="objectId">The ID of the object to clone.</param>
        protected void ProcessPrtgRecord(int objectId)
        {
            client.Clone(objectId, Name, DestinationId);
        }
    }
}
