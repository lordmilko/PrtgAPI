using System;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a new set of notification trigger parameters for adding or editing a notification trigger.</para>
    /// 
    /// <para type="description">The New-NotificationTriggerParameters cmdlet creates a set of trigger parameters for creating or editing
    /// a notification trigger. When editing notification triggers, New-NotificationTriggerParameters should only be used when multiple
    /// values require updating. For updating a single notification trigger property Edit-NotificationTriggerProperty should be
    /// used instead.</para>
    /// 
    /// <para type="description">When creating a new notification trigger, the trigger's parameters can either be imported
    /// from an existing notification trigger's properties and then further modified, or defined manually from scratch.</para>
    /// 
    /// <para type="description">Based on the type of notification trigger specified, New-NotificationTriggerParameters will
    /// create one of several TriggerParameter type objects, exposing only the parameters relevant to that trigger type.</para>
    /// 
    /// <para type="description">When working with TriggerParameters objects, all parameter properties support nullable values,
    /// allowing you to clear any properties you wish to remove or undo. The exception to this however is Notification Action
    /// related properties. When a Notification Action is set to null, it will set the property to the "empty" notification
    /// action. This allows you to easily clear unwanted notification actions within the PRTG Interface.</para>
    /// 
    /// <para type="description">When editing existing notification triggers, all properties on the TriggerParameters object
    /// will be initially set to null. Specifying a value for a property will highlight to PrtgAPI that that property should
    /// be updated when the trigger request is executed. While holding a value of null by default, Notification Actions will
    /// not set themselves with the "empty" notification action unless this value is explicitly specified. Note that
    /// if you wish to undo modifying a notification action property you have set to null, you will be unable to do so
    /// without creating a brand new TriggerParameters object.</para>
    /// 
    /// <para type="description">For trigger types where the Channel property is supported (Speed, Threshold and Volume)
    /// when defining the trigger on a Probe, Group or Device, enum values "Primary", "Total", "TrafficIn" and "TrafficOut" must be used.
    /// When applied directly to a Sensor, a specific Channel or Channel ID must be used. To safely resolve all channels
    /// supported by the sensor the Get-Channel cmdlet can be used. Once a Channel or Channel ID has been assigned to
    /// a trigger parameter's Channel property, it is wrapped as a TriggerChannel object and the underlying type cannot be retrieved.
    /// As such, if you wish to refer to your original type, you must keep a copy of the value assigned to the Channel property for
    /// further use in your code.</para>
    /// 
    /// <example>
    ///     <code>
    ///         C:\> $params = New-TriggerParameters 1234 State
    ///
    ///         C:\> $params | Add-Trigger
    ///     </code>
    ///     <para>Add a new state notification trigger with default values to object with ID 1234</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = New-TriggerParameters 1234 3 State
    ///         C:\> $params.Latency = 40
    ///
    ///         C:\> $params | Set-Trigger
    ///     </code>
    ///     <para>Edit the state notification trigger with sub ID 3 on the object with ID 1234, setting the Latency to 40 seconds</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = Get-Sensor -Id 1001 | Get-Trigger -Type State -Inherited $false | New-TriggerParameters 1234
    ///         C:\> $params.OffNotificationAction = $null
    ///
    ///         C:\> $params | Add-Trigger
    ///     </code>
    ///     <para>Create a new notification trigger on the object with ID 1234 from the state trigger on the sensor with ID 1001, setting the OffNotificationAction to "None"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = Get-Sensor -Id 1001 | Get-Trigger -Type State -Inherited $false | New-TriggerParameters
    ///         C:\> $params.Latency = 30
    ///
    ///         C:\> $params | Set-Trigger
    ///     </code>
    ///     <para>Edit the notification trigger on the sensor with ID 1001, setting the latency to 30 seconds</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $sensor = Get-Sensor -Id 1001
    ///         C:\> $channel = $sensor | Get-Channel "Available Memory"
    /// 
    ///         C:\> $params = $sensor | New-TriggerParameters Threshold
    ///         C:\> $params.Channel = $channel
    ///
    ///         C:\> $params | Add-Trigger
    ///     </code>
    ///     <para>Create a new notification trigger on the sensor with ID 1001 that alerts based on the value of its "Available Memory" channel.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = New-TriggerParameters 1001 Threshold
    ///         C:\> $params.Channel = 1
    ///
    ///         C:\> $params | Add-Trigger
    ///     </code>
    ///     <para>Create a new threshold notification trigger on the sensor with ID 1001 that alerts based on the value of the channel with ID 1.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Notification-Triggers#add-1">Online version:</para>
    /// <para type="link">Edit-NotificationTriggerProperty</para>
    /// <para type="link">Add-NotificationTrigger</para> 
    /// <para type="link">Set-NotificationTrigger</para>
    /// <para type="link">Get-Channel</para>
    /// 
    /// //todo: i dont think requirevalueattribute is working? when should it work? how do we test creating some corrupt triggers?
    /// //todo: have examples for the various ways of setting the channel for a threshold trigger, plus some examples for setting the various properties of volume/speed/threshold triggers
    /// </summary>
    [OutputType(typeof(TriggerParameters))]
    [Cmdlet(VerbsCommon.New, "NotificationTriggerParameters")]
    public class NewNotificationTriggerParameters : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The object a trigger should be created for.</para>
        /// </summary>
        [Parameter(ParameterSetName = ParameterSet.Add, Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">The ID of the object the notification trigger will be created for or the ID of the object whose triggers should be modified.</para>
        /// </summary>
        [Parameter(ParameterSetName = ParameterSet.AddManual, Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the object the notification trigger will be created for.")]
        [Parameter(ParameterSetName = ParameterSet.EditManual, Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the object the notification trigger will be created for.")]
        [Parameter(ParameterSetName = ParameterSet.AddFrom, Mandatory = true, Position = 0, HelpMessage = "The ID of the object the notification trigger will be created for.")]
        public int? Id { get; set; }

        /// <summary>
        /// <para type="description">The notification trigger import trigger parameters from.</para>
        /// </summary>
        [Parameter(ParameterSetName = ParameterSet.AddFrom, Mandatory = true, ValueFromPipeline = true, HelpMessage = "The notification trigger whose properties will be used as the basis of creating a new trigger.")]
        [Parameter(ParameterSetName = ParameterSet.EditFrom, Mandatory = true, ValueFromPipeline = true, HelpMessage = "The notification trigger whose properties will be used as the basis of creating a new trigger.")]
        public NotificationTrigger Source { get; set; }

        /// <summary>
        /// <para type="description">The Sub ID of the trigger to manipulate.</para>
        /// </summary>
        [Parameter(ParameterSetName = ParameterSet.EditManual, Mandatory = true, Position = 1, HelpMessage = "The sub ID of the notification trigger to edit.")]
        public int? TriggerId { get; set; }

        /// <summary>
        /// <para type="description">The type of notification trigger to manipulate.</para>
        /// </summary>
        [Parameter(ParameterSetName = ParameterSet.Add, Mandatory = true, Position = 0, HelpMessage = "The type of notification trigger to create.")]
        [Parameter(ParameterSetName = ParameterSet.AddManual, Mandatory = true, Position = 1, HelpMessage = "The type of notification trigger to create.")]
        [Parameter(ParameterSetName = ParameterSet.EditManual, Mandatory = true, Position = 2, HelpMessage = "The type of notification trigger to edit.")]
        public TriggerType? Type { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Source != null)
                Type = Source.Type;

            switch (Type)
            {
                case TriggerType.Change:
                    WriteObject(CreateParameters<ChangeTriggerParameters>());
                    break;
                case TriggerType.Speed:
                    WriteObject(CreateParameters<SpeedTriggerParameters>());
                    break;
                case TriggerType.State:
                    WriteObject(CreateParameters<StateTriggerParameters>());
                    break;
                case TriggerType.Threshold:
                    WriteObject(CreateParameters<ThresholdTriggerParameters>());
                    break;
                case TriggerType.Volume:
                    WriteObject(CreateParameters<VolumeTriggerParameters>());
                    break;
                default:
                    throw new NotImplementedException($"Handler of trigger type '{Type}' is not implemented.");
            }
        }

        private T CreateParameters<T>()
        {
            try
            {
                if (Object != null)
                    Id = Object.Id;

                if (Source != null)
                {
                    if (Id != null)
                        return (T)Activator.CreateInstance(typeof(T), MakeEither(Id.Value), Source); //Add from an existing notification trigger
                    else
                        return (T)Activator.CreateInstance(typeof(T), Source); //Edit from an existing notification trigger
                }

                if (TriggerId != null)
                    return (T)Activator.CreateInstance(typeof(T), MakeEither(Id.Value), TriggerId); //Edit a notification trigger
                else
                    return (T)Activator.CreateInstance(typeof(T), MakeEither(Id.Value)); //Create a new notification trigger
            }
            catch(TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;

                throw;
            }
        }

        private PrtgAPI.Either<IPrtgObject, int> MakeEither(PrtgAPI.Either<IPrtgObject, int> value) => value;
    }
}
