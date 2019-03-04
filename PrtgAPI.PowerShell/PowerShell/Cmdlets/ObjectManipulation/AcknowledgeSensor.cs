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
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/State-Manipulation#acknowledge-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Pause-Object</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Confirm, "Sensor", SupportsShouldProcess = true)]
    public class AcknowledgeSensor : PrtgMultiOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The sensor to acknowledge.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Until)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Forever)]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// <para type="description">A message to display on the object indicating the reason it is acknowledged.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Until)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Forever)]
        public string Message { get; set; }

        /// <summary>
        /// <para type="description">The duration to acknowledge the sensor for, in minutes.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Default)]
        public int Duration { get; set; }

        /// <summary>
        /// <para type="description">The datetime at which the object should become unacknowledged.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Until)]
        public DateTime Until { get; set; }

        /// <summary>
        /// <para type="description">Indicates the object should be acknowledged indefinitely.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Forever)]
        public SwitchParameter Forever { get; set; }

        private int? duration;
        private string minutesDescription;
        private string durationDescription;
        private string whatIfDescription;

        internal override string ProgressActivity => "Acknowledge PRTG Sensors";

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessingEx()
        {
            switch (ParameterSetName)
            {
                case ParameterSet.Default:
                    duration = Duration;
                    break;

                case ParameterSet.Until:
                    duration = (int)Math.Ceiling((Until - DateTime.Now).TotalMinutes);
                    break;

                case ParameterSet.Forever:
                    break;

                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }

            if (duration < 1 && ParameterSetName != ParameterSet.Forever)
                throw new ArgumentException("Duration evaluated to less than one minute. Please specify -Forever or a duration greater than or equal to one minute.");

            minutesDescription = duration == 1 ? "minute" : "minutes";
            durationDescription = Forever.IsPresent ? "forever" : $"for {duration} {minutesDescription}";
            whatIfDescription = Forever.IsPresent ? "forever" : $"{duration} {minutesDescription}";
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"{Sensor.Name} (ID: {Sensor.Id}) (Duration: {whatIfDescription})"))
                ExecuteOrQueue(Sensor);
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformSingleOperation()
        {
            ExecuteOperation(() => client.AcknowledgeSensor(Sensor.Id, duration, Message), $"Acknowledging sensor '{Sensor.Name}' {durationDescription}");
        }

        /// <summary>
        /// Invokes this cmdlet's action against all queued items from the pipeline.
        /// </summary>
        /// <param name="ids">The Object IDs of all queued items.</param>
        protected override void PerformMultiOperation(int[] ids)
        {
            ExecuteMultiOperation(() => client.AcknowledgeSensor(ids, duration, Message), $"Acknowledging {GetCommonObjectBaseType()} {GetListSummary()} {durationDescription}");
        }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Sensor;
    }
}
