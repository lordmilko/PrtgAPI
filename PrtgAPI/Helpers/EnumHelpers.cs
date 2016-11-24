using System;
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

        public static T DescriptionToEnum<T>(this string value, bool toStringFallback = true)
        {
            foreach (var field in typeof (T).GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof (DescriptionAttribute)) as DescriptionAttribute;

                if (attribute != null)
                {
                    if (attribute.Description == value)
                        return (T) field.GetValue(null);
                }
                else
                {
                    if (field.Name == value)
                        return (T) field.GetValue(null);
                }
            }

            if(!toStringFallback)
                throw new ArgumentException("Is not a description for any value in " + typeof (T), nameof(value));

            return value.ToEnum<T>();
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

        public static T GetEnumAttribute<T>(this Enum element, bool mandatory = false) where T : Attribute
        {
            var attributes = element.GetType().GetMember(element.ToString()).First().GetCustomAttributes(typeof(T), false);

            if (attributes.Any())
                return (T)attributes.First();

            if (!mandatory)
                return null;
            else
                throw new MissingAttributeException(element.GetType(), element.ToString(), typeof (T));
        } 

        public static ParameterType GetParameterType(this Parameter element)
        {
            var attributes = element.GetType().GetMember(element.ToString()).First().GetCustomAttributes(typeof (ParameterTypeAttribute), false);

            if (attributes.Length > 0)
            {
                return ((ParameterTypeAttribute) attributes.First()).Type;
            }

            throw new Exceptions.Internal.MissingParameterTypeException(element);
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
