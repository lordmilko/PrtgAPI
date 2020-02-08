using System;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies properties that apply to Sensors.
    /// </summary>
    public interface ISensor : ISensorOrDevice
    {
        /// <summary>
        /// Last value of this sensor's primary channel with value unit. If this sensor's primary channel has been recently changed, the sensor may need to be paused and unpaused (otherwise it may just display "No data").
        /// </summary>
        string DisplayLastValue { get; }

        /// <summary>
        /// The raw last value of this sensor's primary channel.
        /// </summary>
        double? LastValue { get; }

        /// <summary>
        /// Device this sensor monitors.
        /// </summary>
        string Device { get; }

        /// <summary>
        /// Percentage indicating overall downtime of this object over its entire lifetime. See also: <see cref="Uptime"/>.
        /// </summary>
        double? Downtime { get; }

        /// <summary>
        /// Total amount of time sensor has ever been in a <see cref="Status.Down"/> or <see cref="Status.DownAcknowledged"/> state.
        /// </summary>
        TimeSpan? TotalDowntime { get; }

        /// <summary>
        /// Amount of time passed since this object was last in an <see cref="Status.Up"/> state. If this object is currently <see cref="Status.Up"/>, this value is null.
        /// </summary>
        TimeSpan? DownDuration { get; }

        /// <summary>
        /// Percentage indicating overall uptime of this object over its entire lifetime. See also: <see cref="Downtime"/>.
        /// </summary>
        double? Uptime { get; }

        /// <summary>
        /// Total amount of time sensor has ever been in a <see cref="Status.Up"/> state.
        /// </summary>
        TimeSpan? TotalUptime { get; }

        /// <summary>
        /// Amount of time passed since sensor was last in a <see cref="Status.Down"/> state. If sensor is currently <see cref="Status.Down"/>, this value is null.
        /// </summary>
        TimeSpan? UpDuration { get; }

        /// <summary>
        /// Total amount of time this sensor has been in an up or down state.
        /// </summary>
        TimeSpan TotalMonitorTime { get; }

        /// <summary>
        /// When data collection on this sensor began.
        /// </summary>
        DateTime? DataCollectedSince { get; }

        /// <summary>
        /// When this sensor last checked for a value.
        /// </summary>
        DateTime? LastCheck { get; }

        /// <summary>
        /// When this object was last in an <see cref="Status.Up"/> state.
        /// </summary>
        DateTime? LastUp { get; }

        /// <summary>
        /// When this value was last in a down state.
        /// </summary>
        DateTime? LastDown { get; }

        /// <summary>
        /// CSV of sensor values for the past 24 hours. Numbers are stored as 5 minute averages. Value contains two sets of CSVs: measured values and errors. Sets are separated by a pipe . If MiniGraphs are disabled, this value is null.
        /// </summary>
        string MiniGraph { get; }
    }
}
