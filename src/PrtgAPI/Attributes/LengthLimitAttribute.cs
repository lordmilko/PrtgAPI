using System;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies that there is a limit to the length or number of items that can be applied to a field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    sealed class LengthLimitAttribute : Attribute
    {
        public LengthLimitAttribute(int length)
        {
            Length = length;
        }

        public int Length { get; private set; }
    }
}
