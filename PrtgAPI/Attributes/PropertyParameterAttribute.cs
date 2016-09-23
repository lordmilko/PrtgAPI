using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    sealed class PropertyParameterAttribute : Attribute
    {
        public PropertyParameterAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
