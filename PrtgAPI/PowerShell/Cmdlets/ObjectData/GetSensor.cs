using System.Management.Automation;
using PrtgAPI.Parameters.ObjectData;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves sensors from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-Sensor cmdlet retrieves sensors from a PRTG Server. Sensors are the fundamental unit of monitoring
    /// in PRTG, and the most populous type of object in the system by far. Get-Sensor provides a variety of methods of filtering the sensors
    /// requested from PRTG, including by sensor name, ID, status and tags as well as parent probe/group/device. Multiple filters
    /// can be used in conjunction to further limit the number of results returned.</para>
    /// <para type="description">For scenarios in which you wish to filter on properties not covered by parameters available in Get-Sensor,
    /// a custom <see cref="SearchFilter"/> object can be created specifying the field name, condition and value to filter upon. For information
    /// on properties that can be filtered on, see New-SearchFilter. When searching for Sensor Factory objects, please note that these
    /// objects do not respond to server side filters by "type". To filter for Sensor Factory sensors, filter by the tag "factorysensor".</para>
    /// <para type="description">When invoked with no arguments, Get-Sensor will query the number of sensors present on your PRTG Server.
    /// If PrtgAPI detects the number is about a specified threshold, PrtgAPI will split the request up into several smaller requests
    /// which will each be invoked in parallel. Results will then be "streamed" to the pipeline in the order they arrive. A progress
    /// bar will also be visible up the top indicating the total number of sensors retrieved/remaining.</para>
    /// <para type="description">If you attempt to cancel a large request (Ctrl+C) and immediately issue another request (of any size),
    /// PRTG may fail to immediately respond until it has finished processing the request you initially issued. Please keep this in mind
    /// when dealing with systems with an extreme number of sensors (>10,000).</para>
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
    ///     <code>C:\> Get-Sensor -Tags wmimemorysensor</code>
    ///     <para>Get all sensors that have the tag "wmimemorysensor"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Count 1</code>
    ///     <para>Get only 1 sensor from PRTG.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> New-SearchFilter type contains deprecated | Get-Sensor</code>
    ///     <para>Get all deprecated sensors.</para>
    /// </example>
    /// 
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// <para type="link">Get-Channel</para>
    /// <para type="link">New-SearchFilter</para> 
    /// </summary>
    [OutputType(typeof(Sensor))]
    [Cmdlet(VerbsCommon.Get, "Sensor")]
    public class GetSensor : PrtgTableCmdlet<Sensor, SensorParameters>
    {
        /// <summary>
        /// <para type="description">The device to retrieve sensors for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">The probe to retrieve sensors for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Probe Probe { get; set; }

        /// <summary>
        /// <para type="description">The group to retrieve sensors for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Group Group { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSensor"/> class.
        /// </summary>
        public GetSensor() : base(Content.Sensors, 500)
        {
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            if (Device != null)
                AddPipelineFilter(Property.ParentId, Device.Id);
            if (Group != null)
                AddPipelineFilter(Property.Group, Group.Name);
            if (Probe != null)
                AddPipelineFilter(Property.Probe, Probe.Name);

            base.ProcessAdditionalParameters();
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving sensors from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override SensorParameters CreateParameters() => new SensorParameters();
    }
}
