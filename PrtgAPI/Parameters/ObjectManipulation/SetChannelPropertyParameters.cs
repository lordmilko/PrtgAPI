using System;
using System.Reflection;

namespace PrtgAPI.Parameters
{
    sealed class SetChannelPropertyParameters : BaseSetObjectPropertyParameters<ChannelProperty>
    {
        private int channelId;

        public int SensorId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public SetChannelPropertyParameters(int sensorId, int channelId, ChannelProperty property, object value)
        {
            SensorId = sensorId;
            this.channelId = channelId;

            AddTypeSafeValue(property, value, true);
        }

        protected override PropertyInfo GetPropertyInfo(Enum property)
        {
            return GetPropertyInfoViaPropertyParameter<Channel>(property);
        }

        protected override string GetParameterName(ChannelProperty property, PropertyInfo info)
        {
            //Underscore between property name and channelId is inserted by GetParameterNameStatic
            return $"{GetParameterNameStatic(property, info)}{channelId}";
        }
    }
}
