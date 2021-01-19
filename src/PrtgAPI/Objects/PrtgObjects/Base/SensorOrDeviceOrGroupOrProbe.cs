using System;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Base class for Sensors, Devices, Groups and Probes, containing properties that apply to all four object types.</para>
    /// </summary>
    public class SensorOrDeviceOrGroupOrProbe : SensorOrDeviceOrGroupOrProbeOrTicket, ISensorOrDeviceOrGroupOrProbe
    {
        // ################################## Sensors, Devices, Groups, Probes, Reports ##################################
        // There is a copy in both SensorOrDeviceOrGroupOrProbe and Report

        private string schedule;

        /// <summary>
        /// Monitoring schedule of this object. If this object is a report, this property displays the report generation schedule. If this object does not have a schedule, this value is null.
        /// </summary>
        [XmlElement("schedule")]
        [PropertyParameter(Property.Schedule)]
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
        [PropertyParameter(Property.BaseType)]
        public BaseType BaseType
        {
            get { return baseType.Value; }
            set { baseType = value; }
        }

        /// <summary>
        /// URL of this object.
        /// </summary>
        [XmlElement("baselink")]
        [PropertyParameter(Property.Url)]
        public string Url { get; set; }

        // ################################## Sensors, Devices, Groups, Probes ##################################

        /// <summary>
        /// Number of each notification trigger type defined on this object, as well as whether this object inherits any triggers from its parent object.<para/>
        /// This property does not work in non-English version of PRTG.
        /// </summary>
        [PropertyParameter(Property.NotificationTypes)]
        public NotificationTypes NotificationTypes => notificationTypes == null ? new NotificationTypes(string.Empty) : new NotificationTypes(notificationTypes); //todo: add custom handling for this

        [XmlElement("notifiesx")]
        internal string notificationTypes { get; set; }

        #region Interval

        private string displayInterval;
        private TimeSpan? inheritedOrSetInterval;

        [XmlElement("intervalx_raw")]
        internal TimeSpan ObjectInterval { get; set; }

        /// <summary>
        /// Scanning interval for this sensor or default scanning interval for sensors under this object.
        /// </summary>
        [PropertyParameter(Property.Interval)]
        public TimeSpan Interval
        {
            get { return inheritedOrSetInterval ?? ObjectInterval; }
            set { inheritedOrSetInterval = value; }
        }

        [XmlElement("intervalx")]
        internal string inheritInterval
        {
            get { return displayInterval; }
            set
            {
                displayInterval = value;

                if (value != null)
                {
                    var start = value.IndexOf("(");

                    if (start != -1)
                    {
                        var end = value.IndexOf(")");

                        var str = value.Substring(start + 1, end - start - 1);

                        inheritedOrSetInterval = TimeSpan.FromSeconds(Convert.ToInt32(str));
                    }
                }
            }
        }

        /// <summary>
        /// Whether this object's Interval is inherited from its parent object.
        /// </summary>
        public bool InheritInterval => inheritInterval?.Contains("(") ?? false;

        #endregion

        /// <summary>
        /// An <see cref="Access"/> value specifying the access rights of the API Request User on the specified object.
        /// </summary>
        [XmlElement("access_raw")]
        [PropertyParameter(Property.Access)]
        public Access Access { get; set; }

        /// <summary>
        /// Name of the object the monitoring of this object is dependent on.
        /// </summary>
        [XmlElement("dependency_raw")]
        [PropertyParameter(Property.Dependency)]
        public string Dependency { get; set; }

        /// <summary>
        /// Position of this object within its parent object.
        /// </summary>
        [XmlElement("position")]
        [PropertyParameter(Property.Position)]
        public int Position { get; set; }

        /// <summary>
        /// <see cref="PrtgAPI.Status"/> indicating this object's monitoring state.
        /// </summary>
        [XmlElement("status_raw")]
        [PropertyParameter(Property.Status)]
        public Status Status { get; set; }

        private string comments;

        /// <summary>
        /// Comments present on this object.
        /// </summary>
        [XmlElement("comments")]
        [PropertyParameter(Property.Comments)]
        public string Comments
        {
            get { return comments; }
            set { comments = value?.Trim(); }
        }
    }
}
