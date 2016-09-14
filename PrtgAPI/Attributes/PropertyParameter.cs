using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    sealed class PropertyParameter : Attribute
    {
        public PropertyParameter(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
