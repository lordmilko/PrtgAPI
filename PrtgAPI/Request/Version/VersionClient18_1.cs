using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PrtgAPI.Parameters;

namespace PrtgAPI.Request
{
    internal partial class VersionClient18_1 : VersionClient17_4
    {
        internal VersionClient18_1(PrtgClient client) : base(RequestVersion.v18_1, client)
        {
        }

        /// <summary>
        /// Group the channels we're modifying together based the value and field that contains an existing limit value.
        /// </summary>
        /// <param name="channels">The list of channels to group.</param>
        /// <param name="parameters">The parameters to add.</param>
        /// <returns></returns>
        private List<ChannelLimitAnalysis> GetGroupedLimitChannels(ICollection<Channel> channels, ChannelParameter[] parameters)
        {
            Debug.Assert(channels.Select(c => c.Id).Distinct().Count() == 1, $"Channels containing more than ID were passed to {nameof(GetGroupedLimitChannels)}");

            var analyzer = new ChannelLimitAnalyzer(new List<Channel>(channels));

            var list = new List<ChannelLimitAnalysis>();

            //Try and get the limit property that has the most amount of channels associated with it out of all remaining channels.
            //As we remove channels from the analyzer, each property will update the channels that should be associated with it,
            //eventually leaving us with the standalone channels that did not share any of their limit property values with any
            //other channel.
            while (analyzer.Channels.Count > 0)
            {
                var record = analyzer.GetProperty();

                if (record.Channels == null)
                    break;

                list.Add(record);

                foreach (var r in record.Channels)
                    analyzer.Channels.Remove(r);
            }

            ThrowIfAnalyzerContainsRemainingChannels(analyzer, channels, parameters);

            return list;
        }

        protected override bool NeedChannels(SetChannelPropertyParameters parameters, ChannelParameter[] channelParameters)
        {
            return base.NeedChannels(parameters, channelParameters) || NeedsLimit(channelParameters);
        }

        private void ThrowIfAnalyzerContainsRemainingChannels(ChannelLimitAnalyzer analyzer, ICollection<Channel> channels, ChannelParameter[] parameters)
        {
            //Ideally, we should have now removed all channels from the analyzer, having identified at least one limit property
            //on each channel that has an existing value. Since NeedsLimit() would have prevented us from going down this path if
            //the first limit of a channel were being specified in this request, we can safely conclude we need to throw an error
            //that at least one limit property must be included when trying to specify properties like Error/WarningLimitMessage, etc.

            if (analyzer.Channels.Count > 0)
            {
                var sensorIdBuilder = new StringBuilder();

                for (var i = 0; i < analyzer.Channels.Count; i++)
                {
                    sensorIdBuilder.Append(analyzer.Channels[i].SensorId);

                    if (i < analyzer.Channels.Count - 2)
                        sensorIdBuilder.Append(", ");
                    else if (i == analyzer.Channels.Count - 2)
                        sensorIdBuilder.Append(" and ");
                }

                var sensorIds = sensorIdBuilder.ToString();

                var builder = new StringBuilder();

                var propertyNames = string.Join(", ", parameters.Select(p => $"'{p.Property}'"));
                var values = string.Join(", ", parameters.Select(p => $"'{p.Value ?? ("null")}'"));

                var channelId = channels.First().Id;

                if (parameters.Length > 1)
                    builder.Append($"Cannot set properties {propertyNames} to values {values} for Channel ID {channelId}: ");
                else
                    builder.Append($"Cannot set property {propertyNames} to value {values} for Channel ID {channelId}: ");

                if (analyzer.Channels.Count > 1)
                    builder.Append($"Sensor IDs {sensorIds} do not have a limit value defined on them. ");
                else
                    builder.Append($"Sensor ID {sensorIds} does not have a limit value defined on it. ");

                builder.Append($"Please set one of '{nameof(ChannelProperty.UpperErrorLimit)}', '{nameof(ChannelProperty.LowerErrorLimit)}', '{nameof(ChannelProperty.UpperWarningLimit)}' or '{nameof(ChannelProperty.LowerWarningLimit)}' first and then try again.");

                throw new InvalidOperationException(builder.ToString());
            }
        }

        protected override List<SetChannelPropertyTask> GetSetChannelPropertyTasks(List<Channel> channels, ChannelParameter[] channelParameters)
        {
            if (NeedsLimit(channelParameters))
            {
                var groups = GetGroupedLimitChannels(channels, channelParameters);

                var tasks = groups.Select(g =>
                {
                    var p = channelParameters.ToList();
                    p.Add(new ChannelParameter(g.Property, g.LimitValue));

                    return new SetChannelPropertyTask(g.Channels, p.ToArray());
                }).ToList();

                return tasks;
            }
            else
                return base.GetSetChannelPropertyTasks(channels, channelParameters);
        }

        private bool NeedsLimit(ChannelParameter[] @params)
        {
            var needsLimit = @params.Any(p => p.Property == ChannelProperty.ErrorLimitMessage || p.Property == ChannelProperty.WarningLimitMessage || (p.Property == ChannelProperty.LimitsEnabled && IsTrue(p.Value)));

            if (needsLimit)
            {
                if (@params.Any(p => p.Property == ChannelProperty.UpperErrorLimit || p.Property == ChannelProperty.LowerErrorLimit || p.Property == ChannelProperty.UpperWarningLimit || p.Property == ChannelProperty.LowerWarningLimit))
                    return false;

                return true;
            }

            return false;
        }

        private bool IsTrue(object val)
        {
            if (val == null)
                return false;

            if (val is bool)
                return (bool)val;

            bool b;

            if (bool.TryParse(val.ToString(), out b))
                return b;

            return false;
        }
    }
}
