using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    class NotificationTriggerJsonItem
    {
        private string Value { get; }

        private NotificationTriggerJsonItem(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        public static NotificationTriggerJsonItem ChangeTrigger(string typeName = "Change Trigger", string subId = "8", string onNotificationAction = "300|Email and push notification to admin")
        {
            ArgumentNullThrower(typeName, nameof(typeName));
            ArgumentNullThrower(subId, nameof(subId));
            ArgumentNullThrower(onNotificationAction, nameof(onNotificationAction));

            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("\"type\": \"change\",");
            builder.Append($"\"typename\": \"{typeName}\",");
            builder.Append($"\"subid\": {subId},");
            builder.Append($"\"onnotificationid\": \"{onNotificationAction}\",");
            builder.Append($"\"onnotificationid_input\": \"{ (GetNotificationInput("onnotificationid", onNotificationAction))}\"");
            builder.Append("}");

            return new NotificationTriggerJsonItem(builder.ToString());
        }

        public static NotificationTriggerJsonItem StateTrigger(string typeName = "State Trigger", string subId = "1", string nodest = "Down",
            string latency = "60", string onNotificationAction = "300|Email and push notification to admin", string offNotificationAction = "-1|None", string escLatency = "300",
            string escNotificationAction = "302|Ticket Notification", string repeatival = "6", string[] nodestInput = null)
        {
            if (nodestInput == null)
                nodestInput = new[] { "Down", "Warning", "Unusual", "Down (Partial)", "Up", "Unknown" };

            ArgumentNullThrower(typeName, nameof(typeName));
            ArgumentNullThrower(subId, nameof(subId));
            ArgumentNullThrower(nodest, nameof(nodest));
            ArgumentNullThrower(latency, nameof(latency));
            ArgumentNullThrower(onNotificationAction, nameof(onNotificationAction));
            ArgumentNullThrower(offNotificationAction, nameof(offNotificationAction));
            ArgumentNullThrower(escLatency, nameof(escLatency));
            ArgumentNullThrower(escNotificationAction, nameof(escNotificationAction));
            ArgumentNullThrower(repeatival, nameof(repeatival));

            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("\"type\": \"state\",");
            builder.Append($"\"typename\": \"{typeName}\",");
            builder.Append($"\"subid\": {subId},");
            builder.Append($"\"nodest\": \"{nodest}\",");
            builder.Append($"\"nodest_input\": \"{GetNodestInput(nodestInput, nodest)}\",");
            builder.Append($"\"latency\": {latency},");
            builder.Append("\"latency_input\": \"<input class=\\\"text\\\"  data-rule-number=\\\"true\\\" data-rule-required=\\\"true\\\" data-rule-min=\\\"0\\\" data-rule-max=\\\"999999\\\" type=\\\"text\\\" name=\\\"latency_1\\\" id=\\\"latency_1\\\" autocomplete=\\\"off\\\" value=\\\"600\\\" >\",");
            builder.Append($"\"onnotificationid\": \"{onNotificationAction}\",");
            builder.Append($"\"onnotificationid_input\": \"{(GetNotificationInput("onnotificationid", onNotificationAction))}\",");
            builder.Append($"\"offnotificationid\": \"{offNotificationAction}\",");
            builder.Append($"\"offnotificationid_input\": \"{(GetNotificationInput("offnotificationid", offNotificationAction))}\",");
            builder.Append($"\"esclatency\": {escLatency},");
            builder.Append("\"esclatency_input\": \"<input class=\\\"text\\\"  data-rule-number=\\\"true\\\" data-rule-required=\\\"true\\\" data-rule-min=\\\"0\\\" data-rule-max=\\\"999999\\\" type=\\\"text\\\" name=\\\"esclatency_1\\\" id=\\\"esclatency_1\\\" autocomplete=\\\"off\\\" value=\\\"900\\\" >\",");
            builder.Append($"\"escnotificationid\": \"{escNotificationAction}\",");
            builder.Append($"\"escnotificationid_input\": \"{(GetNotificationInput("escnotificationid", escNotificationAction))}\",");
            builder.Append($"\"repeatival\": {repeatival},");
            builder.Append("\"repeatival_input\": \"<input class=\\\"text\\\"  data-rule-number=\\\"true\\\" data-rule-min=\\\"0\\\" data-rule-max=\\\"999999\\\" type=\\\"text\\\" name=\\\"repeatival_1\\\" id=\\\"repeatival_1\\\" autocomplete=\\\"off\\\" value=\\\"0\\\" >\"");
            builder.Append("}");

            return new NotificationTriggerJsonItem(builder.ToString());
        }

        public static NotificationTriggerJsonItem ThresholdTrigger(string typeName = "Threshold Trigger", string subId = "7", string channel = "Primary", string condition = "Above", string threshold = "20",
            string latency = "60", string onNotificationAction = "-1|no notification", string offNotificationAction = "-1|no notification", string[] channelInput = null, string[] conditionInput = null)
        {
            if (channelInput == null)
                channelInput = new[] { "Primary", "Total", "Traffic In", "Traffic Out" };

            if (conditionInput == null)
                conditionInput = new[] { "Above", "Below", "Equal to", "Not Equal to" };

            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("\"type\": \"threshold\",");
            builder.Append($"\"typename\": \"{typeName}\",");
            builder.Append($"\"subid\": {subId},");
            builder.Append($"\"channel\": \"{channel}\",");
            builder.Append($"\"channel_input\": \"{GetChannelInput(channelInput, channel)}\",");
            builder.Append($"\"condition\": \"{condition}\",");
            builder.Append($"\"condition_input\": \"{GetConditionInput(conditionInput, condition)}\",");
            builder.Append($"\"threshold\": {threshold},");
            builder.Append("\"threshold_input\": \"<input type=\\\"text\\\" class=\\\"text\\\"  data-rule-number=\\\"true\\\" data-rule-required=\\\"true\\\" autocomplete=\\\"off\\\" name=\\\"threshold_5\\\" id=\\\"threshold_5\\\" value=\\\"0\\\" >\",");
            builder.Append($"\"latency\": {latency},");
            builder.Append("\"latency_input\": \"<input class=\\\"text\\\"  data-rule-number=\\\"true\\\" data-rule-required=\\\"true\\\" data-rule-min=\\\"0\\\" data-rule-max=\\\"999999\\\" type=\\\"text\\\" name=\\\"latency_5\\\" id=\\\"latency_5\\\" autocomplete=\\\"off\\\" value=\\\"60\\\" >\",");
            builder.Append($"\"onnotificationid\": \"{onNotificationAction}\",");
            builder.Append($"\"onnotificationid_input\": \"{(GetNotificationInput("onnotificationid", onNotificationAction))}\",");
            builder.Append($"\"offnotificationid\": \"{offNotificationAction}\",");
            builder.Append($"\"offnotificationid_input\": \"{(GetNotificationInput("offnotificationid", offNotificationAction))}\"");
            builder.Append("}");

            return new NotificationTriggerJsonItem(builder.ToString());
        }

        public static NotificationTriggerJsonItem SpeedTrigger(string typeName = "Speed Trigger", string subId = "5", string channel = "Primary", string condition = "Above", string threshold = "20",
            string unitSize = "bit", string unitTime = "second", string latency = "50", string onNotificationAction = "-1|no notification", string offNotificationAction = "-1|no notification", string[] channelInput = null,
            string[] conditionInput = null, string[] unitTimeInput = null)
        {
            if (channelInput == null)
                channelInput = new[] { "Primary", "Total", "Traffic In", "Traffic Out" };

            if (conditionInput == null)
                conditionInput = new[] { "Above", "Below", "Equal to", "Not Equal to" };

            if (unitTimeInput == null)
                unitTimeInput = new[] { "second", "minute", "hour", "day" };

            ArgumentNullThrower(typeName, nameof(typeName));
            ArgumentNullThrower(subId, nameof(subId));
            ArgumentNullThrower(channel, nameof(channel));
            ArgumentNullThrower(threshold, nameof(threshold));
            ArgumentNullThrower(unitSize, nameof(unitSize));
            ArgumentNullThrower(unitTime, nameof(unitTime));
            ArgumentNullThrower(latency, nameof(latency));
            ArgumentNullThrower(onNotificationAction, nameof(onNotificationAction));
            ArgumentNullThrower(offNotificationAction, nameof(offNotificationAction));

            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("\"type\": \"speed\",");
            builder.Append($"\"typename\": \"{typeName}\",");
            builder.Append($"\"subid\": {subId},");
            builder.Append($"\"channel\": \"{channel}\",");
            builder.Append($"\"channel_input\": \"{GetChannelInput(channelInput, channel)}\",");
            builder.Append($"\"condition\": \"{condition}\",");
            builder.Append($"\"condition_input\": \"{GetConditionInput(conditionInput, condition)}\",");
            builder.Append($"\"threshold\": {threshold},");
            builder.Append("\"threshold_input\": \"<input type=\\\"text\\\" class=\\\"text\\\"  data-rule-number=\\\"true\\\" data-rule-required=\\\"true\\\" autocomplete=\\\"off\\\" name=\\\"threshold_4\\\" id=\\\"threshold_4\\\" value=\\\"0\\\" >\",");
            builder.Append($"\"unitsize\": \"{unitSize}\",");
            builder.Append("\"unitsize_input\": \"<select name=\\\"unitsize_4\\\" class=\\\"combo\\\" id=\\\"unitsize_4\\\" ><option  value=\\\"11\\\" selected=\\\"selected\\\"  id=\\\"unitsize11\\\">bit</option><option  value=\\\"12\\\" id=\\\"unitsize12\\\">kbit</option><option  value=\\\"13\\\" id=\\\"unitsize13\\\">Mbit</option><option  value=\\\"14\\\" id=\\\"unitsize14\\\">Gbit</option><option  value=\\\"15\\\" id=\\\"unitsize15\\\">Tbit</option><option  value=\\\"6\\\" id=\\\"unitsize6\\\">Byte</option><option  value=\\\"7\\\" id=\\\"unitsize7\\\">KByte</option><option  value=\\\"8\\\" id=\\\"unitsize8\\\">MByte</option><option  value=\\\"9\\\" id=\\\"unitsize9\\\">GByte</option><option  value=\\\"10\\\" id=\\\"unitsize10\\\">TByte</option></select>\",");
            builder.Append($"\"unittime\": \"{unitTime}\",");
            builder.Append($"\"unittime_input\": \"{GetUnitTimeInput(unitTimeInput, unitTime)}\",");
            builder.Append($"\"latency\": {latency},");
            builder.Append("\"latency_input\": \"<input class=\\\"text\\\"  data-rule-number=\\\"true\\\" data-rule-required=\\\"true\\\" data-rule-min=\\\"0\\\" data-rule-max=\\\"999999\\\" type=\\\"text\\\" name=\\\"latency_4\\\" id=\\\"latency_4\\\" autocomplete=\\\"off\\\" value=\\\"60\\\" >\",");
            builder.Append($"\"onnotificationid\": \"{onNotificationAction}\",");
            builder.Append($"\"onnotificationid_input\": \"{(GetNotificationInput("onnotificationid", onNotificationAction))}\",");
            builder.Append($"\"offnotificationid\": \"{offNotificationAction}\",");
            builder.Append($"\"offnotificationid_input\": \"{(GetNotificationInput("offnotificationid", offNotificationAction))}\"");
            builder.Append("}");

            return new NotificationTriggerJsonItem(builder.ToString());
        }

        public static NotificationTriggerJsonItem VolumeTrigger(string typeName = "Volume Trigger", string subId = "6", string channel = "Total", string threshold = "20", string unitSize = "KByte",
            string period = "Week", string onNotificationAction = "301|Email to all members of group PRTG Users Group|email:Email|", string[] channelInput = null, string[] periodInput = null)
        {
            if (channelInput == null)
                channelInput = new[] { "Primary", "Total", "Traffic In", "Traffic Out" };

            if (periodInput == null)
                periodInput = new[] { "Hour", "Day", "Week", "Month" };

            ArgumentNullThrower(typeName, nameof(typeName));
            ArgumentNullThrower(subId, nameof(subId));
            ArgumentNullThrower(channel, nameof(channel));
            ArgumentNullThrower(threshold, nameof(threshold));
            ArgumentNullThrower(unitSize, nameof(unitSize));
            ArgumentNullThrower(period, nameof(period));
            ArgumentNullThrower(onNotificationAction, nameof(onNotificationAction));

            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("\"type\": \"volume\",");
            builder.Append($"\"typename\": \"{typeName}\",");
            builder.Append($"\"subid\": {subId},");
            builder.Append($"\"channel\": \"{channel}\",");
            builder.Append($"\"channel_input\": \"{GetChannelInput(channelInput, channel)}\",");
            builder.Append($"\"threshold\": {threshold},");
            builder.Append("\"threshold_input\": \"<input type=\\\"text\\\" class=\\\"text\\\"  data-rule-number=\\\"true\\\" data-rule-required=\\\"true\\\" autocomplete=\\\"off\\\" name=\\\"threshold_3\\\" id=\\\"threshold_3\\\" value=\\\"5\\\" >\",");
            builder.Append($"\"unitsize\": \"{unitSize}\",");
            builder.Append("\"unitsize_input\": \"<select name=\\\"unitsize_3\\\" class=\\\"combo\\\" id=\\\"unitsize_3\\\" ><option  value=\\\"6\\\" id=\\\"unitsize6\\\">Byte</option><option  value=\\\"7\\\" selected=\\\"selected\\\"  id=\\\"unitsize7\\\">KB</option><option  value=\\\"8\\\" id=\\\"unitsize8\\\">MB</option><option  value=\\\"9\\\" id=\\\"unitsize9\\\">GB</option><option  value=\\\"10\\\" id=\\\"unitsize10\\\">TB</option></select>\",");
            builder.Append($"\"period\": \"{period}\",");
            builder.Append($"\"period_input\": \"{GetPeriodInput(periodInput, period)}\",");
            builder.Append($"\"onnotificationid\": \"{onNotificationAction}\",");
            builder.Append($"\"onnotificationid_input\": \"{(GetNotificationInput("onnotificationid", onNotificationAction))}\"");
            builder.Append("}");

            return new NotificationTriggerJsonItem(builder.ToString());
        }

        private static string GetNotificationInput(string tagName, string selected)
        {
            var builder = new StringBuilder();

            //Trim the name to just "302|" or something, so even when we're testing with values with foreign descriptions we can still select the right one
            selected = selected.Substring(0, selected.IndexOf("|") + 1);

            builder.Append($"<select name=\\\"{tagName}_1\\\" id=\\\"{tagName}_1\\\">");
            builder.Append("<option value=\\\"-1|no notification||\\\" >no notification</option>");
            builder.Append("<option value=\\\"300|Email and push notification to admin|email:Email,push:Push Notification|\\\">Email and push notification to admin</option>");
            builder.Append("<option value=\\\"301|Email to all members of group PRTG Users Group|email:Email|\\\">Email to all members of group PRTG Users Group</option>");
            builder.Append("<option value=\\\"302|Ticket Notification|ticketnoti:Ticket|\\\">Ticket Notification</option>");
            builder.Append("</select>");

            var result = builder.ToString();

            var r = SetSelectedValue(result, selected);

            return r;
        }

        private static string GetChannelInput(string[] arr, string selected)
        {
            var builder = new StringBuilder();

            builder.Append("<select name=\\\"channel_3\\\" class=\\\"combo\\\" id=\\\"channel_3\\\" >");
            builder.Append($"<option  value=\\\"-999\\\" id=\\\"channel-999\\\">{arr[0]}</option>");
            builder.Append($"<option  value=\\\"-1\\\" id=\\\"channel-1\\\">{arr[1]}</option>");
            builder.Append($"<option  value=\\\"0\\\" id=\\\"channel0\\\">{arr[2]}</option>");
            builder.Append($"<option  value=\\\"1\\\" id=\\\"channel1\\\">{arr[3]}</option>");

            if (arr.Length > 4)
            {
                for (var i = 0; i < arr.Length - 4; i++)
                {
                    builder.Append($"<option  value=\\\"{10 + i}\\\" id=\\\"channel1\\\">{arr[i + 4]}</option>");
                }
            }

            builder.Append("</select>");

            var result = builder.ToString();

            var r = SetSelectedEnum(result, selected);

            return r;
        }

        private static string GetPeriodInput(string[] arr, string selected)
        {
            var builder = new StringBuilder();

            builder.Append("<select name=\\\"period_3\\\" class=\\\"combo\\\" id=\\\"period_3\\\" >");
            builder.Append($"<option  value=\\\"0\\\" id=\\\"period0\\\">{arr[0]}</option>");
            builder.Append($"<option  value=\\\"1\\\" id=\\\"period1\\\">{arr[1]}</option>");
            builder.Append($"<option  value=\\\"2\\\" id=\\\"period2\\\">{arr[2]}</option>");
            builder.Append($"<option  value=\\\"3\\\" id=\\\"period3\\\">{arr[3]}</option>");
            builder.Append("</select>");

            var result = builder.ToString();

            var r = SetSelectedEnum(result, selected);

            return r;
        }

        private static string GetNodestInput(string[] arr, string selected)
        {
            var builder = new StringBuilder();

            builder.Append("<select name=\\\"nodest_1\\\" class=\\\"combo\\\" id=\\\"nodest_1\\\" >");
            builder.Append($"<option  value=\\\"0\\\" id=\\\"nodest0\\\">{arr[0]}</option>");
            builder.Append($"<option  value=\\\"1\\\" id=\\\"nodest1\\\">{arr[1]}</option>");
            builder.Append($"<option  value=\\\"2\\\" id=\\\"nodest2\\\">{arr[2]}</option>");
            builder.Append($"<option  value=\\\"3\\\" id=\\\"nodest3\\\">{arr[3]}</option>");
            builder.Append($"<option  value=\\\"4\\\" id=\\\"nodest4\\\">{arr[4]}</option>");
            builder.Append($"<option  value=\\\"5\\\" id=\\\"nodest5\\\">{arr[5]}</option>");
            builder.Append("</select>");

            var result = builder.ToString();

            var r = SetSelectedEnum(result, selected);

            return r;
        }

        private static string GetConditionInput(string[] arr, string selected)
        {
            var builder = new StringBuilder();

            builder.Append("<select name=\\\"condition_4\\\" class=\\\"combo\\\" id=\\\"condition_4\\\" >");

            var values = arr.Select((v, i) => $"<option  value=\\\"{i}\\\" id=\\\"condition{i}\\\">{arr[i]}</option>");

            foreach (var value in values)
                builder.Append(value);

            builder.Append("</select>");

            var result = builder.ToString();

            var r = SetSelectedEnum(result, selected);

            return r;
        }

        private static string GetUnitTimeInput(string[] arr, string selected)
        {
            var builder = new StringBuilder();

            builder.Append("<select name=\\\"unittime_4\\\" class=\\\"combo\\\" id=\\\"unittime_4\\\" >");
            builder.Append($"<option  value=\\\"1\\\" id=\\\"unittime1\\\">{arr[0]}</option>");
            builder.Append($"<option  value=\\\"2\\\" id=\\\"unittime2\\\">{arr[1]}</option>");
            builder.Append($"<option  value=\\\"3\\\" id=\\\"unittime3\\\">{arr[2]}</option>");
            builder.Append($"<option  value=\\\"4\\\" id=\\\"unittime4\\\">{arr[3]}</option>");
            builder.Append("</select>");

            return SetSelectedEnum(builder.ToString(), selected);
        }

        private static string SetSelectedValue(string ddl, string selectedValue)
        {
            var pattern = $"(<option value=\\\\\"{Regex.Escape(selectedValue)}.*?\\\\\".*?>).+?</option>";

            return SetSelected(ddl, selectedValue, pattern);
        }

        private static string SetSelectedEnum(string ddl, string selectedEnum)
        {
            var pattern = $"(<option.*?>){Regex.Escape(selectedEnum)}</option>";

            return SetSelected(ddl, selectedEnum, pattern);
        }

        private static string SetSelected(string ddl, string selectedValue, string pattern)
        {
            var match = Regex.Match(ddl, pattern);

            if (!match.Success)
                throw new InvalidOperationException($"Cannot select value '{selectedValue}' as it was not found in the dropdown list");

            var group = match.Groups[1];

            var pos = group.Index + group.Length;

            var r = ddl.Insert(pos - 1, " selected=\\\"selected\\\"");

            return r;
        }

        private static void ArgumentNullThrower(object obj, string name)
        {
            if (obj == null)
                throw new ArgumentNullException(name);
        }
    }
}
