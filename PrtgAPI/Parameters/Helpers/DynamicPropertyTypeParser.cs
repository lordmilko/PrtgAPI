using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Request.Serialization.Cache;

namespace PrtgAPI.Parameters.Helpers
{
    internal class DynamicPropertyTypeParser
    {
        public Enum Property { get; set; }
        public Type PropertyType { get; set; }
        public Type ValueType { get; set; }

        public object Value { get; }
        public PropertyCache Cache { get; }

        public TypeCategory? Type { get; private set; }

        public DynamicPropertyTypeParser(Enum property, PropertyCache cache, object value)
        {
            Property = property;
            Value = value;
            Cache = cache;

            GetParameterTypes();
        }

        private void GetParameterTypes()
        {
            PropertyType = Cache.Property.PropertyType;
            ValueType = Value?.GetType();

            var propertyTypeUnderlying = Nullable.GetUnderlyingType(PropertyType);

            if (ValueType == null)
            {
                if (propertyTypeUnderlying != null)
                    PropertyType = propertyTypeUnderlying;

                return;
            }
            
            //If the property type is nullable, set it to its underlying type. Ideally, we would want to check
            //that both the property type and value type aren't both nullable, and then set whichever one
            //was nullable to its underlying type, however since ValueType will be boxed to its underlying
            //type T, it is never nullable and therefore we don't need to perform this check
            if (propertyTypeUnderlying != null)
                PropertyType = propertyTypeUnderlying;
        }

        /// <summary>
        /// Validate and retrieve the serialized format of a value based on its type.
        /// </summary>
        /// <returns></returns>
        public object ParseValue()
        {
            object val;
            bool isArray = false;
            string splittableStringChar = null;

            //Transform the value according to its properties' data type

            if (Property.GetEnumAttribute<TypeAttribute>() != null)
            {
                if (Value is Request.IFormattable)
                {
                    Type = TypeCategory.Other;
                    return ((Request.IFormattable)Value).GetSerializedFormat();
                }

                throw new NotSupportedException("Serializng a TypeAttribute type that does not implement IFormattable is not currently supported");
            }

            if (PropertyType.IsArray)
            {
                PropertyType = PropertyType.GetElementType();

                if (ValueType?.IsArray == true)
                {
                    ValueType = ValueType.GetElementType();

                    //When an array is specified by PowerShell, the array will be a generic object[]
                    if (ValueType == typeof (object) && Value != null)
                    {
                        ValueType = ((object[])Value).First().GetType();
                    }

                    isArray = true;

                    splittableStringChar = Cache.GetAttribute<SplittableStringAttribute>()?.Character.ToString();

                    if(splittableStringChar == null)
                        throw new NotSupportedException($"Cannot serialize value for array property {Property} as the property is missing a {nameof(SplittableStringAttribute)}");
                    //we should downcast this too
                }
                //if our value is not an array, change property type to the underlying type

                //if our value IS an array, set the array flag, and change the property to the underlying type
                    //we'll then have checks in all our sections later to handle combining the array according to its splittable string

            }

            //todo: if we're an array type, and our value is not an array type, we'll change our property type
            //to its underlying type, and then set a flag to say we were an array so we know to combine according to our splittable
            //string attribute

            //String, Int and Double can be used as is
            if (PropertyType == typeof (string))
            {
                Type = TypeCategory.String;

                if (isArray)
                {
                    //No need to check whether value was null; if value was null, isArray would
                    //not have been set

                    return string.Join(splittableStringChar, ((Array)Value).Cast<object>().ToArray());
                }

                val = Value?.ToString();
            }
            else if (PropertyType == typeof (double) || PropertyType == typeof(int))
            {
                val = ParseNumericValue(isArray);
            }
            else
            {
                if (Value == null)
                    throw new ArgumentNullException(nameof(Value), $"Value 'null' could not be assigned to property '{Cache.Property.Name}' of type '{Cache.Property.PropertyType}'. Null may only be assigned to properties of type string, int and double.");

                if (isArray)
                    throw new NotSupportedException($"Properties containing arrays of type {PropertyType} are not currently supported");

                //Convert bool to int
                if (PropertyType == typeof(bool))
                    val = ParseBoolValue();
                else if (PropertyType.IsEnum)
                {
                    var useAlternateXml = Cache.GetAttribute<TypeLookupAttribute>()?.Class == typeof(XmlEnumAlternateName);

                    if (useAlternateXml)
                        val = ParseEnumValue<XmlEnumAlternateName>();
                    else
                        val = ParseEnumValue<XmlEnumAttribute>();

                    if (val == null)
                        ThrowEnumArgumentException();
                    else
                        Type = TypeCategory.Enum;
                }
                else
                    val = ParseOtherValue();

                if (val == null)
                    throw new ArgumentException($"Value '{Value}' could not be assigned to property '{Cache.Property.Name}'. Expected type: '{PropertyType}'. Actual type: '{ValueType}'.");
            }

            return val;
        }

        private object ParseNumericValue(bool isArray)
        {
            object val = null;

            if (isArray)
                throw new NotSupportedException($"Properties containing arrays of type {PropertyType} are not currently supported");

            //If the value is convertable to a double, it is either an int or a double

            if (!string.IsNullOrEmpty(Value?.ToString()))
            {
                double doubleResult;

                //If the value is a double
                if (Value != null && double.TryParse(Value.ToString(), out doubleResult))
                {
                    //If we're actually looking for an int, see if this double is actually an integer
                    if (PropertyType == typeof(int))
                    {
                        if (Convert.ToInt32(doubleResult) == doubleResult)
                        {
                            //If so, that's cool. When we ToString, we'll get an integer value anyway
                            Type = TypeCategory.Number;
                            val = doubleResult.ToString(CultureInfo.CurrentCulture);
                        }
                    }
                    else
                    {
                        Type = TypeCategory.Number;
                        val = doubleResult.ToString(CultureInfo.CurrentCulture);
                    }
                }

                //If we still don't have a value, since we already verified our value is not null or empty we must have a value of an invalid type
                if (val == null)
                    throw new InvalidTypeException($"Value '{Value}' could not be assigned to property '{Cache.Property.Name}'. Expected type: '{PropertyType}'. Actual type: '{ValueType}'.");
            }

            return val;
        }

        private object ParseEnumValue<T>() where T : XmlEnumAttribute
        {
            object val = null;

            //If our value type was an enum, get its XmlEnumAttribute immediately
            if (PropertyType == ValueType)
                val = ((Enum)Value).GetEnumAttribute<T>(true).Name;
            else
            {
                //Otherwise, our value may have been a string. See if any enum members are named after the specified value
                if (Enum.GetNames(PropertyType).Any(x => x.ToLower() == Value.ToString().ToLower()))
                    val = ((Enum)Enum.Parse(PropertyType, Value.ToString(), true)).GetEnumAttribute<T>(true).Name;
                else
                {
                    //If the enum represents a set of numeric values and our value was an integer,
                    int result;

                    if (PropertyType.GetTypeCache().Cache.GetAttribute<NumericEnumAttribute>() != null && int.TryParse(Value.ToString(), out result))
                    {
                        var enumVal = Enum.Parse(PropertyType, Value.ToString());

                        val = ((Enum)enumVal).GetEnumAttribute<T>(true).Name;
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
                val = Convert.ToInt32((bool) Value);
            }
            else
            {
                bool boolVal;

                if (Value != null && bool.TryParse(Value.ToString(), out boolVal))
                {
                    Type = TypeCategory.Boolean;
                    val = Convert.ToInt32(boolVal);
                }
            }

            return val;
        }

        private object ParseOtherValue()
        {
            object val;

            if (PropertyType == ValueType)
            {
                if (typeof (Request.IFormattable).IsAssignableFrom(PropertyType))
                {
                    Type = TypeCategory.Other;
                    val = ((Request.IFormattable)Value).GetSerializedFormat();
                }
                    
                else
                    throw new InvalidTypeException($"Cannot serialize value of type {PropertyType}; type does not implement {nameof(Request.IFormattable)}");
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