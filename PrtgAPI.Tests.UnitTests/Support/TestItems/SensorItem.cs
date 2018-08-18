using System.Diagnostics;

namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    [DebuggerDisplay("Name: {Name,nq}, Id: {ObjId,nq}, ParentId: {ParentId,nq}")]
    public class SensorItem : BaseItem
    {
        public string Probe { get; set; }
        public string Group { get; set; }
        public string LastValue { get; set; }
        public string LastValueRaw { get; set; }
        public string Device { get; set; }
        public string Downtime { get; set; }
        public string DowntimeRaw { get; set; }
        public string DowntimeTime { get; set; }
        public string DowntimeTimeRaw { get; set; }
        public string DowntimeSince { get; set; }
        public string DowntimeSinceRaw { get; set; }
        public string Uptime { get; set; }
        public string UptimeRaw { get; set; }
        public string UptimeTime { get; set; }
        public string UptimeTimeRaw { get; set; }
        public string UptimeSince { get; set; }
        public string UptimeSinceRaw { get; set; }
        public string Knowntime { get; set; }
        public string KnowntimeRaw { get; set; }
        public string CumSince { get; set; }
        public string CumSinceRaw { get; set; }
        public string LastCheck { get; set; }
        public string LastCheckRaw { get; set; }
        public string LastUp { get; set; }
        public string LastUpRaw { get; set; }
        public string LastDown { get; set; }
        public string LastDownRaw { get; set; }
        public string Minigraph { get; set; }
        public string Schedule { get; set; }
        public string BaseType { get; set; }
        public string BaseLink { get; set; }
        public string BaseLinkRaw { get; set; }
        public string ParentId { get; set; }
        public string NotifiesX { get; set; }
        public string Interval { get; set; }
        public string IntervalRaw { get; set; }
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
        public string Tags { get; set; }
        public string Active { get; set; }
        public string ActiveRaw { get; set; }
        public string Name { get; set; }
        public string Comments { get; set; }
        public string Position { get; set; }
        public string PositionRaw { get; set; }

        internal SensorItem(string probe = "Chicago", string group = "Servers", string lastValue = "69 %",
            string lastValueRaw = "69.000", string device = "dc1",
            string downtime = "0.0847%", string downtimeRaw = "000000000000847", string downtimeTime = "1 h 37 m",
            string downtimeTimeRaw = "000000000005820",
            string downtimeSince = "90 d", string downtimeSinceRaw = "000000007850785", string uptime = "99.9153%",
            string uptimeRaw = "000000000999153",
            string uptimeTime = "79 d", string uptimeTimeRaw = "000000006862624", string uptimeSince = "43 d",
            string uptimeSinceRaw = "000000003753723",
            string knowntime = "90%", string knowntimeRaw = "000000006868444",
            string cumSince = "14/09/2016 7:51:26 PM <span class=\"percent\">[88 d ago]</span>",
            string cumSinceRaw = "42627.4107215162",
            string lastCheck = "12/12/2016 4:03:45 PM <span class=\"percent\">[51 s ago]</span>",
            string lastCheckRaw = "42716.2109422917",
            string lastUp = "12/12/2016 4:03:45 PM <span class=\"percent\">[51 s ago]</span>",
            string lastUpRaw = "42716.2109422917",
            string lastDown = "30/10/2016 5:22:34 AM <span class=\"percent\">[43 d ago]</span>",
            string lastDownRaw = "42672.7656715046",
            string minigraph = "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0|0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0",
            string schedule = "Saturdays [GMT+1000]", string baseType = "sensor", string baseLink = "/sensor.htm?id=2203",
            string baseLinkRaw = "2203", string parentId = "2193",
            string notifiesX = "Inherited", string interval = "60 s", string intervalRaw = "0000000060",
            string intervalx = "Inherited (60)",
            string intervalXRaw = "0000000060", string access = "Full", string accessRaw = "0000000400",
            string dependency = "Parent",
            string dependencyRaw = "dc1",
            string favorite = "<span class=\"objectisnotfavorite icon-gray ui-icon ui-icon-flag\" id=\"fav-2203\" onclick=\"_Prtg.objectTools.faveObject.call(this,2203,'toggle');return false;\"></span>",
            string favoriteRaw = "1", string status = "Up ", string statusRaw = "3", string priority = "3",
            string message = "<div class=\"status\">OK<div class=\"moreicon\"></div></div>",
            string messageRaw = "OK", string type = "WMI Logical Disk I/O BETA", string typeRaw = "wmilogicaldiskv2",
            string tags = "wmilogicalsensor C_OS_Win",
            string active = "True", string activeRaw = "-1", string objid = "2203", string name = "Volume IO _Total",
            string comments = "Do not delete!", string position = "60", string positionRaw = "0000000060")
        {
            Probe = probe;
            Group = group;
            LastValue = lastValue;
            LastValueRaw = lastValueRaw;
            Device = device;
            Downtime = downtime;
            DowntimeRaw = downtimeRaw;
            DowntimeTime = downtimeTime;
            DowntimeTimeRaw = downtimeTimeRaw;
            DowntimeSince = downtimeSince;
            DowntimeSinceRaw = downtimeSinceRaw;
            Uptime = uptime;
            UptimeRaw = uptimeRaw;
            UptimeTime = uptimeTime;
            UptimeTimeRaw = uptimeTimeRaw;
            UptimeSince = uptimeSince;
            UptimeSinceRaw = uptimeSinceRaw;
            Knowntime = knowntime;
            KnowntimeRaw = knowntimeRaw;
            CumSince = cumSince;
            CumSinceRaw = cumSinceRaw;
            LastCheck = lastCheck;
            LastCheckRaw = lastCheckRaw;
            LastUp = lastUp;
            LastUpRaw = lastUpRaw;
            LastDown = lastDown;
            LastDownRaw = lastDownRaw;
            Minigraph = minigraph;
            Schedule = schedule;
            BaseType = baseType;
            BaseLink = baseLink;
            BaseLinkRaw = baseLinkRaw;
            ParentId = parentId;
            NotifiesX = notifiesX;
            Interval = interval;
            IntervalRaw = intervalRaw;
            IntervalX = intervalx;
            IntervalXRaw = intervalXRaw;
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
            Tags = tags;
            Active = active;
            ActiveRaw = activeRaw;
            ObjId = objid;
            Name = name;
            Comments = comments;
            Position = position;
            PositionRaw = positionRaw;
        }
    }
}