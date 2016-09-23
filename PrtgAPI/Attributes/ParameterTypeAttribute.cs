using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class ParameterTypeAttribute : Attribute
    {
        public ParameterTypeAttribute(PrtgAPI.ParameterType type)
        {
            this.Type = type;
        }

        public PrtgAPI.ParameterType Type { get; private set; }
    }
}
