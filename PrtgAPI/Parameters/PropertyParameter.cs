namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents a single parameter for modifying an <see cref="ObjectProperty"/>.
    /// </summary>
    public class PropertyParameter : PropertyParameter<ObjectProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyParameter"/> class.
        /// </summary>
        /// <param name="property">The property to modify.</param>
        /// <param name="value">The value to set the specified <paramref name="property"/> to.</param>
        public PropertyParameter(ObjectProperty property, object value) : base(property, value)
        {
        }
    }
}
