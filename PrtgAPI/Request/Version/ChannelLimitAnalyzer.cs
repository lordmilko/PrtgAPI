using System;
using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.Request
{
    class ChannelLimitAnalyzer
    {
        public List<Channel> Channels { get; set; }

        public ChannelLimitAnalyzer(List<Channel> channels)
        {
            Channels = channels;
        }

        public ChannelLimitAnalysis UpperErrorLimit => GetHighest(c => c.UpperErrorLimit, ChannelProperty.UpperErrorLimit);
        public ChannelLimitAnalysis LowerErrorLimit => GetHighest(c => c.LowerErrorLimit, ChannelProperty.LowerErrorLimit);
        public ChannelLimitAnalysis UpperWarningLimit => GetHighest(c => c.UpperWarningLimit, ChannelProperty.UpperWarningLimit);
        public ChannelLimitAnalysis LowerWarningLimit => GetHighest(c => c.LowerWarningLimit, ChannelProperty.LowerWarningLimit);

        public ChannelLimitAnalysis GetProperty()
        {
            //Identify the limit property that has the highest incidence of channels sharing its value. e.g. There could be
            //two channels that share the same UpperErrorLimit, but there could be three that share the same LowerErrorLimit!

            var highest = UpperErrorLimit;

            Func<ChannelLimitAnalysis, bool> isHigher = p => p.Channels != null && (highest.Channels == null || p.Channels.Count > highest.Channels.Count);

            if (isHigher(LowerErrorLimit))
                highest = LowerErrorLimit;

            if (isHigher(UpperWarningLimit))
                highest = UpperWarningLimit;

            if (isHigher(LowerWarningLimit))
                highest = LowerWarningLimit;

            return highest;
        }

        private ChannelLimitAnalysis GetHighest(Func<Channel, double?> selector, ChannelProperty property)
        {
            var group = Channels
                .GroupBy(selector)                 //e.g. group by UpperErrorLimit
                .Where(g => g.Key != null)         //excluding those that don't have an UpperErrorLimit
                .OrderByDescending(g => g.Count()) //select the group of sensors that have the highest number of sensors with any particular UpperErrorLimit
                .FirstOrDefault();

            //Associate ChannelProperty.UpperErrorLimit with the group of channels that have the highest incidence of any particular UpperErrorLimit
            return new ChannelLimitAnalysis(group?.Key, group?.ToList(), property);
        }
    }
}
