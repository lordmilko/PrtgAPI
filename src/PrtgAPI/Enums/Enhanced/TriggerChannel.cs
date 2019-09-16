using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a channel that can cause a Notification Trigger to activate.
    /// </summary>
    public class TriggerChannel : ISerializable, IEquatable<TriggerChannel>, IEnumEx
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
        /// <param name="value">The value to parse.</param>
        /// <returns>A <see cref="TriggerChannel"/> that encapsulates the passed value.</returns>
        public static TriggerChannel Parse(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            TriggerChannel channel;

            if (!TryParse(value, out channel))
                throw new ArgumentException($"Cannot convert value '{value}' of type '{value.GetType()}' to type '{nameof(TriggerChannel)}'. Value type must be convertable to one of {typeof(StandardTriggerChannel).FullName}, {typeof(Channel).FullName} or {typeof(int).FullName}.", nameof(value));

            return channel;
        }

        /// <summary>
        /// Converts an object of one of several types to a <see cref="TriggerChannel"/>. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">When this method returns, if the <paramref name="value"/> was successfully converted to a <see cref="TriggerChannel"/>, this parameter
        /// contains the result of that conversion. If the conversion was unsuccessful, this parameter will be set to null. </param>
        /// <returns>True if the value was successfully parsed. Otherwise, false.</returns>
        public static bool TryParse(object value, out TriggerChannel result)
        {
            if (value == null)
            {
                result = null;
                return false;
            }

            if (value is TriggerChannel)
            {
                result = (TriggerChannel)value;
                return true;
            }

            if (value is string)
            {
                //Convert it to an enum, and then let the enum handler below convert it to a static member
                var val = EnumExtensions.XmlToEnum<XmlEnumAttribute>(value.ToString(), typeof(StandardTriggerChannel), false);

                if (val != null)
                    value = val;
            }

            if (value is StandardTriggerChannel)
            {
                result = ParseChannelEnum((StandardTriggerChannel)value);
                return true;
            }

            if (value is Channel)
            {
                result = new TriggerChannel((Channel)value);
                return true;
            }

            int intValue;

            if (int.TryParse(value.ToString(), out intValue))
            {
                result = new TriggerChannel(intValue);
                return true;
            }

            result = null;
            return false;
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
                    throw new NotImplementedException($"Handler missing for channel '{channel}'.");
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

            if (ChannelMaybeEnum(channel))
                @enum = channel is int ? null : EnumExtensions.XmlToEnum<T>(channel.ToString(), typeof(StandardTriggerChannel), false);

            if (@enum != null)
                return Parse(@enum);
            else
            {
                var result = Parse(channel);

                return result;
            }
        }

        private static bool ChannelMaybeEnum(object channel)
        {
            if (channel is Channel)
                return false;

            var triggerChannel = channel as TriggerChannel;

            if (triggerChannel != null)
            {
                if (triggerChannel.channel is int || triggerChannel.channel is Channel)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerChannel"/> from a Channel.
        /// </summary>
        /// <param name="channel"></param>
        public TriggerChannel(Channel channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            this.channel = channel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerChannel"/> class from a Channel ID.
        /// </summary>
        public TriggerChannel(int channelId)
        {
            channel = channelId;
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

        string ISerializable.GetSerializedFormat()
        {
            //The serialized format of a TriggerChannel is an integer.

            if (channel is Channel)
                return ((Channel)channel).Id.ToString();

            if (channel is StandardTriggerChannel)
                return ((StandardTriggerChannel) channel).EnumToXml();

            int value;

            if (int.TryParse(channel.ToString(), out value))
                return value.ToString();

            return channel.ToString();
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects target the same channel.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        [ExcludeFromCodeCoverage]
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            TriggerChannel c;

            if (TryParse(other, out c))
                return IsEqual(c);

            return false;
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object obj is
        /// Equal to this. The specified object is equal to this if both
        /// objects target the same channel.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        [ExcludeFromCodeCoverage]
        public bool Equals(TriggerChannel other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return IsEqual(other);
        }

        private bool IsEqual(TriggerChannel other)
        {
            return ((ISerializable)this).GetSerializedFormat() == ((Request.ISerializable)other).GetSerializedFormat();
        }

        /// <summary>
        /// Returns a hash code for this object. If two trigger channels target
        /// the same channel, they will have the same hash code.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;

                result = (result * 419) ^ Convert.ToInt32(((ISerializable)this).GetSerializedFormat());

                return result;
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
