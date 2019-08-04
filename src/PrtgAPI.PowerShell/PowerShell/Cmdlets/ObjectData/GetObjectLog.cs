using System;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Request;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves object logs from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-ObjectLog cmdlet retrieves event logs from a PRTG Server. If no object is specified,
    /// Get-ObjectLog will retrieve results from the Root PRTG Group (ID: 0). Logs are ordered from newest to oldest. When retrieving logs
    /// from an object, all logs on child objects are also included. By default, PRTG only stores 30 days worth of logs.</para> 
    /// 
    /// <para type="description">If no date range or count is specified, by default Get-ObjectLog will retrieve all logs defined on
    /// the specified object for the last 7 days unless the specified object is the root group (ID: 0) or a probe, in which cause
    /// only logs that have occurred today will be retrieved.</para>
    /// 
    /// <para type="description">When specifying a date range, well known constants as well as manual start and end
    /// times can be specified. When specifying a date and time, the meaning of -<see cref="StartDate"/> and -<see cref="EndDate"/>
    /// are dependent upon the order with which the logs are being output. When logs are ordered from newest to oldest
    /// -<see cref="StartDate"/> refers to the time closest to now, while -<see cref="EndDate"/> represents the time furthest
    /// away from now. When logs are ordered from oldest to newest (i.e. when -<see cref="Wait"/> is specified) -<see cref="StartDate"/>
    /// represents the point in time logs furthest away from now logs should be retrieved from going into the future.</para>
    /// 
    /// <para type="descrption">If a -<see cref="StartDate"/> is specified without specifying an -<see cref="EndDate"/>,
    /// Get-ObjectLog will default to retrieving logs for the past 7 days prior to the -<see cref="StartDate"/>, unless the
    /// specified object is the root group (ID: 0) or a probe, in which case Get-ObjectLog will default to retrieving logs
    /// for the past 24 hours from the start time. When specifying well known constants, logs are retrieved from the specified
    /// point in time until the current time.</para>
    /// 
    /// <para type="description">Logs can be streamed continuously from a PRTG Object by specifying the -<see cref="Wait"/> parameter.
    /// When -<see cref="Wait"/> is specified (also aliased as -Tail) PrtgAPI will continuously poll PRTG for new logs according
    /// to a specified -<see cref="Interval"/>, outputting them to the console as they arrive in order from oldest to newest. If
    /// no -<see cref="Interval"/> is specified, by default Get-ObjectLog will poll once per second. A -<see cref="StartDate"/>
    /// can optionally be specified, specifying the initial point in time PrtgAPI should retrieve logs from. When -<see cref="Wait"/>
    /// is specified -<see cref="EndDate"/> will have no effect. Specifying an -<see cref="EndDate"/> in conjunction with -<see cref="Wait"/>
    /// will cause a warning to be emitted to the warning stream specifying that the -<see cref="EndDate"/> parameter will be ignored.
    /// </para>
    /// 
    /// <para type="description">Logs can be filtered to those of one or more event types by specifying the -Status parameter.
    /// Logs can also be filtered according to their event name, however note that name based filtering of Get-ObjectLog is
    /// performed client side, not server side. As such, specifying a -Name in conjunction with -Count will not work. This can be
    /// cirvumvented using Select-Object with the -First parameter instead.</para>
    /// 
    /// <para type="description">Note that while Get-ObjectLog considers the "start time" as being the point in time closest to now
    /// and the "end time" as the point in time furthest away from now when logs are ordered from newest to oldest, PRTG's underlying
    /// API actually defines these in the opposite way. Since logs are ordered from newest to oldest however, PrtgAPI flips these
    /// definitions as to prevent any confusion. Keep this in mind in the event the -Verbose parameter is specified, as the start
    /// and end times will appear to be switched. When -<see cref="Wait"/> is specified the meaning of -<see cref="StartDate"/>
    /// and -<see cref="EndDate"/> are flipped to match their meaningings in the underlying API so that logs can continuously
    /// be retrieved.</para>
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
    ///     <para/>
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
    ///     <code>C:\> Get-ObjectLog -Status Disconnected -Count 3</code>
    ///     <para>Retrieve the last 3 times a probe disconnected.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device exch-1 | Get-ObjectLog ping | select -First 4</code>
    ///     <para>Retrieve the last 4 events that occurred to the sensor named "ping" on the device named "exch-1".</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-ObjectLog -Id 1001 -EndDate $null</code>
    ///     <para>Retrieve all logs from the object with ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-ObjectLog -Status Connected -Wait</code>
    ///     <para>Continuously poll PRTG for new Probe Connected events, requesting once every second.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Historical-Information#logs-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [OutputType(typeof(Log))]
    [Cmdlet(VerbsCommon.Get, "ObjectLog", DefaultParameterSetName = ParameterSet.DateTime)]
    public class GetObjectLog : PrtgTableCmdlet<Log, LogParameters>, IWatchableCmdlet
    {
        /// <summary>
        /// <para type="description">Object to retrieve logs for. If no object is specified, defaults to the root object (group ID: 0)</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = ParameterSet.DateTime)]
        [Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = ParameterSet.RecordAge)]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// <para type="description">ID of the object to retrieve logs for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DateTimeManual)]
        public int Id { get; set; }

        /// <summary>
        /// <para type="description">Start time to retrieve logs from. If no value is specified, defaults to the current date and time.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DateTime)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DateTimeManual)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// <para type="description">End time to retrieve logs until. If no value is specified, defaults to 7 prior from the <see cref="StartDate"/>.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DateTime)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DateTimeManual)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// <para type="description">Time period to retrieve logs from. If no value is specified, retrieves logs from 7 days ago to the current date and time.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.RecordAge)]
        public RecordAge? Period { get; set; } //todo: document change on wiki

        /// <summary>
        /// <para type="description">Only retrieve objects that match a specific status.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public LogStatus[] Status { get; set; }

        /// <summary>
        /// <para type="description">Indicates Get-ObjectLog should continuously retrieve new records from PRTG according to a specified polling -<see cref="Interval"/>.</para> 
        /// </summary>
        [Alias("Tail")]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DateTimeManual)]
        public SwitchParameter Wait { get; set; }

        /// <summary>
        /// <para type="description">Interval with which Get-ObjectLog should poll for new records when using -<see cref="Wait"/>.</para> 
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DateTimeManual)]
        public int Interval { get; set; } = 1;

        bool IWatchableCmdlet.WatchStream { get; set; }

        int? id;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetObjectLog"/> class.
        /// </summary>
        public GetObjectLog() : base(Content.Logs, true)
        {
        }        
        internal override bool StreamCount()
        {
            return true;
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (Wait)
                ((IWatchableCmdlet) this).WatchStream = true;

            if (ParameterSetName == ParameterSet.DateTimeManual)
                id = Id;
            else
                id = Object?.Id;

            base.ProcessRecordEx();
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            ProcessStatusFilter();

            if (Wait)
                ProcessWatchDateFilters();
            else
            {
                ProcessDateFilter(Property.EndDate, StartDate); //Closest to now
                ProcessDateFilter(Property.StartDate, EndDate); //Furthest from now
            }
            
            ProcessRecordAgeFilter();

            ProcessUnspecifiedRange();

            base.ProcessAdditionalParameters();
        }

        private void ProcessWatchDateFilters()
        {
            if (EndDate != null)
                WriteWarning($"Ignoring -{nameof(EndDate)} as cmdlet is executing in Watch Mode. To specify a start time use -{nameof(StartDate)}");

            if (StartDate != null)
                ProcessDateFilter(Property.StartDate, StartDate);
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
            {
                AddPipelineFilter(property, TypeHelpers.DateToString(datetime.Value), false);
            }
        }

        private void ProcessRecordAgeFilter()
        {
            //"All" is not a valid RecordAge. If "All" is specified, we don't want to limit
            if (Period != null && Period != RecordAge.All)
                AddPipelineFilter(Property.RecordAge, Period, false);

            if (Period == RecordAge.All)
                StreamProvider.ForceStream = true;
        }

        private void ProcessUnspecifiedRange()
        {
            if (ProgressManager.WatchStream)
            {
                StreamProvider.ForceStream = true;

                if (StartDate == null)
                    ProcessDateFilter(Property.StartDate, DateTime.Now.AddMinutes(-1));

                return;
            }

            //If a start date, time period and a count haven't been specified, for performance with
            //larger installs limit the records to those in the past 7 days
            if (Unspecified(nameof(EndDate)) && Unspecified(nameof(Period)) && Unspecified(nameof(Count)))
            {
                if (Unspecified(nameof(StartDate)))
                {
                    //Retrieve records for the last week. If this is a high traffic node (such as the root or a probe)
                    //however, only retrieve records for the past day.

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
                if (Count == null && RequiresStreaming() || Count > ObjectEngine.SerialStreamThreshold)
                {
                    StreamProvider.ForceStream = true;
                }
            }
        }

        private bool Unspecified(string name)
        {
            return !MyInvocation.BoundParameters.ContainsKey(name);
        }

        private bool RequiresStreaming()
        {
            return ContainsRootNode() || Object?.baseType == BaseType.Probe;
        }

        private bool ContainsRootNode()
        {
            if (id == null || id == 0)
                return true;

            return false;
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving logs from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override LogParameters CreateParameters() => new LogParameters(id);
    }
}
