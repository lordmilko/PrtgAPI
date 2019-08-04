using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    sealed class SplittableStringAttribute : Attribute
    {
        public char Character { get; private set; }

        public SplittableStringAttribute(char character)
        {
            Character = character;
        }
    }
}
