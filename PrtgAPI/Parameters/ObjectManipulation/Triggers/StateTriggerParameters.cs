using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    public class StateTriggerParameters : TriggerParameters
    {
        public NotificationAction OffNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OffNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OffNotificationAction, value); }
        }

        public NotificationAction EscalationNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.EscalationNotificationAction); }
            set { SetNotificationAction(TriggerProperty.EscalationNotificationAction, value); }
        }

        public int? Latency
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.Latency); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Latency, value);
            }
        }

        public int? EscalationLatency
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.EscalationLatency); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.EscalationLatency, value);
            }
        }
        //our adder function should validate each property has a value, and tell the caller they messed up
        public int? RepeatInterval
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.RepeatInterval); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.RepeatInterval, value);
            }
        }

        public TriggerSensorState? State
        {
            get
            {
                var value = GetCustomParameterValue(TriggerProperty.StateTrigger);

                return value == null ? (TriggerSensorState?) null : (TriggerSensorState) value;
            }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.StateTrigger, (int?) value);
            }
        }

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
