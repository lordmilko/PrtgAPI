using System;
using System.Reflection;

namespace PrtgAPI.Parameters
{
    sealed class SetChannelPropertyParameters : BaseSetObjectPropertyParameters<ChannelProperty>
    {
        private int channelId;

        public int[] SensorIds
        {
            get { return (int[])this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        protected override int[] ObjectIdsInternal
        {
            get { return SensorIds; }
            set { SensorIds = value; }
        }

        public SetChannelPropertyParameters(int[] sensorIds, int channelId, ChannelParameter[] parameters, Tuple<ChannelProperty, object> versionSpecific = null)
        {
            if (sensorIds == null)
                throw new ArgumentNullException(nameof(sensorIds));

            if (sensorIds.Length == 0)
                throw new ArgumentException("At least one Sensor ID must be specified", nameof(sensorIds));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Length == 0)
                throw new ArgumentException("At least one parameter must be specified", nameof(parameters));

            SensorIds = sensorIds;
            this.channelId = channelId;

            foreach (var prop in parameters)
            {
                AddTypeSafeValue(prop.Property, prop.Value, true);
            }

            if (versionSpecific != null)
                AddDependentProperty(versionSpecific.Item2, versionSpecific.Item1);

            RemoveDuplicateParameters();
        }

        protected override PropertyInfo GetPropertyInfo(Enum property)
        {
            return GetPropertyInfoViaPropertyParameter<Channel>(property);
        }

        protected override string GetParameterName(Enum property, PropertyInfo info)
        {
            //Underscore between property name and channelId is inserted by GetParameterNameStatic
            return $"{GetParameterNameStatic(property, info)}{channelId}";
        }
    }
}
