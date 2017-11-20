using System;
using System.ComponentModel;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Indicates whether a member is exposed by the PRTG API. If a member is not publically exposed,
    /// PRTG may filter or process the associated member differently from fully documented members.<para/>
    /// If this member is a URL function, the <see cref="DescriptionAttribute"/> of this function specifies
    /// the absolute path to the URI resource.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class UndocumentedAttribute : Attribute
    {
    }
}
