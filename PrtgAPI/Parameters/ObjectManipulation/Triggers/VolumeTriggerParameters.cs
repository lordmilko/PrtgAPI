using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for adding/modifying <see cref="TriggerType.Volume"/> <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public class VolumeTriggerParameters : TriggerParameters
    {
        /// <summary>
        /// The channel of the sensor this trigger should apply to.
        /// </summary>
        public TriggerChannel? Channel
        {
            get { return (TriggerChannel?) GetCustomParameterValue(TriggerProperty.Channel); }
            set
            {
                if(value != null)
                    UpdateCustomParameter(TriggerProperty.Channel, value);
            }
        }

        /// <summary>
        /// The time component of the volume limit that causes this trigger to activate.
        /// </summary>
        public TriggerPeriod? Period
        {
            get { return (TriggerPeriod?) GetCustomParameterValue(TriggerProperty.Period); }
            set
            {
                if(value != null)
                    UpdateCustomParameter(TriggerProperty.Period, value);
            }
        }

        /// <summary>
        /// The unit component of the volume limit that causes this trigger to activate.
        /// </summary>
        public TriggerVolumeUnitSize? UnitSize
        {
            get { return (TriggerVolumeUnitSize?) GetCustomParameterValue(TriggerProperty.UnitSize); }
            set
            {
                if(value != null)
                    UpdateCustomParameter(TriggerProperty.UnitSize, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTriggerParameters"/> class.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="triggerId">If this trigger is being edited, the trigger's sub ID. If this trigger is being added, this value is null.</param>
        /// <param name="action">Whether to add a new trigger or modify an existing one.</param>
        public VolumeTriggerParameters(int objectId, int? triggerId, ModifyAction action) : base(TriggerType.Volume, objectId, triggerId, action)
        {
        }
    }
}
