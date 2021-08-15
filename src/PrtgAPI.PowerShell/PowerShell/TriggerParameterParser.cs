using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell
{
    class TriggerParameterParser
    {
        private static Type[] triggerParameterTypes = typeof(TriggerParameters).Assembly.GetTypes().Where(t => typeof(TriggerParameters).IsAssignableFrom(t)).ToArray();

        private static PropertyCache[] triggerProperties = triggerParameterTypes.SelectMany(t => t.GetTypeCache().Properties.Where(p => p.GetAttribute<PropertyParameterAttribute>() != null)).ToArray();

        private PrtgClient client;
        private List<NotificationAction> actions;

        internal TriggerParameterParser(PrtgClient client)
        {
            this.client = client;
        }

        internal void UpdateParameterValue(TriggerParameter parameter, Lazy<PrtgObject> @object)
        {
            if (TriggerParameters.IsNotificationAction(parameter.Property) && parameter.Value != null)
            {
                if (!(parameter.Value is NameOrObject<NotificationAction>) && (parameter.Value is NotificationAction || parameter.Value is string))
                {
                    if (parameter.Value is NotificationAction)
                        parameter.Value = new NameOrObject<NotificationAction>((NotificationAction) parameter.Value);
                    else
                        parameter.Value = new NameOrObject<NotificationAction>((string) parameter.Value);
                }

                if (parameter.Value is NameOrObject<NotificationAction>)
                    UpdateNotificationAction(parameter);
            }

            if (parameter.Property == TriggerProperty.Channel)
                UpdateChannel(parameter, @object);
        }

        private void UpdateNotificationAction(TriggerParameter parameter)
        {
            var nameOrObject = (NameOrObject<NotificationAction>)parameter.Value;

            if (nameOrObject.IsObject)
                parameter.Value = nameOrObject.Object;
            else
            {
                if (actions == null)
                    actions = client.GetNotificationActions();

                var wildcard = new WildcardPattern(nameOrObject.Name, WildcardOptions.IgnoreCase);
                var single = actions.Where(a => wildcard.IsMatch(a.Name)).ToList();

                if (single.Count == 1)
                    parameter.Value = single.Single();
                else if (single.Count > 1)
                    throw new InvalidOperationException($"Notification Action wildcard '{nameOrObject.Name}' on parameter '{parameter.Property}' is ambiguous between the actions: {single.ToQuotedList()}. Please specify a more specific action name.");
                else
                    throw new InvalidOperationException($"Could not find a notification action matching the wildcard expression '{nameOrObject.Name}' for use with parameter '{parameter.Property}'.");
            }
        }

        private void UpdateChannel(TriggerParameter parameter, Lazy<PrtgObject> @object)
        {
            var str = parameter.Value?.ToString();

            if (@object.Value.Type == ObjectType.Sensor && !string.IsNullOrWhiteSpace(str))
                UpdateChannelFromChannel(parameter, @object, str);
            else
            {
                if (str == null)
                    throw new ArgumentNullException($"Cannot specify 'null' for parameter '{parameter.Property}'. Value must non-null and convertable to one of {typeof(StandardTriggerChannel).FullName}, {typeof(Channel).FullName} or {typeof(int).FullName}.", (Exception) null);

                parameter.Value = TriggerChannel.Parse(parameter.Value);
            }
        }

        private void UpdateChannelFromChannel(TriggerParameter parameter, Lazy<PrtgObject> @object, string str)
        {
            //str could either be a wildcard specifying the name of a channel, or a specified channel ID.
            int intResult;

            if (int.TryParse(str, out intResult))
            {
                parameter.Value = client.GetChannel(@object.Value.Id, intResult);
                return;
            }

            var channels = client.GetChannels(@object.Value.Id);

            var wildcard = new WildcardPattern(str, WildcardOptions.IgnoreCase);

            var matches = channels.Where(c => wildcard.IsMatch(c.Name)).ToArray();

            if (matches.Length == 0)
            {
                string str2 = null;

                if (channels.Count > 0)
                    str2 = $"Specify one of the following channel names and try again: {channels.ToQuotedList()}";
                else
                    str2 = "No channels currently exist on this sensor";

                throw new NonTerminatingException($"Channel wildcard '{str}' does not exist on sensor '{@object}' (ID: {@object.Value.Id}). {str2}.");
            }
            if (matches.Length == 1)
                parameter.Value = matches.Single();
            else
                throw new NonTerminatingException($"Channel wildcard '{str}' on parameter '{parameter.Property}' is ambiguous between the channels: {matches.ToQuotedList()}.");
        }

        internal static Type GetPropertyType(TriggerProperty e)
        {
            //Channel could be convertable to a TriggerChannel, or could be the name of a channel to retrieve
            //(in which case we need to TryParse TriggerChannel manually)
            if (e == TriggerProperty.Channel)
                return typeof(object);

            var type = triggerProperties
                .First(p => (TriggerProperty)p.GetAttribute<PropertyParameterAttribute>().Property == e).Property
                .PropertyType;

            if (type == typeof(NotificationAction))
                type = typeof(NameOrObject<NotificationAction>);

            return type;
        }
    }
}
