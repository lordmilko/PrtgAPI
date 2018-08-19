using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies the category a value belongs to.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CategoryAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the category this attribute's associated field belongs to.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the category this attribute's associated field belongs to.</param>
        public CategoryAttribute(string name)
        {
            Name = name;
        }
    }
}
