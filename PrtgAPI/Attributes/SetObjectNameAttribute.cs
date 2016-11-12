using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class SetObjectNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public SetObjectNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
