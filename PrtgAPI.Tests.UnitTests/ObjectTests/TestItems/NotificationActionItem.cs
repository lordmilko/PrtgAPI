namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestItems
{
    public class NotificationActionItem : BaseItem
    {
        public string Name { get; set; }

        internal NotificationActionItem(
            string objid = "300", string name = "Email and push notification to admin")
        {
            ObjId = objid;
            Name = name;
        }
    }
}
