using System.Collections.Generic;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents a set of parameters that can be specified as the basis for determining the parameters required to create a new sensor.
    /// </summary>
    public class SensorQueryTargetParameters : Dictionary<string, object>, ISensorQueryTargetParameters
    {
        SensorMultiQueryTargetParameters ISensorQueryTargetParameters.ToMultiQueryParameters() => new SensorMultiQueryTargetParameters(null, this);
    }
}
