using System.Diagnostics;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
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
    /// <para type="description">Open-PrtgObject is only compatible with objects that contain a URL field; namely Probes, Devices, Groups and Sensors.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Device dc-1 | Get-Sensor ping | Open-PrtgObject</code>
    ///     <para>Open all sensors named "ping" under devices named "dc-1" in the default web browser.</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Open, "PrtgObject")]
    public class OpenPrtgObject : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The object to open.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            var server = PrtgUrl.AddUrlPrefix(client.Server);

            ExecuteOperation(() => Process.Start($"{server}{Object.Url}"), "Opening PRTG Objects", $"Opening {Object.BaseType.ToString().ToLower()} '{Object.Name}'");
        }
    }
}
