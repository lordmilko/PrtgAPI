using System.Diagnostics;

namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    [DebuggerDisplay("Name: {Name,nq}, Id: {ObjId,nq}")]
    public class ScheduleItem : BaseItem
    {
        public string BaseLink { get; set; }
        public string BaseLinkRaw { get; set; }
        public string Type { get; set; }
        public string TypeRaw { get; set; }
        public string Active { get; set; }
        public string ActiveRaw { get; set; }
        public string Name { get; set; }

        internal ScheduleItem(string baseLink = "/editschedule.htm?id=620", string baseLinkRaw = "620", string type = "Schedule", string typeRaw = "schedule",
            string active = "True", string activeRaw = "-1", string name = "Weekdays [GMT+0800]", string objid = "620")
        {
            BaseLink = baseLink;
            BaseLinkRaw = baseLinkRaw;
            Type = type;
            TypeRaw = typeRaw;
            Active = active;
            ActiveRaw = activeRaw;
            Name = name;
            ObjId = objid;
        }
    }
}