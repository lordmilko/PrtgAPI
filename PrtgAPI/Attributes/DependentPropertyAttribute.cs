using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    sealed class DependentPropertyAttribute : Attribute
    {
        /// <summary>
        /// The name of the property to depend on.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The value that should be assigned to the dependent property when this property is assigned a value.<para/>
        /// For example, if a property does not apply unless its section inheritance is disabled, the required value is inheritance = false.
        /// </summary>
        public object RequiredValue { get; private set; }

        /// <summary>
        /// Indicates whether the property specified by <see cref="Name"/> has a dependency on this field when its value is <see cref="RequiredValue"/><para/>
        /// For example, if a value can have automatic and manual modes, the manual value depends on the mode being manual, however when the mode is manual
        /// the manual value must also be specified.
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
