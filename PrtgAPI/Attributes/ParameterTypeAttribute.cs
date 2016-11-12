using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class ParameterTypeAttribute : Attribute
    {
        public PrtgAPI.ParameterType Type { get; private set; }

        public ParameterTypeAttribute(PrtgAPI.ParameterType type)
        {
            this.Type = type;
        }
    }
}
