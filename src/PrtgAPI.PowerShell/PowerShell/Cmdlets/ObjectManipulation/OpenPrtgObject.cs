using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Request;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Opens the URL of a PRTG Object in the PRTG Web Interface.</para>
    /// 
    /// <para type="description">The Open-PrtgObject cmdlet opens the web page of a PRTG Object in your default browser.
    /// Care should be taken with Open-PrtgObject, as your system may experience performance issues if too many URLs are opened at one time.</para>
    /// 
    /// <para type="description">Open-PrtgObject is only compatible with objects that contain a URL field; namely Probes, Devices, Groups Sensors,
    /// Notification Actions and Schedules.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Device dc-1 | Get-Sensor ping | Open-PrtgObject</code>
    ///     <para>Open all sensors named "ping" under devices named "dc-1" in the default web browser.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Miscellaneous#open-prtg-objects">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Cmdlet(VerbsCommon.Open, "PrtgObject", DefaultParameterSetName = ParameterSet.Default)]
    public class OpenPrtgObject : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The sensor, device, group or probe to open.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">The notification action to open.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Notification)]
        public NotificationAction Notification { get; set; }

        /// <summary>
        /// <para type="description">The schedule to open.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Schedule)]
        public Schedule Schedule { get; set; }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            switch (ParameterSetName)
            {
                case ParameterSet.Default:
                    ExecuteOperation(Object, Object.Url);
                    break;
                case ParameterSet.Notification:
                    ExecuteOperation(Notification, Notification.Url);
                    break;
                case ParameterSet.Schedule:
                    ExecuteOperation(Schedule, Schedule.Url);
                    break;
                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }
        }

        private void ExecuteOperation(PrtgObject obj, string url)
        {
            var server = PrtgRequestMessage.AddUrlPrefix(client.Server);

            ExecuteOperation(() => Process.Start($"{server}{url}"), $"Opening {obj.GetTypeDescription()} '{obj.Name}'");
        }

        internal override string ProgressActivity => "Opening PRTG Objects";
    }
}
