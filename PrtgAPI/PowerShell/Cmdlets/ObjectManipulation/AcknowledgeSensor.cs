using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Mark a <see cref="SensorStatus.Down"/> sensor as <see cref="SensorStatus.DownAcknowledged"/>.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Confirm, "Sensor")]
    public class AcknowledgeSensor : PrtgCmdlet
    {
        /// <summary>
        /// The sensor to acknowledge.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Default")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Until")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Forever")]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// A message to display on the object indicating the reason it is acknowledged.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Default")]
        [Parameter(Mandatory = false, ParameterSetName = "Until")]
        [Parameter(Mandatory = false, ParameterSetName = "Forever")]
        public string Message { get; set; }

        /// <summary>
        /// The duration to acknowledge the sensor for, in minutes.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Default")]
        public int? Duration { get; set; }

        /// <summary>
        /// The datetime at which the object should become unacknowledged.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Until")]
        public DateTime? Until { get; set; }

        /// <summary>
        /// Indicates the object should be acknowledged indefinitely.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Forever")]
        public SwitchParameter Forever { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            int? duration = null;

            switch (ParameterSetName)
            {
                case "Default":
                    duration = Duration;
                    break;

                case "Until":
                    duration = (int)Math.Floor((Until.Value - DateTime.Now).TotalMinutes);
                    break;

                case "Forever":
                    break;

            }

            client.AcknowledgeSensor(Sensor.Id, Message, duration);
        }
    }
}
