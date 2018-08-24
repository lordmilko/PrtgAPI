using System;
using System.Collections.Generic;
using System.Diagnostics;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.TreeNodes
{
    [DebuggerDisplay("Name: {Name,nq}, TotalGroups: {TotalGroups}, TotalDevices: {TotalDevices}, TotalSensors: {TotalSensors}")]
    class ProbeNode : TreeNode<ProbeItem>, IGroupContainer
    {
        public List<GroupNode> Groups { get; set; } = new List<GroupNode>();
        public List<DeviceNode> Devices { get; set; } = new List<DeviceNode>();

        public int TotalGroups => GetTotalGroups(this);
        public int TotalDevices => GetTotalDevices(this);
        public int TotalSensors => GetTotalSensors(this);

        private NodeFactory factory = new NodeFactory();

        public ProbeNode(string name, params ITreeNode[] content) : base(name)
        {
            foreach (var item in content)
            {
                if (item is GroupNode)
                    AddNode(Groups, (GroupNode) item);
                else if (item is DeviceNode)
                    AddNode(Devices, (DeviceNode) item);
                else
                    throw UnknownContent(content);
            }

            Id = factory.GetProbeId();
            AssignIds(this);
        }

        private void AssignIds(IGroupContainer node)
        {
            //Assign IDs to all groups directly descending from this node
            foreach (var group in node.Groups)
            {
                group.Id = factory.GetGroupId();
                group.Parent = node;
            }

            //Assogm IDs to all devices directly descending from this node
            foreach (var device in node.Devices)
            {
                device.Id = factory.GetDeviceId();
                device.Parent = node;

                //Assign IDs to all sensors directly descending from each device
                foreach (var sensor in device.Sensors)
                {
                    sensor.Id = factory.GetSensorId();
                    sensor.Parent = device;
                }
            }

            foreach (var group in node.Groups)
            {
                AssignIds(group);
            }
        }

        public override ProbeItem GetTestItem()
        {
            throw new NotImplementedException();
        }

        public List<SensorNode> GetSensors(bool recurse) => GetSensors(this, recurse);
        public List<DeviceNode> GetDevices(bool recurse) => GetDevices(this, recurse);
    }
}
