using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Disable monitoring on an object.
    /// </summary>
    [Cmdlet("Pause", "Object")]
    public class PauseObject : PrtgCmdlet
    {
        /// <summary>
        /// The object to pause.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Default")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Until")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Forever")]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// A message to display on the object indicating the reason it is paused.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Default")]
        [Parameter(Mandatory = false, ParameterSetName = "Until")]
        [Parameter(Mandatory = false, ParameterSetName = "Forever")]
        public string Message { get; set; }

        /// <summary>
        /// The duration to pause the object for, in minutes.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Default", HelpMessage = "The duration to pause the object for, in minutes.")]
        public int? Duration { get; set; }

        /// <summary>
        /// The datetime at which the object should be unpaused.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Until")]
        public DateTime? Until { get; set; }

        /// <summary>
        /// Indicates the object should be paused indefinitely.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Forever")]
        public SwitchParameter Forever { get; set; }

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

            client.Pause(Object.Id, Message, duration);
        }
    }
}
