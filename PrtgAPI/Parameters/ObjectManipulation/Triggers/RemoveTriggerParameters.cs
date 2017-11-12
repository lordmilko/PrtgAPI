using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class RemoveTriggerParameters : BaseActionParameters
    {
        public RemoveTriggerParameters(NotificationTrigger trigger) : base(ValidateTrigger(trigger))
        {
            TriggerId = trigger.SubId;
        }

        private static int ValidateTrigger(NotificationTrigger trigger)
        {
            if (trigger.Inherited)
                throw new InvalidOperationException($"Cannot remove trigger {trigger.SubId} from Object ID: {trigger.ObjectId} as it is inherited from Object ID: {trigger.ParentId}");

            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            return trigger.ObjectId;
        }

        public int TriggerId
        {
            get { return (int)this[Parameter.SubId]; }
            set { this[Parameter.SubId] = value; }
        }
    }
}
