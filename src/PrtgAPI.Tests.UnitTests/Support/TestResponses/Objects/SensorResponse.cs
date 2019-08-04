using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class SensorResponse : BaseResponse<SensorItem>
    {
        internal SensorResponse(params SensorItem[] sensors) : base("sensors", sensors)
        {
        }

        public override XElement GetItem(SensorItem item)
        {
            var xml = new XElement("item",
                new XElement("probe", item.Probe),
                new XElement("group", item.Group),
                new XElement("lastvalue", item.LastValue),
                new XElement("lastvalue_raw", item.LastValueRaw),
                new XElement("device", item.Device),
                new XElement("downtime", item.Downtime),
                new XElement("downtime_raw", item.DowntimeRaw),
                new XElement("downtimetime", item.DowntimeTime),
                new XElement("downtimetime_raw", item.DowntimeTimeRaw),
                new XElement("downtimesince", item.DowntimeSince),
                new XElement("downtimesince_raw", item.DowntimeSinceRaw),
                new XElement("uptime", item.Uptime),
                new XElement("uptime_raw", item.UptimeRaw),
                new XElement("uptimetime", item.UptimeTime),
                new XElement("uptimetime_raw", item.UptimeTimeRaw),
                new XElement("uptimesince", item.UptimeSince),
                new XElement("uptimesince_raw", item.UptimeSinceRaw),
                new XElement("knowntime", item.Knowntime),
                new XElement("knowntime_raw", item.KnowntimeRaw),
                new XElement("cumsince", item.CumSince),
                new XElement("cumsince_raw", item.CumSinceRaw),
                new XElement("lastcheck", item.LastCheck),
                new XElement("lastcheck_raw", item.LastCheckRaw),
                new XElement("lastup", item.LastUp),
                new XElement("lastup_raw", item.LastUpRaw),
                new XElement("lastdown", item.LastDown),
                new XElement("lastdown_raw", item.LastDownRaw),
                new XElement("minigraph", item.Minigraph),
                new XElement("schedule", item.Schedule),
                new XElement("basetype", item.BaseType),
                new XElement("baselink", item.BaseLink),
                new XElement("baselink_raw", item.BaseLinkRaw),
                new XElement("parentid", item.ParentId),
                new XElement("notifiesx", item.NotifiesX),
                new XElement("interval", item.Interval),
                new XElement("interval_raw", item.IntervalRaw),
                new XElement("intervalx", item.IntervalX),
                new XElement("intervalx_raw", item.IntervalXRaw),
                new XElement("access", item.Access),
                new XElement("access_raw", item.AccessRaw),
                new XElement("dependency", item.Dependency),
                new XElement("dependency_raw", item.DependencyRaw),
                new XElement("favorite", item.Favorite),
                new XElement("favorite_raw", item.FavoriteRaw),
                new XElement("status", item.Status),
                new XElement("status_raw", item.StatusRaw),
                new XElement("priority", item.Priority),
                new XElement("message", item.Message),
                new XElement("message_raw", item.MessageRaw),
                new XElement("type", item.Type),
                new XElement("type_raw", item.TypeRaw),
                new XElement("tags", item.Tags),
                new XElement("active", item.Active),
                new XElement("active_raw", item.ActiveRaw),
                new XElement("objid", item.ObjId),
                new XElement("name", item.Name),
                new XElement("comments", item.Comments),
                new XElement("position", item.Position),
                new XElement("position_raw", item.PositionRaw)
            );

            return xml;
        }
    }
}
