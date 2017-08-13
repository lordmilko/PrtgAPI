using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Clones a device within PRTG.</para>
    /// 
    /// <para type="description">The Clone-Device cmdlet duplicates a PRTG Device, including all sensors and settings defined under it.</para>
    /// <para type="description">To clone a device you must specify the Object ID of the group or probe the cloned device will sit under.
    /// If a Name is not specified, Clone-Device will automatically name the device as "Clone of &lt;device&gt;, where &lt;device&gt;
    /// is the name of the original device.</para>
    /// <para type="description">If you wish to specify a custom hostname/IP Address for the device, you can specify the HostName parameter. If a HostName
    /// is not specified, the hostname/IP Address of the original device will be used.</para>
    /// <para type="description">When a device has been cloned, by default Clone-Device will attempt to resolve the object into a PrtgAPI Device.
    /// Based on the speed of your PRTG Server, this can sometimes result in a delay of 5-10 seconds due to the delay with which
    /// PRTG clones the object. If Clone-Device cannot resolve the resultant object on the first attempt, PrtgAPI will make a further
    /// 10 retries, pausing for a successively greater duration between each try. After each failed attempt a warning will be displayed indicating
    /// the number of attempts remaining. Object resolution can be aborted at any time by pressing an escape sequence such as Ctrl+C.</para>
    /// <para type="description">If you do not wish to resolve the resultant object, you can specify -Resolve:$false, which will
    /// cause Clone-Device to output a clone summary, including the object ID, name and hostname of the new object. As PRTG pauses all cloned
    /// objects by default, it is generally recommended to resolve the new object so that you may unpause the object with Resume-Object.</para>
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
    /// 
    /// <para type="link">Get-Device</para>
    /// <para type="link">Clone-Group</para>
    /// <para type="link">Clone-Sensor</para>
    /// <para type="link">Resume-Object</para>
    /// </summary>
    [OutputType(typeof(Device))]
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