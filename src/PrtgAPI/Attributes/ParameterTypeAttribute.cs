using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class ParameterTypeAttribute : Attribute
    {
        public ParameterType Type { get; private set; }

        public ParameterTypeAttribute(ParameterType type)
        {
            Type = type;
        }
    }
}
