using System;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies that an enum represents a set of numeric values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    sealed class NumericEnumAttribute : Attribute
    {
    }
}
