using System;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Mark a <see cref="SensorStatus.Down"/> sensor as <see cref="SensorStatus.DownAcknowledged"/>.</para>
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
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Pause-Object</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Confirm, "Sensor", SupportsShouldProcess = true)]
    public class AcknowledgeSensor : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The sensor to acknowledge.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Default")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Until")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Forever")]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// <para type="description">A message to display on the object indicating the reason it is acknowledged.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Default")]
        [Parameter(Mandatory = false, ParameterSetName = "Until")]
        [Parameter(Mandatory = false, ParameterSetName = "Forever")]
        public string Message { get; set; }

        /// <summary>
        /// <para type="description">The duration to acknowledge the sensor for, in minutes.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Default")]
        public int? Duration { get; set; }

        /// <summary>
        /// <para type="description">The datetime at which the object should become unacknowledged.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Until")]
        public DateTime? Until { get; set; }

        /// <summary>
        /// <para type="description">Indicates the object should be acknowledged indefinitely.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Forever")]
        public SwitchParameter Forever { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            int? duration = null;

            switch (ParameterSetName)
            {
                case "Default":
                    duration = Duration;
                    break;

                case "Until":
                    duration = (int)Math.Ceiling((Until.Value - DateTime.Now).TotalMinutes);
                    break;

                case "Forever":
                    break;
            }

            if (duration < 1)
                throw new ArgumentException("Duration evaluated to less than one minute. Please specify -Forever or a duration greater than or equal to one minute.");

            if (ShouldProcess($"{Sensor.Name} (ID: {Sensor.Id})"))
                client.AcknowledgeSensor(Sensor.Id, duration, Message);
        }
    }
}
