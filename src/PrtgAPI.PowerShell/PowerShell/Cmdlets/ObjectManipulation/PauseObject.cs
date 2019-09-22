using System;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Disables monitoring on a PRTG object.</para>
    /// 
    /// <para type="description">The Pause-Object cmdlet disables monitoring of an object in PRTG. When an object is paused, all children
    /// of the object are paused as well. Child objects can be independently paused and unpaused while their parent is paused,
    /// however their states will not modify as long as their parent is overriding them.</para>
    /// 
    /// <para type="description">When pausing an object, you must specify how long to pause the object for. While in a paused state
    /// PRTG will not attempt to execute any sensor objects covered by the paused object. Objects that have not been paused forever
    /// will be automatically unpaused when their pause duration expires. For information on how to unpause an object manually, see Resume-Object.</para>
    /// 
    /// <para type="description">By default, Pause-Object will operate in Batch Mode. In Batch Mode, Pause-Object
    /// will not execute a request for each individual object, but will rather store each item in a queue to pause
    /// all objects at once, via a single request. This allows PrtgAPI to be extremely performant in performing operations
    /// against a large number of objects.</para>
    /// 
    /// <para type="description">If the pipeline is cancelled (either due to a cmdlet throwing an exception
    /// or the user pressing Ctrl-C) before fully completing, Pause-Object will not generate a request against PRTG.
    /// If you wish to disable Batch Mode and fully process objects individually one at a time, this can be achieved
    /// by specifying -Batch:$false.</para>
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
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Pause-Object -Id 1001 -Duration 10</code>
    ///     <para>Pause the object with ID 1001 for 10 minutes.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/State-Manipulation#pause-1">Online version:</para>
    /// <para type="link">Resume-Object</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Suspend, "Object", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class PauseObject : PrtgTimedStateCmdlet
    {
        /// <summary>
        /// <para type="description">The object to pause.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Until)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Forever)]
        public SensorOrDeviceOrGroupOrProbe Object
        {
            get { return (Sensor) ObjectInternal; }
            set { ObjectInternal = value; }
        }

        /// <summary>
        /// <para type="description">ID of the object to pause.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.UntilManual)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.ForeverManual)]
        public int[] Id
        {
            get { return IdInternal; }
            set { IdInternal = value; }
        }

        /// <summary>
        /// <para type="description">The duration to pause the object for, in minutes.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Default, HelpMessage = "The duration to pause the object for, in minutes.")]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual, HelpMessage = "The duration to pause the object for, in minutes.")]
        public int Duration
        {
            get { return DurationInternal; }
            set { DurationInternal = value; }
        }

        /// <summary>
        /// <para type="description">The datetime at which the object should be unpaused.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Until)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.UntilManual)]
        public DateTime Until
        {
            get { return UntilInternal; }
            set { UntilInternal = value; }
        }

        /// <summary>
        /// <para type="description">Indicates the object should be paused indefinitely.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Forever)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.ForeverManual)]
        public SwitchParameter Forever
        {
            get { return ForeverInternal; }
            set { ForeverInternal = value; }
        }

        /// <summary>
        /// <para type="description">A message to display on the object indicating the reason it is paused.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Message { get; set; }

        internal override void Action(int[] ids) => client.PauseObject(ids, duration, Message);

        /// <summary>
        /// Initializes a new instance of the <see cref="AcknowledgeSensor"/> class.
        /// </summary>
        public PauseObject() : base("Pausing", "object", true)
        {
        }
    }
}
