using System;
using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.Tests.UnitTests.TreeNodes
{
    abstract class TreeNode<TTestItem> : ITreeNode
    {
        public string Name { get; set; }

        public int Id { get; set; } = -1;

        public int ParentId => Parent?.Id ?? -1;

        public ITreeNode Parent { get; set; }

        protected TreeNode(string name)
        {
            Name = name;
        }

        protected void AddNode<T>(List<T> list, T item) where T : ITreeNode
        {
            list.Add(item);
        }

        protected Exception UnknownContent(object content)
        {
            throw new NotImplementedException($"Don't know how to handle content '{content.GetType()}'");
        }

        protected int GetTotalGroups(IGroupContainer node)
        {
            return node.Groups.Count + node.Groups.Sum(GetTotalGroups);
        }

        protected int GetTotalDevices(IGroupContainer node)
        {
            return node.Devices.Count + node.Groups.Sum(GetTotalDevices);
        }

        protected int GetTotalSensors(IGroupContainer node)
        {
            return node.Devices.Sum(d => d.Sensors.Count) + node.Groups.Sum(GetTotalSensors);
        }

        protected ITreeNode GetProbe(ITreeNode node)
        {
            if (node is ProbeNode)
                return node;

            if (node.Parent == null)
                return null;

            return GetProbe(node.Parent);
        }

        protected List<SensorNode> GetSensors(ISensorContainer node, bool recurse)
        {
            var sensors = new List<SensorNode>();

            if (node is IDeviceContainer)
            {
                foreach (var device in ((IDeviceContainer) node).Devices)
                {
                    sensors.AddRange(device.GetSensors(recurse));
                }
            }

            if (node is IGroupContainer && recurse)
            {
                foreach (var group in ((IGroupContainer) node).Groups)
                {
                    sensors.AddRange(GetSensors(group, true));
                }
            }

            return sensors;
        }

        protected List<DeviceNode> GetDevices(IDeviceContainer node, bool recurse)
        {
            var devices = node.Devices;

            if (node is IGroupContainer && recurse)
            {
                foreach (var group in ((IGroupContainer)node).Groups)
                {
                    devices.AddRange(GetDevices(group, true));
                }
            }

            return devices;
        }

        public abstract TTestItem GetTestItem();
    }
}
