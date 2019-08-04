using System;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies that a value should be used as-is, without further processing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class LiteralValueAttribute : Attribute
    {
    }
}
