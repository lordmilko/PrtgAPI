using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Allows inserting custom parameters not supported by PrtgAPI.
    /// </summary>
    public class CustomParameter
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the parameter.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <param name="value">The value of the parameter. The caller is responsible for ensuring the value and type contains the correct capitalization and is formatted corectly when converted <see cref="ToString"/>.</param>
        public CustomParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Format this parameter for use in a <see cref="PrtgUrl"/>.
        /// </summary>
        /// <returns>The formatted representation of this parameter.</returns>
        public override string ToString()
        {
            return $"{Name}={Value}";
        }
    }
}
