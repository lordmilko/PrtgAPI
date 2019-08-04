using System;
using System.ComponentModel;
using System.Xml.Serialization;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

namespace PrtgAPI
{
    interface IStringEnum
    {
        string StringValue { get; }
    }

    /// <summary>
    /// Provides a type safe wrapper around a string value that may or may not be convertable to a specified
    /// enum type.
    /// </summary>
    /// <typeparam name="TEnum">The type of enum to convert to.</typeparam>
    public class StringEnum<TEnum> : IEquatable<StringEnum<TEnum>>, IStringEnum where TEnum : struct
    {
        private bool split;

        /// <summary>
        /// Gets the type safe enum version of <see cref="StringValue"/>. If <see cref="StringValue"/> is not convertable
        /// to type <typeparamref name="TEnum"/>, this value is null.
        /// </summary>
        public TEnum? Value { get; }

        /// <summary>
        /// Gets the raw string value encapsulated by this wrapper.<para/>If only a string value is provided when
        /// this object is instantiated, the string value may be changed during construction if the string deserializes
        /// to a <see cref="Value"/> that has a <see cref="DescriptionAttribute"/>.
        /// </summary>
        public string StringValue { get; private set; }

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StringEnum{TEnum}"/> class with a specified enum value.<para/>
        /// The <see cref="StringValue"/> will be derlived from the enum's <see cref="XmlEnumAttribute"/> or <see cref="DescriptionAttribute"/>
        /// if applicable.
        /// </summary>
        /// <param name="enumValue">The enum to encapsulate.</param>
        public StringEnum(TEnum enumValue)
        {
            if (!Enum.IsDefined(typeof(TEnum), enumValue))
                throw GetArgumentException(enumValue.ToString());

            StringValue = Serialize(enumValue);
            Value = enumValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringEnum{TEnum}"/> class with a specified string value.
        /// </summary>
        /// <param name="stringValue">The string value to encapsulate.</param>
        public StringEnum(string stringValue)
        {
            if (stringValue == null)
                throw new ArgumentNullException(nameof(stringValue));

            if (string.IsNullOrWhiteSpace(stringValue))
                throw new ArgumentException($"{nameof(stringValue)} cannot be empty or whitespace.", nameof(stringValue));

            StringValue = stringValue;

            var val = Deserialize(stringValue);

            if (val != null)
                Value = (TEnum) val;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringEnum{TEnum}"/> class with a specified enum and string value.
        /// </summary>
        /// <param name="stringValue">The string value to encapsulate.</param>
        /// <param name="enumValue">The enum value to encapsulate.</param>
        public StringEnum(string stringValue, TEnum enumValue) : this(stringValue)
        {
            split = true;
            Value = enumValue;
        }

        #endregion
        #region Operators

        /// <summary>
        /// Determines whether two values of type <see cref="StringEnum{TEnum}"/> are equal.
        /// </summary>
        /// <param name="first">The first value to compare.</param>
        /// <param name="second">The second value to compare.</param>
        /// <returns>If both operands are "split" values containing a custom <see cref="Value"/> and <see cref="StringValue"/>, true if both members are equal, otherwise false.
        /// If both operands contain a <see cref="Value"/>, true if these members are equal.
        /// If the <see cref="Value"/> of both operands was equal, or only one operand contained a <see cref="Value"/>, true if the <see cref="StringValue"/> of both operands are equal.
        /// Otherwise, false. For examples of valid comparisons, please see the remarks.
        /// </returns>
        /// <remarks>
        /// Equality Examples
        ///     Sensor (ping) == Sensor (ping)
        ///     Sensor (ping) != Sensor (wmiremoteping)
        ///     Sensor (ping) == Sensor
        ///     Sensor (ping) == ping
        ///     ping == ping
        ///     Device (ping) != Sensor (ping)
        ///     Device != Sensor
        ///     ping != wmiremoteping 
        /// </remarks>
        public static bool operator ==(StringEnum<TEnum> first, StringEnum<TEnum> second)
        {
            return IsEqual(first, second);
        }

        /// <summary>
        /// Determines whether two values of type <see cref="StringEnum{TEnum}"/> are not equal.
        /// </summary>
        /// <param name="first">The first value to compare.</param>
        /// <param name="second">The second value to compare.</param>
        /// <returns>If both operands are "split" values containing a custom <see cref="Value"/> and <see cref="StringValue"/>, true if both members are different, otherwise false.
        /// If both operands contain a <see cref="Value"/>, true if these members are different.
        /// If the <see cref="Value"/> of both operands was equal, or only one operand contained a <see cref="Value"/>, true if the <see cref="StringValue"/> of both operands are different.
        /// Otherwise, true.
        /// </returns>
        public static bool operator !=(StringEnum<TEnum> first, StringEnum<TEnum> second)
        {
            return !IsEqual(first, second);
        }

        /// <summary>
        /// Creates a new <see cref="StringEnum{TEnum}"/> from a specified string value.
        /// </summary>
        /// <param name="value">The value to encapsulate.</param>
        public static implicit operator StringEnum<TEnum>(string value)
        {
            return new StringEnum<TEnum>(value);
        }

        /// <summary>
        /// Creates a new <see cref="StringEnum{TEnum}"/> from a specified enum value.
        /// </summary>
        /// <param name="enumValue">The value to encapsulate.</param>
        public static implicit operator StringEnum<TEnum>(TEnum enumValue)
        {
            return new StringEnum<TEnum>(enumValue);
        }

        #endregion
        #region Equality

        /// <summary>
        /// Returns a boolean indicating whether the specified object <paramref name="obj"/>
        /// is equal to the current object.<para/>
        /// The specified object is equal to this if both objects are of the same type and all of their information combined in either object
        /// represents less than or the same amount of information required to describe the same "value".<para/>
        /// More precisely, two objects are equal if they have the same <see cref="Value"/> and <see cref="StringValue"/>
        /// (ignoring case, if both were specified during construction),<para/>have the same <see cref="Value"/> (if only the
        /// <see cref="Value"/> was specified)<para/>or have the same <see cref="StringValue"/> (ignoring case, if the <see cref="Value"/>
        /// was equal or wasn't specified).
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (obj is StringEnum<TEnum> && Equals((StringEnum<TEnum>)obj))
                return true;

            return false;
        }

        /// <summary>
        /// Returns a boolean indicating whether the specified <see cref="StringEnum{TEnum}"/> is
        /// equal to this object.<para/>
        /// The specified object is equal to this if all of their information combined in either object
        /// represents less than or the same amount of information required to describe the same "value".<para/>
        /// More precisely, two objects are equal if they have the same <see cref="Value"/> and <see cref="StringValue"/>
        /// (ignoring case, if both were specified during construction),<para/>have the same <see cref="Value"/> (if only the
        /// <see cref="Value"/> was specified)<para/>or have the same <see cref="StringValue"/> (ignoring case, if the <see cref="Value"/>
        /// was equal or wasn't specified).
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(StringEnum<TEnum> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            var valueEquals = false;

            if (Value != null && other.Value != null)
            {
                if (!Value.Equals(other.Value))
                    return false;
                else
                    valueEquals = true;
            }

            var strEquals = string.Equals(StringValue, other.StringValue, StringComparison.OrdinalIgnoreCase);

            if (strEquals)
                return true;
            else
            {
                if (valueEquals)
                {
                    //We have a matching Value (e.g. Sensor) but don't have a matching StringValue (which could just be the result of Serialize(enum).
                    //If we are comparing Sensor (ping) == Sensor, we want to return true,
                    //however if we are comparing Sensor (ping) == Sensor (wmiremoteping) we want to return false
                    if (split && other.split)
                        return false;
                    else
                        return split || other.split;
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool IsEqual(StringEnum<TEnum> first, StringEnum<TEnum> second)
        {
            if (ReferenceEquals(first, null))
            {
                if (ReferenceEquals(second, null))
                    return true;
                return false;
            }

            return first.Equals(second);
        }

        /// <summary>
        /// Returns the hash code for this object. If two objects have the same <see cref="StringValue"/>
        /// (ignoring case where the object is a <see cref="StringEnum{TEnum}"/>) they will have the same hash code.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(StringValue);
        }

        #endregion

        private static string Serialize(TEnum enumValue)
        {
            Enum e = (Enum) (object) enumValue;

            var enumCache = e.GetEnumTypeCache();

            var cache = enumCache.ValueCache[e];

            Attribute[] attribs;

            if (cache.Attributes.TryGetValue(typeof(DescriptionAttribute), out attribs))
                return ((DescriptionAttribute)attribs[0]).Description;

            if (cache.Attributes.TryGetValue(typeof(XmlEnumAttribute), out attribs))
                return ((XmlEnumAttribute)attribs[0]).Name;

            return enumValue.ToString().ToLower();
        }

        private object Deserialize(string str)
        {
            var cache = typeof(TEnum).GetTypeCache();

            foreach (var field in cache.Fields)
            {
                var attribute = field.GetAttribute<DescriptionAttribute>();

                if (attribute != null)
                {
                    if (attribute.Description == str)
                    {
                        return (TEnum)field.Field.GetValue(null);
                    }
                }
                else
                {
                    if (field.Field.Name == str)
                    {
                        return (TEnum)field.Field.GetValue(null);
                    }
                }
            }

            //Failed to resolve value to DescriptionAttribute. Try XmlEnum
            var val = EnumExtensions.XmlToEnum(str, typeof(TEnum), typeof(XmlEnumAttribute), false, true, false);

            if (val != null)
                return val;

            TEnum value;

            if (Enum.TryParse(str, true, out value))
            {
                var description = ((Enum) (object) value).GetEnumFieldCache().GetAttribute<DescriptionAttribute>();

                if (description != null)
                    StringValue = description.Description;

                return value;
            }

            return null;
        }

        private static ArgumentException GetArgumentException(string str)
        {
            return new ArgumentException($"'{str}' is not a valid value for type '{typeof(TEnum).Name}'.");
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            if (Value != null)
            {
                if (split)
                    return $"{Value} ({StringValue})";

                return Value.ToString();
            } 

            return StringValue;
        }
    }
}
