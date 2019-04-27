namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    public class NotificationActionItem : BaseItem
    {
        public string Name { get; set; }
        public string ParentId { get; set; }
        public string Type { get; set; }
        public string TypeRaw { get; set; }
        public string Active { get; set; }
        public string ActiveRaw { get; set; }
        public string Tags { get; set; } //note: special property, not normally included in response
        public string BaseLink { get; set; }
        public string BaseLinkRaw { get; set; }

        internal NotificationActionItem(
            string objid = "300", string name = "Email and push notification to admin", string parentId = "-3", string type = "Notification", string typeRaw = "notification",
            string active = "True", string activeRaw = "-1",
            string baselink = "/editnotification.htm?id=300", string baselinkRaw = "300")
        {
            ObjId = objid;
            Name = name;
            ParentId = parentId;
            Type = type;
            TypeRaw = typeRaw;
            Active = active;
            ActiveRaw = activeRaw;
            BaseLink = baselink;
            BaseLinkRaw = baselinkRaw;
        }
    }
}
