using System;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Disable monitoring on an object.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Suspend, "Object", SupportsShouldProcess = true)]
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
                    duration = (int)Math.Floor((Until.Value - DateTime.Now).TotalMinutes);
                    break;

                case "Forever":
                    break;
            }

            if(ShouldProcess($"{Object.Name} (ID: {Object.Id})"))
                client.Pause(Object.Id, duration, Message);
        }
    }
}
