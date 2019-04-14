using System;
using System.Text;

namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    public class NotificationTriggerItem : BaseItem
    {
        public string Content { get; set; }

        private NotificationTriggerItem()
        {
        }

        public static NotificationTriggerItem ChangeTrigger(string onNotificationAction = "301|Email to all members of group PRTG Users Group",
            string parentId = "1", string subId = "8")
        {
            ArgumentNullThrower(onNotificationAction, nameof(onNotificationAction));
            ArgumentNullThrower(parentId, nameof(parentId));
            ArgumentNullThrower(subId, nameof(subId));

            var builder = new StringBuilder();
            builder.Append($"{{\"type\":\"change\",\"condition\":\"change\", \"typename\":\"Change Trigger\", \"onnotificationid\":\"{onNotificationAction}\", ");
            builder.Append($"\"objectlink\":\"<a dependency=\\\"0\\\" thisid=\\\"{parentId}\\\" class=\\\"probemenu isnotpaused isnotfavorite fixed \\\" id=\\\"{parentId}\\\" href=\\\"probenode.htm?id={parentId}\\\">127.0.0.1 (Local Probe) </a>\"}}");

            var item = new NotificationTriggerItem
            {
                Content = builder.ToString(),
                ObjId = subId
            };

            return item;
        }

        public static NotificationTriggerItem StateTrigger(string latency = "60", string escLatency = "300", string repeatival = "0",
            string offNotificationAction = "-1|None", string escNotificationAction = "-1|None", string nodest = "Down",
            string onNotificationAction = "301|Email to all members of group PRTG Users Group", string parentId = "0", string subId = "1")
        {
            ArgumentNullThrower(latency, nameof(latency));
            ArgumentNullThrower(escLatency, nameof(escLatency));
            ArgumentNullThrower(repeatival, nameof(repeatival));
            ArgumentNullThrower(offNotificationAction, nameof(offNotificationAction));
            ArgumentNullThrower(escNotificationAction, nameof(escNotificationAction));
            ArgumentNullThrower(nodest, nameof(nodest));
            ArgumentNullThrower(onNotificationAction, nameof(onNotificationAction));
            ArgumentNullThrower(parentId, nameof(parentId));
            ArgumentNullThrower(subId, nameof(subId));

            var builder = new StringBuilder();
            builder.Append($"{{\"type\":\"state\",\"latency\":\"{latency}\",\"esclatency\":\"{escLatency}\",\"repeatival\":\"{repeatival}\", \"offnotificationid\":\"{offNotificationAction}\", ");
            builder.Append($"\"escnotificationid\":\"{escNotificationAction}\",\"nodest\":\"{nodest}\", \"typename\":\"State Trigger\", \"onnotificationid\":\"{onNotificationAction}\", ");
            builder.Append($"\"objectlink\":\"<a dependency=\\\"-1000\\\" thisid=\\\"{parentId}\\\" class=\\\"rootgroupmenu isnotpaused isnotfavorite fixed\\\" id=\\\"{parentId}\\\" href=\\\"group.htm?id={parentId}\\\">Root </a>\"}}");

            var item = new NotificationTriggerItem
            {
                Content = builder.ToString(),
                ObjId = subId
            };

            return item;
        }

        public static NotificationTriggerItem ThresholdTrigger(string latency = "60",
            string offNotificationAction = "301|Email to all members of group PRTG Users Group",
            string channel = "Primary",
            string condition = "Not Equal to", string threshold = "5",
            string onNotificationAction = "303|Email and push notification to admin", string parentId = "1",
            string subId = "7")
        {
            ArgumentNullThrower(latency, nameof(latency));
            ArgumentNullThrower(offNotificationAction, nameof(offNotificationAction));
            ArgumentNullThrower(channel, nameof(channel));
            ArgumentNullThrower(condition, nameof(condition));
            ArgumentNullThrower(threshold, nameof(threshold));
            ArgumentNullThrower(onNotificationAction, nameof(onNotificationAction));
            ArgumentNullThrower(parentId, nameof(parentId));
            ArgumentNullThrower(subId, nameof(subId));

            var builder = new StringBuilder();
            builder.Append($"{{\"type\":\"threshold\",\"latency\":\"{latency}\", \"offnotificationid\":\"{offNotificationAction}\",");
            builder.Append($"\"channel\":\"{channel}\",\"condition\":\"{condition}\",\"threshold\":\"{threshold}\", \"typename\":\"Threshold Trigger\", \"onnotificationid\":\"{onNotificationAction}\", ");
            builder.Append($"\"objectlink\":\"<a dependency=\\\"0\\\" thisid=\\\"{parentId}\\\" class=\\\"probemenu isnotpaused isnotfavorite fixed \\\" id=\\\"{parentId}\\\" href=\\\"probenode.htm?id={parentId}\\\">127.0.0.1 (Local Probe) </a>\"}}");

            var item = new NotificationTriggerItem
            {
                Content = builder.ToString(),
                ObjId = subId
            };

            return item;
        }

        public static NotificationTriggerItem SpeedTrigger(string latency = "60", string offNotificationAction = "302|Ticket Notification",
            string channel = "Primary", string condition = "Above",
            string threshold = "3", string unitsize = "TByte", string unittime = "d", string onNotificationAction = "303|Email and push notification to admin",
            string parentId = "1", string subId = "5")
        {
            ArgumentNullThrower(latency, nameof(latency));
            ArgumentNullThrower(offNotificationAction, nameof(offNotificationAction));
            ArgumentNullThrower(channel, nameof(channel));
            ArgumentNullThrower(condition, nameof(condition));
            ArgumentNullThrower(threshold, nameof(threshold));
            ArgumentNullThrower(unitsize, nameof(unitsize));
            ArgumentNullThrower(unittime, nameof(unittime));
            ArgumentNullThrower(onNotificationAction, nameof(onNotificationAction));
            ArgumentNullThrower(parentId, nameof(subId));

            var builder = new StringBuilder();
            builder.Append($"{{\"type\":\"speed\",\"latency\":\"{latency}\", \"offnotificationid\":\"{offNotificationAction}\",\"channel\":\"{channel}\", \"condition\":\"{condition}\", \"threshold\":\"{threshold}\", ");
            builder.Append($"\"unitsize\":\"{unitsize}\", \"unittime\":\"{unittime}\", \"typename\":\"Speed Trigger\", \"onnotificationid\":\"{onNotificationAction}\", ");
            builder.Append($"\"objectlink\":\"<a dependency=\\\"0\\\" thisid=\\\"{parentId}\\\" class=\\\"probemenu isnotpaused isnotfavorite fixed \\\" id=\\\"{parentId}\\\" href=\\\"probenode.htm?id={parentId}\\\">127.0.0.1 (Local Probe) </a>\"}}");

            var item = new NotificationTriggerItem
            {
                Content = builder.ToString(),
                ObjId = subId
            };

            return item;
        }

        public static NotificationTriggerItem VolumeTrigger(string channel = "Primary", string threshold = "6",
            string unitsize = "KByte", string period = "Hour",
            string onNotificationAction = "301|Email to all members of group PRTG Users Group", string parentId = "1", string subId = "6")
        {
            ArgumentNullThrower(channel, nameof(channel));
            ArgumentNullThrower(threshold, nameof(threshold));
            ArgumentNullThrower(unitsize, nameof(unitsize));
            ArgumentNullThrower(period, nameof(period));
            ArgumentNullThrower(onNotificationAction, nameof(onNotificationAction));
            ArgumentNullThrower(parentId, nameof(parentId));
            ArgumentNullThrower(subId, nameof(subId));

            var builder = new StringBuilder();
            builder.Append($"{{\"type\":\"volume\",\"channel\":\"{channel}\",\"threshold\":\"{threshold}\", \"unitsize\":\"{unitsize}\", \"period\":\"{period}\", ");
            builder.Append($"\"typename\":\"Volume Trigger\", \"onnotificationid\":\"{onNotificationAction}\", ");
            builder.Append($"\"objectlink\":\"<a dependency=\\\"0\\\" thisid=\\\"{parentId}\\\" class=\\\"probemenu isnotpaused isnotfavorite fixed \\\" id=\\\"{parentId}\\\" href=\\\"probenode.htm?id={parentId}\\\">127.0.0.1 (Local Probe) </a>\"}}");

            var item = new NotificationTriggerItem
            {
                Content = builder.ToString(),
                ObjId = subId
            };

            return item;
        }

        private static void ArgumentNullThrower(object obj, string name)
        {
            if (obj == null)
                throw new ArgumentNullException(name);
        }
    }
}
