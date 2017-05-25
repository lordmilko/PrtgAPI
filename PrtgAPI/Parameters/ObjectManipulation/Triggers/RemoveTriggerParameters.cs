﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class RemoveTriggerParameters : BaseActionParameters
    {
        public RemoveTriggerParameters(int objectId, int triggerId) : base(objectId)
        {
            TriggerId = triggerId;
        }

        public RemoveTriggerParameters(NotificationTrigger trigger) : base(ValidateTrigger(trigger))
        {
            TriggerId = trigger.SubId;
        }

        private static int ValidateTrigger(NotificationTrigger trigger)
        {
            if (trigger.Inherited)
                throw new InvalidOperationException($"Cannot remove trigger {trigger.SubId} from Object {trigger.ObjectId} as it is inherited from Object {trigger.ParentId}");

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