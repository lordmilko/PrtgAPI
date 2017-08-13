using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Clones a sensor within PRTG.</para>
    /// 
    /// <para type="description">The Clone-Sensor cmdlet duplicates a PRTG Sensor, including all channels defined under it.</para>
    /// <para type="description">To clone a sensor, you must specify the Object ID of the device the cloned sensor will sit under.
    /// If a Name is not specified, the name of the sensor being cloned will be used.</para>
    /// <para type="description">When a sensor has been cloned, by default Clone-Sensor will attempt to resolve the object into a PrtgAPI Sensor.
    /// Based on the speed of your PRTG Server, this can sometimes result in a delay of 5-10 seconds due to the delay with which
    /// PRTG clones the object. If Clone-Sensor cannot resolve the resultant object on the first attempt, PrtgAPI will make a further
    /// 10 retries, pausing for a successively greater duration between each try. After each failed attempt a warning will be displayed indicating
    /// the number of attempts remaining. Object resolution can be aborted at any time by pressing an escape sequence such as Ctrl+C.</para>
    /// <para type="description">If you do not wish to resolve the resultant object, you can specify -Resolve:$false, which will
    /// cause Clone-Sensor to output a clone summary, including the object ID and name of the new object. As PRTG pauses all cloned objects
    /// by default, it is generally recommended to resolve the new object so that you may unpause the object with Resume-Object.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1234 | Clone-Sensor 5678</code>
    ///     <para>Clone the sensor with ID 1234 to the device with ID 5678</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1234 | Clone-Sensor 5678 MyNewSensor</code>
    ///     <para>Clone the sensor with ID 1234 to the device with ID 5678 renamed as "MyNewSensor"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1234 | Clone-Sensor 5678 -Resolve:$false</code>
    ///     <para>Clone the sensor with ID 1234 into the device with ID 5678 without resolving the resultant PrtgObject</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Clone-Device</para>
    /// <para type="link">Clone-Group</para>
    /// <para type="link">Resume-Object</para>
    /// </summary>
    [OutputType(typeof(Sensor))]
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