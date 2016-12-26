using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class VolumeTriggerParameters : TriggerParameters
    {
        public TriggerChannel? Channel
        {
            get { return (TriggerChannel?) GetCustomParameterValue(TriggerProperty.Channel); }
            set
            {
                if(value != null)
                    UpdateCustomParameter(TriggerProperty.Channel, value);
            }
        }

        public TriggerPeriod? Period
        {
            get { return (TriggerPeriod?) GetCustomParameterValue(TriggerProperty.Period); }
            set
            {
                if(value != null)
                    UpdateCustomParameter(TriggerProperty.Period, value);
            }
        }

        public TriggerVolumeUnitSize? UnitSize
        {
            get { return (TriggerVolumeUnitSize?) GetCustomParameterValue(TriggerProperty.UnitSize); }
            set
            {
                if(value != null)
                    UpdateCustomParameter(TriggerProperty.UnitSize, value);
            }
        }

        public VolumeTriggerParameters(int objectId, int? triggerId, ModifyAction action) : base(TriggerType.Volume, objectId, triggerId, action)
        {
        }
    }
}
