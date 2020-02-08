using System;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies properties that apply to Sensors, Devices, Groups and Probes.
    /// </summary>
    public interface ISensorOrDeviceOrGroupOrProbe : ISensorOrDeviceOrGroupOrProbeOrTicket
    {
        /// <summary>
        /// Monitoring schedule of this object. If this object is a report, this property displays the report generation schedule. If this object does not have a schedule, this value is null.
        /// </summary>
        string Schedule { get; }

        /// <summary>
        /// Base type of this object ("sensor", "device", etc)
        /// </summary>
        BaseType BaseType { get; }

        /// <summary>
        /// URL of this object.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Number of each notification trigger type defined on this object, as well as whether this object inherits any triggers from its parent object.<para/>
        /// This property does not work in non-English version of PRTG.
        /// </summary>
        NotificationTypes NotificationTypes { get; }

        /// <summary>
        /// Scanning interval for this sensor or default scanning interval for sensors under this object.
        /// </summary>
        TimeSpan Interval { get; }

        /// <summary>
        /// Whether this object's Interval is inherited from its parent object.
        /// </summary>
        bool InheritInterval { get; }

        /// <summary>
        /// An <see cref="Access"/> value specifying the access rights of the API Request User on the specified object.
        /// </summary>
        Access Access { get; }

        /// <summary>
        /// Name of the object the monitoring of this object is dependent on.
        /// </summary>
        string Dependency { get; }

        /// <summary>
        /// Position of this object within its parent object.
        /// </summary>
        int Position { get; }

        /// <summary>
        /// <see cref="PrtgAPI.Status"/> indicating this object's monitoring state.
        /// </summary>
        Status Status { get; }

        /// <summary>
        /// Comments present on this object.
        /// </summary>
        string Comments { get; }
    }
}
