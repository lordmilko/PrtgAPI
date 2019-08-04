using System;
using System.ComponentModel;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class AlternateDescription : DescriptionAttribute
    {
        internal AlternateDescription(string description) : base(description)
        {
        }
    }
}
