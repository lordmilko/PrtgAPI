using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Clone a device within PRTG.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Device", SupportsShouldProcess = true)]
    public class CloneDevice : CloneObject<Device>
    {
        /// <summary>
        /// <para type="description">The device to clone.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Device Device { get; set; }

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
            if (string.IsNullOrEmpty(Name))
                Name = $"Clone of {Device.Name}";

            if (ShouldProcess($"{Device.Name} (ID: {Device.Id}) to destination ID {DestinationId}"))
            {
                var hostnameTouse = HostName ?? Device.Host;
                var id = ExecuteOperation(() => client.CloneObject(Device.Id, Name, HostName ?? Device.Host, DestinationId), Name, Device.Id);

                if (Resolve)
                {
                    ResolveObject(id, i => client.GetDevices(Property.Id, i));
                }
                else
                {
                    var response = new PSObject();
                    response.Properties.Add(new PSNoteProperty("ObjectId", id));
                    response.Properties.Add(new PSNoteProperty("Name", Name));
                    response.Properties.Add(new PSNoteProperty("HostName", hostnameTouse));

                    WriteObject(response);
                }
            }
        }
    }
}