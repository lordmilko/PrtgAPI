using System;

namespace Prtg.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class ParameterType : Attribute
    {
        public ParameterType(Prtg.ParameterType type)
        {
            this.Type = type;
        }

        public Prtg.ParameterType Type { get; private set; }
    }
}
