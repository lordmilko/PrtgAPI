using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves devices from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-Device cmdlet retrieves devices from a PRTG Server. Devices hold sensors used to monitor a particular system.
    /// Get-Device provides a variety of methods of filtering the devices requested from PRTG, including by device name, ID and tags, as well as parent probe/group.
    /// Multiple filters can be used in conjunction to futher limit the number of results returned. Device properties that do not contain explicitly defined
    /// parameters on Get-Device can be specified as dynamic parameters, allowing one or more values to be specified of the specified type. All string parameters
    /// support the use of wildcards.</para>
    /// 
    /// <para type="description">For scenarios in which you wish to exert finer grained control over search filters, a custom <see cref="SearchFilter"/>
    /// object can be created by specifying the field name, condition and value to filter upon. For information on properties that can be filtered upon,
    /// see New-SearchFilter.</para>
    /// 
    /// <para type="description">Get-Device provides two parameter sets for filtering objects by tags. When filtering for devices
    /// that contain one of several tags, the -Tag parameter can be used, performing a logical OR between all specified operands.
    /// For scenarios in which you wish to filter for devices containing ALL specified tags, the -Tags
    /// parameter can be used, performing a logical AND between all specified operands.</para>
    /// 
    /// <para type="description">When requesting devices belonging to a specified group, PRTG will not return any objects that may
    /// be present under further child groups of the parent group. To work around this, by default Get-Device will automatically recurse
    /// child groups if it detects the initial device request did not return all items (as evidenced by the parent group's TotalDevices property.
    /// If you do not wish Get-Device to recurse child groups, this behavior can be overridden by specifying -Recurse:$false.</para>
    /// 
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
    ///     <code>C:\> Get-Device -Tag C_OS_Win,exch</code>
    ///     <para>Get all devices that have the tag "C_OS_Win" or "exch"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Tags ny,C_OS_Win</code>
    ///     <para>Get all Windows servers in New York</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Count 1</code>
    ///     <para>Get only 1 device from PRTG.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Location "*new york*"</code>
    ///     <para>Get all devices whose location contains "new york" using a dynamic parameter.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> flt location contains "new york" | Get-Device</code>
    ///     <para>Get all devices whose location contains "new york" using a SearchFilter.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group -Id 2001 | Get-Device -Recurse:$false</code>
    ///     <para>Get all devices directly under the specified group, ignoring all child groups.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Devices#powershell">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// <para type="link">New-SearchFilter</para>
    /// <para type="link">Add-Device</para> 
    /// </summary>
    [OutputType(typeof(Device))]
    [Cmdlet(VerbsCommon.Get, "Device", DefaultParameterSetName = LogicalAndTags)]
    public class GetDevice : PrtgTableRecurseCmdlet<Device, DeviceParameters>, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">The group to retrieve devices for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public NameOrObject<Group> Group { get; set; }

        /// <summary>
        /// <para type="description">The probe to retrieve devices for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public NameOrObject<Probe> Probe { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to devices with a certain HostName/IP Address. Can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public new string[] Host { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetDevice"/> class.
        /// </summary>
        public GetDevice() : base(Content.Devices, false)
        {
        }

        internal override bool StreamCount()
        {
            return !(Group != null && Recurse);
        }

        /// <summary>
        /// Retrieves additional records not included in the initial request.
        /// </summary>
        /// <param name="parameters">The parameters that were used to perform the initial request.</param>
        protected override IEnumerable<Device> GetAdditionalRecords(DeviceParameters parameters)
        {
            return GetAdditionalGroupRecords(Group, g => g.TotalDevices, parameters);
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            if (Probe != null)
                AddNameOrObjectFilter(Property.Probe, Probe, p => p.Name);
            else if (Group != null)
                AddNameOrObjectFilter(Property.ParentId, Group, g => g.Id, Property.Group);

            ProcessWildcardArrayFilter(Property.Host, Host);

            base.ProcessAdditionalParameters();
        }

        /// <summary>
        /// Process any post retrieval filters specific to the current cmdlet.
        /// </summary>
        /// <param name="records">The records to filter.</param>
        /// <returns>The filtered records.</returns>
        protected override IEnumerable<Device> PostProcessAdditionalFilters(IEnumerable<Device> records)
        {
            records = FilterResponseRecordsByWildcardArray(Host, d => d.Host, records);

            records = FilterResponseRecordsByNameOrObjectName(Group, r => r.Group, records);
            records = FilterResponseRecordsByNameOrObjectName(Probe, r => r.Probe, records);

            return base.PostProcessAdditionalFilters(records);
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving devices from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override DeviceParameters CreateParameters() => new DeviceParameters();
    }
}
