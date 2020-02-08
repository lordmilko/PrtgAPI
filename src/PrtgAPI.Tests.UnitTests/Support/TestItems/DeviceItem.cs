using System.Diagnostics;

namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    [DebuggerDisplay("Name: {Name,nq}, Id: {ObjId,nq}, ParentId: {ParentId,nq}")]
    public class DeviceItem : BaseItem
    {
        public string Group { get; set; }
        public string Probe { get; set; }
        public string Condition { get; set; }
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
        public string Location { get; set; }
        public string LocationRaw { get; set; }
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
        public string Favorite { get; set; }
        public string FavoriteRaw { get; set; }
        public string Status { get; set; }
        public string StatusRaw { get; set; }
        public string Priority { get; set; }
        public string Message { get; set; }
        public string MessageRaw { get; set; }
        public string Type { get; set; }
        public string TypeRaw { get; set; }
        public string Active { get; set; }
        public string ActiveRaw { get; set; }
        public string Name { get; set; }
        public string Comments { get; set; }
        public string Host { get; set; }
        public string Tags { get; set; }
        public string Position { get; set; }
        public string PositionRaw { get; set; }

        internal DeviceItem(
            string group = "127.0.0.1", string probe = "127.0.0.1", string condition = "Auto-Discovery in progress (2%)",
            string upsens = "<a title=\"Probe Device: 4x Up\" class=\"sensorlinkgreen\" href=\"sensors.htm?id=40&filter_status=2&filter_status=3\"><div class=\"sensg\">4</div></a>",
            string upsensRaw = "0000000004",
            string downsens = "<a title=\"dc1: 2x Down\" class=\"sensorlinkred\" href=\"sensors.htm? id = 2193&filter_status=5\"><div class=\"sensr\">2</div></a>",
            string downsensRaw = "0000000002",
            string downacksens = "<a title=\"dc1: 1x Down (Acknowledged)\" class=\"sensorlinkack\" href=\"sensors.htm?id=2193&filter_status=13\"><div class=\"senso\">1</div></a>",
            string downacksensRaw = "0000000001",
            string partialdownsens = "<a title=\"Cluster Probe Device: 1x Down (Partial)\" class=\"sensorlinkpartialred\" href=\"sensors.htm?id=41&filter_status=14\"><div class=\"sensq\">1</div></a>",
            string partialdownsensRaw = "0000000001",
            string warnsens = "<a title=\"Probe Device: 1x Warning\" class=\"sensorlinkwarn\" href=\"sensors.htm?id=40&filter_status=4\"><div class=\"sensy\">1</div></a>",
            string warnsensRaw = "0000000001",
            string pausedsens = "<a title=\"dc1: 2x Paused\" class=\"sensorlinkpaused\" href=\"sensors.htm?id=2193&filter_status=7&filter_status=8&filter_status=9&filter_status=11&filter_status=12\"><div class=\"sensb\">2</div></a>",
            string pausedsensRaw = "0000000002",
            string unusualsens = "<a title=\"Probe Device: 1x Unusual\" class=\"sensorlinkunusual\" href=\"sensors.htm?id=40&filter_status=10\"><div class=\"sensp\">1</div></a>",
            string unusualsensRaw = "0000000001",
            string undefinedsens = "<a title=\"dc1: 3x Unknown\" class=\"sensorlinkblack\" href=\"sensors.htm?id=2469&filter_status=0&filter_status=6&filter_status=1\"><div class=\"sensx\">3</div></a>",
            string undefinedsensRaw = "0000000003", string totalsens = "6", string totalsensRaw = "0000000006",
            string location = "<a href=\"/devices.htm?filter_location=@sub(America%2C%20Queens)\">America, Queens</a>",
            string locationRaw = "America, Queens", string schedule = "Sundays [GMT+1000]", string basetype = "device", string baselink = "/device.htm?id=40", string baselinkRaw = "40",
            string parentId = "1", string notifiesx = "Inherited", string interval = null, string intervalx = "Inherited (60)", string intervalxRaw = "0000000060", string access = "Full",
            string accessRaw = "0000000400", string dependency = "Parent", string dependencyRaw = "127.0.0.1",
            string favorite = "<span class=\"objectisnotfavorite icon-gray ui-icon ui-icon-flag\" id=\"fav-40\" onclick=\"_Prtg.objectTools.faveObject.call(this,40,'toggle');return false;\"></span>",
            string favoriteRaw = "1", string status = "Up ", string statusRaw = "3", string priority = "5", string message = "<div class=\"status\">OK<div class=\"moreicon\"></div></div>",
            string messageRaw = "OK", string type = "Device", string typeRaw = "device", string active = "True", string activeRaw = "-1", string objid = "40", string name = "Probe Device",
            string comments = "System Analysis: Windows Other names: dc1.contoso.com System Info: Manufacturer: VMware, Inc. Model: VMware Virtual Platform System: x64-based PC",
            string host = "127.0.0.1", string tags = "C_OS_WIN", string position = "10", string positionRaw = "0000000010"
        )
        {
            Group = group;
            Probe = probe;
            Condition = condition;
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
            Location = location;
            LocationRaw = locationRaw;
            Schedule = schedule;
            BaseType = basetype;
            BaseLink = baselink;
            BaseLinkRaw = baselinkRaw;
            ParentId = parentId;
            NotifiesX = notifiesx;
            Interval = interval;
            IntervalX = intervalx;
            IntervalXRaw = intervalxRaw;
            Access = access;
            AccessRaw = accessRaw;
            Dependency = dependency;
            DependencyRaw = dependencyRaw;
            Favorite = favorite;
            FavoriteRaw = favoriteRaw;
            Status = status;
            StatusRaw = statusRaw;
            Priority = priority;
            Message = message;
            MessageRaw = messageRaw;
            Type = type;
            TypeRaw = typeRaw;
            Active = active;
            ActiveRaw = activeRaw;
            ObjId = objid;
            Name = name;
            Comments = comments;
            Host = host;
            Tags = tags;
            Position = position;
            PositionRaw = positionRaw;
        }
    }
}
