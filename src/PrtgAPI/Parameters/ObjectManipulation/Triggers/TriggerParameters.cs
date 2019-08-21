using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PrtgAPI.Attributes;
using PrtgAPI.Reflection;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// <para type="description">Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for adding/modifying <see cref="NotificationTrigger"/> objects.</para>
    /// </summary>
    public abstract class TriggerParameters : BaseParameters, IHtmlParameters
    {
        HtmlFunction IHtmlParameters.Function => HtmlFunction.EditSettings;

        private static TriggerProperty[] notificationActionTypes = new[]
        {
            TriggerProperty.OnNotificationAction,
            TriggerProperty.OffNotificationAction,
            TriggerProperty.EscalationNotificationAction
        };

        internal static bool IsNotificationAction(TriggerProperty property) => notificationActionTypes.Contains(property);

        /// <summary>
        /// Gets the ID of the object this trigger applies to.
        /// </summary>
        public int ObjectId { get; }

        /// <summary>
        /// Gets the sub ID of the trigger these parameters apply to. If these parameters are being used to create a new trigger, this value is null.
        /// </summary>
        public int? SubId => subId == "new" ? null : (int?) Convert.ToInt32(subId);

        private string subId;

        /// <summary>
        /// Gets whether these parameters will create a new trigger or modify an existing one.
        /// </summary>
        public ModifyAction Action { get; }

        /// <summary>
        /// Gets the type of notification trigger these triggers will manipulate.
        /// </summary>
        public TriggerType Type { get; }

        /// <summary>
        /// Gets or sets the <see cref="NotificationAction"/> to execute when the trigger activates.
        /// </summary>
        [PropertyParameter(TriggerProperty.OnNotificationAction)]
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
        /// <param name="objectOrId">The object or object ID the trigger will apply to.</param>
        /// <param name="subId">If this trigger is being edited, the trigger's sub ID. If the trigger is being added, this value is null.</param>
        /// <param name="action">Whether to add a new trigger or modify an existing one.</param>
        protected TriggerParameters(TriggerType type, Either<IPrtgObject, int> objectOrId, int? subId, ModifyAction action)
        {
            if (action == ModifyAction.Add && subId != null)
                throw new ArgumentException("SubId must be null when ModifyAction is Add.", nameof(subId));

            if (action == ModifyAction.Edit && subId == null)
                throw new ArgumentException("SubId cannot be null when ModifyAction is Edit.", nameof(subId));

            ObjectId = objectOrId.GetId();

            this.subId = action == ModifyAction.Add ? "new" : subId.Value.ToString();
            Action = action;
            Type = type;

            this[Parameter.Id] = ObjectId;
            this[Parameter.SubId] = this.subId;

            if (action == ModifyAction.Add)
            {
                OnNotificationAction = null;
                Parameters.Add(new CustomParameter("class", type));
                this[Parameter.ObjectType] = "nodetrigger";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerParameters"/> class for creating a new <see cref="NotificationTrigger"/> from an existing one.
        /// </summary>
        /// <param name="type">The type of notification trigger these parameters will manipulate.</param>
        /// <param name="objectOrId">The object or ID of the object the notification trigger will apply to.</param>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used as the basis of this trigger.</param>
        /// <param name="action">Whether these parameters will create a new trigger or edit an existing one.</param>
        protected TriggerParameters(TriggerType type, Either<IPrtgObject, int> objectOrId, NotificationTrigger sourceTrigger, ModifyAction action) : this(type, objectOrId, action == ModifyAction.Edit ? sourceTrigger.SubId : (int?)null, action)
        {
            if (sourceTrigger == null)
                throw new ArgumentNullException(nameof(sourceTrigger));

            if (sourceTrigger.Type != type)
                throw new ArgumentException($"A NotificationTrigger of type '{sourceTrigger.Type}' cannot be used to initialize trigger parameters of type '{type}'.");

            if (action == ModifyAction.Add)
                OnNotificationAction = sourceTrigger.OnNotificationAction;
            else
            {
                if (sourceTrigger.Inherited)
                    throw new InvalidOperationException($"Cannot modify NotificationTrigger '{sourceTrigger.OnNotificationAction}' applied to object ID {sourceTrigger.ObjectId} as this trigger is inherited from object ID {sourceTrigger.ParentId}. To modify this trigger, retrieve it from its parent object.");
            }
        }

        /// <summary>
        /// Retrieves a <see cref="NotificationAction"/> from this object's <see cref="Parameters"/>. If the specified action type does not exist, an empty notification action is returned.
        /// </summary>
        /// <param name="actionType">The type of notification action to retrieve.</param>
        /// <returns>If the notification action exists, the notification action. Otherwise, an empty notification action.</returns>
        protected NotificationAction GetNotificationAction(TriggerProperty actionType)
        {
            ValidateActionType(actionType);

            var value = GetCustomParameterValue(actionType);

            if (value == null)
            {
                if (Action == ModifyAction.Edit)
                    return null;
                return EmptyNotificationAction();
            }

            return (NotificationAction)value;
        }

        /// <summary>
        /// Updates a <see cref="NotificationAction"/> in this object's <see cref="Parameters"/>. If the notification action is null, an empty notification action is inserted. 
        /// </summary>
        /// <param name="actionType">The type of notification action to insert.</param>
        /// <param name="value">The notification action to insert. If this value is null, an empty notification action is inserted.</param>
        protected void SetNotificationAction(TriggerProperty actionType, NotificationAction value)
        {
            ValidateActionType(actionType);

            if (value == null)
                value = EmptyNotificationAction();

            UpdateCustomParameter(actionType, value);
        }

        private void ValidateActionType(TriggerProperty actionType)
        {
            if (!IsNotificationAction(actionType))
                throw new ArgumentException($"'{actionType}' is not a valid notification action type. Please specify one of {string.Join(", ", notificationActionTypes)}.", nameof(actionType));
        }

        /// <summary>
        /// Retrieves the value of a trigger property from this object's <see cref="Parameters"/>.
        /// </summary>
        /// <param name="property">The property to retrieve.</param>
        /// <returns>The value of this property. If the property does not exist, this method returns null.</returns>
        protected object GetCustomParameterValue(TriggerProperty property)
        {
            var index = GetCustomParameterIndex(property);

            if (index == -1)
                return null;

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
                throw new InvalidOperationException($"Trigger property '{property}' cannot be null for trigger type '{Type}'.");

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
        /// Retrieves the enum value of a trigger property from this object's <see cref="Parameters"/> that has been stored using its XML representation.
        /// </summary>
        /// <typeparam name="T">The type of enum stored in the property</typeparam>
        /// <param name="property">The property to retrieve.</param>
        /// <returns>The value of this property. If the property does not exist, this method returns null.</returns>
        protected object GetCustomParameterEnumXml<T>(TriggerProperty property)
        {
            var value = GetCustomParameterValue(property);

            if (value == null)
                return null;

            return value.ToString().XmlToEnum<T>();
        }

        /// <summary>
        /// Retrieves the enum value of a trigger property from this object's <see cref="Parameters"/> that has been stored as an integer.
        /// </summary>
        /// <typeparam name="T">The type of enum to retrieve.</typeparam>
        /// <param name="property">The trigger property whose value should be retrieved.</param>
        /// <returns>If the trigger property has a value, a value of type T. Otherwise, a null value of type T?</returns>
        protected object GetCustomParameterEnumInt<T>(TriggerProperty property) where T : struct
        {
            var value = GetCustomParameterValue(property);
            return value == null ? (T?)null : (T)value;
        }

        /// <summary>
        /// Creates a set of <see cref="TriggerParameters"/> for editing an existing notification trigger.
        /// </summary>
        /// <param name="trigger">The notification trigger to modify.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        /// <returns>A set of trigger parameters containing the information required to modify the specified notification trigger.</returns>
        internal static TriggerParameters Create(NotificationTrigger trigger, TriggerParameter[] parameters)
        {
            switch (trigger.Type)
            {
                case TriggerType.Change:
                    return BindParameters(new ChangeTriggerParameters(trigger), parameters);
                case TriggerType.Speed:
                    return BindParameters(new SpeedTriggerParameters(trigger), parameters);
                case TriggerType.State:
                    return BindParameters(new StateTriggerParameters(trigger), parameters);
                case TriggerType.Threshold:
                    return BindParameters(new ThresholdTriggerParameters(trigger), parameters);
                case TriggerType.Volume:
                    return BindParameters(new VolumeTriggerParameters(trigger), parameters);
                default:
                    throw new NotImplementedException($"Handler of trigger type '{trigger.Type}' is not implemented.");
            }
        }

        /// <summary>
        /// Creates a set of <see cref="TriggerParameters"/> for creating a new notification trigger.
        /// </summary>
        /// <param name="type">The type of notification trigger to create.</param>
        /// <param name="objectId">The ID of the object the notification trigger will be created under.</param>
        /// <param name="parameters">A set of parameters describing the values to assign to each notification trigger property.</param>
        /// <returns>A set of trigger parameters containing the information required to create the specified notification trigger.</returns>
        internal static TriggerParameters Create(TriggerType type, int objectId, TriggerParameter[] parameters)
        {
            switch (type)
            {
                case TriggerType.Change:
                    return BindParameters(new ChangeTriggerParameters(objectId), parameters);
                case TriggerType.Speed:
                    return BindParameters(new SpeedTriggerParameters(objectId), parameters);
                case TriggerType.State:
                    return BindParameters(new StateTriggerParameters(objectId), parameters);
                case TriggerType.Threshold:
                    return BindParameters(new ThresholdTriggerParameters(objectId), parameters);
                case TriggerType.Volume:
                    return BindParameters(new VolumeTriggerParameters(objectId), parameters);
                default:
                    throw new NotImplementedException($"Handler of trigger type '{type}' is not implemented.");
            }
        }

        private static TriggerParameters BindParameters(TriggerParameters triggerParameters, TriggerParameter[] parameters)
        {
            var properties = triggerParameters.GetType().GetTypeCache().Properties;

            foreach (var parameter in parameters)
            {
                var name = parameter.Property.ToString();

                var propertyCache = properties.FirstOrDefault(p => p.Property.Name == name);

                if (propertyCache == null)
                    throw new InvalidOperationException($"Property '{name}' is not a valid property for a trigger of type '{triggerParameters.Type}'.");

                var v = parameter.Value;

                v = TryInvokeTypeParser(parameter.Property, v, triggerParameters);

                //Mainly just converts null to the None Notification Action (via the TriggerProperty's ValueConverter)
                var parser = new Helpers.DynamicPropertyTypeParser(parameter.Property, propertyCache, v)
                {
                    AllowNull = false
                };
                var value = parser.DeserializeValue();

                propertyCache.SetValue(triggerParameters, value);
            }

            return triggerParameters;
        }

        private static object TryInvokeTypeParser(TriggerProperty property, object v, TriggerParameters parameters)
        {
            var propertyInfo = parameters.GetTypeCache().Properties.FirstOrDefault(p => p.GetAttribute<PropertyParameterAttribute>()?.Property.Equals(property) == true)?.Property;

            if (propertyInfo == null)
                throw new InvalidOperationException($"Property '{property}' does not exist on triggers of type '{parameters.Type}'.");

            if (ReflectionExtensions.IsPrtgAPIProperty(typeof(TriggerParameters), propertyInfo) && !propertyInfo.PropertyType.IsEnum)
            {
                var method = propertyInfo.PropertyType.GetMethod("Parse", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);

                if (method != null)
                {
                    try
                    {
                        var newValue = method.Invoke(null, new[] { v });
                        v = newValue;
                    }
                    catch (Exception)
                    {
                        //Don't care if our value wasn't parsable
                    }
                }
            }

            return v;
        }
    }
}
