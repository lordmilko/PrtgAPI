using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PrtgAPI.Parameters;

namespace PrtgAPI.Request
{
    internal partial class VersionClient18_1 : VersionClient
    {
        internal VersionClient18_1(PrtgClient client) : base(RequestVersion.v18_1, client)
        {
        }

        /// <summary>
        /// Group the channels we're modifying together based the value and field that contains an existing limit value.
        /// </summary>
        /// <param name="channels">The list of channels to group.</param>
        /// <param name="channelId">The ID of all of the channels.</param>
        /// <param name="parameters">The parameters to add.</param>
        /// <returns></returns>
        private List<Tuple<double?, List<Channel>, ChannelProperty>> GetGroupedChannels(List<Channel> channels, int channelId, ChannelParameter[] parameters)
        {
            var analyzer = new ChannelLimitAnalyzer(new List<Channel>(channels));

            var list = new List<Tuple<double?, List<Channel>, ChannelProperty>>();

            while (analyzer.Channels.Count > 0)
            {
                var record = analyzer.GetProperty();

                if (record.Item2 == null)
                    break;

                list.Add(record);

                foreach (var r in record.Item2)
                    analyzer.Channels.Remove(r);
            }

            if (analyzer.Channels.Count > 0)
            {
                var sensorIdBuilder = new StringBuilder();

                for(var i = 0; i < analyzer.Channels.Count; i++)
                {
                    sensorIdBuilder.Append(analyzer.Channels[i].SensorId);

                    if (i < analyzer.Channels.Count - 2)
                        sensorIdBuilder.Append(", ");
                    else if(i == analyzer.Channels.Count - 2)
                        sensorIdBuilder.Append(" and ");
                }

                var sensorIds = sensorIdBuilder.ToString();

                var builder = new StringBuilder();

                var propertyNames = string.Join(", ", parameters.Select(p => $"'{p.Property}'"));
                var values = string.Join(", ", parameters.Select(p => $"'{p.Value}'"));

                if(parameters.Length > 1)
                    builder.Append($"Cannot set properties {propertyNames} to values {values} for Channel ID {channelId}: ");
                else
                    builder.Append($"Cannot set property {propertyNames} to value {values} for Channel ID {channelId}: ");
                    builder.Append($"Cannot set property {propertyNames} to value {values} for Channel ID {channelId}: ");

                if (analyzer.Channels.Count > 1)
                    builder.Append($"Sensor IDs {sensorIds} do not have a limit value defined on them. ");
                else
                    builder.Append($"Sensor ID {sensorIds} does not have a limit value defined on it. ");

                builder.Append($"Please set one of '{nameof(ChannelProperty.UpperErrorLimit)}', '{nameof(ChannelProperty.LowerErrorLimit)}', '{nameof(ChannelProperty.UpperWarningLimit)}' or '{nameof(ChannelProperty.LowerWarningLimit)}' first and then try again.");
                throw new InvalidOperationException(builder.ToString());
            }

            return list;
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
