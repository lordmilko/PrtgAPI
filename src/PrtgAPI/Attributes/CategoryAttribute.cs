using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies the category a value belongs to.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    internal sealed class CategoryAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the category this attribute's associated field belongs to.
        /// </summary>
        public Enum Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryAttribute"/> class.
        /// </summary>
        /// <param name="category">The name of the category this attribute's associated field belongs to.</param>
        public CategoryAttribute(ObjectPropertyCategory category)
        {
            Name = category;
        }

        public CategoryAttribute(SystemInfoType type)
        {
            Name = type;
        }
    }
}
