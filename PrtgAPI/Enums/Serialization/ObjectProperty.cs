using System.ComponentModel;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Undocumented;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies the settings of objects that can be interfaced with using the PRTG HTTP API.</para>
    /// </summary>
    public enum ObjectProperty
    {
        #region Access Rights

        [LiteralValue]
        [TypeLookup(typeof(SensorSettings))]
        InheritAccess,

        #endregion
        #region Basic Sensor Settings

        /// <summary>
        /// The name if the object. Must be convertable to <see cref="SensorSettings.Name"/>.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        Name,

        [TypeLookup(typeof(SensorSettings))]
        ParentTags,

        [TypeLookup(typeof(SensorSettings))]
        Tags,

        [TypeLookup(typeof(SensorSettings))]
        Priority,

        #endregion
        #region Debug Options

        [TypeLookup(typeof(SensorSettings))]
        DebugMode,

        #endregion
        #region HTTP Specific

        [TypeLookup(typeof(SensorSettings))]
        Url,

        [TypeLookup(typeof(SensorSettings))]
        HttpRequestMethod,

        [TypeLookup(typeof(SensorSettings))]
        UseSNIFromUrl,

        #endregion
        #region Ping Settings

        [TypeLookup(typeof(SensorSettings))]
        Timeout,

        [TypeLookup(typeof(SensorSettings))]
        PingPacketSize,

        [TypeLookup(typeof(SensorSettings))]
        PingMode,

        [TypeLookup(typeof(SensorSettings))]
        PingCount,

        [TypeLookup(typeof(SensorSettings))]
        PingDelay,

        [TypeLookup(typeof(SensorSettings))]
        AutoAcknowledge,

        #endregion
        #region Scanning Interval

        /// <summary>
        /// Whether to inherit this object's scanning interval settings from its parent.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(SensorSettings))]
        InheritInterval,

        /// <summary>
        /// The <see cref="PrtgAPI.ScanningInterval"/> with which an object refreshes its data.
        /// </summary>
        [Description("interval")]
        [TypeLookup(typeof(SensorSettings))]
        Interval,

        /// <summary>
        /// The <see cref="PrtgAPI.IntervalErrorMode"/> indicating the number of scanning intervals to wait before setting a sensor to <see cref="Status.Down"/> when an error is reported.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        IntervalErrorMode,

        #endregion
        #region Schedules, Dependencies and Maintenance Window

        //todo: make a note that if you set this WITHOUT setting another property, it does nothing. as such, maybe
        //we need to make this a dependent property. note that it would work on devices, just not sensors
        //todo: also note this on sensorsettings and whatever base class we end up using

        [LiteralValue]
        [TypeLookup(typeof(SensorSettings))]
        InheritDependency,

        [TypeLookup(typeof(SensorSettings))]
        MaintenanceEnabled,

        #endregion
        #region Sensor Display

        PrimaryChannel,

        [TypeLookup(typeof(SensorSettings))]
        GraphType,

        StackUnit,

        #endregion
        #region Sensor Settings (EXE/XML)

        [TypeLookup(typeof(SensorSettings))]
        ExeParameters,

        [TypeLookup(typeof(SensorSettings))]
        SetEnvironmentVariables,

        [TypeLookup(typeof(SensorSettings))]
        UseWindowsAuthentication,

        [TypeLookup(typeof(SensorSettings))]
        Mutex,

        [TypeLookup(typeof(SensorSettings))]
        EnableChangeTriggers,

        #endregion
        #region WMI Alternative Query

        [TypeLookup(typeof(SensorSettings))]
        WmiMode,

        #endregion
        #region WMI Remote Ping Configuration

        /// <summary>
        /// The DNS name or IP Address to target. Applies to: WMI Remote Ping
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        Target,

        [TypeLookup(typeof(SensorSettings))]
        PingRemotePacketSize,

        #endregion
    }
}
