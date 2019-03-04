using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves system information for a PRTG Device.</para>
    /// 
    /// <para type="description">The Get-SystemInfo cmdlet retrieves system information for a specified device.
    /// By default, PRTG will probe devices once every 24 hours for enhanced system information, including hardware,
    /// software, process, service and user info. Not all devices may support retrieving system information or
    /// may only support specific system information types.</para>
    /// 
    /// <para type="description">Specific system information types can be retrieved by specifying one or more values
    /// to the -<see cref="Type"/> parameter. If more than one type is specified, results will be bundled up
    /// in a <see cref="PSObject"/> containing a property for each information type. If no types are specified, Get-SystemInfo
    /// will automatically retrieve information for all known information types.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-SystemInfo -Id 1001</code>
    ///     <para>Retrieve system information from the device with ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 1001 | Get-SystemInfo</code>
    ///     <para>Retrieve system information from a piped in device.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-SystemInfo -ID 1001 hardware,software</code>
    ///     <para>Retrieve hardware and software information for the device with ID 1001.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Devices#system-information-1">Online version:</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Refresh-SystemInfo</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "SystemInfo")]
    public class GetSystemInfo : PrtgProgressCmdlet
    {
        /// <summary>
        /// <para type="description">Device to retrieve system information for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Default, ValueFromPipeline = true)]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">ID of the device to retrieve system information for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int Id { get; set; }

        /// <summary>
        /// <para type="description">One or more system information types to retrieve. If no types are specified, all System Information types
        /// will be retrieved.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public SystemInfoType[] Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSystemInfo"/> class.
        /// </summary>
        public GetSystemInfo() : base("System Information")
        {
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            var id = Device?.Id ?? Id;

            WriteProcessProgressRecords(f => ProcessTypes(f, id));
        }

        private object ProcessTypes(Func<int, string, bool> progressCallback, int id)
        {
            if (Type == null || Type.Length == 0)
            {
                Type = typeof(SystemInfoType).GetEnumValues().Cast<SystemInfoType>().ToArray();
            }

            List<Tuple<SystemInfoType, List<IDeviceInfo>>> records = new List<Tuple<SystemInfoType, List<IDeviceInfo>>>();

            for (var i = 0; i < Type.Length; i++)
            {
                var percent = ProgressManager.GetPercentComplete(i + 1, Type.Length);

                progressCallback(percent, $"Retrieving {GetInfoTypeDescription(Type[i])} Info ({i + 1}/{Type.Length})");

                var info = client.GetSystemInfo(id, Type[i]).ToList();
                records.Add(Tuple.Create(Type[i], info));
            }

            return FormatOutput(records);
        }

        private object FormatOutput(List<Tuple<SystemInfoType, List<IDeviceInfo>>> records)
        {
            if (records.Count == 1)
                return records.Single().Item2;

            var psObject = new PSObject();

            foreach(var record in records)
            {
                psObject.Properties.Add(new PSNoteProperty(record.Item1.ToString(), record.Item2));
            }

            return psObject;
        }

        private string GetInfoTypeDescription(SystemInfoType type)
        {
            return Regex.Replace(type.ToString(), "[a-z][A-Z]", m => m.Value[0] + " " + m.Value[1]);
        }
    }
}
