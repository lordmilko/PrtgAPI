using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters.Helpers
{
    internal class DynamicPropertyTypeParser
    {
        public Enum Property { get; set; }
        public Type PropertyType { get; set; }
        public Type ValueType { get; set; }

        public object Value { get; }
        public PropertyInfo Info { get; }

        public TypeCategory? Type { get; private set; }

        public DynamicPropertyTypeParser(Enum property, PropertyInfo info, object value)
        {
            Property = property;
            Value = value;
            Info = info;

            GetParameterTypes();
        }

        private void GetParameterTypes()
        {
            PropertyType = Info.PropertyType;
            ValueType = Value?.GetType();

            var propertyTypeUnderlying = Nullable.GetUnderlyingType(PropertyType);

            if (ValueType == null)
            {
                if (propertyTypeUnderlying != null)
                    PropertyType = propertyTypeUnderlying;

                return;
            }
                
            var valueTypeUnderlying = Nullable.GetUnderlyingType(ValueType);

            //If the property and value types aren't both nullable, if one of them IS nullable set that one to its underlying type (Nullable<T> => T)
            if (!(propertyTypeUnderlying != null && valueTypeUnderlying != null))
            {
                if (propertyTypeUnderlying != null)
                    PropertyType = propertyTypeUnderlying;

                if (valueTypeUnderlying != null)
                    ValueType = valueTypeUnderlying;
            }

            //If the property type is not nullable but the value type is, set the value type to its underlying type (Nullable<T> => T)
            if (propertyTypeUnderlying == null)
            {
                //But the value type is, 
                if (valueTypeUnderlying != null)
                {
                    ValueType = valueTypeUnderlying;
                }
            }
        }

        /// <summary>
        /// Validate and retrieve the serialized format of a value based on its type.
        /// </summary>
        /// <returns></returns>
        public object ParseValue()
        {
            object val;

            //Transform the value according to its properties' data type

            //String, Int and Double can be used as is
            if (PropertyType == typeof (string) || PropertyType == typeof (int) || PropertyType == typeof (double))
            {
                if (PropertyType == typeof (string))
                    Type = TypeCategory.String;
                else
                    Type = TypeCategory.Number;

                val = Value?.ToString();
            }
                
            else
            {
                if (Value == null)
                    throw new ArgumentNullException(nameof(Value), $"Value 'null' could not be assigned to property '{Info.Name}' of type '{Info.PropertyType}'. Null may only be assigned to properties of type string, int and double.");

                //Convert bool to int
                if (PropertyType == typeof(bool))
                    val = ParseBoolValue();
                else if (PropertyType.IsEnum)
                {
                    val = ParseEnumValue();

                    if (val == null)
                        ThrowEnumArgumentException();
                    else
                        Type = TypeCategory.Enum;
                }
                else
                    val = ParseOtherValue();

                if (val == null)
                    throw new ArgumentException($"Value '{Value}' could not be assigned to property '{Info.Name}'. Expected type: '{PropertyType}'. Actual type: '{ValueType}'.");
            }

            return val;
        }

        private object ParseEnumValue()
        {
            object val = null;

            //If our value type was an enum, get its XmlEnumAttribute immediately
            if (PropertyType == ValueType)
                val = ((Enum)Value).GetEnumAttribute<XmlEnumAttribute>(true).Name;
            else
            {
                //Otherwise, our value may have been a string. See if any enum members are named after the specified value
                if (Enum.GetNames(PropertyType).Any(x => x.ToLower() == Value.ToString().ToLower()))
                    val = ((Enum)Enum.Parse(PropertyType, Value.ToString(), true)).GetEnumAttribute<XmlEnumAttribute>(true).Name;
                else
                {
                    //If the enum represents a set of numeric values and our value was an integer,
                    int result;

                    if (PropertyType.GetCustomAttribute<NumericEnumAttribute>() != null && int.TryParse(Value.ToString(), out result))
                    {
                        var enumVal = Enum.Parse(PropertyType, Value.ToString());

                        val = ((Enum)enumVal).GetEnumAttribute<XmlEnumAttribute>(true).Name;
                    }
                }
            }

            return val;
        }

        private object ParseBoolValue()
        {
            object val = null;

            if (ValueType == typeof (bool))
            {
                Type = TypeCategory.Boolean;
                val = ((bool)Value) ? "1" : "0";
            }
                

            return val;
        }

        private object ParseOtherValue()
        {
            object val;

            if (PropertyType == ValueType)
            {
                if (typeof (IFormattable).IsAssignableFrom(PropertyType))
                {
                    Type = TypeCategory.Other;
                    val = ((IFormattable)Value).GetSerializedFormat();
                }
                    
                else
                    throw new InvalidTypeException($"Cannot serialize value of type {PropertyType}; type does not implement {nameof(IFormattable)}");
            }
            else
                throw new InvalidTypeException(PropertyType, ValueType);

            return val;
        }

        public object ToPrimitivePropertyType()
        {
            if (Type == TypeCategory.String)
                return Value.ToString();
            if (Type == TypeCategory.Number)
                return Convert.ToDouble(Value);
            if (Type == TypeCategory.Enum)
            {
                if (PropertyType == typeof(Enum))
                    return Value;
                
                return Enum.Parse(PropertyType, Value.ToString(), true);
            }
            if (Type == TypeCategory.Boolean)
                return Convert.ToBoolean(Value);

            if (Type == null)
                throw new InvalidOperationException("TypeCategory was null, however this should be impossible. Was method ParseValue run first?");

            throw new InvalidOperationException($"Cannot convert value of category '{Type}' to a primative type");
        }

        private void ThrowEnumArgumentException()
        {
            var enumVals = Enum.GetValues(PropertyType).Cast<Enum>().ToList();

            var builder = new StringBuilder();

            for (int i = 0; i < enumVals.Count; i++)
            {
                builder.Append($"'{enumVals[i]}'");

                if (i < enumVals.Count - 2)
                    builder.Append(", ");
                else if (i == enumVals.Count - 2)
                    builder.Append(" or ");
            }

            throw new ArgumentException($"'{Value}' is not a valid value for enum {PropertyType.Name}. Please specify one of {builder}");
        }
    }
}