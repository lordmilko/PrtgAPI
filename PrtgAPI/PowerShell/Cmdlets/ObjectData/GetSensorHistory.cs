using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves historic data values for a PRTG Sensor.</para>
    /// 
    /// <para type="description">The Get-SensorHistory cmdlet retrieves historic data values for all channels of a PRTG Sensor within a specified time period.
    /// By default, values are returned according to the scanning interval defined on the sensor (e.g. values every 60 seconds). Using the -Average parameter,
    /// values can be averaged together to provide a higher level view of a larger time span. Any number of seconds can be specified as the Average,
    /// however note that depending on the interval of the sensor, certain time spans may result in blank values within the sensor results.</para>
    /// <para type="description">Historic data values can be retrieved over any time period via the StartDate and EndDate parameters. If these values
    /// are not overwritten, they will default to 24 hours ago and Now (the time the cmdlet was executed) respectively. If PRTG finds it does not have
    /// enough monitoring data to return for the specified time span, an exception will be thrown. To work around this, you can request date for a larger
    /// time frame and then potentially filter the data with the Where-Object and Select-Object cmdlets.</para>
    /// <para type="description">PrtgAPI will automatically display all numeric channel values as numbers, with the unit of the channel
    /// displayed in brackets next to the channel header (e.g. "Total(%)"). These units are for display purposes only, and so can be ignored
    /// when attempting to extract certain columns from the sensor history result.</para>
    /// 
    /// <example>
    ///     <code>Get-Sensor -Id 1001 | Get-SensorHistory</code>
    ///     <para>Get historical values for all channels on the sensor with ID 1001 over the past 24 hours, using the sensor's default interval.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>Get-Sensor -Id 1001 | Get-SensorHistory -Average 300</code>
    ///     <para>Get historical values for all channels on the sensor with ID 1001 over the past 24 hours, averaging values in 5 minute intervals.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>Get-Sensor -Id 1001 | Get-SensorHistory -StartDate (Get-Date).AddDays(-3) -EndDate (Get-Date).AddDays(-1)</code>
    ///     <para>Get historical values for all channels on the sensor with ID 1001 starting three days ago and ending yesterday.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>Get-Sensor -Tags wmicpu* -count 1 | Get-SensorHistory | select Total</code>
    ///     <para>Get historical values for all channels on a single WMI CPU Load sensor, selecting the "Total" channel (visually displayed as "Total(%)"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>Get-Sensor -Tags wmicpu* -count 1 | where Total -gt 90</code>
    ///     <para>Get historical values for all channels on a single WMI CPU Load sensor where the Total channel's value was greater than 90.</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Select-Object</para>
    /// <para type="link">Where-Object</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "SensorHistory")]
    public class GetSensorHistory : PrtgCmdlet
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
        /// <para type="description">Time span (in seconds) to average results over. For example, a value of 300 will show the average value every 5 minutes.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int Average { get; set; }

        private static Dictionary<string, TypeNameRecord> typeNameMap = new Dictionary<string, TypeNameRecord>();
        private static int count;

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == ParameterSet.Default)
                Id = Sensor.Id;

            var response = client.GetSensorHistory(Id, Average, StartDate, EndDate);

            //Determine the channel to use for each column
            var channelUnitMap = GetChannelUnitMap(response);

            //Get the type name to use for this object
            TypeNameRecord typeNameRecord;
            var isNew = GetOrCreateTypeName(channelUnitMap.Keys.ToList(), out typeNameRecord);

            //Transform the sensor history response into a collection of PSObjects, with values suitable converted to integers
            var list = response.Select(r => CreateObject(r, channelUnitMap, typeNameRecord.TypeName)).ToList();

            var first = list.FirstOrDefault();

            if (first != null)
            {
                var columns = first.Properties.Where(p => p is PSNoteProperty).Select(n => n.Name).ToList();

                var tuples = columns.Select(col => Tuple.Create(col, CreateLabel(col, channelUnitMap))).ToList();

                var coverage = tuples.Last(t => t.Item1 == "Coverage");
                var index = tuples.IndexOf(coverage);
                tuples[index] = Tuple.Create(coverage.Item1, $"{coverage.Item1}(%)");

                if(isNew)
                    FormatGenerator.Generate(typeNameRecord.TypeName, tuples, typeNameRecord.Index);
                
                FormatGenerator.LoadXml(this);
            }

            WriteObject(list, true);
        }

        private string CreateLabel(string column, Dictionary<string, string> channelUnitMap)
        {
            if (channelUnitMap.ContainsKey(column))
            {
                var val = channelUnitMap[column];

                if (val != null)
                {
                    return $"{column}({val})";
                }
            }

            return column;
        }

        private Dictionary<string, string> GetChannelUnitMap(List<SensorHistoryData> response)
        {
            var channelUnitMap = new Dictionary<string, string>();

            var first = response.FirstOrDefault();

            if (first != null)
            {
                foreach (var channel in first.ChannelRecords)
                {
                    var unitValue = response
                        .SelectMany(r => r.ChannelRecords
                            .Where(v => v.Name == channel.Name && !string.IsNullOrEmpty(v.Value))
                            .Select(v2 => v2.Value)
                        ).FirstOrDefault();

                    if (unitValue != null)
                    {
                        if (IsValueLookup(unitValue))
                            unitValue = null;
                        else
                        {
                            if (unitValue.StartsWith("< ") || unitValue.StartsWith("> "))
                            {
                                var firstIndex = unitValue.IndexOf(' ') + 1;
                                var secondIndex = unitValue.IndexOf(' ', firstIndex) + 1;

                                unitValue = unitValue.Substring(secondIndex).Trim();
                            }
                            else
                                unitValue = unitValue.Substring(unitValue.IndexOf(' ') + 1).Trim();
                        }
                    }

                    channelUnitMap[channel.Name] = unitValue;
                }
            }

            return channelUnitMap;
        }

        private PSObject CreateObject(SensorHistoryData date, Dictionary<string, string> channelUnitMap, string typeName)
        {
            var obj = new PSObject();

            obj.Properties.Add(new PSNoteProperty("DateTime", date.DateTime));
            obj.Properties.Add(new PSNoteProperty("SensorId", date.SensorId));

            foreach (var channel in date.ChannelRecords)
            {
                if (channelUnitMap[channel.Name] != null)
                {
                    var value = GetChannelValue(channel);

                    obj.Properties.Add(new PSNoteProperty(channel.Name, value));
                }
                else
                {
                    obj.Properties.Add(new PSNoteProperty(channel.Name, channel.Value));
                }
            }

            obj.Properties.Add(new PSNoteProperty("Coverage", date.Coverage));

            obj.TypeNames.Insert(0, typeName);

            return obj;
        }

        private object GetChannelValue(ChannelHistoryRecord channel)
        {
            double? value = null;

            if (channel.Value != null)
            {
                if (IsValueLookup(channel.Value))
                    return channel.Value;

                var valueStr = channel.Value.Substring(0, channel.Value.IndexOf(' '));

                if (valueStr == "<1")
                    valueStr = "0";
                else if (valueStr == ">99")
                    valueStr = "100";
                else if (valueStr == "<" || valueStr == ">")
                {
                    var first = channel.Value.IndexOf(' ') + 1;
                    var second = channel.Value.IndexOf(' ', first);

                    valueStr = channel.Value.Substring(first, second - first);
                }

                value = Convert.ToDouble(valueStr);
            }

            return value;
        }

        private bool IsValueLookup(string str)
        {
            var ch1 = str[0];

            if (char.IsDigit(ch1))
                return false;

            if (ch1 == '<' || ch1 == '>')
            {
                if (str.Length >= 2)
                {
                    var ch2 = str[1];

                    if (char.IsDigit(ch2))
                        return false;

                    if (ch2 == ' ' && str.Length >= 3)
                    {
                        var ch3 = str[2];

                        if (char.IsDigit(ch3))
                            return false;
                    }
                }
            }

            return true;
        }

        private bool GetOrCreateTypeName(List<string> channels, out TypeNameRecord record)
        {
            var key = string.Join("", channels);

            if (typeNameMap.ContainsKey(key))
            {
                record = typeNameMap[key];
                return false;
            }

            count++;
            record = new TypeNameRecord(count);
            typeNameMap[key] = record;

            return true;
        }
    }
}
