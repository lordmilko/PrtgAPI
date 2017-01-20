using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for adding/modifying <see cref="TriggerType.Change"/> <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public class ChangeTriggerParameters : TriggerParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeTriggerParameters"/> class for creating a new notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        public ChangeTriggerParameters(int objectId) : base(TriggerType.Change, objectId, null, ModifyAction.Add)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeTriggerParameters"/> class for editing an existing notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger is applied to.</param>
        /// <param name="triggerId">The sub ID of the trigger on its parent object.</param>
        public ChangeTriggerParameters(int objectId, int triggerId) : base(TriggerType.Change, objectId, triggerId, ModifyAction.Edit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeTriggerParameters"/> class for creating a new trigger from an existing <see cref="TriggerType.Change"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used.</param>
        public ChangeTriggerParameters(int objectId, NotificationTrigger sourceTrigger) : base(TriggerType.Change, objectId, sourceTrigger)
        {
        }
    }
}
