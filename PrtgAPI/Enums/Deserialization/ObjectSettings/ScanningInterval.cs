using System;
using System.Xml.Serialization;
using PrtgAPI.Helpers;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a well known or custom TimeSpan used for the Scanning Interval of an object.
    /// </summary>
    public class ScanningInterval : IFormattable
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
        /// <param name="interval">The value to parse.</param>
        /// <returns>A ScanningInterval that encapsulates the passed value.</returns>
        public static ScanningInterval Parse(object interval)
        {
            if (interval is string)
            {
                StandardScanningInterval value;

                if (Enum.TryParse(interval.ToString(), true, out value))
                {
                    interval = value;
                }
                else
                {
                    var val = EnumHelpers.XmlToEnum<XmlEnumAttribute>(interval.ToString(), typeof (StandardScanningInterval), false);

                    if (val == null)
                    {
                        TimeSpan timeSpan;

                        if (TimeSpan.TryParse(interval.ToString(), out timeSpan))
                            return timeSpan;
                        else
                            return new ScanningInterval(StringToTimeSpan(interval.ToString()));
                    }
                    else
                        interval = val;
                }
            }

            if (interval is StandardScanningInterval)
            {
                return ParseIntervalEnum((StandardScanningInterval) interval);
            }
            if (interval is TimeSpan)
                return new ScanningInterval((TimeSpan) interval);
            if (interval is int)
            {
                return new TimeSpan(0, 0, Convert.ToInt32(interval));
            }
            else
            {
                TimeSpan timeSpan;

                if (TimeSpan.TryParse(interval.ToString(), out timeSpan))
                {
                    return new ScanningInterval(timeSpan);
                }
                else
                    throw new InvalidCastException($"Cannot convert '{interval}' of type '{interval.GetType()}' to type '{nameof(ScanningInterval)}'. Value type must be convertable to one of PrtgAPI.StandardScanningInterval or System.TimeSpan.");
            }
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
                    throw new NotImplementedException($"Handler missing for interval '{interval}'");
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
        /// Get the <see cref="System.TimeSpan"/> representation of the underlying interval.
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

        string IFormattable.GetSerializedFormat()
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
            else if(time.TotalSeconds >= 10)
                str = $"{time.TotalSeconds} seconds";
            else if (time.TotalSeconds > 1)
                str = $"{time.TotalSeconds} seconds (Not officially supported)";
            else if (time.TotalSeconds == 1)
                str = $"{time.TotalSeconds} second (Not officially supported)";
            else
                throw new NotImplementedException($"Not sure how to handle TimeSpan {time}");

            return $"{time.TotalSeconds}|{str}";
        }

        private static TimeSpan StringToTimeSpan(string str)
        {
            if (!str.Contains("|"))
                throw new FormatException($"Interval '{str}' is not correctly formatted.");

            var secs = str.Substring(0, str.IndexOf('|'));

            var secsInt = Convert.ToInt32(secs);

            return new TimeSpan(0, 0, secsInt);
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
