using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves historic data values for a PRTG Sensor.</para>
    /// 
    /// <para type="description">The Get-SensorHistory cmdlet retrieves historic data values for all channels of a PRTG Sensor within a specified time period.
    /// By default, values are returned according to the scanning interval defined on the sensor (e.g. values every 60 seconds). Using the -<see cref="Average"/> parameter,
    /// values can be averaged together to provide a higher level view of a larger time span. Any number of seconds can be specified as the <see cref="Average"/>,
    /// however note that depending on the interval of the sensor, certain time spans may result in blank values within the sensor results.</para>
    /// 
    /// <para type="description">Historic data values can be retrieved over any time period via the -<see cref="StartDate"/>  and -<see cref="EndDate"/>
    /// parameters. If these values are not specified, records will be returned from between now and 60 minutes ago.
    /// If PRTG finds it does not have enough monitoring data to return for the specified time span, an exception will be thrown. To work around this,
    /// you can request date for a larger time frame and then potentially filter the data with the Where-Object and Select-Object cmdlets.</para>
    /// 
    /// <para type="description">When an -<see cref="EndDate"/> is specified, Get-SensorHistory will split the sensor history query into a series of smaller
    /// requests, emitting results to the pipeline as they arrive. If the -<see cref="Count"/> parameter is speciifed, and -<see cref="EndDate"/> is not
    /// specified, Get-SensorHistory will automatically adjust the -<see cref="EndDate"/> such that the specified number of records can be retrieved, based
    /// on the sensor's Interval or the specified -<see cref="Average" />. If an -<see cref="EndDate"/> has been specified, the number of records
    /// returned may be limited by the number of records available in the specified time period.</para>
    /// 
    /// <para type="description">Due to limitations of the PRTG API, Get-SensorHistory cannot display both downtime and channel value lookups
    /// at the same time. Value lookups can only be displayed when an -<see cref="Average"/> of 0 is specified, while downtime can only
    /// be displayed when a specific average has been specified. By default, if no average is specified Get-SensorHistory will use an average of 0
    /// to enable the display of value lookups. If the -<see cref="Downtime"/> parameter is specified and no -<see cref="Average"/> is specified,
    /// Get-SensorHistory will use the Interval property of the sensor to specify the true average.</para>
    /// 
    /// <para type="description">PrtgAPI will automatically display all numeric channel values as numbers, with the unit of the channel
    /// displayed in brackets next to the channel header (e.g. "Total(%)"). These units are for display purposes only, and so can be ignored
    /// when attempting to extract certain columns from the sensor history result.</para>
    /// 
    /// <para type="description">Note that while Get-SensorHistory considers the "start time" as being the point in time closest to now
    /// and the "end time" as the point in time furthest away from now, PRTG's underlying API actually defines these in the opposite way.
    /// Since records are ordered from newest to oldest however, PrtgAPI flips these definitions as to prevent any confusion. Keep this
    /// in mind in the event the -Verbose parameter is specified, as the start and end times will appear to be switched.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1001 | Get-SensorHistory</code>
    ///     <para>Get historical values for all channels on the sensor with ID 1001 over the past 24 hours, using the sensor's default interval.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1001 | Get-SensorHistory -Average 300</code>
    ///     <para>Get historical values for all channels on the sensor with ID 1001 over the past 24 hours, averaging values in 5 minute intervals.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1001 | Get-SensorHistory -StartDate (Get-Date).AddDays(-1) -EndDate (Get-Date).AddDays(-3)</code>
    ///     <para>Get historical values for all channels on the sensor with ID 1001 starting three days ago and ending yesterday.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags wmicpu* -count 1 | Get-SensorHistory | select Total</code>
    ///     <para>Get historical values for all channels on a single WMI CPU Load sensor, selecting the "Total" channel (visually displayed as "Total(%)"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags wmicpu* -count 1 | where Total -gt 90</code>
    ///     <para>Get historical values for all channels on a single WMI CPU Load sensor where the Total channel's value was greater than 90.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Historical-Information#sensor-history-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Select-Object</para>
    /// <para type="link">Where-Object</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "SensorHistory")]
    public class GetSensorHistory : PrtgObjectCmdlet<PSObject>, IStreamableCmdlet<GetSensorHistory, SensorHistoryRecord, SensorHistoryParameters>
    {
        /// <summary>
        /// <para type="description">Sensor to retrieve historic data for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Default, ValueFromPipeline = true)]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// <para type="description">ID of the sensor to retrieve historic data for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int Id { get; set; }

        /// <summary>
        /// <para type="description">Start time to retrieve history from. If no value is specified, defaults to 24 hours ago.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// <para type="description">End time to retrieve history to. If no value is specified, defaults to the current date and time.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// <para type="description">Time span (in seconds) to average results over. For example, a value of 300 will show the average value every 5 minutes.
        /// If a value of 0 is used, PRTG will include value lookup labels in historical results. If a value other than 0 is used, PRTG will include the Downtime
        /// channel in results.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int? Average { get; set; }

        /// <summary>
        /// <para type="description">Specifies that the Downtime channel should be included in query results in lieu of value lookup labels.
        /// If this parameter is specified and <see cref="Average"/> is not specified, the Interval of the sensor will be used to determine the average.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Default)]
        public SwitchParameter Downtime { get; set; }

        /// <summary>
        /// <para type="description">Limits results to the specified number of items within the specified time period.</para> 
        /// </summary>
        [Parameter(Mandatory = false)]
        public int? Count { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSensorHistory"/> class.
        /// </summary>
        public GetSensorHistory()
        {
            ((IStreamableCmdlet<GetSensorHistory, SensorHistoryRecord, SensorHistoryParameters>)this).StreamProvider = new StreamableCmdletProvider<GetSensorHistory, SensorHistoryRecord, SensorHistoryParameters>(this, true);

            TypeDescription = "Sensor History";
            OperationTypeDescription = "sensor history results";
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == ParameterSet.Default)
                Id = Sensor.Id;

            IEnumerable<PSObject> records;

            var average = Average;

            if (average == null)
            {
                //Sensor is implicitly not null because Downtime is part of the Default parameter set
                average = Downtime ? Convert.ToInt32(Sensor.Interval.TotalSeconds) : 0;
            }
            else
            {
                if (Downtime && average == 0)
                    throw new InvalidOperationException($"Cannot retrieve downtime with an {nameof(Average)} of 0.");
            }

            var parameters = new SensorHistoryParameters(Id, average.Value, StartDate, EndDate, Count);

            if (EndDate == null)
            {
                StreamProvider.StreamResults = false;

                if (Count != null)
                {
                    AdjustCountPeriod(parameters, average.Value);
                }
            }

            if (EndDate != null)
            {
                StreamProvider.ForceStream = true;
                records = StreamProvider.StreamResultsWithProgress(parameters, Count, () => GetFormattedRecords(parameters));

                if (Count != null)
                    records = records.Take(Count.Value);
            }
            else if (ProgressManager.GetRecordsWithVariableProgress)
                records = GetResultsWithVariableProgress(() => GetFormattedRecords(parameters));
            else if (ProgressManager.GetResultsWithProgress)
                records = GetResultsWithProgress(() => GetFormattedRecords(parameters));
            else
                records = GetFormattedRecords(parameters);

            WriteList(records);
        }

        private void AdjustCountPeriod(SensorHistoryParameters parameters, int average)
        {
            //A count was specified without specifying an EndDate. We want to ensure the Start/End date range that
            //is specified is capable of meeting the specified amount.
            var sensor = Sensor;

            if (ParameterSetName == ParameterSet.Manual && average == 0)
                sensor = client.GetSensor(Id);

            //If we're using the default average, get the total TimeSpan (in seconds) we need records for
            var requiredSeconds = (average == 0 ? sensor.Interval.TotalSeconds : average) * Count.Value;

            //An additional hour for good measure. Sometimes we can't scan according to our specified interval
            var requiredTimeSpan = TimeSpan.FromSeconds(requiredSeconds + 3600);

            var requiredEnd = parameters.StartDate - requiredTimeSpan;

            parameters.StartDate = parameters.StartDate;
            parameters.EndDate = requiredEnd;
        }

        private IEnumerable<PSObject> GetFormattedRecords(SensorHistoryParameters parameters)
        {
            IEnumerable<SensorHistoryRecord> records;

            if (EndDate == null)
                records = client.GetSensorHistoryInternal(parameters).Item1;
            else
            {
                records = StreamProvider.StreamRecords<SensorHistoryRecord>(parameters, null);
            }

            var formatter = new SensorHistoryFormatter(this);

            return formatter.Format(records, EndDate != null, Count);
        }

        /// <summary>
        /// Retrieves all records of a specified type from a PRTG Server. Implementors can call different methods of a <see cref="PrtgClient"/> based on the type they wish to retrieve.
        /// </summary>
        /// <returns>A list of records relevant to the caller.</returns>
        [ExcludeFromCodeCoverage]
        protected override IEnumerable<PSObject> GetRecords()
        {
            throw new NotSupportedException();
        }

        #region IStreamableCmdlet

        Tuple<List<SensorHistoryRecord>, int> IStreamableCmdlet<GetSensorHistory, SensorHistoryRecord, SensorHistoryParameters>.GetStreamObjects(SensorHistoryParameters parameters) =>
            client.GetSensorHistoryInternal(parameters);

        async Task<List<SensorHistoryRecord>> IStreamableCmdlet<GetSensorHistory, SensorHistoryRecord, SensorHistoryParameters>.GetStreamObjectsAsync(SensorHistoryParameters parameters) =>
            await client.GetSensorHistoryInternalAsync(parameters, CancellationToken.None).ConfigureAwait(false);

        int IStreamableCmdlet<GetSensorHistory, SensorHistoryRecord, SensorHistoryParameters>.GetStreamTotalObjects(SensorHistoryParameters parameters) =>
            client.GetSensorHistoryTotals(parameters);

        StreamableCmdletProvider<GetSensorHistory, SensorHistoryRecord, SensorHistoryParameters> IStreamableCmdlet<GetSensorHistory, SensorHistoryRecord, SensorHistoryParameters>.StreamProvider { get; set; }

        private StreamableCmdletProvider<GetSensorHistory, SensorHistoryRecord, SensorHistoryParameters> StreamProvider => (
            (IStreamableCmdlet<GetSensorHistory, SensorHistoryRecord, SensorHistoryParameters>)this).StreamProvider;

        #endregion
    }
}
