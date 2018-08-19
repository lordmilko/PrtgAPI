using System;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Indicates whether the associated property requires a value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class RequireValueAttribute : Attribute
    {
        /// <summary>
        /// Gets whether the associated property requires a value.
        /// </summary>
        public bool ValueRequired { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireValueAttribute"/> class.
        /// </summary>
        /// <param name="valueRequired">Whether the associated property requires a value.</param>
        public RequireValueAttribute(bool valueRequired)
        {
            ValueRequired = valueRequired;
        }
    }
}
