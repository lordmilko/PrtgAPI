using System;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a channel that can cause a Notification Trigger to activate.
    /// </summary>
    public class TriggerChannel : IFormattable
    {
        /// <summary>
        /// The sensor's primary channel.
        /// </summary>
        public static readonly TriggerChannel Primary = new TriggerChannel(StandardTriggerChannel.Primary);

        /// <summary>
        /// The sensor's "Total" channel (where applicable).
        /// </summary>
        public static readonly TriggerChannel Total = new TriggerChannel(StandardTriggerChannel.Total);

        /// <summary>
        /// The sensor's "Traffic In" channel (where applicable).
        /// </summary>
        public static readonly TriggerChannel TrafficIn = new TriggerChannel(StandardTriggerChannel.TrafficIn);

        /// <summary>
        /// The sensor's "Traffic Out" channel (where applicable).
        /// </summary>
        public static readonly TriggerChannel TrafficOut = new TriggerChannel(StandardTriggerChannel.TrafficOut);

        internal object channel;

        /// <summary>
        /// Converts an object of one of several types to a <see cref="TriggerChannel"/>. If the specified value is not convertable to <see cref="TriggerChannel"/>, an <see cref="InvalidCastException"/> is thrown.
        /// </summary>
        /// <param name="channel">The value to parse.</param>
        /// <returns>A TriggerChannel that encapsulates the passed value.</returns>
        public static TriggerChannel Parse(object channel)
        {
            if (channel is TriggerChannel)
                return (TriggerChannel)channel;

            if (channel is string)
            {
                StandardTriggerChannel value;

                //Convert it to an enum, and then let the enum handler below convert it to a static member
                if (Enum.TryParse(channel.ToString(), true, out value))
                {
                    channel = value;
                }
            }
            
            if (channel is StandardTriggerChannel)
            {
                return ParseChannelEnum((StandardTriggerChannel) channel);
            }
            else if (channel is Channel)
            {
                return new TriggerChannel((Channel) channel);
            }
            else
            {
                int value;

                if (int.TryParse(channel.ToString(), out value))
                    return new TriggerChannel(value);
                else
                {
                    throw new InvalidCastException($"Cannot convert '{channel}' of type '{channel.GetType()}' to type '{nameof(TriggerChannel)}'. Value type must be convertable to one of PrtgAPI.StandardTriggerChannel, PrtgAPI.Channel or System.Int32.");
                }
            }
        }

        private static TriggerChannel ParseChannelEnum(StandardTriggerChannel channel)
        {
            switch (channel)
            {
                case StandardTriggerChannel.Primary:
                    return Primary;
                case StandardTriggerChannel.Total:
                    return Total;
                case StandardTriggerChannel.TrafficIn:
                    return TrafficIn;
                case StandardTriggerChannel.TrafficOut:
                    return TrafficOut;
                default:
                    throw new NotImplementedException($"Handler missing for channel '{channel}'");
            }
        }

        internal static TriggerChannel ParseForRequest(object channel)
        {
            return ParseInternal<XmlEnumAttribute>(channel);
        }

        internal static TriggerChannel ParseFromResponse(object channel)
        {
            return ParseInternal<XmlEnumAlternateName>(channel);
        }

        private static TriggerChannel ParseInternal<T>(object channel) where T : XmlEnumAttribute
        {
            if (channel == null)
                return null;

            object @enum = null;

            if (!(channel is TriggerChannel && ((TriggerChannel)channel).channel is int))
                @enum = channel is int ? null : EnumHelpers.XmlToEnum<T>(channel.ToString(), typeof(StandardTriggerChannel), false);

            if (@enum != null)
                return Parse(@enum);
            else
            {
                var result = Parse(channel);

                return result;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerChannel"/> from a Channel.
        /// </summary>
        /// <param name="channel"></param>
        public TriggerChannel(Channel channel)
        {
            this.channel = channel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerChannel"/> class from a Channel ID.
        /// </summary>
        public TriggerChannel(int channelId)
        {
            this.channel = channelId;
        }

        private TriggerChannel(StandardTriggerChannel channel)
        {
            this.channel = channel;
        }

        /// <summary>
        /// Creates a new <see cref="TriggerChannel"/> object from a specified Channel.
        /// </summary>
        /// <param name="channel">A custom channel that will apply to a trigger.</param>
        public static implicit operator TriggerChannel(Channel channel)
        {
            return new TriggerChannel(channel);
        }

        /// <summary>
        /// Creates a new <see cref="TriggerChannel"/> from a specified <see cref="StandardTriggerChannel"/>.
        /// </summary>
        /// <param name="channel">A standard channel that will apply to a trigger.</param>
        public static implicit operator TriggerChannel(StandardTriggerChannel channel)
        {
            return Parse(channel);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return channel.ToString();
        }

        string IFormattable.GetSerializedFormat()
        {
            //The serialized format of a TriggerChannel is an integer.

            if (channel is Channel)
            {
                return ((Channel)channel).Id.ToString();
            }
            else if (channel is StandardTriggerChannel)
            {
                return ((StandardTriggerChannel) channel).EnumToXml();
            }
            else
            {
                int value;

                if (int.TryParse(channel.ToString(), out value))
                {
                    return value.ToString();
                }
                else
                {
                    return channel.ToString();
                }
            }
        }
    }

    //XmlEnum is used when modifying triggers on the system
    //XmlEnumAlternateName is used when receiving incoming trigger details

    /// <summary>
    /// Specifies standard sensor channels a notification trigger can monitor.
    /// </summary>
    public enum StandardTriggerChannel
    {
        /// <summary>
        /// The primary channel.
        /// </summary>
        [XmlEnum("-999")]
        Primary,

        /// <summary>
        /// The "Total" channel (where applicable)
        /// </summary>
        [XmlEnum("-1")]
        Total,

        /// <summary>
        /// The "TrafficIn" channel (where applicable)
        /// </summary>
        [XmlEnum("0")]
        [XmlEnumAlternateName("Traffic In")]
        TrafficIn,

        /// <summary>
        /// The "TrafficOut" channel (where applicable)
        /// </summary>
        [XmlEnum("1")]
        [XmlEnumAlternateName("Traffic Out")]
        TrafficOut
    }
}
