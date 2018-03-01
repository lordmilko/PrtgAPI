namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestItems
{
    public class NotificationActionItem : BaseItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string TypeRaw { get; set; }
        public string Active { get; set; }
        public string ActiveRaw { get; set; }
        public string Tags { get; set; } //note: special property, not normally included in response

        internal NotificationActionItem(
            string objid = "300", string name = "Email and push notification to admin", string type = "Notification", string typeRaw = "notification",
            string active = "True", string activeRaw = "-1")
        {
            ObjId = objid;
            Name = name;
            Type = type;
            TypeRaw = typeRaw;
            Active = active;
            ActiveRaw = activeRaw;
        }
    }
}
