namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents a set of parameters used to query the parameters required to create a new sensor.
    /// </summary>
    public interface ISensorQueryTargetParameters
    {
        /// <summary>
        /// Creates a set of query target parameters capable of handling multiple PRTG queries.
        /// </summary>
        /// <returns>A set of query target parameters capable of handling multiple PRTG queries.</returns>
        SensorMultiQueryTargetParameters ToMultiQueryParameters();
    }
}
