using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class UndocumentedAttribute : Attribute
    {
    }
}
