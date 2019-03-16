using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class FactorAttribute : Attribute
    {
        public ChannelPropertyInternal Property { get; }

        internal FactorAttribute(ChannelPropertyInternal property)
        {
            Property = property;
        }
    }
}
