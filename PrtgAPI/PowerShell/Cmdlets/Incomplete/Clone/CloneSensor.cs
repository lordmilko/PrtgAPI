using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Clones a sensor within PRTG.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1234 | Clone-Sensor 5678</code>
    ///     <para>Clone the sensor with ID 1234 to the device, group or probe with ID 5678</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1234 | Clone-Sensor 5678 MyNewSensor</code>
    ///     <para>Clone the sensor with ID 1234 to the device, group or probe with ID 5678 renamed as "MyNewSensor"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1234 | Clone-Sensor 5678 -Resolve</code>
    ///     <para>Clone the sensor with ID 1234 into the device, group or probe with ID 5678 and retrieve the resultant PrtgObject</para>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Sensor", SupportsShouldProcess = true)]
    public class CloneSensor : CloneSensorOrGroup<Sensor>
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
                ProcessRecordEx(Sensor.Id, Sensor.Name, id => client.GetSensors(Property.Id, id));
            }
        }
    }
}