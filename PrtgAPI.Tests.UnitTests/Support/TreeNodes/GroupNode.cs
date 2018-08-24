using System;
using System.Collections.Generic;
using System.Diagnostics;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.TreeNodes
{
    [DebuggerDisplay("Name: {Name,nq} ({Id}), TotalGroups: {TotalGroups}, TotalDevices: {TotalDevices}, TotalSensors: {TotalSensors}")]
    class GroupNode : TreeNode<GroupItem>, IGroupContainer, IDeviceContainer
    {
        public List<GroupNode> Groups { get; set; } = new List<GroupNode>();
        public List<DeviceNode> Devices { get; set; } = new List<DeviceNode>();

        public int TotalGroups => GetTotalGroups(this);
        public int TotalDevices => GetTotalDevices(this);
        public int TotalSensors => GetTotalSensors(this);

        public GroupNode(string name, params ITreeNode[] content) : base(name)
        {
            foreach (var item in content)
            {
                if (item is GroupNode)
                    AddNode(Groups, (GroupNode)item);
                else if (item is DeviceNode)
                    AddNode(Devices, (DeviceNode)item);
                else
                    throw UnknownContent(content);
            }
        }

        public override GroupItem GetTestItem()
        {
            if (Id == -1)
                throw new InvalidOperationException("Id was not initialized");

            if (ParentId == -1)
                throw new InvalidOperationException("ParentId was not initialized");

            return new GroupItem(
                probe: GetProbe(this).Name,
                devicenum: TotalDevices.ToString(),
                devicenumRaw: TotalDevices.ToString(),
                groupnum: TotalGroups.ToString(),
                groupnumRaw: TotalGroups.ToString(),
                totalsens: TotalSensors.ToString(),
                totalsensRaw: TotalSensors.ToString(),
                name: Name,
                objid: Id.ToString(),
                parentId: ParentId.ToString()
            );
        }

        public List<SensorNode> GetSensors(bool recurse) => GetSensors(this, recurse);
        public List<DeviceNode> GetDevices(bool recurse) => GetDevices(this, recurse);
    }
}
