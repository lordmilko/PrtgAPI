using PrtgAPI.Attributes;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for adding/modifying <see cref="TriggerType.State"/> <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public class StateTriggerParameters : TriggerParameters
    {
        /// <summary>
        /// The <see cref="NotificationAction"/> to execute when the trigger's active state clears.
        /// </summary>
        [RequireValue(false)]
        [PropertyParameter(nameof(TriggerProperty.OffNotificationAction))]
        public NotificationAction OffNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OffNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OffNotificationAction, value); }
        }

        /// <summary>
        /// The <see cref="NotificationAction"/> to fire if this trigger remains in an active state for an extended period of time.
        /// </summary>
        [RequireValue(false)]
        [PropertyParameter(nameof(TriggerProperty.EscalationNotificationAction))]
        public NotificationAction EscalationNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.EscalationNotificationAction); }
            set { SetNotificationAction(TriggerProperty.EscalationNotificationAction, value); }
        }

        /// <summary>
        /// The delay (in seconds) this trigger should wait before executing its <see cref="TriggerParameters.OnNotificationAction"/> once activated.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.Latency))]
        public int? Latency
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.Latency); }
            set { UpdateCustomParameter(TriggerProperty.Latency, value); }
        }

        /// <summary>
        /// The delay (in seconds) this trigger should wait before executing its <see cref="EscalationNotificationAction"/>.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.EscalationLatency))]
        public int? EscalationLatency
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.EscalationLatency); }
            set { UpdateCustomParameter(TriggerProperty.EscalationLatency, value); }
        }
        //todo our adder function should validate each property has a value, and tell the caller they messed up

        /// <summary>
        /// The interval (in minutes) with which the <see cref="EscalationNotificationAction"/> should be re-executed.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.RepeatInterval))]
        public int? RepeatInterval
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.RepeatInterval); }
            set { UpdateCustomParameter(TriggerProperty.RepeatInterval, value); }
        }

        /// <summary>
        /// The object state that will cause this trigger to activate.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.State))]
        public TriggerSensorState? State
        {
            get { return (TriggerSensorState?) GetCustomParameterEnumInt<TriggerSensorState>(TriggerProperty.State); }
            set { UpdateCustomParameter(TriggerProperty.State, (int?) value, true); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTriggerParameters"/> class for creating a new notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        public StateTriggerParameters(int objectId) : base(TriggerType.State, objectId, (int?)null, ModifyAction.Add)
        {
            EscalationNotificationAction = null;
            OffNotificationAction = null;
            State = TriggerSensorState.Down;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTriggerParameters"/> class for editing an existing notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger is applied to.</param>
        /// <param name="triggerId">The sub ID of the trigger on its parent object.</param>
        public StateTriggerParameters(int objectId, int triggerId) : base(TriggerType.State, objectId, triggerId, ModifyAction.Edit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTriggerParameters"/> class for creating a new trigger from an existing <see cref="TriggerType.State"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used.</param>
        /// <param name="action">Whether these parameters will create a new trigger or edit an existing one.</param>
        public StateTriggerParameters(int objectId, NotificationTrigger sourceTrigger, ModifyAction action) : base(TriggerType.State, objectId, sourceTrigger, action)
        {
            if (action == ModifyAction.Add)
            {
                OffNotificationAction = sourceTrigger.OffNotificationAction;
                EscalationNotificationAction = sourceTrigger.EscalationNotificationAction;
                Latency = sourceTrigger.Latency;
                EscalationLatency = sourceTrigger.EscalationLatency;
                RepeatInterval = sourceTrigger.RepeatInterval;
                State = sourceTrigger.StateTrigger;
            }
        }
    }
}
