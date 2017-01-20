using System;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies a property for use as the name or value of a URL parameter/query string.
    /// </summary>
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
