using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Utilities;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves sensors from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-Sensor cmdlet retrieves sensors from a PRTG Server. Sensors are the fundamental unit of monitoring
    /// in PRTG, and the most populous type of object in the system by far. Get-Sensor provides a variety of methods of filtering the sensors
    /// requested from PRTG, including by sensor name, ID, status and tags as well as parent probe/group/device. Multiple filters
    /// can be used in conjunction to further limit the number of results returned. Sensor properties that do not contain explicitly defined
    /// parameters on Get-Sensor can be specified as dynamic parameters, allowing one or more values to be specified of the specified type. All string parameters
    /// support the use of wildcards.</para>
    /// 
    /// <para type="description">For scenarios in which you wish to exert finer grained control over search filters,
    /// a custom <see cref="SearchFilter"/> object can be created specifying the field name, condition and value to filter upon. For information
    /// on properties that can be filtered on, see New-SearchFilter. When searching for Sensor Factory objects, please note that these
    /// objects do not respond to server side filters by "type". To filter for Sensor Factory sensors, filter by the tag "factorysensor".</para>
    /// 
    /// <para type="description">When invoked with no arguments, Get-Sensor will query the number of sensors present on your PRTG Server.
    /// If PrtgAPI detects the number is about a specified threshold, PrtgAPI will split the request up into several smaller requests
    /// which will each be invoked one after the other. Results will then be "streamed" to the pipeline as each smaller request completes. A progress
    /// bar will also be visible up the top indicating the total number of sensors retrieved/remaining.</para>
    /// 
    /// <para type="description">Get-Sensor provides two parameter sets for filtering objects by tags. When filtering for sensors
    /// that contain one of several tags, the -Tag parameter can be used, performing a logical OR between all specified operands.
    /// For scenarios in which you wish to filter for sensors containing ALL specified tags, the -Tags
    /// parameter can be used, performing a logical AND between all specified operands.</para>
    /// 
    /// <para type="description">When requesting sensors belonging to a specified group, PRTG will not return any objects that may
    /// be present under further child groups of the parent group. To work around this, by default Get-Sensor will automatically recurse
    /// child groups if it detects the initial sensor request did not return all items (as evidenced by the parent group's TotalSensors property.
    /// If you do not wish Get-Sensor to recurse child groups, this behavior can be overridden by specifying -Recurse:$false.</para>
    /// 
    /// <para type="description">The <see cref="Sensor"/> objects returned from Get-Sensor can be piped to a variety of other cmdlets for further
    /// processing, including Get-Channel, wherein the ID of the sensor will be used to acquire its underlying channels.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor Ping</code>
    ///     <para>Get all sensors named "Ping" (case insensitive)</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor *cpu*</code>
    ///     <para>Get all sensors whose name contains "cpu" (case insensitive)</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 2001</code>
    ///     <para>Get the sensor with ID 2001</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Status Down</code>
    ///     <para>Get all sensors in a "Down" status.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device *fw* | Get-Sensor Ping</code>
    ///     <para>Get all sensors named "Ping" (case insensitive) from all devices whose name contains "fw"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Count 1</code>
    ///     <para>Get only 1 sensor from PRTG.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Tag wmicpu*,wmimem*</code>
    ///     <para>Get all WMI CPU Load and WMI Memory sensors.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags ny,wmicpu*</code>
    ///     <para>Get all WMI CPU Load sensors from all New York devices.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Type *snmp*</code>
    ///     <para>Get all SNMP sensors using a dynamic parameter.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> flt type contains snmp | Get-Sensor</code>
    ///     <para>Get all SNMP sensors using a SearchFilter.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group -Id 2001 | Get-Sensor -Recurse:$false</code>
    ///     <para>Get all sensors from devices directly under the specified group, ignoring all child groups.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Sensors#powershell">Online version:</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// <para type="link">Get-Channel</para>
    /// <para type="link">New-SearchFilter</para>
    /// <para type="link">Add-Sensor</para> 
    /// </summary>
    [OutputType(typeof(Sensor))]
    [Cmdlet(VerbsCommon.Get, "Sensor", DefaultParameterSetName = ParameterSet.LogicalAndTags)]
    public class GetSensor : PrtgTableRecurseCmdlet<Sensor, SensorParameters>, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">The parent <see cref="Device"/> to retrieve sensors for, or a wildcard expression
        /// specifying the names of the devices to retrieve sensors for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public NameOrObject<Device> Device { get; set; }

        /// <summary>
        /// <para type="description">The <see cref="Probe"/> to retrieve sensors for, or a wildcard expression
        /// specifying the names of the probes to retrieve sensors for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public NameOrObject<Probe> Probe { get; set; }

        /// <summary>
        /// <para type="description">The group to retrieve sensors for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public NameOrObject<Group> Group { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSensor"/> class.
        /// </summary>
        public GetSensor() : base(Content.Sensors, true)
        {
        }

        internal override bool StreamCount()
        {
            return !(Group != null && Recurse);
        }

        internal override List<Sensor> GetObjectsInternal(SensorParameters parameters)
        {
            if (Group != null && Group.IsObject)
            {
                var groups = client.GetGroups(Property.Name, Group.Object.Name);

                //If more than 1 group with the specified name exists and we intend on recursing, get the sensors
                //of each device under the group (which will be identified via the group's ID).
                if (groups.Count > 1)
                {
                    client.Log($"Parent group name '{Group}' is not unique and -{nameof(Recurse)} was specified; retrieving sensors by child devices", LogLevel.Trace);

                    //Get the sensors under the parent group. We'll process all the child groups in GetAdditionalGroupRecords
                    return GetSensorsFromGroupNameFilter(Group.Object, true, parameters);
                }
                else
                {
                    client.Log($"Parent group '{Group}' is unique; retrieving sensors by group name", LogLevel.Trace);
                }
            }

            //If we aren't piping from a group or only have 1 group with the specified name,
            //we can use the default implementation, as referring to the group by name will be unambiguous. If
            //we are recursing, we're retrieving the parent group's records; our children's records will be
            //retrieved in GetAdditionalRecords. If we're not recursing, if our group name was ambiguous we
            //retrieved our sensors via devices in GetSensorsFroupGroupNameFilter. GetAdditionalRecords won't return
            //anything when it sees that Recurse == false
            var sensors = base.GetObjectsInternal(parameters);

            if (Group != null && Recurse)
                client.Log($"Found {sensors.Count} {"sensor".Plural(sensors)} in group {Group}", LogLevel.Trace);

            return sensors;
        }

        /// <summary>
        /// Retrieves additional records not included in the initial request.
        /// </summary>
        /// <param name="parameters">The parameters that were used to perform the initial request.</param>
        protected override IEnumerable<Sensor> GetAdditionalRecords(SensorParameters parameters)
        {
            return GetAdditionalGroupRecords(Group, g => g.TotalSensors, parameters);
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            if (Device != null)
                AddNameOrObjectFilter(Property.ParentId, Device, d => d.Id, Property.Device);
            if (Group != null)
                AddNameOrObjectFilter(Property.Group, Group, g => g.Name);
            if (Probe != null)
                AddNameOrObjectFilter(Property.Probe, Probe, p => p.Name);

            ProcessStatusFilter();

            base.ProcessAdditionalParameters();
        }

        /// <summary>
        /// Process any post retrieval filters specific to the current cmdlet.
        /// </summary>
        /// <param name="records">The records to filter.</param>
        /// <returns>The filtered records.</returns>
        protected override IEnumerable<Sensor> PostProcessAdditionalFilters(IEnumerable<Sensor> records)
        {
            records = FilterResponseRecordsByNameOrObjectName(Device, r => r.Device, records);
            records = FilterResponseRecordsByNameOrObjectName(Group, r => r.Group, records);
            records = FilterResponseRecordsByNameOrObjectName(Probe, r => r.Probe, records);

            return base.PostProcessAdditionalFilters(records);
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving sensors from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override SensorParameters CreateParameters() => new SensorParameters();
    }
}
