using System;
using System.ComponentModel;
using System.Linq;

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
                var attribute =
                    Attribute.GetCustomAttribute(field, typeof (DescriptionAttribute)) as DescriptionAttribute;

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

        public static ParameterType GetParameterType(this Parameter element)
        {
            var attributes =
                element.GetType()
                    .GetMember(element.ToString())
                    .First()
                    .GetCustomAttributes(typeof (Attributes.ParameterType), false);

            if (attributes.Length > 0)
            {
                return ((Attributes.ParameterType) attributes.First()).Type;
            }

            throw new Exceptions.Internal.MissingParameterTypeException(element);
        }
    }
}
