using PrtgAPI.Attributes;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for adding/modifying <see cref="TriggerType.State"/> <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public class StateTriggerParameters : TriggerParameters
    {
        /// <summary>
        /// Gets or sets the <see cref="NotificationAction"/> to execute when the trigger's active state clears.
        /// </summary>
        [RequireValue(false)]
        [PropertyParameter(TriggerProperty.OffNotificationAction)]
        public NotificationAction OffNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OffNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OffNotificationAction, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="NotificationAction"/> to fire if this trigger remains in an active state for an extended period of time.
        /// </summary>
        [RequireValue(false)]
        [PropertyParameter(TriggerProperty.EscalationNotificationAction)]
        public NotificationAction EscalationNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.EscalationNotificationAction); }
            set { SetNotificationAction(TriggerProperty.EscalationNotificationAction, value); }
        }

        /// <summary>
        /// Gets or sets the delay (in seconds) this trigger should wait before executing its <see cref="TriggerParameters.OnNotificationAction"/> once activated.
        /// </summary>
        [PropertyParameter(TriggerProperty.Latency)]
        public int? Latency
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.Latency); }
            set { UpdateCustomParameter(TriggerProperty.Latency, value); }
        }

        /// <summary>
        /// Gets or sets the delay (in seconds) this trigger should wait before executing its <see cref="EscalationNotificationAction"/>.
        /// </summary>
        [PropertyParameter(TriggerProperty.EscalationLatency)]
        public int? EscalationLatency
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.EscalationLatency); }
            set { UpdateCustomParameter(TriggerProperty.EscalationLatency, value); }
        }
        //todo our adder function should validate each property has a value, and tell the caller they messed up

        /// <summary>
        /// Gets or sets the interval (in minutes) with which the <see cref="EscalationNotificationAction"/> should be re-executed.
        /// </summary>
        [PropertyParameter(TriggerProperty.RepeatInterval)]
        public int? RepeatInterval
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.RepeatInterval); }
            set { UpdateCustomParameter(TriggerProperty.RepeatInterval, value); }
        }

        /// <summary>
        /// Gets or sets the object state that will cause this trigger to activate.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(TriggerProperty.State)]
        public TriggerSensorState? State
        {
            get { return (TriggerSensorState?) GetCustomParameterEnumInt<TriggerSensorState>(TriggerProperty.State); }
            set { UpdateCustomParameter(TriggerProperty.State, (int?) value, true); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTriggerParameters"/> class for creating a new notification trigger.
        /// </summary>
        /// <param name="objectOrId">The object or object ID the trigger will apply to.</param>
        public StateTriggerParameters(Either<IPrtgObject, int> objectOrId) : base(TriggerType.State, objectOrId, (int?)null, ModifyAction.Add)
        {
            EscalationNotificationAction = null;
            OffNotificationAction = null;
            State = TriggerSensorState.Down;
            Latency = 60;
            EscalationLatency = 300;
            RepeatInterval = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTriggerParameters"/> class for editing an existing notification trigger.
        /// </summary>
        /// <param name="objectOrId">The object or object ID the trigger is applied to. Note: if the trigger is inherited, the ParentId should be specified.</param>
        /// <param name="triggerId">The sub ID of the trigger on its parent object.</param>
        public StateTriggerParameters(Either<IPrtgObject, int> objectOrId, int triggerId) : base(TriggerType.State, objectOrId, triggerId, ModifyAction.Edit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTriggerParameters"/> class for creating a new trigger from an existing <see cref="TriggerType.State"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="objectOrId">The object or object ID the trigger will apply to.</param>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used.</param>
        public StateTriggerParameters(Either<IPrtgObject, int> objectOrId, NotificationTrigger sourceTrigger) : base(TriggerType.State, objectOrId, sourceTrigger, ModifyAction.Add)
        {
            OffNotificationAction = sourceTrigger.OffNotificationAction;
            EscalationNotificationAction = sourceTrigger.EscalationNotificationAction;
            Latency = sourceTrigger.Latency;
            EscalationLatency = sourceTrigger.EscalationLatency;
            RepeatInterval = sourceTrigger.RepeatInterval;
            State = sourceTrigger.State;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTriggerParameters"/> class for editing an existing <see cref="TriggerType.State"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used.</param>
        public StateTriggerParameters(NotificationTrigger sourceTrigger) : base(TriggerType.State, sourceTrigger.ObjectId, sourceTrigger, ModifyAction.Edit)
        {
        }
    }
}
