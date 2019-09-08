using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Reflection.Cache
{
    class EnumValue
    {
        private FieldInfo field;

        private object value;

        public object Value
        {
            get
            {
                if (value == null)
                {
                    value = field.GetValue(null);
                }

                return value;
            }
        }

        public EnumValue(FieldInfo field)
        {
            this.field = field;
        }
    }

    /// <summary>
    /// Maps <see cref="XmlEnumAttribute"/> and <see cref="XmlEnumAlternateName"/> values to their corresponding <see cref="Enum"/> values.
    /// </summary>
    class EnumXmlCache : TypeCache
    {
        private Dictionary<string, Dictionary<Type, EnumValue>> map = new Dictionary<string, Dictionary<Type, EnumValue>>(); //maps a value to its attributes, which are then mapped to their values

        public EnumXmlCache(Type type) : base(type)
        {
            foreach (var field in Fields)
            {
                foreach (var attrib in field.Attributes)
                {
                    if (attrib.Key == typeof (XmlEnumAttribute) || attrib.Key == typeof (XmlEnumAlternateName))
                    {
                        foreach (var val in attrib.Value.Cast<XmlEnumAttribute>())
                        {
                            Dictionary<Type, EnumValue> dict;

                            if (!map.TryGetValue(val.Name, out dict))
                            {
                                dict = new Dictionary<Type, EnumValue>();

                                map[val.Name] = dict;
                            }

                            dict[attrib.Key] = new EnumValue(field.Field);
                        }
                    }
                }
            }
        }

        public object GetValue(string value, Type attribType)
        {
            if (value == null)
                return null;

            Dictionary<Type, EnumValue> dict;

            if (map.TryGetValue(value, out dict))
            {
                EnumValue val;

                if (dict.TryGetValue(attribType, out val))
                    return val.Value;
            }

            return null;
        }
    }
}
