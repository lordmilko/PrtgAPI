using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    sealed class DependentPropertyAttribute : Attribute
    {
        /// <summary>
        /// The name of the property to depend on
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The value that should be assigned to the dependent property.
        /// </summary>
        public object RequiredValue { get; private set; }

        /// <summary>
        /// Indicates whether the property specified by <see cref="Name"/> has a dependency on this field when its value is <see cref="RequiredValue"/>
        /// </summary>
        public bool ReverseDependency { get; private set; }

        public DependentPropertyAttribute(string name, object requiredValue, bool reverseDependency = false)
        {
            Name = name;
            RequiredValue = requiredValue;
            ReverseDependency = reverseDependency;
        }
    }
}
