namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents a set of multiple query target parameters that can be specified as the basis for determining the parameters required to create a new sensor.
    /// </summary>
    public class SensorMultiQueryTargetParameters : ISensorQueryTargetParameters
    {
        /// <summary>
        /// Gets or sets the query target to utilize.
        /// </summary>
        public SensorQueryTarget QueryTarget { get; set; }

        /// <summary>
        /// Gets or sets the query target parameters to utilize.
        /// </summary>
        public SensorQueryTargetParameters Parameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorMultiQueryTargetParameters"/> class.
        /// </summary>
        /// <param name="queryTarget">The query target to utilize.</param>
        /// <param name="parameters">The query target parameters to utilize.</param>
        public SensorMultiQueryTargetParameters(SensorQueryTarget queryTarget, SensorQueryTargetParameters parameters)
        {
            QueryTarget = queryTarget;
            Parameters = parameters;
        }

        SensorMultiQueryTargetParameters ISensorQueryTargetParameters.ToMultiQueryParameters() => this;
    }
}
