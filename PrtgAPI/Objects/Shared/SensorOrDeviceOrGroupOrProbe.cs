using System;
using System.Management.Automation;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using DH = PrtgAPI.Objects.Deserialization.DeserializationHelpers;

namespace PrtgAPI.Objects.Shared
{
    /// <summary>
    /// Properties that apply to Sensors, Devices, Groups and Probes.
    /// </summary>
    public class SensorOrDeviceOrGroupOrProbe : SensorOrDeviceOrGroupOrProbeOrMessageOrTicket
    {
        // ################################## Sensors, Devices, Groups, Probes, Reports ##################################
        // There is a copy in both SensorOrDeviceOrGroupOrProbe and Report

        private string schedule;

        /// <summary>
        /// Monitoring schedule of this object. If this object is a report, this property displays the report generation schedule. If this object does not have a schedule, this value is null.
        /// </summary>
        [XmlElement("schedule")]
        [PropertyParameter(nameof(Property.Schedule))]
        public string Schedule
        {
            get { return schedule; }
            set { schedule = value == string.Empty ? null : value; }
        }

        // ################################## All Tree Objects ##################################

        /// <summary>
        /// Base type of this object ("sensor", "device", etc)
        /// </summary>
        [XmlElement("basetype")]
        [PropertyParameter(nameof(Property.BaseType))]
        public BaseType? BaseType { get; set; }

        /// <summary>
        /// URL of this object.
        /// </summary>
        [XmlElement("baselink")]
        [PropertyParameter(nameof(Property.BaseLink))]
        public string Url { get; set; }

        /// <summary>
        /// ID of this object's parent.
        /// </summary>
        [XmlElement("parentid")]
        [PropertyParameter(nameof(Property.ParentId))]
        public int? ParentId { get; set; }

        // ################################## Sensors, Devices, Groups, Probes ##################################

        /// <summary>
        /// Number of each notification trigger type defined on this object, as well as whether this object inherits any triggers from its parent object.
        /// </summary>
        [PropertyParameter(nameof(Property.NotifiesX))]
        public NotificationTypes NotificationTypes => _RawNotificationTypes == null ? null : new NotificationTypes(_RawNotificationTypes);

        /// <summary>
        /// Raw value used for <see cref="_RawNotificationTypes"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("notifiesx")]
        public string _RawNotificationTypes { get; set; }

        /// <summary>
        /// Scanning interval for this sensor.
        /// </summary>
        [PropertyParameter(nameof(Property.Interval))]
        public TimeSpan? Interval
        {
            get
            {
                //Certain objects (like devices) do not report the intervals that have been defined when interval inheritance has been disabled.
                //As a workaround, when we can extract the value from their intervalx attributes instead.
                //If this statement is true, we've confirmed we need to make a last ditch effort to return a value.
                //Usually however, this expression will return false.
                if (_RawInterval == null && IntervalInherited == false) //If IntervalInherited is false, _RawIntervalInherited should just contain a number.
                {
                    return DH.ConvertPrtgTimeSpan(Convert.ToDouble(_RawIntervalInherited));
                }

                return DH.ConvertPrtgTimeSpan(_RawInterval);
            }
        }

        /// <summary>
        /// Raw value used for <see cref="Interval"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("interval_raw")]
        public double? _RawInterval { get; set; }

        /// <summary>
        /// Whether this object's Interval is inherited from its parent object.
        /// </summary>
        [PropertyParameter(nameof(Property.IntervalX))]
        public bool? IntervalInherited => _RawIntervalInherited?.Contains("Inherited");

        /// <summary>
        /// Raw value used for <see cref="IntervalInherited"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("intervalx")]
        public string _RawIntervalInherited { get; set; }

        /// <summary>
        /// An <see cref="Access"/> value specifying the access rights of the API Request User on the specified object.
        /// </summary>
        [XmlElement("access")]
        [PropertyParameter(nameof(Property.Access))]
        public Access? Access { get; set; }

        /// <summary>
        /// Name of the object the monitoring of this object is dependent on. If dependency is on the parent object, value of DependencyName will be "Parent".
        /// </summary>
        [XmlElement("dependency")]
        [PropertyParameter(nameof(Property.Dependency))]
        public string Dependency { get; set; }

        /// <summary>
        /// Whether this object has been marked as a favorite.
        /// </summary>
        [XmlElement("favorite_raw")]
        [PropertyParameter(nameof(Property.Favorite))]
        public bool? Favorite { get; set; }
    }
}
