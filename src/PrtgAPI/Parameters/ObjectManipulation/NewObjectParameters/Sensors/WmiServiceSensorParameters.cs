using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Request;
using PrtgAPI.Targets;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for creating a new WMI Service sensor.
    /// </summary>
    public class WmiServiceSensorParameters : SensorParametersInternal
    {
        internal override string[] DefaultTags => new[] { "wmiservicesensor", "servicesensor" };

        /// <summary>
        /// Initializes a new instance of the <see cref="WmiServiceSensorParameters"/> class for creating a new set of parameters specifying a single service.
        /// </summary>
        /// <param name="service">The WMI Service to create a sensor for.</param>
        /// <param name="startStopped">Whether PRTG should automatically restart the service in the event it is stopped.</param>
        /// <param name="notifyChanged">Whether PRTG should trigger any <see cref="TriggerType.Change"/> notification triggers defined on the sensor in the event PRTG restarts it.</param>
        /// <param name="monitorPerformance">Whether to collect performance metrics for the service.</param>
        [ExcludeFromCodeCoverage]
        public WmiServiceSensorParameters(WmiServiceTarget service, bool startStopped = false, bool notifyChanged = true, bool monitorPerformance = false) :
            this(new List<WmiServiceTarget> {service}, startStopped, notifyChanged, monitorPerformance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WmiServiceSensorParameters"/> class for creating a new set of parameters specifying one or more services.
        /// </summary>
        /// <param name="services">One or more WMI Services to create sensors for.</param>
        /// <param name="startStopped">Whether PRTG should automatically restart the services in the event they are stopped.</param>
        /// <param name="notifyChanged">Whether PRTG should trigger any <see cref="TriggerType.Change"/> notification triggers defined on the sensors in the event PRTG restarts them.</param>
        /// <param name="monitorPerformance">Whether to collect performance metrics for each service.</param>
        public WmiServiceSensorParameters(IEnumerable<WmiServiceTarget> services, bool startStopped = false, bool notifyChanged = true, bool monitorPerformance = false) : base("Service", SensorType.WmiService)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            Services = services as List<WmiServiceTarget> ?? services.ToList();
            StartStopped = startStopped;
            NotifyChanged = notifyChanged;
            MonitorPerformance = monitorPerformance;

            SetCustomParameterBool(ObjectPropertyInternal.HasService, true);
        }

        /// <summary>
        /// Gets or sets a list of services to create sensors for.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(Parameter.Service)]
        public List<WmiServiceTarget> Services
        {
            get { return (List<WmiServiceTarget>) this[Parameter.Service]; }
            set { this[Parameter.Service] = value; }
        }

        /// <summary>
        /// Gets or sets whether PRTG should automatically restart the services in the event they are stopped.
        /// </summary>
        [PropertyParameter(ObjectProperty.StartStopped)]
        public bool StartStopped
        {
            get { return (bool) GetCustomParameterBool(ObjectProperty.StartStopped); }
            set { SetCustomParameterBool(ObjectProperty.StartStopped, value); }
        }

        /// <summary>
        /// Gets or sets whether PRTG should trigger any <see cref="TriggerType.Change"/> notification triggers defined on the sensors in the event PRTG restarts them.
        /// </summary>
        [PropertyParameter(ObjectProperty.NotifyChanged)]
        public bool NotifyChanged
        {
            get { return (bool) GetCustomParameterBool(ObjectProperty.NotifyChanged); }
            set { SetCustomParameterBool(ObjectProperty.NotifyChanged, value); }
        }

        /// <summary>
        /// Gets or sets whether to collect performance metrics for each service.
        /// </summary>
        [PropertyParameter(ObjectProperty.MonitorPerformance)]
        public bool MonitorPerformance
        {
            get { return (bool) GetCustomParameterBool(ObjectProperty.MonitorPerformance); }
            set { SetCustomParameterBool(ObjectProperty.MonitorPerformance, value); }
        }
    }
}
