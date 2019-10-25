using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Cmdlets;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell
{
    class SensorHistoryFormatter
    {
        private ChannelUnitMap channelUnitMap;
        private TypeNameRecord typeNameRecord;
        private int readCount;

        private GetSensorHistory cmdlet;

        private bool xmlLoaded = false;

        private static Dictionary<string, TypeNameRecord> typeNameMap = new Dictionary<string, TypeNameRecord>();
        private static int mapCount;

        public SensorHistoryFormatter(GetSensorHistory cmdlet)
        {
            this.cmdlet = cmdlet;
        }

        private PSObject PrepareObject(SensorHistoryRecord data, bool isNew = false)
        {
            var ps = CreateObject(data);

            if (!xmlLoaded)
            {
                LoadFormat(ps, isNew);
                xmlLoaded = true;
            }

            return ps;
        }

        public IEnumerable<PSObject> Format(IEnumerable<SensorHistoryRecord> response, bool lazy, int? count)
        {
            var firstResponse = new List<SensorHistoryRecord>();

            foreach (var record in response)
            {
                if (count != null && readCount > count)
                    break;

                if (readCount < 500 || !lazy)
                {
                    readCount++;
                    firstResponse.Add(record);
                }
                else
                {
                    if (firstResponse != null)
                    {
                        foreach (var obj in Init(firstResponse))
                            yield return obj;

                        firstResponse = null;
                    }

                    yield return PrepareObject(record);
                }
            }

            if (firstResponse != null)
            {
                foreach (var obj in Init(firstResponse))
                    yield return obj;
            }
        }

        private IEnumerable<PSObject> Init(List<SensorHistoryRecord> firstResponse)
        {
            //Determine the channel to use for each column
            GetChannelUnitMap(firstResponse);

            //Get the type name to use for this object
            bool isNew = GetOrCreateTypeName(channelUnitMap.Keys.ToList());

            foreach (var obj in firstResponse)
                yield return PrepareObject(obj, isNew);
        }

        private void GetChannelUnitMap(List<SensorHistoryRecord> firstResponse)
        {
            channelUnitMap = new ChannelUnitMap();

            var first = firstResponse.FirstOrDefault();

            if (first != null)
            {
                foreach (var channel in first.ChannelRecords)
                {
                    if (cmdlet.Raw)
                    {
                        channelUnitMap[channel.Name] = null;
                        continue;
                    }

                    var unitValueChannel = firstResponse
                        .SelectMany(r => r.ChannelRecords
                            .Where(v => v.Name == channel.Name && !string.IsNullOrEmpty(v.DisplayValue))
                        ).FirstOrDefault();

                    var unitValue = unitValueChannel?.DisplayValue;

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
                            {
                                if (unitValue.ToCharArray().Count(c => c == ' ') <= 1 && !IsTimeSpan(unitValueChannel))
                                {
                                    unitValue = unitValue.Substring(unitValue.IndexOf(' ') + 1).Trim();
                                }
                                else
                                    unitValue = null;
                            }
                        }
                    }

                    channelUnitMap[channel.Name] = unitValue;
                }
            }
        }

        private bool IsTimeSpan(ChannelHistoryRecord record)
        {
            if (record.DisplayValue != null)
            {
                var split = record.DisplayValue.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                var units = split.Where((v, i) => i % 2 != 0).ToArray(); //Due to 0 based indexing 0, 2, 4, etc has the number, 1, 3, 5, etc has the unit

                if (units.Length > 0 && units.All(v => v == "d" || v == "h" || v == "m" || v == "s") && split.Length % 2 == 0)
                {
                    var total = 0;

                    for (var i = 0; i < split.Length - 1; i += 2)
                    {
                        int intValue;

                        if (!int.TryParse(split[i], out intValue))
                            return false;

                        var unit = split[i + 1];
                        var unitSeconds = GetSecondsForTimeSpanUnit(unit);

                        total += intValue * unitSeconds;
                    }

                    //With large units (such as days) the display value will almost never be the same as the raw value
                    if (total == record.Value)
                        return true;

                    var largestUnit = GetSecondsForTimeSpanUnit(split[1]);

                    //If we elapse the next hour or day, etc and it turns out we're bigger,
                    //then perhaps we are in fact a TimeSpan. Otherwise, whatever our units mean they don't
                    //relate to TimeSpans
                    if(total < record.Value && total + largestUnit > record.Value)
                        return true;
                }
            }

            return false;
        }

        private int GetSecondsForTimeSpanUnit(string unit)
        {
            switch (unit)
            {
                case "s":
                    return 1;
                case "m":
                    return 60;
                case "h":
                    return 3600;
                case "d":
                    return 86400;
                default:
                    throw new NotImplementedException($"Don't know how to handle unit '{unit}'.");
            }
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

        private bool GetOrCreateTypeName(List<string> channels)
        {
            var key = string.Join(string.Empty, channels);

            if (cmdlet.Raw)
                key += "_raw";

            if (typeNameMap.ContainsKey(key))
            {
                typeNameRecord = typeNameMap[key];

                if (typeNameRecord.Impurity > channelUnitMap.Impurity)
                {
                    typeNameRecord.Impurity = channelUnitMap.Impurity; //We will update the XML of this typeNameRecord
                    return true;
                }
                else
                {
                    return false;
                }
            }

            mapCount++;
            typeNameRecord = new TypeNameRecord(mapCount, channelUnitMap.Impurity);
            typeNameMap[key] = typeNameRecord;

            return true;
        }

        private PSObject CreateObject(SensorHistoryRecord date)
        {
            var obj = new PSObject();

            obj.Properties.Add(new PSNoteProperty("DateTime", date.DateTime));
            obj.Properties.Add(new PSNoteProperty("SensorId", date.SensorId));

            foreach (var channel in date.ChannelRecords)
            {
                if (!cmdlet.Raw && (channelUnitMap[channel.Name] != null || IsTimeSpan(channel)))
                {
                    //Implicitly not Raw if we have a channel unit
                    var value = GetChannelValue(channel);

                    obj.Properties.Add(new PSNoteProperty(channel.Name, value));
                }
                else
                {
                    obj.Properties.Add(new PSNoteProperty(channel.Name, cmdlet.Raw ? channel.Value?.ToString() : channel.DisplayValue));
                }
            }

            obj.Properties.Add(new PSNoteProperty("Coverage", date.Coverage));

            obj.TypeNames.Insert(0, typeNameRecord.TypeName);

            return obj;
        }

        private object GetChannelValue(ChannelHistoryRecord channel)
        {
            double? value = null;

            if (channel.DisplayValue != null)
            {
                if (channel.Value != null && IsTimeSpan(channel))
                    return TimeSpan.FromSeconds(channel.Value.Value);

                if (IsValueLookup(channel.DisplayValue))
                    return channel.DisplayValue;

                var space = channel.DisplayValue.IndexOf(' ');

                var valueStr = channel.DisplayValue;

                if (space > 0)
                    valueStr = channel.DisplayValue.Substring(0, space);

                if (valueStr == "<1")
                    valueStr = "0";
                else if (valueStr == ">99")
                    valueStr = "100";
                else if (valueStr == "<" || valueStr == ">")
                {
                    var first = channel.DisplayValue.IndexOf(' ') + 1;
                    var second = channel.DisplayValue.IndexOf(' ', first);

                    valueStr = channel.DisplayValue.Substring(first, second - first);
                }

                value = ConvertUtilities.ToDouble(valueStr);
            }

            return value;
        }

        private void LoadFormat(PSObject obj, bool isNew)
        {
            var columns = obj.Properties.Where(p => p is PSNoteProperty).Select(n => n.Name).ToList();

            var tuples = columns.Select(col => Tuple.Create(col, CreateLabel(col))).ToList();

            var coverage = tuples.Last(t => t.Item1 == "Coverage");
            var index = tuples.IndexOf(coverage);
            tuples[index] = Tuple.Create(coverage.Item1, $"{coverage.Item1}(%)");

            if (isNew)
                FormatGenerator.Generate(typeNameRecord.TypeName, tuples, typeNameRecord.Index);

            FormatGenerator.LoadXml(cmdlet);
        }

        private string CreateLabel(string column)
        {
            if (channelUnitMap.ContainsKey(column))
            {
                var val = channelUnitMap[column];

                if (!string.IsNullOrEmpty(val))
                {
                    return $"{column}({val})";
                }
            }

            return column;
        }
    }
}
