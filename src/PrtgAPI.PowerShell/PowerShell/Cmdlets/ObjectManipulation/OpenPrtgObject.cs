using System.Diagnostics;
using System.Linq;
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
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Open-PrtgObject -Id 1001</code>
    ///     <para>Opens the object with ID 1001 in the default web browser.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Miscellaneous#open-prtg-objects">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Open, "PrtgObject", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
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
        /// <para type="description">ID of the object to open.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual, Position = 0)]
        public int[] Id { get; set; }

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
                case ParameterSet.Manual:
                    ExecuteManual();
                    break;
                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }
        }

        private void ExecuteOperation(PrtgObject obj, string url)
        {
            var server = PrtgRequestMessage.AddUrlPrefix(client.Server);

            if (ShouldProcess($"'{obj}' (ID: {obj.Id}, Type: {obj.Type})"))
                ExecuteOperation(() => Process.Start($"{server}{url}"), $"Opening {obj.GetTypeDescription()} '{obj.Name}'");
        }

        private void ExecuteManual()
        {
            var objs = GetObject.LiftObjects(client, client.GetObjects(Property.Id, Id));

            foreach (var id in Id)
            {
                var match = objs.FirstOrDefault(o => o.Id == id);

                if (match == null)
                {
                    WriteInvalidOperation($"Failed to retrieve object with ID '{id}': object does not exist.");
                }
                else
                {
                    var urlProperty = match.GetType().GetProperty("Url");

                    if (urlProperty == null)
                        WriteInvalidOperation($"Cannot open object '{match}' of type '{match.Type}': object does not have a 'Url' property.", match);
                    else
                        ExecuteOperation(match, (string)urlProperty.GetValue(match));
                }
            }
        }

        internal override string ProgressActivity => "Opening PRTG Objects";
    }
}
