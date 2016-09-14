using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class ParameterType : Attribute
    {
        public ParameterType(PrtgAPI.ParameterType type)
        {
            this.Type = type;
        }

        public PrtgAPI.ParameterType Type { get; private set; }
    }
}
