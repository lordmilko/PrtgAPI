using PrtgAPI.Reflection;
using PrtgAPI.Tree;

namespace PrtgAPI.Tests.UnitTests.Tree
{
    public abstract class BaseTreeTest
    {
        protected ProbeNode DefaultProbe => PrtgNode.Probe(Probe());
        protected GroupNode DefaultGroup => PrtgNode.Group(Group());
        protected DeviceNode DefaultDevice => PrtgNode.Device(Device());
        protected SensorNode DefaultSensor => PrtgNode.Sensor(Sensor());
        protected TriggerNode DefaultTrigger => PrtgNode.Trigger(Trigger());
        protected PropertyNode DefaultProperty => PrtgNode.Property(Property());

        protected Probe Probe(string name = "Local Probe", int id = 1001, int parentId = 1)
        {
            return new Probe
            {
                Name = name,
                Id = id,
                ParentId = parentId,
            };
        }

        protected Group Group(string name = "Servers", int id = 2001, int parentId = 2)
        {
            return new Group
            {
                Name = name,
                Id = id,
                ParentId = parentId
            };
        }

        protected Device Device(string name = "dc-1", int id = 3001, int parentId = 3, string host = "dc-1")
        {
            return new Device
            {
                Name = name,
                Id = id,
                ParentId = parentId,
                Host = host
            };
        }

        protected Sensor Sensor(string name = "VMware Datastore", int id = 4001, int parentId = 4, string type = "vmwaredatastoreextern")
        {
            return new Sensor
            {
                Name = name,
                Id = id,
                ParentId = parentId,
                Type = type
            };
        }

        protected NotificationTrigger Trigger(string name = "Email to Admin", int id = 1)
        {
            var action = new NotificationAction
            {
                Name = name,
                Id = 300
            };

            var obj = new NotificationTrigger
            {
                SubId = id,
                ObjectId = 1001
            };

            var info = obj.GetInternalFieldInfo("onNotificationAction");
            info.SetValue(obj, action);

            info = obj.GetInternalFieldInfo("objectLink");
            info.SetValue(obj, $"<a thisid=\"1001\"/>");

            info = obj.GetInternalFieldInfo("type");
            info.SetValue(obj, "state");

            return obj;
        }

        protected PropertyValuePair Property(string propertyName = "host", string propertyValue = "dc-1")
        {
            return new PropertyValuePair(
                Device(),
                propertyName,
                propertyValue
            );
        }
    }
}
