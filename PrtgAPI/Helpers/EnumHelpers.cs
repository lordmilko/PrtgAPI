using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Objects.Deserialization.Cache;

namespace PrtgAPI.Helpers
{
    [ExcludeFromCodeCoverage]
    static class EnumHelpers
    {
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Get the underlying flags of an enum element. Note: using the enum with value 0 is not supported. Start enum values at 1 instead..
        /// </summary>
        /// <param name="element">The value to get the underlying flags of.</param>
        /// <returns>The underlying flags of the enum, or an empty list.</returns>
        public static IEnumerable<Enum> GetUnderlyingFlags(this Enum element)
        {
            var enums = Enum.GetValues(element.GetType()).Cast<Enum>().ToList();

            return GetUnderlyingFlagsInternal(element, enums).Distinct();
        }

        private static IEnumerable<Enum> GetUnderlyingFlagsInternal(Enum e, List<Enum> enums)
        {
            foreach (var enumListMember in enums)
            {
                if (e.HasFlag(enumListMember) && !e.Equals(enumListMember) && Convert.ToInt32(enumListMember) != 0)
                {
                    //Recurse and see whether this enum has children of its own
                    var result = GetUnderlyingFlagsInternal(enumListMember, enums).ToList();

                    //No children, just return me
                    if (result.Count == 0)
                        yield return enumListMember;
                    else
                    {
                        //Forget me and return all my children
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

        internal static T XmlToEnum<T>(this string value)
        {
            return (T) XmlToEnum<XmlEnumAttribute>(value, typeof (T));
        }

        internal static T XmlAltToEnum<T>(this string value)
        {
            return (T) XmlToEnum<XmlEnumAlternateName>(value, typeof (T));
        }

        internal static object XmlToEnumAnyAttrib(string value, Type type, Type[] attribTypes = null, bool requireValue = true, bool allowFlags = true, bool allowParse = true)
        {
            if (attribTypes == null)
                attribTypes = new[] {typeof (XmlEnumAttribute), typeof (XmlEnumAlternateName)};

            var req = false;

            for (int i = 0; i < attribTypes.Length; i++)
            {
                var val = XmlToEnum(value, type, attribTypes[i], req, allowFlags, allowParse);

                if (val != null)
                    return val;

                if (i < attribTypes.Length - 1 && requireValue)
                    req = true;
            }

            return null;
        }

        internal static object XmlToEnum<TXmlEnumAttribute>(string value, Type type, bool requireValue = true, bool allowFlags = true) where TXmlEnumAttribute : XmlEnumAttribute
        {
            return XmlToEnum(value, type, typeof (TXmlEnumAttribute), requireValue, allowFlags);
        }

        internal static object XmlToEnum(string value, Type type, Type attribType, bool requireValue = true, bool allowFlags = true, bool allowParse = true)
        {
            var val = ReflectionCacheManager.GetEnumXml(type).GetValue(value, attribType);

            if (val != null)
                return val;


            if (!allowParse)
                return null;

            try
            {
                var e = Enum.Parse(type, value, true);

                if (allowFlags == false && ((Enum)e).GetUnderlyingFlags().Any())
                    return null;

                return e;
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
