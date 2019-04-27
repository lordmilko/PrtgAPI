using System.Diagnostics;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Base class for parameter types that modify object properties.
    /// </summary>
    /// <typeparam name="T">The type of property manipulated by this object.</typeparam>
    [DebuggerDisplay("{Property,nq} = {Value}")]
    public abstract class PropertyParameter<T>
    {
        /// <summary>
        /// Gets or sets the property to modify.
        /// </summary>
        public T Property { get; set; }

        /// <summary>
        /// Gets or sets the value to set the <see cref="Property"/> to.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyParameter{T}"/> class.
        /// </summary>
        /// <param name="property">The property to modify.</param>
        /// <param name="value">The value to set the specified <paramref name="property"/> to.</param>
        public PropertyParameter(T property, object value)
        {
            Property = property;
            Value = value;
        }
    }
}
