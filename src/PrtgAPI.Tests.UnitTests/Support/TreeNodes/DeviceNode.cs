using System;
using System.Collections.Generic;
using System.Diagnostics;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.TreeNodes
{
    [DebuggerDisplay("Name: {Name,nq}, Sensors: {Sensors.Count}")]
    class DeviceNode : TreeNode<DeviceItem>, ISensorContainer
    {
        public List<SensorNode> Sensors { get; set; } = new List<SensorNode>();

        public int TotalSensors => Sensors.Count;

        public DeviceNode(string name, params ITreeNode[] content) : base(name)
        {
            foreach (var item in content)
            {
                if (item is SensorNode)
                    AddNode(Sensors, (SensorNode)item);
                else
                    throw UnknownContent(content);
            }
        }

        public override DeviceItem GetTestItem()
        {
            if (Id == -1)
                throw new InvalidOperationException("Id was not initialized");

            if (ParentId == -1)
                throw new InvalidOperationException("ParentId was not initialized");

            return new DeviceItem(
                group: Parent.Name,
                probe: GetProbe(this).Name,
                totalsens: TotalSensors.ToString(),
                totalsensRaw: TotalSensors.ToString(),
                name: Name,
                objid: Id.ToString(),
                parentId: ParentId.ToString()
            );
        }

        public List<SensorNode> GetSensors(bool recurse) => Sensors;
    }
}
