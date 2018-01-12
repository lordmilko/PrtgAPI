using System.Collections.Generic;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/>.
    /// </summary>
    public class Parameters
    {
        internal bool Cookie { get; set; }

        private readonly Dictionary<Parameter, object> parameters = new Dictionary<Parameter, object>();

        /// <summary>
        /// Retrieve the underlying dictionary of parameters.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Parameter, object> GetParameters()
        {
            return parameters;
        }

        /// <summary>
        /// Add or update a <see cref="Parameter"/> for use in a <see cref="PrtgUrl"/>.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public object this[Parameter parameter]
        {
            get
            {
                return parameters.ContainsKey(parameter) ? parameters[parameter] : null;
            }
            set
            {
                if (parameters.ContainsKey(parameter))
                {
                    parameters[parameter] = value;
                }
                else
                {
                    //If this parameter is mutually exclusive with another parameter (such as password/passhash),
                    //if our parameter's counterpart has already been added, replace it
                    var attrib = parameter.GetEnumAttribute<MutuallyExclusiveAttribute>();

                    if (attrib != null)
                    {
                        var counterpart = attrib.Name.ToEnum<Parameter>();

                        if (parameters.ContainsKey(counterpart))
                            parameters.Remove(counterpart);
                    }

                    parameters.Add(parameter, value);
                }
            }
        }
    }
}
