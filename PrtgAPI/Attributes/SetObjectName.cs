using System;

namespace Prtg.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class SetObjectName : Attribute
    {
        public SetObjectName(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
