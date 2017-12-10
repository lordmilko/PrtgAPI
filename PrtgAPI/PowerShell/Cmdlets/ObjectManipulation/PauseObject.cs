using System;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Disables monitoring on a PRTG object.</para>
    /// 
    /// <para type="description">The Pause-Object cmdlet disables monitoring of an object in PRTG. When an object is paused, all children
    /// of the object are paused as well. Child objects can be independently paused and unpaused while their parent is paused,
    /// however their states will not modify as long as their parent is overriding them.</para>
    /// <para type="description">When pausing an object, you must specify how long to pause the object for. While in a paused state
    /// PRTG will not attempt to execute any sensor objects covered by the paused object.</para>
    /// <para>Objects that have not been paused forever will be automatically unpaused when their pause duration expires. For information
    /// on how to unpause an object manually, see Resume-Object.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 2001 | Pause-Object -Duration 60</code>
    ///     <para>Pause the object with ID 2001 for 60 minutes.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe *chicago* | Pause-Object -Until (Get-Date).AddDays(2) -Message "Office move in progress"</code>
    ///     <para>Pause all probes whose names contain "chicago" for three days with a message explaining the reason the object was paused.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device fw-1 | Pause-Object -Forever -Message "Decomissioning"</code>
    ///     <para>Pause all devices named "fw-1" forever with a message explaining the reason the object was paused.</para>
    /// </example>
    /// 
    /// <para type="link">Resume-Object</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Suspend, "Object", SupportsShouldProcess = true)]
    public class PauseObject : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The object to pause.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Default")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Until")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Forever")]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">A message to display on the object indicating the reason it is paused.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Default")]
        [Parameter(Mandatory = false, ParameterSetName = "Until")]
        [Parameter(Mandatory = false, ParameterSetName = "Forever")]
        public string Message { get; set; }

        /// <summary>
        /// <para type="description">The duration to pause the object for, in minutes.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Default", HelpMessage = "The duration to pause the object for, in minutes.")]
        public int? Duration { get; set; }

        /// <summary>
        /// <para type="description">The datetime at which the object should be unpaused.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Until")]
        public DateTime? Until { get; set; }

        /// <summary>
        /// <para type="description">Indicates the object should be paused indefinitely.</para>
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

            if (ShouldProcess($"{Object.Name} (ID: {Object.Id})"))
            {
                var t = duration == 1 ? "minute" : "minutes";
                var t2 = Forever.IsPresent ? "forever" : $"for {duration} {t}";
                ExecuteOperation(() => client.PauseObject(Object.Id, duration, Message), $"Pausing PRTG Objects", $"Pausing {Object.BaseType.ToString().ToLower()} '{Object.Name}' {t2}");
            }
        }
    }
}
