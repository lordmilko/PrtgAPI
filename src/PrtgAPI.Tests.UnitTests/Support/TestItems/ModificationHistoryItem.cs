namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    public class ModificationHistoryItem : BaseItem
    {
        public string DateTime { get; set; }
        public string DateTimeRaw { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
        public string MessageRaw { get; set; }

        public ModificationHistoryItem(string datetime = "24/05/2017 12:53:54 PM", string datetimeRaw = "42879.1207698495",
            string user = "PRTG System Administrator", string message = "Created. 17.2.31.2018", string messageRaw = "17.2.31.2018")
        {
            DateTime = datetime;
            DateTimeRaw = datetimeRaw;
            User = user;
            Message = message;
            MessageRaw = messageRaw;
        }
    }
}
