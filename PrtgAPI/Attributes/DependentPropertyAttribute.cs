using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class DependentPropertyAttribute : Attribute
    {
        public string Name { get; private set; }

        public DependentPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}
