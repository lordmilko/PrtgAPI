using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    sealed class SetChannelPropertyParameters : BaseSetObjectPropertyParameters<ChannelProperty>, IShallowCloneable<SetChannelPropertyParameters>
    {
        private int channelId;

        private Dictionary<string, Enum> channelNameMap = new Dictionary<string, Enum>();

        [ExcludeFromCodeCoverage]
        public int[] SensorIds
        {
            get { return (int[])this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        [ExcludeFromCodeCoverage]
        protected override int[] ObjectIdsInternal
        {
            get { return SensorIds; }
            set { SensorIds = value; }
        }

        public List<Tuple<Enum, FactorAttribute>> FactorParameters { get; private set; }

        public SetChannelPropertyParameters(int[] sensorIds, int channelId, ChannelParameter[] parameters) : this(sensorIds, channelId)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Length == 0)
                throw new ArgumentException("At least one parameter must be specified.", nameof(parameters));

            foreach (var prop in parameters)
            {
                AddTypeSafeValue(prop.Property, prop.Value, true);
            }

            RemoveDuplicateParameters();

            FactorParameters = parser.Properties.Select(p => Tuple.Create(p, p.GetEnumAttribute<FactorAttribute>())).Where(v => v.Item2 != null).ToList();
        }

        private SetChannelPropertyParameters(int[] sensorIds, int channelId) : base(ValidateIds(sensorIds))
        {
            this.channelId = channelId;
        }

        private static int[] ValidateIds(int[] sensorIds)
        {
            if (sensorIds == null)
                throw new ArgumentNullException(nameof(sensorIds));

            if (sensorIds.Length == 0)
                throw new ArgumentException("At least one Sensor ID must be specified.", nameof(sensorIds));

            return sensorIds;
        }

        public override PropertyCache GetPropertyCache(Enum property)
        {
            return ObjectPropertyParser.GetPropertyInfoViaPropertyParameter<Channel>(property);
        }

        protected override string GetParameterName(Enum property, PropertyCache cache)
        {
            //Underscore between property name and channelId is inserted by GetObjectPropertyNameViaCache
            var str = ObjectPropertyParser.GetObjectPropertyNameViaCache(property, cache);

            var name = $"{str}{channelId}";

            channelNameMap[name] = property;

            return name;
        }

        internal Enum GetChannelPropertyFromName(string str)
        {
            Enum value;

            if (channelNameMap.TryGetValue(str, out value))
                return value;

            return null;
        }

        public string GetFactorParameterName(Enum property, PropertyCache cache)
        {
            var str = ObjectPropertyParser.GetObjectPropertyNameViaCache(property, cache);

            str = str.Replace("_factor", $"_{channelId}_factor");

            return str;
        }

        [ExcludeFromCodeCoverage]
        object IShallowCloneable.ShallowClone() => ShallowClone();

        public SetChannelPropertyParameters ShallowClone()
        {
            var newParameters = new SetChannelPropertyParameters(SensorIds, channelId);

            foreach (var parameter in GetParameters().Where(p => p.Key != Parameter.Custom))
                newParameters[parameter.Key] = parameter.Value;

            newParameters[Parameter.Custom] = ((List<CustomParameter>) this[Parameter.Custom]).ToList();

            newParameters.FactorParameters = FactorParameters;
            newParameters.Cookie = Cookie;
            newParameters.parser = parser;

            foreach (var kv in channelNameMap)
                newParameters.channelNameMap[kv.Key] = kv.Value;

            return newParameters;
        }
    }
}
