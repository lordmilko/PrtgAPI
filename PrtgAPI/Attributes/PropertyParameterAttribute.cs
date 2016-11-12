using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    sealed class PropertyParameterAttribute : Attribute
    {
        public string Name { get; private set; }

        public PropertyParameterAttribute(string name)
        {
            this.Name = name;
        }
    }
}
