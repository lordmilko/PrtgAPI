using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Undocumented;

namespace PrtgAPI.Parameters
{
    class SetObjectPropertyParameters : BaseSetObjectPropertyParameters
    {
        //i think the properties in sensorsetting need an enum that links them to an object property
        //and then we use reflection to get the param with a given property and then confirm
        //that the value we were given is convertable to the given type

        //we should then implement this type safety for setchannelproperty as well

        //we need to add handling for inherit error interval

        public int ObjectId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public SetObjectPropertyParameters(int objectId, ObjectProperty property, object value)
        {
            var info = GetPropertyInfoForProperty(property);

            var val = ParseValue(info, value);

            ObjectId = objectId;

            var name = GetParameterName(info, property);


            this[Parameter.Custom] = new CustomParameter(name, val?.ToString());
        }

        public SetObjectPropertyParameters(int objectId, string property, string value)
        {
            ObjectId = objectId;
            this[Parameter.Custom] = new CustomParameter(property, value);
        }

        private Tuple<Type, Type> GetParameterTypes(PropertyInfo prop, object value)
        {
            var propertyType = prop.PropertyType;
            var valueType = value?.GetType();

            if (valueType == null)
                return new Tuple<Type, Type>(propertyType, propertyType);

            var propertyTypeUnderlying = Nullable.GetUnderlyingType(propertyType);
            var valueTypeUnderlying = Nullable.GetUnderlyingType(valueType);

            //If the property and value types aren't both nullable, if one of them IS nullable set that one to its underlying type (Nullable<T> => T)
            if (!(propertyTypeUnderlying != null && valueTypeUnderlying != null))
            {
                if (propertyTypeUnderlying != null)
                    propertyType = propertyTypeUnderlying;

                if (valueTypeUnderlying != null)
                    valueType = valueTypeUnderlying;
            }

            //If the property type is not nullable but the value type is, set the value type to its underlying type (Nullable<T> => T)
            if (propertyTypeUnderlying == null)
            {
                //But the value type is, 
                if (valueTypeUnderlying != null)
                {
                    valueType = valueTypeUnderlying;
                }
            }

            return new Tuple<Type, Type>(propertyType, valueType);
        }

        internal static PropertyInfo GetPropertyInfoForProperty(ObjectProperty property)
        {
            var attr = property.GetEnumAttribute<TypeLookupAttribute>(true);
            var prop = attr.Class.GetProperties().FirstOrDefault(p => p.Name == property.ToString());

            if (prop == null)
                throw new MissingMemberException($"Property {property} cannot be found on type {attr.Class} pointed to by {nameof(TypeLookupAttribute)}");

            return prop;
        }

        private object ParseValue(PropertyInfo prop, object value)
        {
            var tuple = GetParameterTypes(prop, value);
            var propertyType = tuple.Item1;
            var valueType = tuple.Item2;

            object val = null;

            //Transform the value according to its properties' data type

            //String, Int and Double can be used as is
            if (propertyType == typeof(string) || propertyType == typeof(int) || propertyType == typeof(double))
                val = value?.ToString();
            else
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), $"Value 'null' could not be assigned to property '{prop.Name}' of type '{prop.PropertyType}'. Null may only be assigned to properties of type string, int and double.");

                //Convert bool to int
                if (propertyType == typeof(bool))
                {
                    if (valueType == typeof(bool))
                        val = ((bool)value) ? "1" : "0";
                }
                else if (propertyType.IsEnum)
                {
                    val = ParseEnumValue(value, propertyType, valueType);

                    if (val == null)
                    {
                        var enumVals = Enum.GetValues(propertyType).Cast<Enum>().ToList();

                        var builder = new StringBuilder();

                        for (int i = 0; i < enumVals.Count; i++)
                        {
                            builder.Append($"'{enumVals[i]}'");

                            if (i < enumVals.Count - 2)
                                builder.Append(", ");
                            else if(i == enumVals.Count - 2)
                                builder.Append(" or ");
                        }

                        throw new ArgumentException($"'{value}' is not a valid value for enum {propertyType.Name}. Please specify one of {builder}");
                    }
                }
                else
                {
                    if (propertyType == valueType)
                    {
                        if (typeof(IFormattable).IsAssignableFrom(propertyType))
                            val = ((IFormattable)value).GetSerializedFormat();
                        else
                            throw new InvalidTypeException($"Cannot serialize value of type {propertyType}; type does not implement {nameof(IFormattable)}");
                    }
                    else
                        throw new InvalidTypeException(propertyType, valueType);
                }

                if (val == null)
                    throw new ArgumentException($"Value '{value}' could not be assigned to property '{prop.Name}'. Expected type: '{propertyType}'. Actual type: '{valueType}'.");
            }

            return val;
        }

        private object ParseEnumValue(object value, Type propertyType, Type valueType)
        {
            object val = null;

            //If our value type was an enum, get its XmlEnumAttribute immediately
            if (propertyType == valueType)
                val = ((Enum)value).GetEnumAttribute<XmlEnumAttribute>(true).Name;
            else
            {
                //Otherwise, our value may have been a string. See if any enum members are named after the specified value
                if (Enum.GetNames(propertyType).Any(x => x.ToLower() == value.ToString().ToLower()))
                    val = ((Enum)Enum.Parse(propertyType, value.ToString(), true)).GetEnumAttribute<XmlEnumAttribute>(true).Name;
                else
                {
                    //If the enum represents a set of numeric values and our value was an integer,
                    int result;

                    if (propertyType.GetCustomAttribute<NumericEnumAttribute>() != null && int.TryParse(value.ToString(), out result))
                    {
                        var enumVal = Enum.Parse(propertyType, value.ToString());

                        val = ((Enum)enumVal).GetEnumAttribute<XmlEnumAttribute>(true).Name;
                    }
                }
            }

            return val;
        }

        internal static string GetParameterName(PropertyInfo info, ObjectProperty property)
        {
            var attribute = info.GetCustomAttribute<XmlElementAttribute>();
            string name;

            if (attribute == null)
            {
                var description = property.GetDescription(false);

                if (description == null)
                    throw new MissingAttributeException(typeof(ObjectProperty), property.ToString(), typeof(DescriptionAttribute));

                name = description;
            }
            else
            {
                name = attribute.ElementName.Substring("injected_".Length);
            }

            if (property.GetEnumAttribute<LiteralValueAttribute>() == null)
                name += "_";

            return name;
        }
    }
}
