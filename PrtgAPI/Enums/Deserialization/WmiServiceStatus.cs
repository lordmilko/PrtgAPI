using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Indicates the current state of a WMI Service.
    /// </summary>
    public enum WmiServiceStatus
    {
        /// <summary>
        /// The service is stopped. This corresponds to the Win32 <see langword="SERVICE_STOPPED" /> constant, which is defined as 0x00000001.
        /// </summary>
        [XmlEnum("Stopped")]
        Stopped = 1,

        /// <summary>
        /// The service is starting. This corresponds to the Win32 <see langword="SERVICE_START_PENDING" /> constant, which is defined as 0x00000002.
        /// </summary>
        [XmlEnum("Start Pending")]
        StartPending = 2,

        /// <summary>
        /// The service is stopping. This corresponds to the Win32 <see langword="SERVICE_STOP_PENDING" /> constant, which is defined as 0x00000003.
        /// </summary>
        [XmlEnum("Stop Pending")]
        StopPending = 3,

        /// <summary>
        /// The service is running. This corresponds to the Win32 <see langword="SERVICE_RUNNING" /> constant, which is defined as 0x00000004.
        /// </summary>
        [XmlEnum("Running")]
        Running = 4,

        /// <summary>
        /// The service continue is pending. This corresponds to the Win32 <see langword="SERVICE_CONTINUE_PENDING" /> constant, which is defined as 0x00000005.
        /// </summary>
        [XmlEnum("Continue Pending")]
        ContinuePending = 5,

        /// <summary>
        /// The service pause is pending. This corresponds to the Win32 <see langword="SERVICE_PAUSE_PENDING" /> constant, which is defined as 0x00000006.
        /// </summary>
        [XmlEnum("Pause Pending")]
        PausePending = 6,

        /// <summary>
        /// The service is paused.. This corresponds to the Win32 <see langword="SERVICE_PAUSED" /> constant, which is defined as 0x00000007.
        /// </summary>
        [XmlEnum("Paused")]
        Paused = 7
    }
}
