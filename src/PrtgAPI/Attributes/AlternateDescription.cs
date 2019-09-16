using System;
using System.ComponentModel;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false)]
    internal class AlternateDescription : DescriptionAttribute
    {
        internal AlternateDescription(string description) : base(description)
        {
        }
    }
}
