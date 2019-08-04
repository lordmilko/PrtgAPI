using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Utilities;

namespace PrtgAPI.Targets
{
    /// <summary>
    /// Describes a system service on a Microsoft Windows server that can be monitored via WMI.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class WmiServiceTarget : SensorTarget<WmiServiceTarget>
    {
        /// <summary>
        /// Gets the friendly display name of the service.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the description of the service.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Indicates whether the service is currently running or stopped.
        /// </summary>
        public WmiServiceStatus Status { get; }

        private WmiServiceTarget(string raw) : base(raw)
        {
            DisplayName = components[1];
            Description = components[2];
            Status = components[3].XmlToEnum<WmiServiceStatus>();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return DisplayName;
        }

        internal static List<WmiServiceTarget> GetServices(string response)
        {
            return CreateFromCheckbox(response, Parameter.Service, v => new WmiServiceTarget(v));
        }
    }
}
