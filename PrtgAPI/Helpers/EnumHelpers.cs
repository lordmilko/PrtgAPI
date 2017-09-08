using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Exceptions.Internal;

namespace PrtgAPI.Helpers
{
    static class EnumHelpers
    {
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static IEnumerable<Enum> GetUnderlyingFlags(this Enum element)
        {
            var enums = Enum.GetValues(element.GetType()).Cast<Enum>().ToList();

            return GetUnderlyingFlagsInternal(element, enums).Distinct();
        }

        private static IEnumerable<Enum> GetUnderlyingFlagsInternal(Enum e, List<Enum> enums)
        {
            foreach (var enumListMember in enums)
            {
                if (e.HasFlag(enumListMember) && !e.Equals(enumListMember))
                {
                    var result = GetUnderlyingFlagsInternal(enumListMember, enums).ToList();

                    if (result.Count == 0)
                        yield return enumListMember;
                    else
                    {
                        foreach (var val in result)
                        {
                            yield return val;
                        }
                    }
                }
            }
        }

        public static TEnum DescriptionToEnum<TEnum>(this string value, bool toStringFallback = true)
        {
            foreach (var field in typeof (TEnum).GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof (DescriptionAttribute)) as DescriptionAttribute;

                if (attribute != null)
                {
                    if (attribute.Description == value)
                        return (TEnum) field.GetValue(null);
                }
                else
                {
                    if (field.Name == value)
                        return (TEnum) field.GetValue(null);
                }
            }

            if(!toStringFallback)
                throw new ArgumentException("Is not a description for any value in " + typeof (TEnum), nameof(value));

            return value.ToEnum<TEnum>();
        }

        internal static TEnum XmlEnumAlternateNameToEnum<TEnum>(this string value)
        {
            foreach (var field in typeof (TEnum).GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof (XmlEnumAlternateName)) as XmlEnumAlternateName;

                if (attribute != null)
                {
                    if (attribute.Name == value)
                        return (TEnum) field.GetValue(null);
                }
            }

            throw new XmlDeserializationException($"Could not find a member in type {typeof(TEnum)} with an XmlEnumAlternateNameAttribute for value '{value}'");
        }

        internal static T XmlToEnum<T>(this string value)
        {
            return (T) XmlToEnum<XmlEnumAttribute>(value, typeof (T));
        }

        internal static T XmlAltToEnum<T>(this string value)
        {
            return (T) XmlToEnum<XmlEnumAlternateName>(value, typeof (T));
        }

        internal static object XmlToEnum<TXmlEnumAttribute>(string value, Type type, bool requireValue = true) where TXmlEnumAttribute : XmlEnumAttribute
        {
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttributes(field, typeof(TXmlEnumAttribute)).Where(a => a.GetType() == typeof(TXmlEnumAttribute)).Cast<TXmlEnumAttribute>().FirstOrDefault();

                if (attribute != null)
                {
                    if (attribute.Name == value)
                        return field.GetValue(null);
                }
            }

            try
            {
                return Enum.Parse(type, value, true);
            }
            catch
            {
                if (requireValue)
                    throw;

                return null;
            }
        }

        internal static string EnumToXml(this Enum element)
        {
            var attribute = element.GetEnumAttribute<XmlEnumAttribute>();

            if (attribute == null)
                return element.ToString();

            else
                return attribute.Name;
        }

        public static string GetDescription(this Enum element, bool toStringFallback = true)
        {
            var memberInfo = element.GetType().GetMember(element.ToString());

            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo.First().GetCustomAttributes(typeof (DescriptionAttribute), false);

                if (attributes.Length > 0)
                {
                    return ((DescriptionAttribute) attributes.First()).Description;
                }

                if (!toStringFallback)
                    return null;
            }

            return element.ToString();
        }

        public static bool IsUndocumented(this Enum element)
        {
            return element.GetEnumAttribute<UndocumentedAttribute>() != null;
        }

        public static TAttribute GetEnumAttribute<TAttribute>(this Enum element, bool mandatory = false) where TAttribute : Attribute
        {
            var attributes = element.GetType().GetMember(element.ToString()).First().GetCustomAttributes(typeof(TAttribute), false);

            if (attributes.Any())
                return (TAttribute)attributes.First();

            if (!mandatory)
                return null;
            else
                throw new MissingAttributeException(element.GetType(), element.ToString(), typeof (TAttribute));
        } 

        public static ParameterType GetParameterType(this Parameter element)
        {
            var attributes = element.GetType().GetMember(element.ToString()).First().GetCustomAttributes(typeof (ParameterTypeAttribute), false);

            if (attributes.Length > 0)
            {
                return ((ParameterTypeAttribute) attributes.First()).Type;
            }

            throw new MissingParameterTypeException(element);
        }

        public static T[] GetDependentProperties<T>(this Enum element)
        {
            var elms = element.GetType().GetMembers()
                .Where(m => m.GetCustomAttributes(typeof(DependentPropertyAttribute), false)
                .Cast<DependentPropertyAttribute>()
                .Any(a => a.Name == element.ToString()))
                .Select(e => e.Name.ToEnum<T>())
                .ToArray();

            return elms;
        }
    }
}
