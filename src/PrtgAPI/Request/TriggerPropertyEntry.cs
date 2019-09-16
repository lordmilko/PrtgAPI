using System.Diagnostics;
using PrtgAPI.Reflection.Cache;

namespace PrtgAPI.Request
{
    [DebuggerDisplay("JsonName = {JsonName}, Property = {Property}, TypedProperty = {TypedProperty}, TypedRawProperty = {TypedRawProperty}, RawProperty = {RawProperty}")]
    struct TriggerPropertyEntry
    {
        internal string JsonName { get; }

        internal TriggerProperty Property { get; }

        internal PropertyCache TypedProperty { get; }

        internal FieldCache TypedRawField { get; }

        internal PropertyCache RawProperty { get; }

        internal PropertyCache RawInput { get; }

        internal TriggerPropertyEntry(string jsonName, TriggerProperty property, PropertyCache typedProperty, FieldCache typedRawField, PropertyCache rawProperty, PropertyCache rawInput)
        {
            JsonName = jsonName;
            Property = property;
            TypedProperty = typedProperty;
            TypedRawField = typedRawField;
            RawProperty = rawProperty;
            RawInput = rawInput;
        }
    }
}