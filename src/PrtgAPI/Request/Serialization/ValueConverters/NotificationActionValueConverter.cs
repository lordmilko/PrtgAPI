using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Parameters;

namespace PrtgAPI.Request.Serialization.ValueConverters
{
    class NotificationActionValueConverter : IValueConverter
    {
        internal static NotificationActionValueConverter Instance = new NotificationActionValueConverter();

        [ExcludeFromCodeCoverage]
        public object Serialize(object value)
        {
            if (value == null)
                value = TriggerParameters.EmptyNotificationAction();

            var serializable = value as ISerializable;

            if (serializable != null)
                return serializable.GetSerializedFormat();
            else
                return value;
        }

        public object Deserialize(object value)
        {
            if (value == null)
                value = TriggerParameters.EmptyNotificationAction();

            return value;
        }
    }
}
