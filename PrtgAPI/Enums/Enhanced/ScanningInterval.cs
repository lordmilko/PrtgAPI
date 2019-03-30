using System;
using System.Xml.Serialization;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a well known or custom TimeSpan used for the Scanning Interval of an object.
    /// </summary>
    public class ScanningInterval : ISerializable, IEquatable<ScanningInterval>, IEnumEx
    {
        /// <summary>
        /// Scan every 30 seconds.
        /// </summary>
        public static readonly ScanningInterval ThirtySeconds = new ScanningInterval(StandardScanningInterval.ThirtySeconds);

        /// <summary>
        /// Scan every 60 seconds.
        /// </summary>
        public static readonly ScanningInterval SixtySeconds = new ScanningInterval(StandardScanningInterval.SixtySeconds);

        /// <summary>
        /// Scan every 5 minutes.
        /// </summary>
        public static readonly ScanningInterval FiveMinutes = new ScanningInterval(StandardScanningInterval.FiveMinutes);

        /// <summary>
        /// Scan every 10 minutes.
        /// </summary>
        public static readonly ScanningInterval TenMinutes = new ScanningInterval(StandardScanningInterval.TenMinutes);

        /// <summary>
        /// Scan every 15 minutes.
        /// </summary>
        public static readonly ScanningInterval FifteenMinutes = new ScanningInterval(StandardScanningInterval.FifteenMinutes);

        /// <summary>
        /// Scan every 30 minutes
        /// </summary>
        public static readonly ScanningInterval ThirtyMinutes = new ScanningInterval(StandardScanningInterval.ThirtyMinutes);

        /// <summary>
        /// Scan every 1 hour.
        /// </summary>
        public static readonly ScanningInterval OneHour = new ScanningInterval(StandardScanningInterval.OneHour);

        /// <summary>
        /// Scan every 4 hours.
        /// </summary>
        public static readonly ScanningInterval FourHours = new ScanningInterval(StandardScanningInterval.FourHours);

        /// <summary>
        /// Scan every 6 hours.
        /// </summary>
        public static readonly ScanningInterval SixHours = new ScanningInterval(StandardScanningInterval.SixHours);

        /// <summary>
        /// Scan every 12 hours.
        /// </summary>
        public static readonly ScanningInterval TwelveHours = new ScanningInterval(StandardScanningInterval.TwelveHours);

        /// <summary>
        /// Scan every 24 hours.
        /// </summary>
        public static readonly ScanningInterval TwentyFourHours = new ScanningInterval(StandardScanningInterval.TwentyFourHours);

        private readonly object interval;

        /// <summary>
        /// Converts an object of one of several types to a <see cref="ScanningInterval"/>. If the specified value is not convertable to <see cref="ScanningInterval"/>, an <see cref="InvalidCastException"/> is thrown.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>A <see cref="ScanningInterval"/> that encapsulates the passed value.</returns>
        public static ScanningInterval Parse(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            ScanningInterval interval;

            if (!TryParse(value, out interval))
                throw new ArgumentException($"Cannot convert value '{value}' of type '{value.GetType()}' to type '{nameof(ScanningInterval)}'. Value type must be convertable to one of {typeof(StandardScanningInterval).FullName}, {typeof(TimeSpan).FullName} or {typeof(int).FullName}.", nameof(value));

            return interval;
        }

        /// <summary>
        /// Converts an object of one of several types to a <see cref="ScanningInterval"/>. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">When this method returns, if the <paramref name="value"/> was successfully converted to a <see cref="ScanningInterval"/>, this parameter
        /// contains the result of that conversion. If the conversion was unsuccessful, this parameter will be set to null. </param>
        /// <returns>True if the value was successfully parsed. Otherwise, false.</returns>
        public static bool TryParse(object value, out ScanningInterval result)
        {
            if (value == null)
            {
                result = null;
                return false;
            }

            if (value is ScanningInterval)
            {
                result = (ScanningInterval)value;
                return true;
            }

            if (value is string)
            {
                StandardScanningInterval @enum;
                double doubleVal;

                //Try and convert the string to one of the types that can be used as the source of a ScanningInterval

                if (double.TryParse(value.ToString(), out doubleVal) && doubleVal == (int)doubleVal) //Is it a number?
                {
                    value = (int)doubleVal;
                }
                else if (Enum.TryParse(value.ToString(), true, out @enum)) //Is it a Standard Interval?
                {
                    value = @enum;
                }
                else
                {
                    //Is it the serialized form of a Standard Interval?
                    var val = EnumExtensions.XmlToEnum<XmlEnumAttribute>(value.ToString(), typeof(StandardScanningInterval), false);

                    if (val == null) //Must be a TimeSpan then!
                    {
                        TimeSpan stringTimeSpan;

                        if (TimeSpan.TryParse(value.ToString(), out stringTimeSpan)) //Is it a TimeSpan?
                        {
                            result = stringTimeSpan;
                            return true;
                        }

                        TimeSpan serializedTimeSpan;

                        if (TryStringToTimeSpan(value.ToString(), out serializedTimeSpan)) //Is it a serialized Scanning Interval?
                        {
                            result = new ScanningInterval(serializedTimeSpan);
                            return true;
                        }

                        result = null;
                        return false;
                    }

                    value = val;
                }
            }

            //We've tried converting the value to something coherent, now lets see how we did
            //and whether we wound up with any of the known source types.

            if (value is StandardScanningInterval)
            {
                result = ParseIntervalEnum((StandardScanningInterval)value);
                return true;
            }
            if (value is TimeSpan)
            {
                result = new ScanningInterval((TimeSpan)value);
                return true;
            }
            if (value is int)
            {
                result = new TimeSpan(0, 0, Convert.ToInt32(value));
                return true;
            }

            //If the value was a string containing a TimeSpan or even a TimeSpan itself,
            //we would have caught it by now. We haven't matched anything however, and
            //we're getting pretty desperate, so let's just convert it to a string, try
            //and parse it and see where that gets us.
            TimeSpan timeSpan;

            if (TimeSpan.TryParse(value.ToString(), out timeSpan))
            {
                result = new ScanningInterval(timeSpan);
                return true;
            }

            result = null;
            return false;
        }

        private static ScanningInterval ParseIntervalEnum(StandardScanningInterval interval)
        {
            switch (interval)
            {
                case StandardScanningInterval.ThirtySeconds:
                    return ThirtySeconds;
                case StandardScanningInterval.SixtySeconds:
                    return SixtySeconds;
                case StandardScanningInterval.FiveMinutes:
                    return FiveMinutes;
                case StandardScanningInterval.TenMinutes:
                    return TenMinutes;
                case StandardScanningInterval.FifteenMinutes:
                    return FifteenMinutes;
                case StandardScanningInterval.ThirtyMinutes:
                    return ThirtyMinutes;
                case StandardScanningInterval.OneHour:
                    return OneHour;
                case StandardScanningInterval.FourHours:
                    return FourHours;
                case StandardScanningInterval.SixHours:
                    return SixHours;
                case StandardScanningInterval.TwelveHours:
                    return TwelveHours;
                case StandardScanningInterval.TwentyFourHours:
                    return TwentyFourHours;
                default:
                    throw new NotImplementedException($"Handler missing for interval '{interval}'.");
            }
        }

        private ScanningInterval(StandardScanningInterval interval)
        {
            this.interval = interval;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanningInterval"/> class from a TimeSpan.
        /// </summary>
        /// <param name="interval"></param>
        public ScanningInterval(TimeSpan interval)
        {
            this.interval = interval;
        }

        /// <summary>
        /// Creates a new <see cref="ScanningInterval"/> from a specified <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="interval">A custom TimeSpan that will be used for the interval.<para/>
        /// If the PRTG Server has not been configured to recognize the specified TimeSpan, it will be rounded to the nearest valid value.</param>
        public static implicit operator ScanningInterval(TimeSpan interval)
        {
            return Parse(interval);
        }

        /// <summary>
        /// Creates a new <see cref="ScanningInterval"/> from a well known Scanning Interval.
        /// </summary>
        /// <param name="interval">A standard TimeSpan that will be used for the scanning interval.</param>
        public static implicit operator ScanningInterval(StandardScanningInterval interval)
        {
            return Parse(interval);
        }

        private TimeSpan? timeSpan;

        /// <summary>
        /// Gets the <see cref="System.TimeSpan"/> representation of the underlying interval.
        /// </summary>
        public TimeSpan TimeSpan
        {
            get
            {
                if (timeSpan == null)
                {
                    if (interval is TimeSpan)
                        timeSpan = (TimeSpan)interval;
                    else
                    {
                        string str;

                        if (interval is StandardScanningInterval)
                            str = ((StandardScanningInterval)interval).EnumToXml();
                        else
                            str = interval.ToString();

                        timeSpan = StringToTimeSpan(str);
                    }
                }

                return timeSpan.Value;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return TimeSpan.ToString();
        }

        string ISerializable.GetSerializedFormat()
        {
            if (interval is StandardScanningInterval)
            {
                return ((Enum) interval).GetEnumAttribute<XmlEnumAttribute>(true).Name;
            }

            return BuildSerializedFormat();
        }

        string BuildSerializedFormat()
        {
            var time = TimeSpan;

            var str = string.Empty;

            if (time.TotalDays > 1)
                str = $"{time.TotalDays} days";
            else if (time.TotalHours > 1)
                str = $"{time.TotalHours} hours";
            else if (time.TotalHours == 1)
                str = $"{time.TotalHours} hour";
            else if (time.TotalMinutes > 1)
                str = $"{time.TotalMinutes} minutes";
            else if (time.TotalSeconds >= 10)
                str = $"{time.TotalSeconds} seconds";
            else if (time.TotalSeconds > 1)
                str = $"{time.TotalSeconds} seconds (Not officially supported)";
            else if (time.TotalSeconds == 1)
                str = $"{time.TotalSeconds} second (Not officially supported)";
            else
                throw new NotImplementedException($"Not sure how to handle TimeSpan {time}.");

            return $"{time.TotalSeconds}|{str}";
        }

        private static bool TryStringToTimeSpan(string str, out TimeSpan result)
        {
            if (!str.Contains("|"))
            {
                result = default(TimeSpan);
                return false;
            }

            var secs = str.Substring(0, str.IndexOf('|'));

            var secsInt = Convert.ToInt32(secs);

            result = new TimeSpan(0, 0, secsInt);

            return true;
        }

        private static TimeSpan StringToTimeSpan(string str)
        {
            TimeSpan result;

            if (!str.Contains("|") || !TryStringToTimeSpan(str, out result))
                throw new FormatException($"Interval '{str}' is not correctly formatted.");

            return result;
        }

        /// <summary>
        /// Returns a boolean indicating if the passed in object <paramref name="other"/> is
        /// equal to this object. The specified object is equal to this if both
        /// objects are of the same type and have the same <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            ScanningInterval i;

            if (TryParse(other, out i))
                return Equals(i);

            return false;
        }

        /// <summary>
        /// Returns a hash code for this object. If two objects are both scanning intervals and
        /// have the same <see cref="TimeSpan"/>, they will have the same hash code.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;
                result = (result * 397) ^ TimeSpan.GetHashCode();

                return result;
            }
        }

        /// <summary>
        /// Returns a boolean indicating if the specified <see cref="ScanningInterval"/> is
        /// equal to this object. The specified object is equal to this if both values have
        /// the same <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(ScanningInterval other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return TimeSpan.Equals(other.TimeSpan);
        }
    }

    /// <summary>
    /// Specifies the interval at which a sensor initiates a scan to refresh its data.
    /// </summary>
    public enum StandardScanningInterval
    {
        /// <summary>
        /// Scan every 30 seconds.
        /// </summary>
        [XmlEnum("30|30 seconds")]
        ThirtySeconds,

        /// <summary>
        /// Scan every 60 seconds.
        /// </summary>
        [XmlEnum("60|60 seconds")]
        SixtySeconds,

        /// <summary>
        /// Scan every 5 minutes.
        /// </summary>
        [XmlEnum("300|5 minutes")]
        FiveMinutes,

        /// <summary>
        /// Scan every 10 minutes.
        /// </summary>
        [XmlEnum("600|10 minutes")]
        TenMinutes,

        /// <summary>
        /// Scan every 15 minutes.
        /// </summary>
        [XmlEnum("900|15 minutes")]
        FifteenMinutes,

        /// <summary>
        /// Scan every 30 minutes.
        /// </summary>
        [XmlEnum("1800|30 minutes")]
        ThirtyMinutes,

        /// <summary>
        /// Scan every 1 hour.
        /// </summary>
        [XmlEnum("3600|1 hour")]
        OneHour,

        /// <summary>
        /// Scan every 4 hours.
        /// </summary>
        [XmlEnum("14400|4 hours")]
        FourHours,

        /// <summary>
        /// Scan every 6 hours.
        /// </summary>
        [XmlEnum("21600|6 hours")]
        SixHours,

        /// <summary>
        /// Scan every 12 hours.
        /// </summary>
        [XmlEnum("43200|12 hours")]
        TwelveHours,

        /// <summary>
        /// Scan every 24 hours.
        /// </summary>
        [XmlEnum("86400|24 hours")]
        TwentyFourHours,
    }
}
