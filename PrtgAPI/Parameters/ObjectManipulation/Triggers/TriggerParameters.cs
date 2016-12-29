using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters use to construct a <see cref="PrtgUrl"/> for adding/modifying <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public abstract class TriggerParameters : Parameters
    {
        private int objectId;
        private string subId;
        private ModifyAction action;

        /// <summary>
        /// The <see cref="NotificationAction"/> to execute when the trigger activates.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerParameters"/> class.
        /// </summary>
        /// <param name="type">The type of notification trigger these parameters will manipulate.</param>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="subId">If this trigger is being edited, the trigger's sub ID. If the trigger is being added, this value is null.</param>
        /// <param name="action">Whether to add a new trigger or modify an existing one.</param>
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

        /// <summary>
        /// Retrieve a <see cref="NotificationAction"/> from this object's <see cref="Parameters"/>. If the specified action type does not exist, an empty notification action is returned.
        /// </summary>
        /// <param name="actionType">The type of notification action to retrieve.</param>
        /// <returns>If the notification action exists, the notification action. Otherwise, an empty notification action.</returns>
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

        /// <summary>
        /// Updates a <see cref="NotificationAction"/> in this object's <see cref="Parameters"/>. If the notification action is null, an empty notification action is inserted. 
        /// </summary>
        /// <param name="actionType">The type of notification action to insert.</param>
        /// <param name="value">The notification action to insert. If this value is null, an empty notification action is inserted.</param>
        protected void SetNotificationAction(TriggerProperty actionType, NotificationAction value)
        {
            if (actionType != TriggerProperty.OnNotificationAction && actionType != TriggerProperty.OffNotificationAction && actionType != TriggerProperty.EscalationNotificationAction)
                throw new ArgumentException($"{actionType} is not a valid notification action type");

            if (value == null)
                value = EmptyNotificationAction();

            UpdateCustomParameter(actionType, value);
        }

        /// <summary>
        /// Retrieve the value of a trigger property from this object's <see cref="Parameters"/>.
        /// </summary>
        /// <param name="property">The property to retrieve.</param>
        /// <returns>The value of this property. If the property does not exist, this method returns null.</returns>
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

        /// <summary>
        /// Updates the value of a trigger property in this object's <see cref="Parameters"/>. If the trigger property does not exist, it will be added.
        /// </summary>
        /// <param name="property">The property whose value will be updated.</param>
        /// <param name="value">The value to insert.</param>
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
