using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    public abstract class TriggerParameters : Parameters
    {
        private int objectId;
        private string subId;
        private ModifyAction action;

        public NotificationAction OnNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OnNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OnNotificationAction, value); }
        }

        private List<CustomParameter> Parameters
        {
            get
            {
                if (this[Parameter.Custom] == null)
                    this[Parameter.Custom] = new List<CustomParameter>();

                return (List<CustomParameter>)this[Parameter.Custom];
            }
        }

        protected TriggerParameters(TriggerType type, int objectId, int? subId, ModifyAction action)
        {
            if (action == ModifyAction.Add && subId != null)
                throw new ArgumentException("SubId must be null when ModifyAction is Add", nameof(subId));
            else if (action == ModifyAction.Edit && subId == null)
                throw new ArgumentException("SubId cannot be null when ModifyAction is Edit", nameof(subId));

            OnNotificationAction = null;

            this.objectId = objectId;

            this.subId = action == ModifyAction.Add ? "new" : subId.Value.ToString();
            this.action = action;

            this[Parameter.Id] = objectId;
            this[Parameter.SubId] = this.subId;

            if (action == ModifyAction.Add)
            {
                Parameters.Add(new CustomParameter("class", type.ToString().ToLower())); //todo: does this tolower itself during prtgurl construction? check debug output!
                Parameters.Add(new CustomParameter("objecttype", "nodetrigger"));
            }
                
        }

        protected NotificationAction GetNotificationAction(TriggerProperty actionType)
        {
            if (actionType != TriggerProperty.OnNotificationAction && actionType != TriggerProperty.OffNotificationAction && actionType != TriggerProperty.EscalationNotificationAction)
                throw new ArgumentException($"{actionType} is not a valid notification action type");

            var value = GetCustomParameterValue(actionType);

            if (value == null)
                return EmptyNotificationAction();
            else
                return (NotificationAction)value;
        }

        protected void SetNotificationAction(TriggerProperty actionType, NotificationAction value)
        {
            if (actionType != TriggerProperty.OnNotificationAction && actionType != TriggerProperty.OffNotificationAction && actionType != TriggerProperty.EscalationNotificationAction)
                throw new ArgumentException($"{actionType} is not a valid notification action type");

            if (value == null)
                value = EmptyNotificationAction();

            UpdateCustomParameter(actionType, value);
        }

        protected object GetCustomParameterValue(TriggerProperty property)
        {
            var index = GetCustomParameterIndex(property);

            if (index == -1)
                return null;
            else
                return Parameters[index].Value;
        }

        private int GetCustomParameterIndex(TriggerProperty property)
        {
            var index = Parameters.FindIndex(a => a.Name.StartsWith(property.GetDescription()));

            return index;
        }

        protected void UpdateCustomParameter(TriggerProperty property, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var name = $"{property.GetDescription()}_{subId}";
            var parameter = new CustomParameter(name, value);

            var index = GetCustomParameterIndex(property);

            if (index == -1)
                Parameters.Add(parameter);
            else
                Parameters[index] = parameter;
        }

        internal static NotificationAction EmptyNotificationAction()
        {
            return new NotificationAction { Id = -1, Name = "None" };
        }
    }
}
