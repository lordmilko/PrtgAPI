using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Refreshes system information for a PRTG Device.</para>
    /// 
    /// <para type="description">The Refresh-SystemInfo cmdlet refreshes system information for a specified devoce. By default,
    /// PRTG theoretically probes devices for all system information types once every 24 hours. If you find this is not happening
    /// however, or otherwise wish to refresh sooner, this can be forced with the Refresh-SystemInfo cmdlet.</para>
    /// 
    /// <para type="description">The types of system information that should be refreshed can be specified with the -<see cref="Type"/> parameter.
    /// If no types are specified, Refresh-SystemInfo will request that all information types be updated.</para>
    /// 
    /// <para type="description">PRTG is capable of performing up to 24 simultaneous system information scans. Any additional scans that
    /// are requested will be queued until a scan slot becomes available.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Device dc* | Refresh-SystemInfo</code>
    ///     <para>Refresh all system information for all devices whose name starts with "dc"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Refresh-SystemInfo -Id 1001 -Type System,Users</code>
    ///     <para>Refresh the System and Users information types for the device with ID 1001</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Devices#system-information-1">Online version:</para>
    /// <para type="link">Get-SystemInfo</para>
    /// </summary>
    [Cmdlet(VerbsData.Update, "SystemInfo", DefaultParameterSetName = ParameterSet.Default)]
    public class RefreshSystemInfo : PrtgPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">The device to refresh system information for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">The ID of the device to refresh system information for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int Id { get; set; }

        /// <summary>
        /// <para type="description">The types of system information to refresh. If no types are specified, all types will be refreshed.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public SystemInfoType[] Type { get; set; }

        internal override string ProgressActivity => "Refreshing System Information";

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            ExecuteOperation(() => client.RefreshSystemInfo(Device?.Id ?? Id, Type), GetProgressText());
        }

        private string GetProgressText()
        {
            if (Device != null)
                return $"Refreshing information on device '{Device}'";

            return $"Refreshing information on device with ID '{Id}'";
        }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Device;
    }
}
