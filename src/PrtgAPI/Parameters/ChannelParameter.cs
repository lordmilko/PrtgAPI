namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents a single parameter for modifying a <see cref="ChannelProperty"/>.
    /// </summary>
    public class ChannelParameter : PropertyParameter<ChannelProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelParameter"/> class.
        /// </summary>
        /// <param name="property">The property to modify.</param>
        /// <param name="value">The value to set the specified <paramref name="property"/> to.</param>
        public ChannelParameter(ChannelProperty property, object value) : base(property, value)
        {
        }
    }
}
