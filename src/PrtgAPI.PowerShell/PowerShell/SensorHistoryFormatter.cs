using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Cmdlets;

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
                    var unitValue = firstResponse
                        .SelectMany(r => r.ChannelRecords
                            .Where(v => v.Name == channel.Name && !string.IsNullOrEmpty(v.DisplayValue))
                            .Select(v2 => v2.DisplayValue)
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
            var key = string.Join("", channels);

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
                if (channelUnitMap[channel.Name] != null)
                {
                    var value = GetChannelValue(channel);

                    obj.Properties.Add(new PSNoteProperty(channel.Name, value));
                }
                else
                {
                    obj.Properties.Add(new PSNoteProperty(channel.Name, channel.DisplayValue));
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

                value = Convert.ToDouble(valueStr);
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
