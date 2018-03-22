using System.Collections.Generic;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/>.
    /// </summary>
    interface IParameters
    {
        bool Cookie { get; set; }

        object this[Parameter parameter] { get; set; }

        Dictionary<Parameter, object> GetParameters();
    }
}
