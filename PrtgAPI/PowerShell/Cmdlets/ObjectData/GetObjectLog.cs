using System;
using System.Management.Automation;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves object logs from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-ObjectLog cmdlet retrieves event logs from a PRTG Server. If no object is specified,
    /// Get-ObjectLog will retrieve results from the Root PRTG Group (ID: 0). Logs are ordered from newest to oldest. When retrieving logs from an object, all logs
    /// on child objects are also included.</para>
    /// 
    /// <para type="description">If no date range or count is specified, by default Get-ObjectLog will retrieve all logs defined on
    /// the specified object for the last 7 days unless the specified object is the root group (ID: 0) or a probe, in which cause
    /// only logs that have occurred today will be retrieved.</para>
    /// 
    /// <para type="description">When specifying a date range, well known constants as well as manual start and end
    /// times can be specified. When specifying a date and time, Get-ObjectLog considers the "start time" as the time
    /// closest to now, while the "end time" is the point in time furthest away from now. If a -StartTime is specified
    /// without specifying an -EndTime, Get-ObjectLog will default to retrieving logs for the past 7 days prior to the -StartTime,
    /// unless the specified object is the root group (ID: 0) or a probe, in which case Get-ObjectLog will default to retrieving
    /// logs for the past 24 hours from the start time. When specifying well known constants, logs are retrieved from the specified
    /// point in time until the current time.</para>
    /// 
    /// <para type="description">Logs can be filtered to those of one or more event types by specifying the -Status parameter.
    /// Logs can also be filtered according to their event name, however note that name based filtering of Get-ObjectLog is
    /// performed client side, not server side. As such, specifying a -Name in conjunction with -Count will not work. This can be
    /// cirvumvented using Select-Object with the -First parameter instead.</para>
    /// 
    /// <para type="description">Note that while Get-ObjectLog considers the "start time" as being the point in time closest to now
    /// and the "end time" as the point in time furthest away from now, PRTG's underlying API actually defines these in the opposite way.
    /// Since logs are ordered from newest to oldest however, PrtgAPI flips these definitions as to prevent any confusion. Keep this
    /// in mind in the event the -Verbose parameter is specified, as the start and end times will appear to be switched.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-ObjectLog</code>
    ///     <para>Retrieve all logs from the root group (ID: 0) from today.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-ObjectLog -Count 4000</code>
    ///     <para>Retrieve the last 4000 logs from the root group.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device dc-1 | Get-ObjectLog</code>
    ///     <para>Retrieve all logs on device "dc-1" for the last week.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-ObjectLog -Start (Get-Date).AddDays(-3)</code>
    ///     <para>Retrieve all logs from the root node from 3 and 4 days ago.</para>
    ///     <para></para>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 2460 | Get-ObjectLog -EndDate (Get-Date).AddDays(-4)</code>
    ///     <para>Retrieve all logs from the sensor with ID 2460 between now and 4 days ago.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe *contoso* | Get-ObjectLog -Since LastWeek</code>
    ///     <para>Retrieve all logs from all probes whose name contains "contoso" between now and last week.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-ObjectLog -Status ProbeDisconnected -Count 3</code>
    ///     <para>Retrieve the last 3 times a probe disconnected.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device exch-1 | Get-ObjectLog ping | select -First 4</code>
    ///     <para>Retrieve the last 4 events that occurred to the sensor named "ping" on the device named "exch-1".</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ObjectLog", DefaultParameterSetName = ParameterSet.RecordAge)]
    public class GetObjectLog : PrtgTableCmdlet<Log, LogParameters>
    {
        /// <summary>
        /// <para type="description">Object to retrieve logs for. If no object is specified, defaults to the root object (group ID: 0)</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">Start time to retrieve logs from. If no value is specified, defaults to the current date and time.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DateTime)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// <para type="description">End time to retrieve logs until. If no value is specified, defaults to 7 prior from the <see cref="StartDate"/>.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DateTime)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// <para type="description">Time period to retrieve logs from. If no value is specified, retrieves logs from 7 days ago to the current date and time.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.RecordAge)]
        public RecordAge? Since { get; set; }

        /// <summary>
        /// <para type="description">Only retrieve objects that match a specific status.</para>
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public LogStatus[] Status { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetObjectLog"/> class.
        /// </summary>
        public GetObjectLog() : base(Content.Messages, 500, true)
        {
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            ProcessStatusFilter();
            ProcessDateFilter(Property.EndDate, StartDate);
            ProcessDateFilter(Property.StartDate, EndDate);
            ProcessRecordAgeFilter();

            ProcessUnspecifiedRange();

            base.ProcessAdditionalParameters();
        }

        private void ProcessStatusFilter()
        {
            if (Status != null)
            {
                foreach (var value in Status)
                {
                    AddPipelineFilter(Property.Status, value);
                }
            }
        }

        private void ProcessDateFilter(Property property, DateTime? datetime)
        {
            if (datetime != null)
                AddPipelineFilter(property, ParameterHelpers.DateToString(datetime.Value), false);
        }

        private void ProcessRecordAgeFilter()
        {
            if (Since != null && Since != RecordAge.AllTime)
            {
                AddPipelineFilter(Property.RecordAge, Since, false);
            }

            if (Since == RecordAge.AllTime)
                StreamProvider.ForceStream = true;
        }

        private void ProcessUnspecifiedRange()
        {
            //If a start date, time period and a count haven't been specified, for performance with
            //larger installs limit the records to those in the past 7 days
            if (EndDate == null && Since == null && Count == null)
            {
                if (StartDate == null)
                {
                    //Retrieve records for the last week. If this is the root node however, only retrieve
                    //records for the past day.

                    var duration = RecordAge.LastWeek;

                    if (RequiresStreaming())
                        duration = RecordAge.Today;

                    AddPipelineFilter(Property.RecordAge, duration, false);
                }
                else
                {
                    var duration = -7;

                    if (RequiresStreaming())
                        duration = -1;

                    ProcessDateFilter(Property.StartDate, StartDate.Value.AddDays(duration));
                }

                if (RequiresStreaming())
                    StreamProvider.ForceStream = true;
            }
            else
            {
                if ((Count == null && RequiresStreaming()) || Count > 20000)
                {
                    StreamProvider.ForceStream = true;
                }
            }
        }

        private bool RequiresStreaming()
        {
            return ContainsRootNode() || Object.BaseType == BaseType.Probe;
        }

        private bool ContainsRootNode()
        {
            if (Object == null || Object.Id == 0)
                return true;

            return false;
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving logs from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override LogParameters CreateParameters() => new LogParameters(Object?.Id);
    }
}
