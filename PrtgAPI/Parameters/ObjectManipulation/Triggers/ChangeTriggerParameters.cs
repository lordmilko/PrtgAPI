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
        /// Initializes a new instance of the <see cref="ChangeTriggerParameters"/> class.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="triggerId">If this trigger is being edited, the trigger's sub ID. If this trigger is being added, this value is null.</param>
        /// <param name="action">Whether to add a new trigger or modify an existing one.</param>
        public ChangeTriggerParameters(int objectId, int? triggerId, ModifyAction action) : base(TriggerType.Change, objectId, triggerId, action)
        {
        }
    }
}
