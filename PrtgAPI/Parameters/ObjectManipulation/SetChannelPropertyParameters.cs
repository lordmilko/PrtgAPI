using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Undocumented;

namespace PrtgAPI.Parameters
{
    class SetChannelPropertyParameters : BaseSetObjectPropertyParameters
    {
        public int SensorId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public List<CustomParameter> CustomParameters
        {
            get { return (List<CustomParameter>)this[Parameter.Custom]; }
            set { this[Parameter.Custom] = value; }
        }

        public SetChannelPropertyParameters(int sensorId, int channelId, ChannelProperty property, object value)
        {
            var attrib = property.GetEnumAttribute<RequireValueAttribute>();

            if (value == null && (attrib == null || (attrib != null && attrib.ValueRequired)))
                throw new ArgumentNullException($"Property '{property}' does not support null values.");

            var customParams = GetChannelSetObjectPropertyCustomParams(channelId, property, value);

            SensorId = sensorId;
            CustomParameters = customParams;
        }

        private List<CustomParameter> GetChannelSetObjectPropertyCustomParams(int channelId, ChannelProperty property, object value)
        {
            bool valAsBool;
            var valIsBool = bool.TryParse(value?.ToString(), out valAsBool);

            List<CustomParameter> customParams = new List<CustomParameter>();

            if (valIsBool)
            {
                if (valAsBool)
                {
                    value = 1;
                }

                else //if we're disabling a property, check if there are values dependent on us. if so, disable them too!
                {
                    value = 0;

                    var associatedProperties = property.GetDependentProperties<ChannelProperty>();

                    customParams.AddRange(associatedProperties.Select(prop => CreateCustomParameter(channelId, prop, string.Empty)));
                }
            }
            else //if we're enabling a property, check if there are values we depend on. if so, enable them!
            {
                var dependentProperty = property.GetEnumAttribute<DependentPropertyAttribute>();

                if (dependentProperty != null)
                {
                    customParams.Add(CreateCustomParameter(channelId, dependentProperty.Name.ToEnum<ChannelProperty>(), "1"));
                }
            }

            customParams.Add(CreateCustomParameter(channelId, property, value));

            return customParams;
        }

        CustomParameter CreateCustomParameter(int objectId, Enum property, object value)
        {
            return new CustomParameter($"{property.GetDescription()}_{objectId}", value?.ToString());
        }
    }
}
