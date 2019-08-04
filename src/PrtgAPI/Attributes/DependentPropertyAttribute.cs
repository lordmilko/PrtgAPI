using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    internal sealed class DependentPropertyAttribute : Attribute
    {
        public Enum Property { get; set; }

        /// <summary>
        /// The value that should be assigned to the dependent property when this property is assigned a value.<para/>
        /// For example, if a property does not apply unless its section inheritance is disabled, the required value is inheritance = false.
        /// </summary>
        public object RequiredValue { get; private set; }

        /// <summary>
        /// Indicates whether the property specified by <see cref="Property"/> has a dependency on this field when its value is <see cref="RequiredValue"/><para/>
        /// For example, if a value can have automatic and manual modes, the manual value depends on the mode being manual, however when the mode is manual
        /// the manual value must also be specified.
        /// </summary>
        public bool ReverseDependency { get; private set; }

        public DependentPropertyAttribute(ObjectProperty property, object requiredValue, bool reverseDependency = false)
            : this((Enum)property, requiredValue, reverseDependency)
        {
        }

        public DependentPropertyAttribute(ObjectPropertyInternal property, object requiredValue, bool reverseDependency = false)
            : this((Enum)property, requiredValue, reverseDependency)
        {
        }

        public DependentPropertyAttribute(ChannelProperty property, object requiredValue, bool reverseDependency = false)
            : this((Enum)property, requiredValue, reverseDependency)
        {
        }

        public DependentPropertyAttribute(string property, Type type, object requiredValue, bool reverseDependency = false)
            : this((Enum)Enum.Parse(type, property), requiredValue, reverseDependency)
        {
        }

        private DependentPropertyAttribute(Enum property, object requiredValue, bool reverseDependency = false)
        {
            Property = property;
            RequiredValue = requiredValue;
            ReverseDependency = reverseDependency;
        }
    }
}
