using System.ComponentModel;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies the settings of objects that can be interfaced with using the PRTG HTTP API.</para>
    /// </summary>
    public enum ObjectProperty
    {
        /*Name,
        Tags,
        Priority,
        DebugMode,
        WmiMode,*/

        //todo: maybe we shouldnt be stripping the underscores in our objectproperty objects
        //that way we dont have to respecify things here?
        //how come the interval property doesnt have an underscore after it. does it work if you use an underscore?
        //what is used if you set the interval and watch it with fiddler. if we can use an underscore maybe try and extract
        //this from the objectproperty xmlelement tags

        /// <summary>
        /// The DNS name or IP Address to target. Applies to: WMI Remote Ping
        /// </summary>
        [Description("targetaddress_")]
        Target,

        [Description("httpurl_")]
        Url,

        /// <summary>
        /// Whether to inherit this object's scanning interval settings from its parent.
        /// </summary>
        [Description("intervalgroup")]
        InheritScanningInterval,

        /// <summary>
        /// The <see cref="PrtgAPI.ScanningInterval"/> with which an object refreshes its data.
        /// </summary>
        [Description("interval")]
        ScanningInterval,

        /// <summary>
        /// The <see cref="PrtgAPI.ErrorIntervalDown"/> indicating the number of scanning intervals to wait before setting a sensor to <see cref="Status.Down"/> when an error is reported.
        /// </summary>
        [Description("errorintervalsdown_")]
        ErrorIntervalDown
    }
}
