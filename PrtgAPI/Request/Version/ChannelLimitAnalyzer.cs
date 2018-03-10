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

        public Tuple<double?, List<Channel>, ChannelProperty> UpperErrorLimit => GetHighest(c => c.UpperErrorLimit, ChannelProperty.UpperErrorLimit);
        public Tuple<double?, List<Channel>, ChannelProperty> LowerErrorLimit => GetHighest(c => c.LowerErrorLimit, ChannelProperty.LowerErrorLimit);
        public Tuple<double?, List<Channel>, ChannelProperty> UpperWarningLimit => GetHighest(c => c.UpperWarningLimit, ChannelProperty.UpperWarningLimit);
        public Tuple<double?, List<Channel>, ChannelProperty> LowerWarningLimit => GetHighest(c => c.LowerWarningLimit, ChannelProperty.LowerWarningLimit);

        public Tuple<double?, List<Channel>, ChannelProperty> GetProperty()
        {
            var highest = UpperErrorLimit;

            Func<Tuple<double?, List<Channel>, ChannelProperty>, bool> isHigher = p => p.Item2 != null && (highest.Item2 == null || p.Item2.Count > highest.Item2.Count);

            if (isHigher(LowerErrorLimit))
                highest = LowerErrorLimit;

            if (isHigher(UpperWarningLimit))
                highest = UpperWarningLimit;

            if (isHigher(LowerWarningLimit))
                highest = LowerWarningLimit;

            return highest;
        }

        private Tuple<double?, List<Channel>, ChannelProperty> GetHighest(Func<Channel, double?> selector, ChannelProperty property)
        {
            var group = Channels.GroupBy(selector).Where(g => g.Key != null).OrderByDescending(g => g.Count()).FirstOrDefault();

            return Tuple.Create(group?.Key, group?.ToList(), property);
        }
    }
}
