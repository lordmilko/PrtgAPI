using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters use to construct a <see cref="PrtgUrl"/> for adding/modifying <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public abstract class TriggerParameters : Parameters
    {
        /// <summary>
        /// The ID of the object this trigger will apply to.
        /// </summary>
        public int ObjectId { get; private set; }

        /// <summary>
        /// If this trigger is being edited, the trigger's sub ID. If the trigger is being added, this value is null.
        /// </summary>
        public string SubId => subId == "new" ? null : subId;

        private string subId;

        /// <summary>
        /// Whether to add a new trigger or modify an existing one.
        /// </summary>
        public ModifyAction Action { get; private set; }

        /// <summary>
        /// The type of notification trigger this object will manipulate.
        /// </summary>
        public TriggerType Type { get; private set; }

        /// <summary>
        /// The <see cref="NotificationAction"/> to execute when the trigger activates.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.OnNotificationAction))]
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
        /// <param name="type">The type of notification trigger this object will manipulate.</param>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="subId">If this trigger is being edited, the trigger's sub ID. If the trigger is being added, this value is null.</param>
        /// <param name="action">Whether to add a new trigger or modify an existing one.</param>
        protected TriggerParameters(TriggerType type, int objectId, int? subId, ModifyAction action)
        {
            if (action == ModifyAction.Add && subId != null)
                throw new ArgumentException("SubId must be null when ModifyAction is Add", nameof(subId));
            else if (action == ModifyAction.Edit && subId == null)
                throw new ArgumentException("SubId cannot be null when ModifyAction is Edit", nameof(subId));

            if(action == ModifyAction.Add)
                OnNotificationAction = null;

            ObjectId = objectId;

            this.subId = action == ModifyAction.Add ? "new" : subId.Value.ToString();
            Action = action;
            Type = type;

            this[Parameter.Id] = objectId;
            this[Parameter.SubId] = this.subId;

            if (action == ModifyAction.Add)
            {
                Parameters.Add(new CustomParameter("class", type.ToString().ToLower())); //todo: does this tolower itself during prtgurl construction? check debug output!
                Parameters.Add(new CustomParameter("objecttype", "nodetrigger"));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerParameters"/> class for creating a new <see cref="NotificationTrigger"/> from an existing one.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="objectId"></param>
        /// <param name="sourceTrigger"></param>
        protected TriggerParameters(TriggerType type, int objectId, NotificationTrigger sourceTrigger) : this(type, objectId, null, ModifyAction.Add)
        {
            if (sourceTrigger == null)
                throw new ArgumentNullException(nameof(sourceTrigger));

            if (sourceTrigger.Type != type)
                throw new ArgumentException($"A NotificationTrigger of type '{sourceTrigger.Type}' cannot be used to initialize trigger parameters of type '{type}'");

            OnNotificationAction = sourceTrigger.OnNotificationAction;
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
            {
                if (Action == ModifyAction.Edit)
                    return null;
                return EmptyNotificationAction();
            }
                
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
        /// <param name="requireValue">Indicates whether null is allowed for the specified property.</param>
        protected void UpdateCustomParameter(TriggerProperty property, object value, bool requireValue = false)
        {
            if (value == null && requireValue && Action != ModifyAction.Edit)
                throw new InvalidOperationException($"Trigger property '{property}' cannot be null for trigger type '{Type}'");

            var name = $"{property.GetDescription()}_{this.subId}";
            var parameter = new CustomParameter(name, value);

            var index = GetCustomParameterIndex(property);

            if (index == -1)
                Parameters.Add(parameter);
            else
            {
                if (value == null)
                    Parameters.RemoveAt(index);
                else
                {
                    Parameters[index] = parameter;
                }
            }
        }

        internal static NotificationAction EmptyNotificationAction()
        {
            return new NotificationAction { Id = -1, Name = "None" };
        }

        /// <summary>
        /// Retrieve the enum value of a trigger property from this object's <see cref="Parameters"/> that has been stored using its XML representation.
        /// </summary>
        /// <typeparam name="T">The type of enum stored in the property</typeparam>
        /// <param name="property">The property to retrieve.</param>
        /// <returns>The value of this property. If the property does not exist, this method returns null.</returns>
        protected object GetCustomParameterEnumXml<T>(TriggerProperty property)
        {
            var value = GetCustomParameterValue(property);

            if (value == null)
                return null;

            return EnumHelpers.XmlToEnum<T>(value.ToString());
        }

        /// <summary>
        /// Retrieve the enum value of a trigger property from this object's <see cref="Parameters"/> that has been stored as an integer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        protected object GetCustomParameterEnumInt<T>(TriggerProperty property) where T : struct
        {
            var value = GetCustomParameterValue(property);
            return value == null ? (T?)null : (T)value;
        }
    }
}
