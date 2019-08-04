using System.Collections.Generic;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/>.
    /// </summary>
    public interface IParameters
    {
        /// <summary>
        /// Specifies whether to use a stored Cookie for authentication requests. If this value is false,
        /// authentication will be performed with a username and password/passhash.
        /// </summary>
        bool Cookie { get; }

        /// <summary>
        /// Gets or sets a <see cref="Parameter"/> for use in a <see cref="PrtgRequestMessage"/>.
        /// </summary>
        /// <param name="parameter">The parameter to retrieve or modify.</param>
        /// <returns>The value of the specified parameter. If the parameter does not exist, null.</returns>
        object this[Parameter parameter] { get; set; }

        /// <summary>
        /// Retrieves the underlying dictionary of parameters stored in this object.
        /// </summary>
        /// <returns>The underlying dictionary of parameters.</returns>
        IDictionary<Parameter, object> GetParameters();
    }
}
