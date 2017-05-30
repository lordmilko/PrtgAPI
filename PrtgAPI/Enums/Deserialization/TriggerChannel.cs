using System;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

namespace PrtgAPI
{
    public class TriggerChannel : IFormattable
    {
        public static readonly TriggerChannel Primary = new TriggerChannel(GeneralTriggerChannel.Primary);

        public static readonly TriggerChannel Total = new TriggerChannel(GeneralTriggerChannel.Total);

        public static readonly TriggerChannel TrafficIn = new TriggerChannel(GeneralTriggerChannel.TrafficIn);

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

            var @enum = channel is int ? null : EnumHelpers.XmlToEnum<T>(channel.ToString(), typeof(GeneralTriggerChannel), false);

            if (@enum != null)
                return Parse(@enum, null);
            else
            {
                var result = Parse(channel, channelId);

                return result;
            }
                
        }

        public TriggerChannel(Channel channel)
        {
            this.channel = channel;
        }

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

        public static implicit operator TriggerChannel(Channel channel)
        {
            return new TriggerChannel(channel);
        }

        public static implicit operator TriggerChannel(GeneralTriggerChannel channel)
        {
            return Parse(channel, null);
        }

        public override string ToString()
        {
            return channel.ToString();
        }

        public string GetSerializedFormat()
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
    /// Specifies the sensor channels a notification trigger can monitor.
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
