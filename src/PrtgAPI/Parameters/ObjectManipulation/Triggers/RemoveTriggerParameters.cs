using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class RemoveTriggerParameters : BaseActionParameters, IHtmlParameters
    {
        HtmlFunction IHtmlParameters.Function => HtmlFunction.RemoveSubObject;

        public RemoveTriggerParameters(NotificationTrigger trigger) : base(ValidateTrigger(trigger))
        {
            TriggerId = trigger.SubId;
        }

        private static int ValidateTrigger(NotificationTrigger trigger)
        {
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger), "Trigger cannot be null.");

            if (trigger.Inherited)
                throw new InvalidOperationException($"Cannot remove trigger {trigger.SubId} from Object ID: {trigger.ObjectId} as it is inherited from Object ID: {trigger.ParentId}.");

            return trigger.ObjectId;
        }

        public int TriggerId
        {
            get { return (int)this[Parameter.SubId]; }
            set { this[Parameter.SubId] = value; }
        }
    }
}
