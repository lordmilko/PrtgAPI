using PrtgAPI.Parameters;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a resource that can be specified as the basis for determining the parameters required to create a new sensor.
    /// </summary>
    public class SensorQueryTarget : ISensorQueryTargetParameters
    {
        /// <summary>
        /// The name/value of the resource.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorQueryTarget"/> class.
        /// </summary>
        /// <param name="value">The value to use for the query target.</param>
        public SensorQueryTarget(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a new <see cref="SensorQueryTarget"/> from a specified value.
        /// </summary>
        /// <param name="value">The value to use for the query target.</param>
        public static implicit operator SensorQueryTarget(string value)
        {
            if (value == null)
                return null;

            return new SensorQueryTarget(value);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Value;
        }

        SensorMultiQueryTargetParameters ISensorQueryTargetParameters.ToMultiQueryParameters() => new SensorMultiQueryTargetParameters(this, null);
    }
}
