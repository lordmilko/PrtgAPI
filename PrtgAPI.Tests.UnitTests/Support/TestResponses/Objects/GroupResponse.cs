using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class GroupResponse : BaseResponse<GroupItem>
    {
        public GroupResponse(params GroupItem[] groups) : base("groups", groups)
        {
        }

        public override XElement GetItem(GroupItem item)
        {
            var xml = new XElement("item",
                new XElement("probe", item.Probe),
                new XElement("fold", item.Fold),
                new XElement("fold_raw", item.FoldRaw),
                new XElement("groupnum", item.Groupnum),
                new XElement("groupnum_raw", item.GroupnumRaw),
                new XElement("devicenum", item.Devicenum),
                new XElement("devicenum_raw", item.DevicenumRaw),
                new XElement("condition", item.Condition),
                new XElement("upsens", item.UpSens),
                new XElement("upsens_raw", item.UpSensRaw),
                new XElement("downsens", item.DownSens),
                new XElement("downsens_raw", item.DownSensRaw),
                new XElement("downacksens", item.DownAckSens),
                new XElement("downacksens_raw", item.DownAckSensRaw),
                new XElement("partialdownsens", item.PartialDownSens),
                new XElement("partialdownsens_raw", item.PartialDownSensRaw),
                new XElement("warnsens", item.WarnSens),
                new XElement("warnsens_raw", item.WarnSensRaw),
                new XElement("pausedsens", item.PausedSens),
                new XElement("pausedsens_raw", item.PausedSensRaw),
                new XElement("unusualsens", item.UnusualSens),
                new XElement("unusualsens_raw", item.UnusualSensRaw),
                new XElement("undefinedsens", item.UndefinedSens),
                new XElement("undefinedsens_raw", item.UndefinedSensRaw),
                new XElement("totalsens", item.TotalSens),
                new XElement("totalsens_raw", item.TotalSensRaw),
                new XElement("schedule", item.Schedule),
                new XElement("basetype", item.BaseType),
                new XElement("baselink", item.BaseLink),
                new XElement("baselink_raw", item.BaseLinkRaw),
                new XElement("parentid", item.ParentId),
                new XElement("notifiesx", item.NotifiesX),
                new XElement("interval", item.Interval),
                new XElement("intervalx", item.IntervalX),
                new XElement("intervalx_raw", item.IntervalXRaw),
                new XElement("access", item.Access),
                new XElement("access_raw", item.AccessRaw),
                new XElement("dependency", item.Dependency),
                new XElement("dependency_raw", item.DependencyRaw),
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
                new XElement("comments", item.Comments),
                new XElement("objid", item.ObjId),
                new XElement("name", item.Name),
                new XElement("position", item.Position),
                new XElement("position_raw", item.PositionRaw)
            );

            return xml;
        }
    }
}
