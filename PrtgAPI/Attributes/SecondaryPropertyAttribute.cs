using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class SecondaryPropertyAttribute : Attribute
    {
        public string Name { get; private set; }

        public SecondaryPropertyAttribute(string name)
        {
            this.Name = name;
        }
    }
}
