using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    //things to note: if you dont specify a name it gets clone of "original name"
    //                if you dont specify a hostname it uses the hostname/ip of the original device
    //objects are paused after you clone them

    /// <summary>
    /// <para type="synopsis">Clones a device within PRTG.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Device -Id 1234 | Clone-Device 5678</code>
    ///     <para>Clone the device with ID 1234 to the group or probe with ID 5678</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 1234 | Clone-Device 5678 MyNewDevice 192.168.1.1</code>
    ///     <para>Clone the device with ID 1234 into the group or probe with ID 5678 renamed as "MyNewDevice" with IP Address 192.168.1.1</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 1234 | Clone-Device 5678 -Resolve:$false</code>
    ///     <para>Clone the device with ID 1234 to the group or probe with ID 5678 without resolving the resultant PrtgObject.</para>
    ///     <para/>
    /// </example>
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
                var hostnameToUse = HostName ?? Device.Host;
                ExecuteOperation(() => Clone(hostnameToUse), Device.Name, Device.Id);
            }
        }

        private void Clone(string hostnameToUse)
        {
            var id = client.CloneObject(Device.Id, Name, hostnameToUse, DestinationId);

            if (Resolve)
            {
                ResolveObject(id, i => client.GetDevices(Property.Id, i));
            }
            else
            {
                var response = new PSObject();
                response.Properties.Add(new PSNoteProperty("ObjectId", id));
                response.Properties.Add(new PSNoteProperty("Name", Name));
                response.Properties.Add(new PSNoteProperty("HostName", hostnameToUse));

                WriteObject(response);
            }
        }
    }
}