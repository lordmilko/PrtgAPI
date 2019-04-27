using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace PrtgAPI
{
    class ChannelHistoryRecordRaw
    {
        [XmlAttribute("channelid")]
        public int ChannelId { get; set; }

        [XmlText]
        public double Value { get; set; }
    }

    /// <summary>
    /// Represents the value of a channel at a given date and time.
    /// </summary>
    public class ChannelHistoryRecord : IEventObject
    {
        /// <summary>
        /// The date and time to which this object's historical values apply.
        /// </summary>
        public DateTime DateTime { get; set; }

        [ExcludeFromCodeCoverage]
        int IEventObject.ObjectId => SensorId;

        /// <summary>
        /// The ID of the sensor to which this history record applies.
        /// </summary>
        public int SensorId { get; set; }

        /// <summary>
        /// The name of the channel.
        /// </summary>
        [XmlAttribute("channel")]
        public string Name { get; set; }

        /// <summary>
        /// The ID of the channel.
        /// </summary>
        [XmlAttribute("channelid")]
        public int ChannelId { get; set; }

        /// <summary>
        /// Average display value of the channel during the specified time period. This value can change based on the time span to use for the averaging interval.
        /// </summary>
        [XmlText]
        public string DisplayValue { get; set; }

        /// <summary>
        /// Average value of the channel during the specified time period. If no data was collected during the specified time period
        /// (such as if the sensor was in error) this value will be null.<para/>
        /// If this channel's unit can be expressed in one of several multiples, the lowest possible multiple will be used (e.g. megabytes -> bytes).<para/>
        /// This value can change based on the time span to use for the averaging interval.
        /// </summary>
        [XmlIgnore]
        public double? Value { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Name}: {DisplayValue}";
        }
    }
}