using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves devices from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-Device cmdlet retrieves devices from a PRTG Server. Devices hold sensors used to monitor a particular system.
    /// Get-Device provides a variety of methods of filtering the devices requested from PRTG, including by device name, ID and tags, as well as parent probe/group.
    /// Multiple filters can be used in conjunction to futher limit the number of results returned.</para>
    /// <para type="description">For scenarios in which you wish to filter on properties not covered by parameters available in Get-Device, a custom <see cref="SearchFilter"/>
    /// object can be created by specifying the field name, condition and value to filter upon. For information on properties that can be filtered upon,
    /// see New-SearchFilter.</para>
    /// <para type="description">The <see cref="Device"/> objects returned from Get-Device can be piped to a variety of other cmdlets for further processing, including Get-Sensor,
    /// wherein the ID of each device will be used to filter for its parent sensors.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Device dc-1</code>
    ///     <para>Get all devices named "dc-1" (case insensitive)</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device *exch*</code>
    ///     <para>Get all devices whose name contains "exch" (case insensitive)</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 2000</code>
    ///     <para>Get the device with ID 2000</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe contoso | Get-Device</code>
    ///     <para>Get all devices from the probe named "contoso"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Tags C_OS_Win</code>
    ///     <para>Get all devices that have the tag "C_OS_Win"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Count 1</code>
    ///     <para>Get only 1 device from PRTG.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> flt location contains "new york" | Get-Device</code>
    ///     <para>Get all devices whose location contains "new york"</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// <para type="link">New-SearchFilter</para>
    /// </summary>
    [OutputType(typeof(Device))]
    [Cmdlet(VerbsCommon.Get, "Device")]
    public class GetDevice : PrtgTableCmdlet<Device, DeviceParameters>
    {
        /// <summary>
        /// <para type="description">The group to retrieve devices for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Group Group { get; set; }

        /// <summary>
        /// <para type="description">The probe to retrieve devices for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Probe Probe { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetDevice"/> class.
        /// </summary>
        public GetDevice() : base(Content.Devices, null)
        {
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            if (Probe != null)
                AddPipelineFilter(Property.Probe, Probe.Name);
            else if (Group != null)
                AddPipelineFilter(Property.ParentId, Group.Id);

            base.ProcessAdditionalParameters();
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving devices from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override DeviceParameters CreateParameters() => new DeviceParameters();
    }
}
