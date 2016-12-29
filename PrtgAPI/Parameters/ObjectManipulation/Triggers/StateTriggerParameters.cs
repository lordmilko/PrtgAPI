using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Helpers;

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
        public NotificationAction OffNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OffNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OffNotificationAction, value); }
        }

        /// <summary>
        /// The <see cref="NotificationAction"/> to fire if this trigger remains in an active state for an extended period of time.
        /// </summary>
        public NotificationAction EscalationNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.EscalationNotificationAction); }
            set { SetNotificationAction(TriggerProperty.EscalationNotificationAction, value); }
        }

        /// <summary>
        /// The delay (in seconds) this trigger should wait before executing its <see cref="TriggerParameters.OnNotificationAction"/> once activated.
        /// </summary>
        public int? Latency
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.Latency); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Latency, value);
            }
        }

        /// <summary>
        /// The delay (in seconds) this trigger should wait before executing its <see cref="EscalationNotificationAction"/>.
        /// </summary>
        public int? EscalationLatency
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.EscalationLatency); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.EscalationLatency, value);
            }
        }
        //todo our adder function should validate each property has a value, and tell the caller they messed up

        /// <summary>
        /// The interval (in minutes) with which the <see cref="EscalationNotificationAction"/> should be re-executed.
        /// </summary>
        public int? RepeatInterval
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.RepeatInterval); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.RepeatInterval, value);
            }
        }

        /// <summary>
        /// The object state that will cause this trigger to activate.
        /// </summary>
        public TriggerSensorState? State
        {
            get
            {
                var value = GetCustomParameterValue(TriggerProperty.StateTrigger);

                //If the value is null, cast to nullable. Otherwise, cast to a concrete value (then implicitly to nullable)
                return value == null ? (TriggerSensorState?) null : (TriggerSensorState) value;
            }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.StateTrigger, (int?) value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTriggerParameters"/> class.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="triggerId">If this trigger is being edited, the trigger's sub ID. If this trigger is being added, this value is null.</param>
        /// <param name="action">Whether to add a new trigger or modify an existing one.</param>
        /// <param name="state">The object state that will cause this trigger to activate.</param>
        public StateTriggerParameters(int objectId, int? triggerId, ModifyAction action, TriggerSensorState? state) : base(TriggerType.State, objectId, triggerId, action)
        {
            if (action == ModifyAction.Add)
            {
                if (state == null)
                    throw new ArgumentException("TriggerSensorState is mandatory when ModifyAction is Add", nameof(state));

                EscalationNotificationAction = null;
                OffNotificationAction = null;
                State = state.Value;
            }

            //we need to check for null values when modifyaction is add for EVERYTHING

            //if (state != null)
            //    AddTriggerParameter(TriggerProperty.StateTrigger, ((int?)state).ToString());
            //AddTriggerParameter(TriggerProperty.Latency, latency.ToString());
            //AddTriggerParameter(TriggerProperty.OnNotificationAction, FormatNotificationAction(onNotificationAction));
            //what does the "no notification" action look like
        }
    }
}
