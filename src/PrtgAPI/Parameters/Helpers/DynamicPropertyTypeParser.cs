using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters.Helpers
{
    [DebuggerDisplay("Property = {Property}, Type = {Type?.ToString(),nq}, Value = {Value}, ValueType = {ValueType.FullName,nq}")]
    internal class DynamicPropertyTypeParser
    {
        public Enum Property { get; }
        public Type PropertyType { get; }
        public Type ValueType { get; }

        public object Value { get; }
        public PropertyCache Cache { get; }

        public bool AllowNull { get; set; } = true;

        public TypeCategory? Type { get; set; }

        public DynamicPropertyTypeParser(Enum property, PropertyCache cache, object value)
        {
            Property = property;
            Value = PSObjectUtilities.CleanPSObject(value);
            Cache = cache;

            //Get the PropertyType and ValueType values after unwrapping any nullable types

            PropertyType = Cache.Property.PropertyType;
            ValueType = Value?.GetType();

            var propertyTypeUnderlying = Nullable.GetUnderlyingType(PropertyType);

            if (ValueType == null)
            {
                if (propertyTypeUnderlying != null)
                    PropertyType = propertyTypeUnderlying;
            }
            else
            {
                //If the property type is nullable, set it to its underlying type. Ideally, we would want to check
                //that both the property type and value type aren't both nullable, and then set whichever one
                //was nullable to its underlying type, however since ValueType will be boxed to its underlying
                //type T, it is never nullable and therefore we don't need to perform this check
                if (propertyTypeUnderlying != null)
                    PropertyType = propertyTypeUnderlying;
            }
        }

        /// <summary>
        /// Validate and retrieve the serialized format of a value based on its type.
        /// </summary>
        /// <returns>The serialized value.</returns>
        public object SerializeValue() => ParseValue(SerializationMode.Serialize);

        public object DeserializeValue() => ParseValue(SerializationMode.Deserialize);     

        private object ParseValue(SerializationMode mode)
        {
            var machine = new TypeConversionStateMachine(this, mode);

            machine.Run();

            Type = machine.Type;

            return machine.NewValue;
        }

        [ExcludeFromCodeCoverage]
        internal object ToPrimitivePropertyType()
        {
            if (Type == TypeCategory.String)
                return Value.ToString();
            if (Type == TypeCategory.Number)
                return Value == null ? 0.0 : ConvertUtilities.ToDouble(Value.ToString());
            if (Type == TypeCategory.Enum)
            {
                if (PropertyType == typeof(Enum))
                    return Value;
                
                return Enum.Parse(PropertyType, Value.ToString(), true);
            }
            if (Type == TypeCategory.Boolean)
                return Convert.ToBoolean(Value);

            if (Type == null)
                throw new InvalidOperationException($"{nameof(TypeCategory)} was null, however this should be impossible. Was method {nameof(ParseValue)} run first?");

            throw new InvalidOperationException($"Cannot convert value of category '{Type}' to a primitive type.");
        }

        internal CustomParameter GetParameter(Func<Enum, PropertyCache, string> nameResolver)
        {
            return new CustomParameter(nameResolver(Property, Cache), SerializeValue());
        }
    }
}
