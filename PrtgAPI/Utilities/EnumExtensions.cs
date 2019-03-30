using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;

namespace PrtgAPI.Utilities
{
    [ExcludeFromCodeCoverage]
    static class EnumExtensions
    {
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Get the underlying flags of an enum element. If the enum has no underlying elements, an empty list is returned. Note: using the enum with value 0 is not supported. Start enum values at 1 instead..
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
            var cache = typeof(TEnum).GetTypeCache();

            foreach (var field in cache.Fields)
            {
                var attribute = field.GetAttribute<DescriptionAttribute>();

                if (attribute != null)
                {
                    if (attribute.Description == value)
                        return (TEnum) field.Field.GetValue(null);
                }
                else
                {
                    if (field.Field.Name == value)
                        return (TEnum) field.Field.GetValue(null);
                }
            }

            if (!toStringFallback)
                throw new ArgumentException($"'{value}' is not a description for any value in {typeof(TEnum)}.", nameof(value));

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

        internal static object XmlToEnum(string value, Type type, Type attribType, bool requireValue = true, bool allowFlags = true, bool allowParse = true, bool allowNumeric = false)
        {
            var enumXmlCache = ReflectionCacheManager.GetEnumXml(type);

            var val = enumXmlCache.Cache.GetValue(value, attribType);

            if (val != null)
                return val;

            if (!allowParse)
                return null;

            try
            {
                var underlying = Nullable.GetUnderlyingType(type);

                if (underlying != null)
                    type = underlying;

                //todo: make this a tryparse, and then reparse and throw if we failed to parse

                var e = Enum.Parse(type, value, true);

                if (allowFlags == false && ((Enum)e).GetUnderlyingFlags().Any())
                    return null;

                if (!allowNumeric && e.ToString().ToLower() != value.ToLower())
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
            var description = element.GetEnumFieldCache().GetAttribute<DescriptionAttribute>();

            if (description != null)
                return description.Description;

            if (!toStringFallback)
                return null;

            return element.ToString();
        }

        public static bool IsUndocumented(this Enum element)
        {
            return element.GetEnumAttribute<UndocumentedAttribute>() != null;
        }

        public static TAttribute[] GetEnumAttributes<TAttribute>(this Enum element, bool mandatory = false)
            where TAttribute : Attribute
        {
            var cache = element.GetEnumFieldCache();

            if (cache == null)
                throw new InvalidOperationException($"Cannot retrieve {typeof(TAttribute)} from element '{element}'; value is not a member of type {element.GetType()}.");

            var attribute = cache.GetAttributes<TAttribute>();

            if (attribute.Length > 0)
                return attribute;

            if (!mandatory)
                return null;
            else
                throw new MissingAttributeException(element.GetType(), element.ToString(), typeof(TAttribute));
        }

        public static TAttribute GetEnumAttribute<TAttribute>(this Enum element, bool mandatory = false) where TAttribute : Attribute
        {
            var cache = element.GetEnumFieldCache();
            
            if(cache == null)
                throw new InvalidOperationException($"Cannot retrieve {typeof(TAttribute)} from element '{element}'; value is not a member of type {element.GetType()}.");

            var attribute = cache.GetAttribute<TAttribute>();

            if (attribute != null)
                return attribute;

            if (!mandatory)
                return null;

            throw new MissingAttributeException(element.GetType(), element.ToString(), typeof (TAttribute));
        }

        public static ParameterType GetParameterType(this Parameter element)
        {
            var attribute = element.GetEnumFieldCache().GetAttribute<ParameterTypeAttribute>();

            if (attribute != null)
                return attribute.Type;

            throw new MissingParameterTypeException(element);
        }

        public static Enum[] GetDependentProperties(this Enum element)
        {
            var elms = element.GetEnumTypeCache().ValueCache
                .Where(e =>
                    Equals(e.Value.GetAttribute<DependentPropertyAttribute>()?.Property, element)
                )
                .Select(e => e.Key)
                .OrderBy(e => e)
                .ToArray();

            return elms;
        }
    }
}
