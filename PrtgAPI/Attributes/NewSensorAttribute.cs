using System;
using PrtgAPI.Parameters;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies configuration options used when parsing <see cref="NewSensorParameters"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal class NewSensorAttribute : Attribute
    {
        /// <summary>
        /// Indicates the name of the parameters object should be ignored as it will be dynamically generated.
        /// </summary>
        public bool DynamicName { get; set; }

        /// <summary>
        /// Indicates that all properties of a parameters object are optional, and that a sensor can successfully be created
        /// with each parameter's default values.
        /// </summary>
        public bool ConfigOptional { get; set; }
    }
}
