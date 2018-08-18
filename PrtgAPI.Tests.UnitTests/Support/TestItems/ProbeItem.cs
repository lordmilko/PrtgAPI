using System.Diagnostics;

namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    [DebuggerDisplay("Name: {Name,nq}, Id: {ObjId,nq}, ParentId: {ParentId,nq}")]
    public class ProbeItem : BaseItem
    {
        public string Fold { get; set; }
        public string FoldRaw { get; set; }
        public string Groupnum { get; set; }
        public string GroupnumRaw { get; set; }
        public string Devicenum { get; set; }
        public string DevicenumRaw { get; set; }
        public string Condition { get; set; }
        public string ConditionRaw { get; set; }
        public string UpSens { get; set; }
        public string UpSensRaw { get; set; }
        public string DownSens { get; set; }
        public string DownSensRaw { get; set; }
        public string DownAckSens { get; set; }
        public string DownAckSensRaw { get; set; }
        public string PartialDownSens { get; set; }
        public string PartialDownSensRaw { get; set; }
        public string WarnSens { get; set; }
        public string WarnSensRaw { get; set; }
        public string PausedSens { get; set; }
        public string PausedSensRaw { get; set; }
        public string UnusualSens { get; set; }
        public string UnusualSensRaw { get; set; }
        public string UndefinedSens { get; set; }
        public string UndefinedSensRaw { get; set; }
        public string TotalSens { get; set; }
        public string TotalSensRaw { get; set; }
        public string Schedule { get; set; }
        public string BaseType { get; set; }
        public string BaseLink { get; set; }
        public string BaseLinkRaw { get; set; }
        public string ParentId { get; set; }
        public string NotifiesX { get; set; }
        public string Interval { get; set; }
        public string IntervalX { get; set; }
        public string IntervalXRaw { get; set; }
        public string Access { get; set; }
        public string AccessRaw { get; set; }
        public string Dependency { get; set; }
        public string DependencyRaw { get; set; }
        public string Status { get; set; }
        public string StatusRaw { get; set; }
        public string Priority { get; set; }
        public string Message { get; set; }
        public string MessageRaw { get; set; }
        public string Type { get; set; }
        public string TypeRaw { get; set; }
        public string Tags { get; set; }
        public string Active { get; set; }
        public string ActiveRaw { get; set; }
        public string Comments { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string PositionRaw { get; set; }

        internal ProbeItem(string fold = "True", string foldRaw = "1", string groupnum = "2", string groupnumRaw = "0000000002",
            string devicenum = "3", string devicenumRaw = "0000000003", string condition = "Connected", string conditionRaw = "2",
            string upsens = "&lt;a title=&quot;127.0.0.1: 33x Up&quot; class=&quot;sensorlinkgreen&quot; href=&quot;sensors.htm?id=1&amp;filter_status=2&amp;filter_status=3&quot;&gt;&lt;div class=&quot;sensg&quot;&gt;33&lt;/div&gt;&lt;/a&gt;",
            string upsensRaw = "0000000033",
            string downsens = "&lt;a title=&quot;127.0.0.1: 3x Down&quot; class=&quot;sensorlinkred&quot; href=&quot;sensors.htm?id=1&amp;filter_status=5&quot;&gt;&lt;div class=&quot;sensr&quot;&gt;3&lt;/div&gt;&lt;/a&gt;",
            string downsensRaw = "0000000003",
            string downacksens = "&lt;a title=&quot;127.0.0.1: 1x Down (Acknowledged)&quot; class=&quot;sensorlinkack&quot; href=&quot;sensors.htm?id=1&amp;filter_status=13&quot;&gt;&lt;div class=&quot;senso&quot;&gt;1&lt;/div&gt;&lt;/a&gt;",
            string downacksensRaw = "0000000001",
            string partialdownsens = "<a title=\"Cluster Probe Device: 1x Down (Partial)\" class=\"sensorlinkpartialred\" href=\"sensors.htm?id=41&filter_status=14\"><div class=\"sensq\">1</div></a>",
            string partialdownsensRaw = "0000000001",
            string warnsens = "<a title=\"Probe Device: 1x Warning\" class=\"sensorlinkwarn\" href=\"sensors.htm?id=40&filter_status=4\"><div class=\"sensy\">1</div></a>",
            string warnsensRaw = "0000000001",
            string pausedsens = "&lt;a title=&quot;127.0.0.1: 10x Paused&quot; class=&quot;sensorlinkpaused&quot; href=&quot;sensors.htm?id=1&amp;filter_status=7&amp;filter_status=8&amp;filter_status=9&amp;filter_status=11&amp;filter_status=12&quot;&gt;&lt;div class=&quot;sensb&quot;&gt;10&lt;/div&gt;&lt;/a&gt;",
            string pausedsensRaw = "0000000010",
            string unusualsens = "<a title=\"Probe Device: 1x Unusual\" class=\"sensorlinkunusual\" href=\"sensors.htm?id=40&filter_status=10\"><div class=\"sensp\">1</div></a>",
            string unusualsensRaw = "0000000001",
            string undefinedsens = "&lt;a title=&quot;127.0.0.1: 1x Unknown&quot; class=&quot;sensorlinkblack&quot; href=&quot;sensors.htm?id=1&amp;filter_status=0&amp;filter_status=6&amp;filter_status=1&quot;&gt;&lt;div class=&quot;sensx&quot;&gt;1&lt;/div&gt;&lt;/a&gt;",
            string undefinedsensRaw = "0000000001", string totalsens = "48", string totalsensRaw = "0000000048", string schedule = "Sundays [GMT+1000]",
            string basetype = "probe", string baselink = "/probenode.htm?id=1", string baselinkRaw = "1", string parentid = "0",
            string notifiesx = "Inherited 1 State 1 Speed 1 Volume 1 Threshold 1 Change", string interval = null, string intervalx = "Inherited (60)",
            string intervalxRaw = "0000000060", string access = "Full", string accessRaw = "0000000400", string dependency = "Parent", string dependencyRaw = "Root",
            string status = "Up ", string statusRaw = "3", string priority = "3",
            string message = "&lt;div class=&quot;status&quot;&gt;OK&lt;div class=&quot;moreicon&quot;&gt;&lt;/div&gt;&lt;/div&gt;",
            string messageRaw = "OK", string type = "Probe", string typeRaw = "probenode", string tags = "Office_Probe", string active = "True", string activeRaw = "-1",
            string comments = "HP DL120 in Level 3 Storage Room", string objid = "1", string name = "127.0.0.1", string position = "20", string positionRaw = "0000000020")
        {
            Fold = fold;
            FoldRaw = foldRaw;
            Groupnum = groupnum;
            GroupnumRaw = groupnumRaw;
            Devicenum = devicenum;
            DevicenumRaw = devicenumRaw;
            Condition = condition;
            ConditionRaw = conditionRaw;
            UpSens = upsens;
            UpSensRaw = upsensRaw;
            DownSens = downsens;
            DownSensRaw = downsensRaw;
            DownAckSens = downacksens;
            DownAckSensRaw = downacksensRaw;
            PartialDownSens = partialdownsens;
            PartialDownSensRaw = partialdownsensRaw;
            WarnSens = warnsens;
            WarnSensRaw = warnsensRaw;
            PausedSens = pausedsens;
            PausedSensRaw = pausedsensRaw;
            UnusualSens = unusualsens;
            UnusualSensRaw = unusualsensRaw;
            UndefinedSens = undefinedsens;
            UndefinedSensRaw = undefinedsensRaw;
            TotalSens = totalsens;
            TotalSensRaw = totalsensRaw;
            Schedule = schedule;
            BaseType = basetype;
            BaseLink = baselink;
            BaseLinkRaw = baselinkRaw;
            ParentId = parentid;
            NotifiesX = notifiesx;
            Interval = interval;
            IntervalX = intervalx;
            IntervalXRaw = intervalxRaw;
            Access = access;
            AccessRaw = accessRaw;
            Dependency = dependency;
            DependencyRaw = dependencyRaw;
            Status = status;
            StatusRaw = statusRaw;
            Priority = priority;
            Message = message;
            MessageRaw = messageRaw;
            Type = type;
            TypeRaw = typeRaw;
            Tags = tags;
            Active = active;
            ActiveRaw = activeRaw;
            Comments = comments;
            ObjId = objid;
            Name = name;
            Position = position;
            PositionRaw = positionRaw;
        }
    }
}
