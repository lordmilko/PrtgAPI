using System.Text;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class ServerStatusResponse : IWebResponse
    {
        private ServerStatusItem item;

        public ServerStatusResponse(ServerStatusItem item)
        {
            this.item = item;
        }

        public string GetResponseText(ref string address)
        {
            //"ToDos": "", 
            //"ActivationAlert": "" 

            var builder = new StringBuilder();

            builder.Append("{ ");
            builder.Append($"\"NewMessages\": \"{item.NewMessages}\", ");
            builder.Append($"\"NewAlarms\": \"{item.NewAlarms}\", ");
            builder.Append($"\"Alarms\": \"{item.Alarms}\", ");
            builder.Append($"\"PartialAlarms\": \"{item.PartialAlarms}\", ");
            builder.Append($"\"AckAlarms\": \"{item.AckAlarms}\", ");
            builder.Append($"\"UnusualSens\": \"{item.UnusualSens}\", ");
            builder.Append($"\"UpSens\": \"{item.UpSens}\", ");
            builder.Append($"\"WarnSens\": \"{item.WarnSens}\", ");
            builder.Append($"\"PausedSens\": \"{item.PausedSens}\", ");
            builder.Append($"\"UnknownSens\": \"{item.UnknownSens}\", ");
            builder.Append($"\"NewTickets\": \"{item.NewTickets}\", ");
            builder.Append($"\"UserId\": \"{item.UserId}\", ");
            builder.Append($"\"UserTimeZone\": \"{item.UserTimeZone}\", ");
            builder.Append($"\"ToDos\": \"{item.ToDos}\", ");
            builder.Append($"\"Favs\": {item.Favs}, ");
            builder.Append($"\"Clock\": \"{item.Clock}\", ");
            builder.Append($"\"Version\": \"{item.Version}\", ");
            builder.Append($"\"BackgroundTasks\": \"{item.BackgroundTasks}\", ");
            builder.Append($"\"CorrelationTasks\": \"{item.CorrelationTasks}\", ");
            builder.Append($"\"AutoDiscoTasks\": \"{item.AutoDiscoTasks}\", ");
            builder.Append($"\"ReportTasks\": \"{item.ReportTasks}\", ");
            builder.Append($"\"EditionType\": \"{item.EditionType}\", ");
            builder.Append($"\"PRTGUpdateAvailable\": {item.PrtgUpdateAvailable}, ");
            builder.Append($"\"MaintExpiryDays\": \"{item.MaintExpiryDays}\", ");
            builder.Append($"\"TrialExpiryDays\": {item.TrialExpiryDays}, ");
            builder.Append($"\"CommercialExpiryDays\": \"{item.CommercialExpiryDays}\", ");
            builder.Append($"\"Overloadprotection\": {item.OverloadProtection}, ");
            builder.Append($"\"ClusterType\":\"{item.ClusterType}\", ");
            builder.Append($"\"ClusterNodeName\":\"{item.ClusterNodeName}\", ");
            builder.Append($"\"IsAdminUser\": {item.IsAdminUser}, ");
            builder.Append($"\"ReadOnlyUser\": \"{item.ReadOnlyUser}\", ");
            builder.Append($"\"TicketUser\": \"{item.TicketUser}\", ");
            builder.Append($"\"ReadOnlyAllowAcknowledge\": \"{item.ReadOnlyAllowAcknowledge}\", ");
            builder.Append($"\"LowMem\": {item.LowMem}, ");
            builder.Append($"\"ActivationAlert\": \"{item.ActivationAlert}\", ");
            builder.Append($"\"PRTGHost\": \"{item.PrtgHost}\", ");
            builder.Append($"\"MaxSensorCount\": \"{item.MaxSensorCount}\", ");
            builder.Append($"\"Activated\": \"{item.Activated}\"");
            builder.Append("}");

            return builder.ToString();
        }
    }
}
