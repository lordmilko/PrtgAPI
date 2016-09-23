using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class SetObjectNameAttribute : Attribute
    {
        public SetObjectNameAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
