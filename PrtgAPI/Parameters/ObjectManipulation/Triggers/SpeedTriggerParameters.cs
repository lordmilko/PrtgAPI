using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class SpeedTriggerParameters : TriggerParameters
    {
        public NotificationAction OffNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OffNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OffNotificationAction, value); }
        }

        public TriggerChannel? Channel
        {
            get { return (TriggerChannel?) GetCustomParameterValue(TriggerProperty.Channel); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Channel, value);
            }
        }

        public int? Latency
        {
            get { return (int?) GetCustomParameterValue(TriggerProperty.Latency); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Latency, value);
            }
        }

        public TriggerCondition? Condition
        {
            get { return (TriggerCondition?) GetCustomParameterValue(TriggerProperty.Condition); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Condition, value);
            }
        }

        public int? Threshold
        {
            get { return (int?) GetCustomParameterValue(TriggerProperty.Threshold); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Threshold, value);
            }
        }

        public TriggerUnitTime? UnitTime
        {
            get { return (TriggerUnitTime?) GetCustomParameterValue(TriggerProperty.UnitTime); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.UnitTime, value);
            }
        }

        public TriggerUnitSize? UnitSize
        {
            get { return (TriggerUnitSize?) GetCustomParameterValue(TriggerProperty.UnitSize); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.UnitSize, value);
            }
        }

        public SpeedTriggerParameters(int objectId, int? triggerId, ModifyAction action) : base(TriggerType.Volume, objectId, triggerId, action)
        {
            if(action == ModifyAction.Add)
                OffNotificationAction = null;
        }
    }
}
