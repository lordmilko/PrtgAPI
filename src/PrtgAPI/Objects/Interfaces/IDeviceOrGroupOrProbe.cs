namespace PrtgAPI
{
    /// <summary>
    /// Specifies properties that apply to Devices, Groups and Probes.
    /// </summary>
    public interface IDeviceOrGroupOrProbe : ISensorOrDeviceOrGroupOrProbe
    {
        /// <summary>
        /// Number of sensors in <see cref="Status.Up"/> state.
        /// </summary>
        int UpSensors { get; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Down"/> state.
        /// </summary>
        int DownSensors { get; }

        /// <summary>
        /// Number of sensors in <see cref="Status.DownAcknowledged"/> state.
        /// </summary>
        int DownAcknowledgedSensors { get; }

        /// <summary>
        /// Number of sensors in <see cref="Status.DownPartial"/> state.
        /// </summary>
        int PartialDownSensors { get; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Warning"/> state.
        /// </summary>
        int WarningSensors { get; }

        /// <summary>
        /// Number of sensors in <see cref="Status.PausedByUser"/>, <see cref="Status.PausedByDependency"/>, <see cref="Status.PausedBySchedule"/> or <see cref="Status.PausedByLicense"/> state.
        /// </summary>
        int PausedSensors { get; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Unusual"/> state.
        /// </summary>
        int UnusualSensors { get; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Unknown"/> state.
        /// </summary>
        int UnknownSensors { get; }

        /// <summary>
        /// Total number of sensors contained under this object in any <see cref="Status"/> state.
        /// </summary>
        int TotalSensors { get; }
    }
}
