namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents a single parameter for modifying a <see cref="TriggerProperty"/>.
    /// </summary>
    public class TriggerParameter : PropertyParameter<TriggerProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerParameter"/> class.
        /// </summary>
        /// <param name="property">The property to modify.</param>
        /// <param name="value">The value to set the specified <paramref name="property"/> to.</param>
        public TriggerParameter(TriggerProperty property, object value) : base(property, value)
        {
        }
    }
}
