using System.Text;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class MultiCheckBoxResponse : BaseTargetResponse
    {
        private bool errors;
        private bool discards;
        private bool unicast;
        private bool nonunicast;
        private bool multicast;
        private bool broadcast;
        private bool unknown;

        public MultiCheckBoxResponse(bool errors = false, bool discards = false, bool unicast = false,
            bool nonunicast = false, bool multicast = false, bool broadcast = false, bool unknown = false)
        {
            this.errors = errors;
            this.discards = discards;
            this.unicast = unicast;
            this.nonunicast = nonunicast;
            this.multicast = multicast;
            this.broadcast = broadcast;
            this.unknown = unknown;
        }

        protected override string GetResponseText()
        {
            var builder = new StringBuilder();

            builder.Append("<input type=\"hidden\" name=\"trafficmode_\" value=\"standinfornoselection\">");

            //Errors
            builder.Append("<span class=\"makebuttonset checkboxbuttonset\">");
            builder.Append($"<input type=\"checkbox\" class=\"checkbox\" name=\"trafficmode_\" value=\"errors\"{Get(errors)} id=\"trafficmode_errors\" >");
            builder.Append("<label for=\"trafficmode_errors\">Errors In &amp; Out</label><br>");

            //Discards
            builder.Append($"<input type=\"checkbox\" class=\"checkbox\" name=\"trafficmode_\" value=\"discards\"{Get(discards)} id=\"trafficmode_discards\" >");
            builder.Append("<label for=\"trafficmode_discards\">Discards In &amp; Out</label><br>");

            //Unicast
            builder.Append($"<input type=\"checkbox\" class=\"checkbox\" name=\"trafficmode_\" value=\"unicast\"{Get(unicast)} id=\"trafficmode_unicast\" >");
            builder.Append("<label for=\"trafficmode_unicast\">Unicast Packets In &amp; Out</label><br>");

            //Nonunicast
            builder.Append($"<input type=\"checkbox\" class=\"checkbox\" name=\"trafficmode_\" value=\"nonunicast\"{Get(nonunicast)} id=\"trafficmode_nonunicast\" >");
            builder.Append("<label for=\"trafficmode_nonunicast\">Non Unicast Packets In &amp; Out (32bit only)</label><br>");

            //Multicast
            builder.Append($"<input type=\"checkbox\" class=\"checkbox\" name=\"trafficmode_\" value=\"multicast\"{Get(multicast)} id=\"trafficmode_multicast\" >");
            builder.Append("<label for=\"trafficmode_multicast\">Multicast Packets In &amp; Out (64bit only)</label><br>");

            //Broadcast
            builder.Append($"<input type=\"checkbox\" class=\"checkbox\" name=\"trafficmode_\" value=\"broadcast\"{Get(broadcast)} id=\"trafficmode_broadcast\" >");
            builder.Append("<label for=\"trafficmode_broadcast\">Broadcast Packets In &amp; Out (64bit only)</label><br>");

            //Unknown
            builder.Append($"<input type=\"checkbox\" class=\"checkbox\" name=\"trafficmode_\" value=\"unknown\"{Get(unknown)} id=\"trafficmode_unknown\" >");
            builder.Append("<label for=\"trafficmode_unknown\">Unknown Protocols</label><br>");
            builder.Append("</span><br></div>");

            return builder.ToString();
        }

        private string Get(bool val)
        {
            if (val)
                return "checked=\"checked\"";

            return string.Empty;
        }
    }
}
