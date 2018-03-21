using System;
using System.Collections.Generic;
using PrtgAPI.Attributes;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for creating a new WMI Service sensor.
    /// </summary>
    public class WmiServiceSensorParameters : SensorParametersInternal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WmiServiceSensorParameters"/> class for creating a new set of parameters specifying a single service.
        /// </summary>
        /// <param name="service">The WMI Service to create a sensor for.</param>
        /// <param name="startStopped">Whether PRTG should automatically restart the service in the event it is stopped.</param>
        /// <param name="notifyStarted">Whether PRTG should trigger any <see cref="TriggerType.Change"/> notification triggers defined on the sensor in the event PRTG restarts it.</param>
        /// <param name="monitorPerformance">Whether to collect performance metrics for the service.</param>
        /// <param name="tags">Tags that should be applied to this sensor. If this value is null or no tags are specified, default values are "wmiservicesensor" and "servicesensor".</param>
        public WmiServiceSensorParameters(WmiServiceTarget service, bool startStopped = false, bool notifyStarted = true, bool monitorPerformance = false, params string[] tags) :
            this(new List<WmiServiceTarget>() {service}, startStopped, notifyStarted, monitorPerformance, tags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WmiServiceSensorParameters"/> class for creating a new set of parameters specifying one or more services.
        /// </summary>
        /// <param name="services">One or more WMI Services to create sensors for.</param>
        /// <param name="startStopped">Whether PRTG should automatically restart the services in the event they are stopped.</param>
        /// <param name="notifyStarted">Whether PRTG should trigger any <see cref="TriggerType.Change"/> notification triggers defined on the sensors in the event PRTG restarts them.</param>
        /// <param name="monitorPerformance">Whether to collect performance metrics for each service.</param>
        /// <param name="tags">Tags that should be applied to this sensor. If this value is null or no tags are specified, default values are "wmiservicesensor" and "servicesensor".</param>
        public WmiServiceSensorParameters(List<WmiServiceTarget> services, bool startStopped = false, bool notifyStarted = true, bool monitorPerformance = false,
            params string[] tags) : base("Service", SensorType.WmiService)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (tags == null || tags.Length == 0)
                Tags = new[] { "wmiservicesensor", "servicesensor" };

            Services = services;
            StartStopped = startStopped;
            NotifyStarted = notifyStarted;
            MonitorPerformance = monitorPerformance;

            SetCustomParameterBool("service_", true);
        }

        /// <summary>
        /// A list of services to create sensors for.
        /// </summary>
        [RequireValue(true)]
        public List<WmiServiceTarget> Services
        {
            get { return (List<WmiServiceTarget>) this[Parameter.Service]; }
            set { this[Parameter.Service] = value; }
        }

        /// <summary>
        /// Whether PRTG should automatically restart the services in the event they are stopped.
        /// </summary>
        public bool StartStopped
        {
            get { return (bool) GetCustomParameterBool(ObjectProperty.StartStopped); }
            set { SetCustomParameterBool(ObjectProperty.StartStopped, value); }
        }

        /// <summary>
        /// Whether PRTG should trigger any <see cref="TriggerType.Change"/> notification triggers defined on the sensors in the event PRTG restarts them.
        /// </summary>
        public bool NotifyStarted
        {
            get { return (bool)GetCustomParameterBool(ObjectProperty.NotifyStarted); }
            set { SetCustomParameterBool(ObjectProperty.NotifyStarted, value); }
        }

        /// <summary>
        /// Whether to collect performance metrics for each service.
        /// </summary>
        public bool MonitorPerformance
        {
            get { return (bool) GetCustomParameterBool(ObjectProperty.MonitorPerformance); }
            set { SetCustomParameterBool(ObjectProperty.MonitorPerformance, value); }
        }
    }
}
