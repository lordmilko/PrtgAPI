using System.Text;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TriggerOverviewResponse : IWebResponse
    {
        public string GetResponseText(ref string address)
        {
            var builder = new StringBuilder();

            builder.Append("{ ");
                builder.Append("\"supported\": [");
                    builder.Append("\"state\",");
                    builder.Append("\"speed\",");
                    builder.Append("\"volume\",");
                    builder.Append("\"threshold\",");
                    builder.Append("\"change\"");
                builder.Append("],");
                builder.Append("\"data\": [");
                    builder.Append("{ ");
                        builder.Append("\"type\":\"state\",");
                        builder.Append("\"typename\":\"State Trigger\",");
                        builder.Append("\"subid\":1,");
                        builder.Append("\"nodest\":\"Down\",");
                        builder.Append("\"nodest_input\":\"<select name=\\\"nodest_1\\\" class=\\\"combo\\\" id=\\\"nodest_1\\\" ><option  value=\\\"0\\\" selected=\\\"selected\\\"  id=\\\"nodest0\\\">Down</option><option  value=\\\"1\\\" id=\\\"nodest1\\\">Warning</option><option  value=\\\"2\\\" id=\\\"nodest2\\\">Unusual</option><option  value=\\\"3\\\" id=\\\"nodest3\\\">Partial Down</option></select>\",");
                        builder.Append("\"latency\":70,");
                        builder.Append("\"latency_input\":\"<input class=\\\"text\\\"  data-rule-number=\\\"true\\\" data-rule-required=\\\"true\\\" data-rule-min=\\\"0\\\" data-rule-max=\\\"999999\\\" type=\\\"text\\\" name=\\\"latency_1\\\" id=\\\"latency_1\\\" autocomplete=\\\"off\\\" value=\\\"70\\\" >\",");
                        builder.Append("\"onnotificationid\":\"300|Email and push notification to admin\",");
                        builder.Append("\"onnotificationid_input\":\"<select name=\\\"onnotificationid_1\\\" id=\\\"onnotificationid_1\\\" ><option value=\\\"-1|no notification|\\\">no notification</option><option value=\\\"300|Email and push notification to admin|\\\" selected=\\\"selected\\\" >Email and push notification to admin</option><option value=\\\"301|Email to all members of group PRTG Users Group|\\\">Email to all members of group PRTG Users Group</option><option value=\\\"302|Ticket Notification|\\\">Ticket Notification</option></select>\",");
                        builder.Append("\"offnotificationid\":\"302|Ticket Notification\",");
                        builder.Append("\"offnotificationid_input\":\"<select name=\\\"offnotificationid_1\\\" id=\\\"offnotificationid_1\\\" ><option value=\\\"-1|no notification|\\\">no notification</option><option value=\\\"300|Email and push notification to admin|\\\">Email and push notification to admin</option><option value=\\\"301|Email to all members of group PRTG Users Group|\\\">Email to all members of group PRTG Users Group</option><option value=\\\"302|Ticket Notification|\\\" selected=\\\"selected\\\" >Ticket Notification</option></select>\",");
                        builder.Append("\"esclatency\":310,");
                        builder.Append("\"esclatency_input\":\"<input class=\\\"text\\\"  data-rule-number=\\\"true\\\" data-rule-required=\\\"true\\\" data-rule-min=\\\"0\\\" data-rule-max=\\\"999999\\\" type=\\\"text\\\" name=\\\"esclatency_1\\\" id=\\\"esclatency_1\\\" autocomplete=\\\"off\\\" value=\\\"310\\\" >\",");
                        builder.Append("\"escnotificationid\":\"301|Email to all members of group PRTG Users Group\",");
                        builder.Append("\"escnotificationid_input\":\"<select name=\\\"escnotificationid_1\\\" id=\\\"escnotificationid_1\\\" ><option value=\\\"-1|no notification|\\\">no notification</option><option value=\\\"300|Email and push notification to admin|\\\">Email and push notification to admin</option><option value=\\\"301|Email to all members of group PRTG Users Group|\\\" selected=\\\"selected\\\" >Email to all members of group PRTG Users Group</option><option value=\\\"302|Ticket Notification|\\\">Ticket Notification</option></select>\",");
                        builder.Append("\"repeatival\":6,");
                        builder.Append("\"repeatival_input\":\"<input class=\\\"text\\\"  data-rule-number=\\\"true\\\" data-rule-min=\\\"0\\\" data-rule-max=\\\"999999\\\" type=\\\"text\\\" name=\\\"repeatival_1\\\" id=\\\"repeatival_1\\\" autocomplete=\\\"off\\\" value=\\\"6\\\" >\"");
                    builder.Append("},");
                    builder.Append("{ ");
                        builder.Append("\"type\":\"change\",");
                        builder.Append("\"typename\":\"Change Trigger\",");
                        builder.Append("\"subid\":5,");
                        builder.Append("\"onnotificationid\":\"302|Ticket Notification\",");
                        builder.Append("\"onnotificationid_input\":\"<select name=\\\"onnotificationid_5\\\" id=\\\"onnotificationid_5\\\" ><option value=\\\"-1|no notification|\\\">no notification</option><option value=\\\"300|Email and push notification to admin|\\\">Email and push notification to admin</option><option value=\\\"301|Email to all members of group PRTG Users Group|\\\">Email to all members of group PRTG Users Group</option><option value=\\\"302|Ticket Notification|\\\" selected=\\\"selected\\\" >Ticket Notification</option></select>\"");
                    builder.Append("}");
                builder.Append("],");
                builder.Append("\"readonly\":false");
            builder.Append("}");

            return builder.ToString();
        }
    }
}
