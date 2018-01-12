using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Undocumented;

namespace PrtgAPI
{
    /// <summary>
    /// Describes a system service on a Microsoft Windows system that can be monitored via WMI.
    /// </summary>
    public class WmiServiceTarget : SensorTarget<WmiServiceTarget>
    {
        /// <summary>
        /// The friendly display name of the service.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The description of the service.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Indicates whether the service is currently running or stopped.
        /// </summary>
        public ServiceControllerStatus Status { get; }

        private WmiServiceTarget(string raw) : base(raw)
        {
            Name = components[0];
            DisplayName = components[1];
            Description = components[2];
            Status = components[3].ToEnum<ServiceControllerStatus>();
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
            var services = ObjectSettings.GetInput(response)
                .Where(i => i.Type == Html.InputType.Checkbox && i.Name == "service__check")
                .Select(i => i.Value).Select(v => new WmiServiceTarget(v))
                .ToList();

            return services;
        }
    }
}
