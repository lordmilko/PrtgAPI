using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class ThresholdTriggerParameters : TriggerParameters
    {
        public NotificationAction OffNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OffNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OffNotificationAction, value); }
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

        public int? Threshold
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.Threshold); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Threshold, value);
            }
        }

        public TriggerCondition? Condition
        {
            get { return (TriggerCondition?)GetCustomParameterValue(TriggerProperty.Condition); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Condition, value);
            }
        }

        public TriggerChannel? Channel
        {
            get { return (TriggerChannel?)GetCustomParameterValue(TriggerProperty.Channel); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Channel, value);
            }
        }

        public ThresholdTriggerParameters(int objectId, int? triggerId, ModifyAction action) : base(TriggerType.Volume, objectId, triggerId, action)
        {
            if (action == ModifyAction.Add)
                OffNotificationAction = null;
        }
    }
}
