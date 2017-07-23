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
        public static readonly TriggerChannel Primary = new TriggerChannel(GeneralTriggerChannel.Primary);

        /// <summary>
        /// The sensor's "Total" channel (where applicable).
        /// </summary>
        public static readonly TriggerChannel Total = new TriggerChannel(GeneralTriggerChannel.Total);

        /// <summary>
        /// The sensor's "Traffic In" channel (where applicable).
        /// </summary>
        public static readonly TriggerChannel TrafficIn = new TriggerChannel(GeneralTriggerChannel.TrafficIn);

        /// <summary>
        /// The sensor's "Traffic Out" channel (where applicable).
        /// </summary>
        public static readonly TriggerChannel TrafficOut = new TriggerChannel(GeneralTriggerChannel.TrafficOut);

        internal object channel;

        internal int? channelId;

        internal static TriggerChannel Parse(object channel, int? channelId)
        {
            if (channel is string)
            {
                GeneralTriggerChannel value;

                if (Enum.TryParse(channel.ToString(), true, out value))
                {
                    channel = value;
                }
            }

            if (channel is GeneralTriggerChannel)
            {
                switch ((GeneralTriggerChannel) channel)
                {
                    case GeneralTriggerChannel.Primary:
                        return Primary;
                    case GeneralTriggerChannel.Total:
                        return Total;
                    case GeneralTriggerChannel.TrafficIn:
                        return TrafficIn;
                    case GeneralTriggerChannel.TrafficOut:
                        return TrafficOut;
                    default:
                        throw new NotImplementedException($"Handler missing for channel '{channel}'");
                }
            }
            else
            {
                int value;

                if (int.TryParse(channel.ToString(), out value))
                    return new TriggerChannel(value);
                else
                {
                    return new TriggerChannel(channel.ToString(), channelId.Value);
                }
            }
        }

        internal static TriggerChannel ParseForRequest(object channel)
        {
            return ParseInternal<XmlEnumAttribute>(channel, null);
        }

        internal static TriggerChannel ParseFromResponse(object channel, int? channelId)
        {
            return ParseInternal<XmlEnumAlternateName>(channel, channelId);
        }

        private static TriggerChannel ParseInternal<T>(object channel, int? channelId) where T : XmlEnumAttribute
        {
            if (channel == null)
                return null;

            object @enum = null;

            if (!(channel is TriggerChannel && ((TriggerChannel)channel).channel is int))
                @enum = channel is int ? null : EnumHelpers.XmlToEnum<T>(channel.ToString(), typeof(GeneralTriggerChannel), false);

            if (@enum != null)
                return Parse(@enum, null);
            else
            {
                var result = Parse(channel, channelId);

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

        private TriggerChannel(GeneralTriggerChannel channel)
        {
            this.channel = channel;
        }

        private TriggerChannel(string channel, int channelId)
        {
            this.channel = channel;
            this.channelId = channelId;
        }

        /// <summary>
        /// Creates a new <see cref="TriggerChannel"/> object from a specified Channel.
        /// </summary>
        /// <param name="channel"></param>
        public static implicit operator TriggerChannel(Channel channel)
        {
            return new TriggerChannel(channel);
        }

        /// <summary>
        /// Creates a new <see cref="TriggerChannel"/> from a specified <see cref="GeneralTriggerChannel"/>.
        /// </summary>
        /// <param name="channel"></param>
        public static implicit operator TriggerChannel(GeneralTriggerChannel channel)
        {
            return Parse(channel, null);
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
            if (channel is Channel)
            {
                return ((Channel)channel).Id.ToString();
            }
            else if (channel is GeneralTriggerChannel)
            {
                return ((GeneralTriggerChannel) channel).EnumToXml();
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
                    return channelId.ToString();
                }
            }
        }
    }

    //XmlEnum is used when modifying triggers on the system
    //XmlEnumAlternateName is used when receiving incoming trigger details

    /// <summary>
    /// Specifies standard sensor channels a notification trigger can monitor.
    /// </summary>
    public enum GeneralTriggerChannel
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
