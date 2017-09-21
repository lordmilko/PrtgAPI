using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "SensorHistory")]
    public class GetSensorHistory : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "Default", ValueFromPipeline = true)]
        public Sensor Sensor { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Manual")]
        public int Id { get; set; }

        [Parameter(Mandatory = false)]
        public DateTime? StartDate { get; set; }

        [Parameter(Mandatory = false)]
        public DateTime? EndDate { get; set; }

        [Parameter(Mandatory = false)]
        public int Average { get; set; }

        private static Dictionary<string, TypeNameRecord> typeNameMap = new Dictionary<string, TypeNameRecord>();
        private static int count;

        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == "Default")
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
