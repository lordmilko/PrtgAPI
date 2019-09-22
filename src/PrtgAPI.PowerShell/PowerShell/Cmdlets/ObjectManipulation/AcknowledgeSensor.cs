using System;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Marks a <see cref="Status.Down"/> sensor as <see cref="Status.DownAcknowledged"/>.</para>
    /// 
    /// <para type="description">The Acknowledge-Sensor cmdlet acknowledges a sensor that is currently in a 'Down' state. When a sensor
    /// is acknowledged, it no longer generates notification triggers and is moved from the 'Down' sensors page to the 'Down (Acknowledged)'
    /// sensors page.</para>
    /// 
    /// <para type="description">When acknowledging a sensor, you must specify how long to acknowledge the sensor for. If the sensor is still in a down
    /// state when the acknowledgement period expires, the sensor will return to a Down state. While in a Down (Acknowledged) state
    /// PRTG will continue refreshing the sensor according to its scanning interval. If at any time the requisite conditions are met
    /// for the sensor to enter an 'Up' state, the sensor will turn 'Up' and no further action will be required. If an acknowledged
    /// sensor that has gone Up fails and returns to a Down state, the sensor will need to be re-acknowledged.</para>
    /// <para type="description">If a sensor is continually flapping, it may be better to pause the sensor rather than acknowledge it.
    /// For more information on pausing objects, see Pause-Object.</para>
    /// 
    /// <para type="description">By default, Acknowledge-Sensor will operate in Batch Mode. In Batch Mode, Acknowledge-Sensor
    /// will not execute a request for each individual object, but will rather store each item in a queue to acknowledge
    /// all objects at once, via a single request. This allows PrtgAPI to be extremely performant in performing operations
    /// against a large number of objects.</para>
    /// 
    /// <para type="description">If the pipeline is cancelled (either due to a cmdlet throwing an exception
    /// or the user pressing Ctrl-C) before fully completing, Acknowledge-Sensor will not generate a request against PRTG.
    /// If you wish to disable Batch Mode and fully process objects individually one at a time, this can be achieved
    /// by specifying -Batch:$false.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Status Down | Acknowledge-Sensor -Duration 60</code>
    ///     <para>Acknowledge all down sensors for the next 60 minutes</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Status Down | Acknowledge-Sensor -Until (Get-Date).AddDays(30) -Message "Honestly, CBF"</code>
    ///     <para>Acknowledge all down sensors for the next 30 days with a message explaining the reason the sensor was acknowledged</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor Ping -Status Down | Acknowledge-Sensor -Forever</code>
    ///     <para>Acknowledge all down ping sensors forever (or until they comes back up by themselves)</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Acknowledge-Sensor -Id 1001 -Duration 10</code>
    ///     <para>Acknowledge the sensor with ID 1001 for 10 minutes.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/State-Manipulation#acknowledge-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Pause-Object</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Confirm, "Sensor", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class AcknowledgeSensor : PrtgTimedStateCmdlet
    {
        /// <summary>
        /// <para type="description">The sensor to acknowledge.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Until)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Forever)]
        public Sensor Sensor
        {
            get { return (Sensor) ObjectInternal; }
            set { ObjectInternal = value; }
        }

        /// <summary>
        /// <para type="description">ID of the sensor to acknowledge.</para>
        /// </summary>
        [Alias("SensorId")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.UntilManual)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.ForeverManual)]
        public int[] Id
        {
            get { return IdInternal; }
            set { IdInternal = value; }
        }

        /// <summary>
        /// <para type="description">The duration to acknowledge the sensor for, in minutes.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int Duration
        {
            get { return DurationInternal; }
            set { DurationInternal = value; }
        }

        /// <summary>
        /// <para type="description">The datetime at which the object should become unacknowledged.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Until)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.UntilManual)]
        public DateTime Until
        {
            get { return UntilInternal; }
            set { UntilInternal = value; }
        }

        /// <summary>
        /// <para type="description">Indicates the object should be acknowledged indefinitely.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Forever)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.ForeverManual)]
        public SwitchParameter Forever
        {
            get { return ForeverInternal; }
            set { ForeverInternal = value; }
        }

        /// <summary>
        /// <para type="description">A message to display on the object indicating the reason it is acknowledged.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Message { get; set; }

        internal override void Action(int[] ids) => client.AcknowledgeSensor(ids, duration, Message);

        /// <summary>
        /// Initializes a new instance of the <see cref="AcknowledgeSensor"/> class.
        /// </summary>
        public AcknowledgeSensor() : base("Acknowledging", "sensor", false)
        {
        }
    }
}
