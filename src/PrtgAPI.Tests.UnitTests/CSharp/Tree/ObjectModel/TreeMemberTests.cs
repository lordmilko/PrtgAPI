using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Reflection;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tree;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tests.UnitTests.Tree
{
    [TestClass]
    public class TreeMemberTests : BaseTreeTest
    {
        #region Ancestors

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Ancestors()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.Device(Device(),
                        PrtgNode.Sensor(Sensor()),
                        PrtgNode.Sensor(Sensor("Ping"))
                    ),
                    PrtgNode.Device(Device("exch-1"))
                )
            );

            var servers = probe["Servers"];
            var dc = servers["dc-1"];
            var sensor = dc["VMware Datastore"];

            var ancestors = sensor.Ancestors().ToList();

            Assert.AreEqual(3, ancestors.Count);
            Assert.AreEqual(dc, ancestors[0]);
            Assert.AreEqual(servers, ancestors[1]);
            Assert.AreEqual(probe, ancestors[2]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_AncestorsAndSelf()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.Device(Device(),
                        PrtgNode.Sensor(Sensor()),
                        PrtgNode.Sensor(Sensor("Ping"))
                    ),
                    PrtgNode.Device(Device("exch-1"))
                )
            );

            var servers = probe["Servers"];
            var dc = servers["dc-1"];
            var sensor = dc["VMware Datastore"];

            var ancestors = sensor.AncestorsAndSelf().ToList();

            Assert.AreEqual(4, ancestors.Count);
            Assert.AreEqual(sensor, ancestors[0]);
            Assert.AreEqual(dc, ancestors[1]);
            Assert.AreEqual(servers, ancestors[2]);
            Assert.AreEqual(probe, ancestors[3]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Ancestors_Indexer_DuplicateName()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.Device(Device(),
                        PrtgNode.Sensor(Sensor()),
                        PrtgNode.Sensor(Sensor("Ping"))
                    ),
                    PrtgNode.Device(Device())
                )
            );

            var servers = probe["Servers"];
            var dc = servers["dc-1"];

            var ancestors = dc.Ancestors().ToList();

            Assert.AreEqual(2, ancestors.Count);
            Assert.AreEqual(servers, ancestors[0]);
            Assert.AreEqual(probe, ancestors[1]);
        }

        #endregion
        #region Descendants

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_DescendantNodes()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.Device(Device(),
                        PrtgNode.Sensor(Sensor()),
                        PrtgNode.Sensor(Sensor("Ping"))
                    ),
                    PrtgNode.Device(Device("exch-1"))
                )
            );

            var descendants = probe.DescendantNodes().ToList();

            Assert.AreEqual(5, descendants.Count);

            var group = probe["Servers"];
            var dc = group["dc-1"];
            var sensor1 = dc["VMware Datastore"];
            var sensor2 = dc["Ping"];
            var exch = group["exch-1"];

            Assert.AreEqual(group, descendants[0]);
            Assert.AreEqual(dc, descendants[1]);
            Assert.AreEqual(sensor1, descendants[2]);
            Assert.AreEqual(sensor2, descendants[3]);
            Assert.AreEqual(exch, descendants[4]);
        }

        #endregion
        #region Indexer
            #region TreeOrphan

        [UnitTest]
        [TestMethod]
        public void Tree_TreeOrphan_Indexer_SingleChild()
        {
            var tree = PrtgOrphan.Probe(Probe(),
                PrtgOrphan.Device(Device("Device1"))
            );

            var device = tree["Device1"];

            Assert.IsTrue(device is PrtgOrphan);
            Assert.IsFalse(device is PrtgOrphanCollection);
            Assert.AreEqual("Device1", device.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_TreeOrphan_Indexer_IgnoreCase()
        {
            var tree = PrtgOrphan.Probe(Probe(),
                PrtgOrphan.Device(Device("Device1"))
            );

            var device = tree["device1", true];

            Assert.IsTrue(device is PrtgOrphan);
            Assert.IsFalse(device is PrtgOrphanCollection);
            Assert.AreEqual("Device1", device.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_TreeOrphan_Indexer_MultipleChildren()
        {
            var tree = PrtgOrphan.Probe(Probe(),
                PrtgOrphan.Device(Device("Device1"),
                    PrtgOrphan.Sensor(Sensor("Ping"))
                ),
                PrtgOrphan.Device(Device("Device1"),
                    PrtgOrphan.Sensor(Sensor("Pong"))
                ),
                PrtgOrphan.Device(Device("Device2"))
            );

            var devices = tree["Device1"];

            Assert.IsTrue(devices is PrtgOrphanGrouping);
            Assert.AreEqual(2, ((PrtgOrphanGrouping) devices).Group.Count);
            Assert.AreEqual(2, devices.Children.Count);
            Assert.AreEqual("Ping", devices.Children[0].ToString());
            Assert.AreEqual("Pong", devices.Children[1].ToString());
            Assert.AreEqual("Device1", devices.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_TreeOrphan_Indexer_GrandChild_FromMultipleChindren()
        {
            var tree = PrtgOrphan.Probe(Probe(),
                PrtgOrphan.Device(Device("Device1"),
                    PrtgOrphan.Sensor(Sensor("Sensor1"))),
                PrtgOrphan.Device(Device("Device1"),
                    PrtgOrphan.Sensor(Sensor("Sensor2"))),
                PrtgOrphan.Device(Device("Device2"),
                    PrtgOrphan.Sensor(Sensor("Sensor1"))
                )
            );

            var devices = tree["Device1"];
            var sensor = devices["Sensor1"];

            Assert.IsTrue(sensor is SensorOrphan);
            Assert.AreEqual("Sensor1", sensor.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_TreeOrphan_Indexer_NoChildren()
        {
            var tree = PrtgOrphan.Probe(Probe(),
                PrtgOrphan.Device(Device("Device1"))
            );

            Assert.IsNull(tree["Device2"]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_TreeOrphan_Indexer_Collection()
        {
            var tree = PrtgOrphan.TriggerCollection(PrtgOrphan.Trigger(Trigger()));

            var child = tree["Email to Admin"];
            Assert.IsNotNull(child);
            Assert.AreEqual("Email to Admin", child.Name);
            Assert.AreEqual(tree.Children[0], child);
        }

            #endregion
            #region TreeNode

        [UnitTest]
        [TestMethod]
        public void Tree_TreeNode_Indexer_SingleChild()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var device = tree["Device1"];

            Assert.IsTrue(device is PrtgNode);
            Assert.IsFalse(device is PrtgNodeCollection);
            Assert.AreEqual("Device1", device.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_TreeNode_Indexer_IgnoreCase()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var device = tree["device1", true];

            Assert.IsTrue(device is PrtgNode);
            Assert.IsFalse(device is PrtgNodeCollection);
            Assert.AreEqual("Device1", device.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_TreeNode_Indexer_MultipleChildren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Ping"))
                ),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Pong"))
                ),
                PrtgNode.Device(Device("Device2"))
            );

            var devices = tree["Device1"];

            Assert.IsTrue(devices is PrtgNodeGrouping);
            Assert.AreEqual(2, ((PrtgNodeGrouping) devices).Group.Count);
            Assert.AreEqual(2, devices.Children.Count);
            Assert.AreEqual("Ping", devices.Children[0].ToString());
            Assert.AreEqual("Pong", devices.Children[1].ToString());
            Assert.AreEqual("Device1", devices.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_TreeNode_Indexer_GrandChild_FromMultipleChindren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Sensor1"))),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Sensor2"))),
                PrtgNode.Device(Device("Device2"),
                    PrtgNode.Sensor(Sensor("Sensor1"))
                )
            );

            var devices = tree["Device1"];
            var sensor = devices["Sensor1"];

            Assert.IsTrue(sensor is SensorNode);
            Assert.AreEqual("Sensor1", sensor.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_TreeNode_Indexer_NoChildren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            Assert.IsNull(tree["Device2"]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_TreeNode_Indexer_Collection()
        {
            var tree = PrtgNode.TriggerCollection(DefaultTrigger);

            var child = tree["Email to Admin"];
            Assert.IsNotNull(child);
            Assert.AreEqual("Email to Admin", child.Name);
            Assert.AreEqual(tree.Children[0], child);
        }

            #endregion
            #region CompareOrphan

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Indexer_SingleChild_Original()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device2"))
            );

            var comparison = first.CompareTo(second).Orphan;

            var device = comparison["Device1"];

            Assert.IsTrue(device is CompareOrphan);
            Assert.IsFalse(device is CompareOrphanCollection);
            Assert.AreEqual("Device1", device.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Indexer_SingleChild_New()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1")),
                PrtgNode.Device(Device("Device2"))
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device3"))
            );

            var comparison = first.CompareTo(second).Orphan;

            var device = comparison["Device1"];

            Assert.IsTrue(device is CompareOrphan);
            Assert.IsFalse(device is CompareOrphanCollection);
            Assert.AreEqual("Device1", device.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Indexer_IgnoreCase()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var comparison = tree.CompareTo(tree).Orphan;

            var device = comparison["device1", true];

            Assert.IsTrue(device is CompareOrphan);
            Assert.IsFalse(device is CompareOrphanCollection);
            Assert.AreEqual("Device1", device.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Indexer_MultipleChildren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Ping"))
                ),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Pong"))
                ),
                PrtgNode.Device(Device("Device2"))
            );

            var comparison = tree.CompareTo(tree).Orphan;

            var devices = comparison["Device1"];

            Assert.IsTrue(devices is CompareOrphanGrouping);
            Assert.AreEqual(2, ((CompareOrphanGrouping) devices).Group.Count);
            Assert.AreEqual(2, devices.Children.Count);
            Assert.AreEqual("Ping", devices.Children[0].ToString());
            Assert.AreEqual("Pong", devices.Children[1].ToString());
            Assert.AreEqual("Device1", devices.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Indexer_GrandChild_FromMultipleChindren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Sensor1"))),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Sensor2"))),
                PrtgNode.Device(Device("Device2"),
                    PrtgNode.Sensor(Sensor("Sensor1"))
                )
            );

            var comparison = tree.CompareTo(tree).Orphan;

            var devices = comparison["Device1"];
            var sensor = devices["Sensor1"];

            Assert.IsTrue(sensor.First is SensorNode);
            Assert.AreEqual("Sensor1", sensor.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Indexer_NoChildren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var comparison = tree.CompareTo(tree).Orphan;

            Assert.IsNull(comparison["Device2"]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Indexer_Collection()
        {
            var tree = PrtgNode.TriggerCollection(DefaultTrigger);

            var comparison = tree.CompareTo(tree).Orphan;

            var child = comparison["Email to Admin"];
            Assert.IsNotNull(child);
            Assert.AreEqual("Email to Admin", child.Name);
            Assert.AreEqual(comparison.Children[0], child);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Indexer_ReplacedOrphan()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device2", 3002))
            );

            var comparison = first.CompareTo(second).Orphan;

            var removedDevice = comparison["Device1"];
            var addedDevice = comparison["Device2"];

            Assert.AreEqual<TreeNodeDifference>(TreeNodeDifference.Removed, removedDevice.Difference);
            Assert.AreEqual<TreeNodeDifference>(TreeNodeDifference.Added, addedDevice.Difference);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Indexer_RenamedOrphan()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device2"))
            );

            var comparison = first.CompareTo(second).Orphan;

            var originalDevice = comparison["Device1"];
            var renamedDevice = comparison["Device2"];

            Assert.AreEqual(originalDevice, renamedDevice);
        }

            #endregion
            #region CompareNode

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Indexer_SingleChild_Original()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device2"))
            );

            var comparison = first.CompareTo(second);

            var device = comparison["Device1"];

            Assert.IsTrue(device is CompareNode);
            Assert.IsFalse(device is CompareNodeCollection);
            Assert.AreEqual("Device1", device.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Indexer_SingleChild_New()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1")),
                PrtgNode.Device(Device("Device2"))
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device3"))
            );

            var comparison = first.CompareTo(second);

            var device = comparison["Device1"];

            Assert.IsTrue(device is CompareNode);
            Assert.IsFalse(device is CompareNodeCollection);
            Assert.AreEqual("Device1", device.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Indexer_IgnoreCase()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var comparison = tree.CompareTo(tree);

            var device = comparison["device1", true];

            Assert.IsTrue(device is CompareNode);
            Assert.IsFalse(device is CompareNodeCollection);
            Assert.AreEqual("Device1", device.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Indexer_MultipleChildren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Ping"))
                ),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Pong"))
                ),
                PrtgNode.Device(Device("Device2"))
            );

            var comparison = tree.CompareTo(tree);

            var devices = comparison["Device1"];

            Assert.IsTrue(devices is CompareNodeGrouping);
            Assert.AreEqual(2, ((CompareNodeGrouping) devices).Group.Count);
            Assert.AreEqual(2, devices.Children.Count);
            Assert.AreEqual("Ping", devices.Children[0].ToString());
            Assert.AreEqual("Pong", devices.Children[1].ToString());
            Assert.AreEqual("Device1", devices.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Indexer_GrandChild_FromMultipleChindren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Sensor1"))),
                PrtgNode.Device(Device("Device1"),
                    PrtgNode.Sensor(Sensor("Sensor2"))),
                PrtgNode.Device(Device("Device2"),
                    PrtgNode.Sensor(Sensor("Sensor1"))
                )
            );

            var comparison = tree.CompareTo(tree);

            var devices = comparison["Device1"];
            var sensor = devices["Sensor1"];

            Assert.IsTrue(sensor.First is SensorNode);
            Assert.AreEqual("Sensor1", sensor.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Indexer_NoChildren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var comparison = tree.CompareTo(tree);

            Assert.IsNull(comparison["Device2"]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Indexer_Collection()
        {
            var tree = PrtgNode.TriggerCollection(DefaultTrigger);

            var comparison = tree.CompareTo(tree);

            var child = comparison["Email to Admin"];
            Assert.IsNotNull(child);
            Assert.AreEqual("Email to Admin", child.Name);
            Assert.AreEqual(comparison.Children[0], child);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Indexer_ReplacedNode()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device2", 3002))
            );

            var comparison = first.CompareTo(second);

            var removedDevice = comparison["Device1"];
            var addedDevice = comparison["Device2"];

            Assert.AreEqual<TreeNodeDifference>(TreeNodeDifference.Removed, removedDevice.Difference);
            Assert.AreEqual<TreeNodeDifference>(TreeNodeDifference.Added, addedDevice.Difference);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Indexer_RenamedNode()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device1"))
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Device2"))
            );

            var comparison = first.CompareTo(second);

            var originalDevice = comparison["Device1"];
            var renamedDevice = comparison["Device2"];

            Assert.AreEqual(originalDevice, renamedDevice);
        }

            #endregion
        #endregion
        #region Find
            #region PrtgOrphan

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_FindOrphan_Untyped()
        {
            var parent = PrtgOrphan.Group(Group(),
                PrtgOrphan.Device(Device("Child1"),
                    PrtgOrphan.Sensor(Sensor("Child3"))
                ),
                PrtgOrphan.Device(Device("Child2"))
            );

            var match = parent.FindOrphan(n => n.Name == "Child3");

            Assert.AreEqual("Child3", match.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_FindOrphan_Untyped_NoMatches()
        {
            var parent = PrtgOrphan.Group(Group(),
                PrtgOrphan.Device(Device("Child1"),
                    PrtgOrphan.Sensor(Sensor("Child3"))
                ),
                PrtgOrphan.Device(Device("Child2"))
            );

            var match = parent.FindOrphan(n => n.Name == "Child4");

            Assert.IsNull(match);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_FindOrphan_Untyped_MultipleMatches_Throws()
        {
            var parent = PrtgOrphan.Group(Group(),
                PrtgOrphan.Device(Device("Child"),
                    PrtgOrphan.Sensor(Sensor("Child"))
                ),
                PrtgOrphan.Device(Device("Child"))
            );

            AssertEx.Throws<InvalidOperationException>(
                () => parent.FindOrphan(n => n.Name == "Child"),
                "Sequence contains more than one element"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_FindOrphans_Untyped()
        {
            var parent = PrtgOrphan.Group(Group(),
                PrtgOrphan.Device(Device("Child"),
                    PrtgOrphan.Sensor(Sensor("Child"))
                ),
                PrtgOrphan.Device(Device("Child"))
            );

            var matches = parent.FindOrphans(n => n.Name == "Child");

            Assert.AreEqual(3, matches.Count());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_FindOrphan_Typed()
        {
            var parent = PrtgOrphan.Group(Group(),
                PrtgOrphan.Device(Device("Child"),
                    PrtgOrphan.Sensor(Sensor("Child"))
                ),
                PrtgOrphan.Device(Device("Child"))
            );

            var match = parent.FindOrphan<SensorOrphan>(n => n.Name == "Child");

            Assert.IsNotNull(match);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_FindOrphan_Typed_NoMatches()
        {
            var parent = PrtgOrphan.Group(Group(),
                PrtgOrphan.Device(Device("Child"),
                    PrtgOrphan.Sensor(Sensor("Child"))
                ),
                PrtgOrphan.Device(Device("Child"))
            );

            var match = parent.FindOrphan<SensorOrphan>(n => n.Name == "Child1");

            Assert.IsNull(match);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_FindOrphan_Typed_MultipleMatches_Throws()
        {
            var parent = PrtgOrphan.Group(Group(),
                PrtgOrphan.Device(Device("Child"),
                    PrtgOrphan.Sensor(Sensor("Child"))
                ),
                PrtgOrphan.Device(Device("Child"))
            );

            AssertEx.Throws<InvalidOperationException>(
                () => parent.FindOrphan<DeviceOrphan>(n => n.Name == "Child"),
                "Sequence contains more than one element"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_FindOrphans_Typed()
        {
            var parent = PrtgOrphan.Group(Group(),
                PrtgOrphan.Device(Device("Child"),
                    PrtgOrphan.Sensor(Sensor("Child"))
                ),
                PrtgOrphan.Device(Device("Child"))
            );

            var matches = parent.FindOrphans<DeviceOrphan>(n => n.Name == "Child");

            Assert.AreEqual(2, matches.Count());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_FindOrphans_NullRoot()
        {
            var result = OrphanExtensions.FindOrphan((PrtgOrphan) null, s => true);

            Assert.IsNull(result);
        }

            #endregion
            #region PrtgNode

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_FindNode_Untyped()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child1"),
                    PrtgNode.Sensor(Sensor("Child3"))
                ),
                PrtgNode.Device(Device("Child2"))
            );

            var match = parent.FindNode(n => n.Name == "Child3");

            Assert.AreEqual("Child3", match.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_FindNode_Untyped_NoMatches()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child1"),
                    PrtgNode.Sensor(Sensor("Child3"))
                ),
                PrtgNode.Device(Device("Child2"))
            );

            var match = parent.FindNode(n => n.Name == "Child4");

            Assert.IsNull(match);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_FindNode_Untyped_MultipleMatches_Throws()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child"),
                    PrtgNode.Sensor(Sensor("Child"))
                ),
                PrtgNode.Device(Device("Child"))
            );

            AssertEx.Throws<InvalidOperationException>(
                () => parent.FindNode(n => n.Name == "Child"),
                "Sequence contains more than one element"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_FindNodes_Untyped()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child"),
                    PrtgNode.Sensor(Sensor("Child"))
                ),
                PrtgNode.Device(Device("Child"))
            );

            var matches = parent.FindNodes(n => n.Name == "Child");

            Assert.AreEqual(3, matches.Count());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_FindNode_Typed()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child"),
                    PrtgNode.Sensor(Sensor("Child"))
                ),
                PrtgNode.Device(Device("Child"))
            );

            var match = parent.FindNode<SensorNode>(n => n.Name == "Child");

            Assert.IsNotNull(match);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_FindNode_Typed_NoMatches()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child"),
                    PrtgNode.Sensor(Sensor("Child"))
                ),
                PrtgNode.Device(Device("Child"))
            );

            var match = parent.FindNode<SensorNode>(n => n.Name == "Child1");

            Assert.IsNull(match);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_FindNode_Typed_MultipleMatches_Throws()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child"),
                    PrtgNode.Sensor(Sensor("Child"))
                ),
                PrtgNode.Device(Device("Child"))
            );

            AssertEx.Throws<InvalidOperationException>(
                () => parent.FindNode<DeviceNode>(n => n.Name == "Child"),
                "Sequence contains more than one element"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_FindNodes_Typed()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child"),
                    PrtgNode.Sensor(Sensor("Child"))
                ),
                PrtgNode.Device(Device("Child"))
            );

            var matches = parent.FindNodes<DeviceNode>(n => n.Name == "Child");

            Assert.AreEqual(2, matches.Count());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_FindNodes_NullRoot()
        {
            var result = NodeExtensions.FindNode((PrtgNode) null, s => true);

            Assert.IsNull(result);
        }

            #endregion
            #region CompareNode

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_FindNode()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child1"),
                    PrtgNode.Sensor(Sensor("Child3"))
                ),
                PrtgNode.Device(Device("Child2"))
            );

            var comparison = parent.CompareTo(parent);

            var match = comparison.FindNode(n => n.Name == "Child3");

            Assert.AreEqual("Child3", match.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_FindNode_NoMatches()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child1"),
                    PrtgNode.Sensor(Sensor("Child3"))
                ),
                PrtgNode.Device(Device("Child2"))
            );

            var comparison = parent.CompareTo(parent);

            var match = comparison.FindNode(n => n.Name == "Child4");

            Assert.IsNull(match);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_FindNode_MultipleMatches_Throws()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child"),
                    PrtgNode.Sensor(Sensor("Child"))
                ),
                PrtgNode.Device(Device("Child"))
            );

            var comparison = parent.CompareTo(parent);

            AssertEx.Throws<InvalidOperationException>(
                () => comparison.FindNode(n => n.Name == "Child"),
                "Sequence contains more than one element"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_FindNodes_WithPredicate()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child"),
                    PrtgNode.Sensor(Sensor("Child"))
                ),
                PrtgNode.Device(Device("Child"))
            );

            var comparison = parent.CompareTo(parent);

            var matches = comparison.FindNodes(n => n.Name == "Child");

            Assert.AreEqual(3, matches.Count());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_FindNodes_NoPredicate()
        {
            var parent = PrtgNode.Group(Group(),
                PrtgNode.Device(Device("Child"),
                    PrtgNode.Sensor(Sensor("Child"))
                ),
                PrtgNode.Device(Device("Child"))
            );

            var comparison = parent.CompareTo(parent);

            var matches = comparison.FindNodes(null);

            Assert.AreEqual(3, matches.Count());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_FindNodes_NullRoot()
        {
            var result = NodeExtensions.FindNode((CompareNode) null, s => true);

            Assert.IsNull(result);
        }

            #endregion
        #endregion
        #region Insert

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_InsertNodeAfter()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var newProbe = probe.InsertNodeAfter(probe["dc-1"], PrtgNode.Group(Group()));

            Assert.AreNotEqual(probe, newProbe);
            Assert.IsTrue(newProbe["dc-1"] == newProbe.Children[0]);
            Assert.IsTrue(newProbe["Servers"] == newProbe.Children[1]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_InsertNodeBefore()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var newProbe = probe.InsertNodeBefore(probe["dc-1"], PrtgNode.Group(Group()));

            Assert.AreNotEqual(probe, newProbe);
            Assert.IsTrue(newProbe["Servers"] == newProbe.Children[0]);
            Assert.IsTrue(newProbe["dc-1"] == newProbe.Children[1]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_InsertNodesAfter()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var newProbe = probe.InsertNodesAfter(
                probe["dc-1"],
                PrtgNode.Group(Group()),
                PrtgNode.Group(Group("Linux"))
            );

            Assert.AreNotEqual(probe, newProbe);
            Assert.IsTrue(newProbe["dc-1"] == newProbe.Children[0]);
            Assert.IsTrue(newProbe["Servers"] == newProbe.Children[1]);
            Assert.IsTrue(newProbe["Linux"] == newProbe.Children[2]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_InsertNodesBefore()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var newProbe = probe.InsertNodesBefore(
                probe["dc-1"],
                PrtgNode.Group(Group()),
                PrtgNode.Group(Group("Linux"))
            );

            Assert.AreNotEqual(probe, newProbe);
            Assert.IsTrue(newProbe["Servers"] == newProbe.Children[0]);
            Assert.IsTrue(newProbe["Linux"] == newProbe.Children[1]);
            Assert.IsTrue(newProbe["dc-1"] == newProbe.Children[2]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_InsertNodeAfter_NullRoot()
        {
            var result = NodeExtensions.InsertNodeAfter<DeviceNode>(null, DefaultDevice, DefaultDevice);

            Assert.IsNull(result);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_InsertNodeBefore_NullRoot()
        {
            var result = NodeExtensions.InsertNodeBefore<DeviceNode>(null, DefaultDevice, DefaultDevice);

            Assert.IsNull(result);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_InsertNodeAfter_NullNodeInList()
        {
            var root = DefaultDevice;

            var result = root.InsertNodeAfter(null, DefaultDevice);

            Assert.AreEqual(root, result);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_InsertNodeBefore_NullNodeInList()
        {
            var root = DefaultDevice;

            var result = root.InsertNodeBefore(null, DefaultDevice);

            Assert.AreEqual(root, result);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_InsertNodeAfter_NullNewNode()
        {
            var probe = DefaultProbe.WithChildren(DefaultDevice);

            AssertEx.Throws<ArgumentNullException>(
                () => probe.InsertNodeAfter(probe["dc-1"], null),
                "List contained a null element."
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_InsertNodeBefore_NullNewNode()
        {
            var probe = DefaultProbe.WithChildren(DefaultDevice);

            AssertEx.Throws<ArgumentNullException>(
                () => probe.InsertNodeBefore(probe["dc-1"], null),
                "List contained a null element."
            );
        }

        #endregion
        #region RemoveNodes

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_RemoveNodes_SingleChild()
        {
            var device = PrtgNode.Device(Device(),
                PrtgNode.Sensor(Sensor()),
                PrtgNode.Sensor(Sensor("Ping"))
            );

            var ping = device["Ping"];

            Assert.AreEqual(2, device.Children.Count);
            Assert.IsNotNull(ping);

            var deviceWithoutPing = device.RemoveNodes(ping);

            Assert.AreEqual(1, deviceWithoutPing.Children.Count);
            Assert.IsNull(deviceWithoutPing["Ping"]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_RemoveNodes_SingleGrandChild()
        {
            var group = PrtgNode.Group(Group(),
                PrtgNode.Device(Device(),
                    PrtgNode.Sensor(Sensor()),
                    PrtgNode.Sensor(Sensor("Ping"))
                )
            );

            var ping = group.FindNode<SensorNode>(s => s.Name == "Ping");

            Assert.IsNotNull(ping);

            var newGroup = group.RemoveNode(ping);

            var newPing = newGroup.FindNode<SensorNode>(s => s.Name == "Ping");

            Assert.IsNull(newPing);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_RemoveNode_NullRoot()
        {
            var result = NodeExtensions.RemoveNode<DeviceNode>(null, DefaultDevice);

            Assert.IsNull(result);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_RemoveNode_NullNode()
        {
            var root = DefaultDevice;

            var result = root.RemoveNode(null);

            Assert.AreEqual(root, result);
        }

        #endregion
        #region ReplaceNodes

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ReplaceNode_SameNode()
        {
            var device = PrtgNode.Device(Device(),
                PrtgNode.Sensor(Sensor()),
                PrtgNode.Sensor(Sensor("Ping"))
            );

            var ping = device["Ping"];
            Assert.IsNotNull(ping);

            var sameDevice = device.ReplaceNode(ping, ping);
            Assert.AreEqual(device, sameDevice);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ReplaceNodes_SingleChild()
        {
            var device = PrtgNode.Device(Device(),
                PrtgNode.Sensor(Sensor()),
                PrtgNode.Sensor(Sensor("Ping"))
            );

            Assert.IsNotNull(device["Ping"]);
            Assert.IsNull(device["Pong"]);

            var newDevice = device.ReplaceNode(device["Ping"], PrtgNode.Sensor(Sensor("Pong")));

            Assert.IsNull(newDevice["Ping"]);
            Assert.IsNotNull(newDevice["Pong"]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ReplaceNodes_SingleGrandChild()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group("Servers")),
                PrtgNode.Device(Device(),
                    PrtgNode.Sensor(Sensor("Pong"))
                )
            );

            var pong = probe["dc-1"]["Pong"];
            Assert.IsNotNull(pong);

            var newProbe = probe.ReplaceNode(pong, PrtgNode.Sensor(Sensor("Ping")));

            Assert.IsNull(newProbe["dc-1"]["Pong"]);
            Assert.IsNotNull(newProbe["dc-1"]["Ping"]);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ReplaceNodes_MultipleNodes()
        {
            var device = PrtgNode.Device(Device(),
                PrtgNode.Sensor(Sensor()),
                PrtgNode.Sensor(Sensor("Ping"))
            );

            var newNode = device.ReplaceNodes(device.Children, (o, r) =>
            {
                if (o.Name == "VMware Datastore")
                    return PrtgNode.Trigger(Trigger());

                return null;
            });

            Assert.AreNotEqual(device, newNode);
            Assert.AreEqual(1, newNode.Children.Count);
            Assert.AreEqual("Email to Admin", newNode.Children[0].Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ReplaceNode_NullRoot()
        {
            var result = NodeExtensions.ReplaceNode<DeviceNode>(null, DefaultDevice, DefaultDevice);

            Assert.IsNull(result);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ReplaceNode_NullOldNode()
        {
            var root = DefaultDevice;

            var result = root.ReplaceNode(null, DefaultDevice);

            Assert.AreEqual(root, result);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ReplaceNode_NullNewNode()
        {
            var root = DefaultProbe.WithChildren(DefaultDevice);

            AssertEx.Throws<ArgumentNullException>(
                () => root.ReplaceNode(root["dc-1"], null),
                "List contained a null element."
            );
        }

        #endregion
        #region Update
            #region PrtgOrphan

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_Update_WithChildren()
        {
            var excluded = new[]
            {
                PrtgNodeType.Trigger,
                PrtgNodeType.Property,
                PrtgNodeType.Collection,
                PrtgNodeType.Grouping
            };

            var types = Enum.GetValues(typeof(PrtgNodeType)).Cast<PrtgNodeType>().Where(v => !excluded.Contains(v)).ToArray();

            foreach (var type in types)
            {
                var original = CompleteOrphanTree();

                var orphan = original[type];

                var sameOrphan = orphan.WithChildren(orphan.Children);
                Assert.AreEqual(orphan, sameOrphan);

                //params array won't be reference equal to the existing children
                var newOrphan = orphan.WithChildren(orphan.Children.ToArray());

                CompareOrphans(orphan, newOrphan);
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_Update_WithValue()
        {
            var excluded = new[]
            {
                PrtgNodeType.Collection,
                PrtgNodeType.Grouping
            };

            var types = Enum.GetValues(typeof(PrtgNodeType)).Cast<PrtgNodeType>().Where(v => !excluded.Contains(v)).ToArray();

            var memberwiseClone = typeof(object).GetInternalMethod("MemberwiseClone");

            foreach (var type in types)
            {
                var original = CompleteOrphanTree();

                var orphan = original[type];
                var clonedValue = memberwiseClone.Invoke(orphan.Value, null);
                Assert.AreNotEqual(orphan.Value, clonedValue);

                var withValue = orphan.GetType().GetMethod("WithValue", BindingFlags.Instance | BindingFlags.NonPublic);

                var sameNode = withValue.Invoke(orphan, new[] { orphan.Value });
                Assert.AreEqual(orphan, sameNode);

                var newOrphan = (PrtgOrphan) withValue.Invoke(orphan, new[] { clonedValue });

                CompareOrphans(orphan, newOrphan);
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_Update_WithChildren_WithGrouping_RemovesGrouping()
        {
            var tree = PrtgOrphan.Device(Device(),
                PrtgOrphan.Sensor(Sensor()),
                PrtgOrphan.Sensor(Sensor())
            );

            var grouping = tree["VMware Datastore"];
            Assert.AreEqual(PrtgNodeType.Grouping, grouping.Type);

            var newTree = PrtgOrphan.Device(Device("exch-1"), grouping);
            Assert.AreEqual(2, newTree.Children.Count);

            AssertEx.Throws<InvalidOperationException>(
                () => PrtgOrphan.Group(Group(), grouping),
                "Node 'VMware Datastore (ID: 4001)' of type 'Sensor' cannot be a child of a node of type 'Group'."
            );
        }

        private void CompareOrphans(PrtgOrphan oldOrphan, PrtgOrphan newOrphan)
        {
            Assert.AreNotEqual(oldOrphan, newOrphan);

            var oldDescendants = oldOrphan.DescendantOrphans().ToArray();
            var newDescendants = newOrphan.DescendantOrphans().ToArray();

            var zipped = oldDescendants.Zip(newDescendants, Tuple.Create);
            
            Assert.AreEqual(oldDescendants.Length, newDescendants.Length);

            foreach (var tuple in zipped)
            {
                Assert.AreEqual(tuple.Item1.GetType(), tuple.Item2.GetType());
                Assert.AreEqual(tuple.Item1, tuple.Item2);
            }
        }

        private Dictionary<PrtgNodeType, PrtgOrphan> CompleteOrphanTree()
        {
            var probe = PrtgOrphan.Probe(Probe(),
                PrtgOrphan.Group(Group(),
                    PrtgOrphan.Device(Device(),
                        PrtgOrphan.Sensor(Sensor(),
                            PrtgOrphan.Trigger(Trigger()),
                            PrtgOrphan.Property(Property())
                        )
                    )
                )
            );

            return new Dictionary<PrtgNodeType, PrtgOrphan>
            {
                [PrtgNodeType.Probe] = probe,
                [PrtgNodeType.Group] = probe.FindOrphan<GroupOrphan>(),
                [PrtgNodeType.Device] = probe.FindOrphan<DeviceOrphan>(),
                [PrtgNodeType.Sensor] = probe.FindOrphan<SensorOrphan>(),
                [PrtgNodeType.Trigger] = probe.FindOrphan<TriggerOrphan>(),
                [PrtgNodeType.Property] = probe.FindOrphan<PropertyOrphan>(),
            };
        }

            #endregion
            #region PrtgNode

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Update_WithChildren()
        {
            var excluded = new[]
            {
                PrtgNodeType.Trigger,
                PrtgNodeType.Property,
                PrtgNodeType.Collection,
                PrtgNodeType.Grouping
            };

            var types = Enum.GetValues(typeof(PrtgNodeType)).Cast<PrtgNodeType>().Where(v => !excluded.Contains(v)).ToArray();

            foreach (var type in types)
            {
                var original = CompleteTree();

                var node = original[type];

                var sameNode = node.WithChildren(node.Children);
                Assert.AreEqual(node, sameNode);

                //params array won't be reference equal to the existing children
                var newNode = node.WithChildren(node.Children.ToArray());

                CompareNodes(node, newNode);
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Update_WithValue()
        {
            var excluded = new[]
            {
                PrtgNodeType.Collection,
                PrtgNodeType.Grouping
            };

            var types = Enum.GetValues(typeof(PrtgNodeType)).Cast<PrtgNodeType>().Where(v => !excluded.Contains(v)).ToArray();

            var memberwiseClone = typeof(object).GetInternalMethod("MemberwiseClone");

            foreach (var type in types)
            {
                var original = CompleteTree();

                var node = original[type];
                var clonedValue = memberwiseClone.Invoke(node.Value, null);
                Assert.AreNotEqual(node.Value, clonedValue);

                var withValue = node.GetType().GetMethod("WithValue");

                var sameNode = withValue.Invoke(node, new[] {node.Value});
                Assert.AreEqual(node, sameNode);

                var newNode = (PrtgNode) withValue.Invoke(node, new[] {clonedValue});

                CompareNodes(node, newNode);
            }
        }

        private void CompareNodes(PrtgNode oldNode, PrtgNode newNode)
        {
            if (oldNode is ProbeNode)
                Assert.IsNull(oldNode.Parent, $"Expected the parent of '{oldNode}' of type '{oldNode.GetType().Name}' to be null");
            else
                Assert.IsNotNull(oldNode.Parent, $"Expected '{oldNode}' of type '{oldNode.GetType().Name}' to have a parent");

            Assert.IsNull(newNode.Parent, $"Expected the parent of '{newNode}' of type '{newNode.GetType().Name}' to be null");

            var oldDescendants = oldNode.DescendantNodesAndSelf().ToArray();
            var newDescendants = newNode.DescendantNodesAndSelf().ToArray();

            var zipped = oldDescendants.Zip(newDescendants, Tuple.Create);

            Assert.AreEqual(oldDescendants.Length, newDescendants.Length);

            foreach (var tuple in zipped)
            {
                Assert.AreEqual(tuple.Item1.GetType(), tuple.Item2.GetType());
                Assert.AreNotEqual(tuple.Item1, tuple.Item2);
            }
        }

        private Dictionary<PrtgNodeType, PrtgNode> CompleteTree()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.Device(Device(),
                        PrtgNode.Sensor(Sensor(),
                            PrtgNode.Trigger(Trigger()),
                            PrtgNode.Property(Property())
                        )
                    )
                )
            );

            return new Dictionary<PrtgNodeType, PrtgNode>
            {
                [PrtgNodeType.Probe] = probe,
                [PrtgNodeType.Group] = probe.FindNode<GroupNode>(),
                [PrtgNodeType.Device] = probe.FindNode<DeviceNode>(),
                [PrtgNodeType.Sensor] = probe.FindNode<SensorNode>(),
                [PrtgNodeType.Trigger] = probe.FindNode<TriggerNode>(),
                [PrtgNodeType.Property] = probe.FindNode<PropertyNode>(),
            };
        }

            #endregion
            #region CompareOrphan

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Update_WithChildren()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device()),
                PrtgNode.Device(Device(id: 3002))
            );

            var second = PrtgNode.Probe(Probe(id: 1002));

            var comparison = first.CompareTo(second).Orphan;
            var devices = comparison["Local Probe"]["dc-1"] as CompareOrphanGrouping;

            Assert.IsInstanceOfType(comparison, typeof(CompareOrphanRoot));
            Assert.IsInstanceOfType(devices, typeof(CompareOrphanGrouping));
            Assert.AreEqual(typeof(CompareOrphan), devices.Group[0].GetType());

            Assert.AreEqual(2, comparison.Children.Count);
            Assert.AreEqual(2, devices.Group.Count);

            var newParent = devices.Group[0].WithChildren(comparison, devices);
            Assert.AreEqual(4, newParent.Children.Count);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Update_Root_WithNull()
        {
            var comparison = GetCompareNodeUpdateTree().Orphan;

            Assert.IsNull(comparison.Update(null));
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Update_Root_NoChildren()
        {
            var comparison = GetCompareNodeUpdateTree().Orphan;

            Assert.IsNull(comparison.Update(Enumerable.Empty<CompareOrphan>()));
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Update_Root_OneChild()
        {
            var comparison = GetCompareNodeUpdateTree().Orphan;

            var newNode = comparison.Update(new[] { comparison.Children.First() });

            Assert.AreEqual(typeof(CompareOrphan), newNode.GetType());
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Update_Root_OneNullChild()
        {
            var comparison = GetCompareNodeUpdateTree().Orphan;

            var newNode = comparison.Update(new CompareOrphan[] { null });

            Assert.IsNull(newNode);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Update_Root_TwoChildren()
        {
            var comparison = GetCompareNodeUpdateTree().Orphan;

            var newNode = comparison.Update(comparison.Children.ToList());

            Assert.IsInstanceOfType(newNode, typeof(CompareOrphanRoot));
            Assert.AreNotEqual(comparison, newNode);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Update_Root_TwoChildren_OneNull()
        {
            var comparison = GetCompareNodeUpdateTree().Orphan;

            AssertEx.Throws<ArgumentNullException>(
                () => comparison.Update(new[] { null, comparison.Children[0] }),
                "List contained a null element"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_Update_Root_OverTwoChildren_Throws()
        {
            var comparison = GetCompareNodeUpdateTree().Orphan;

            AssertEx.Throws<InvalidOperationException>(
                () => comparison.Update(new[] { comparison.Children[0], comparison.Children[1], comparison.Children[0] }),
                "A CompareOrphanRoot cannot have more than two children."
            );
        }

            #endregion
            #region CompareNode

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Update_WithChildren()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device()),
                PrtgNode.Device(Device(id: 3002))
            );

            var second = PrtgNode.Probe(Probe(id: 1002));

            var comparison = first.CompareTo(second);
            var devices = comparison["Local Probe"]["dc-1"] as CompareNodeGrouping;

            Assert.IsInstanceOfType(comparison, typeof(CompareNodeRoot));
            Assert.IsInstanceOfType(devices, typeof(CompareNodeGrouping));
            Assert.AreEqual(typeof(CompareNode), devices.Group[0].GetType());

            Assert.AreEqual(2, comparison.Children.Count);
            Assert.AreEqual(2, devices.Group.Count);

            var newParent = devices.Group[0].WithChildren(comparison, devices);
            Assert.AreEqual(4, newParent.Children.Count);
        }

                #region Root

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Update_Root_WithNull()
        {
            var comparison = GetCompareNodeUpdateTree();

            Assert.IsNull(comparison.Update(null));
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Update_Root_NoChildren()
        {
            var comparison = GetCompareNodeUpdateTree();

            Assert.IsNull(comparison.Update(Enumerable.Empty<CompareNode>()));
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Update_Root_OneChild()
        {
            var comparison = GetCompareNodeUpdateTree();

            var newNode = comparison.Update(new[]{comparison.Children.First()});

            Assert.AreEqual(typeof(CompareNode), newNode.GetType());
            Assert.IsNull(newNode.Parent);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Update_Root_OneNullChild()
        {
            var comparison = GetCompareNodeUpdateTree();

            var newNode = comparison.Update(new CompareNode[] { null });

            Assert.IsNull(newNode);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Update_Root_TwoChildren()
        {
            var comparison = GetCompareNodeUpdateTree();

            var newNode = comparison.Update(comparison.Children.ToList());

            Assert.IsInstanceOfType(newNode, typeof(CompareNodeRoot));
            Assert.AreNotEqual(comparison, newNode);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Update_Root_TwoChildren_OneNull()
        {
            var comparison = GetCompareNodeUpdateTree();

            AssertEx.Throws<ArgumentNullException>(
                () => comparison.Update(new[] { null, comparison.Children[0] }),
                "List contained a null element"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Update_Root_OverTwoChildren_Throws()
        {
            var comparison = GetCompareNodeUpdateTree();

            AssertEx.Throws<InvalidOperationException>(
                () => comparison.Update(new[] { comparison.Children[0], comparison.Children[1], comparison.Children[0] }),
                "A CompareNodeRoot cannot have more than two children."
            );
        }

        private CompareNode GetCompareNodeUpdateTree()
        {
            var first = PrtgNode.Probe(Probe());
            var second = PrtgNode.Probe(Probe(id: 1002));

            var comparison = first.CompareTo(second);
            Assert.IsInstanceOfType(comparison, typeof(CompareNodeRoot));

            return comparison;
        }

                #endregion
                #region Normal

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Update_WithNull()
        {
            var node = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var comparison = node.CompareTo(node);

            Assert.AreEqual(1, comparison.Children.Count);

            var result = comparison.Update(null);

            Assert.AreEqual(0, result.Children.Count);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Update_OneNullChild()
        {
            var node = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var comparison = node.CompareTo(node);

            Assert.AreEqual(1, comparison.Children.Count);

            AssertEx.Throws<ArgumentNullException>(
                () => comparison.Update(new[] { null, comparison["dc-1"] }),
                "List contained a null element"
            );
        }

                #endregion
            #endregion
        #endregion
        #region ValidChildren
            #region PrtgOrphan

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_ValidChildren_Sensor()
        {
            var valid = new PrtgOrphan[]
            {
                DefaultTrigger.Orphan,
                DefaultProperty.Orphan,
                PrtgOrphan.PropertyCollection((PropertyOrphan) DefaultProperty.Orphan)
            };

            var invalid = new PrtgOrphan[]
            {
                DefaultSensor.Orphan,
                DefaultDevice.Orphan,
                DefaultGroup.Orphan,
                DefaultProbe.Orphan,
                PrtgOrphan.Grouping(DefaultSensor.Orphan)
            };

            ValidateChildren(DefaultSensor.Orphan, valid, invalid);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_ValidChildren_Device()
        {
            var valid = new PrtgOrphan[]
            {
                DefaultSensor.Orphan,
                DefaultTrigger.Orphan,
                DefaultProperty.Orphan,
                PrtgOrphan.PropertyCollection((PropertyOrphan) DefaultProperty.Orphan)
            };

            var invalid = new PrtgOrphan[]
            {
                DefaultDevice.Orphan,
                DefaultGroup.Orphan,
                DefaultProbe.Orphan,
                PrtgOrphan.Grouping(DefaultDevice.Orphan)
            };

            ValidateChildren(DefaultDevice.Orphan, valid, invalid);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_ValidChildren_Group()
        {
            var valid = new PrtgOrphan[]
            {
                DefaultDevice.Orphan,
                DefaultGroup.Orphan,
                DefaultTrigger.Orphan,
                DefaultProperty.Orphan,
                PrtgOrphan.PropertyCollection(PrtgOrphan.Property(Device(), ObjectProperty.Name, "test"))
            };

            var invalid = new PrtgOrphan[]
            {
                DefaultSensor.Orphan,
                PrtgOrphan.Grouping(DefaultSensor.Orphan)
            };

            AssertEx.Throws<InvalidOperationException>(
                () => DefaultGroup.WithChildren(DefaultProbe),
                "Node 'Local Probe (ID: 1001)' of type 'Probe' cannot be a child of a group that is not the PRTG Root Node."
            );

            AssertEx.Throws<InvalidOperationException>(
                () => DefaultGroup.Orphan.WithChildren(PrtgOrphan.Group(Group(id: 0))),
                "Cannot add the PRTG Root Node as the child of another group."
            );

            ValidateChildren(DefaultGroup.Orphan, valid, invalid);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_ValidChildren_Probe()
        {
            var valid = new PrtgOrphan[]
            {
                DefaultDevice.Orphan,
                DefaultGroup.Orphan,
                PrtgOrphan.PropertyCollection((PropertyOrphan) DefaultProperty.Orphan)
            };

            var invalid = new PrtgOrphan[]
            {
                DefaultSensor.Orphan,
                PrtgOrphan.Grouping(DefaultSensor.Orphan)
            };

            AssertEx.Throws<InvalidOperationException>(
                () => DefaultProbe.Orphan.WithChildren(PrtgOrphan.Group(Group(id: 0))),
                "Cannot add the PRTG Root Node as the child of a probe."
            );

            ValidateChildren(DefaultProbe.Orphan, valid, invalid);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_ValidChildren_Trigger()
        {
            var invalid = new PrtgOrphan[]
            {
                DefaultSensor.Orphan,
                DefaultDevice.Orphan,
                DefaultGroup.Orphan,
                DefaultProbe.Orphan,
                DefaultTrigger.Orphan,
                DefaultProperty.Orphan,
                PrtgOrphan.PropertyCollection((PropertyOrphan) DefaultProperty.Orphan),
                PrtgOrphan.Grouping(DefaultSensor.Orphan)
            };

            var parent = DefaultTrigger.Orphan;

            foreach (var orphan in invalid)
            {
                AssertEx.Throws<InvalidOperationException>(
                    () => parent.WithChildren(orphan),
                    "Cannot add children to orphan of type 'Trigger': orphan does not support children."
                );
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_ValidChildren_Property()
        {
            var invalid = new PrtgOrphan[]
            {
                DefaultSensor.Orphan,
                DefaultDevice.Orphan,
                DefaultGroup.Orphan,
                DefaultProbe.Orphan,
                DefaultTrigger.Orphan,
                DefaultProperty.Orphan,
                PrtgOrphan.PropertyCollection((PropertyOrphan) DefaultProperty.Orphan),
                PrtgOrphan.Grouping(DefaultSensor.Orphan)
            };

            var parent = DefaultProperty.Orphan;

            foreach (var orphan in invalid)
            {
                AssertEx.Throws<InvalidOperationException>(
                    () => parent.WithChildren(orphan),
                    "Cannot add children to orphan of type 'Property': orphan does not support children."
                );
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_ValidChildren_TriggerCollection()
        {
            var collection = PrtgOrphan.TriggerCollection();

            var valid = new[]
            {
                DefaultTrigger.Orphan
            };

            var invalid = new PrtgOrphan[]
            {
                DefaultSensor.Orphan,
                DefaultDevice.Orphan,
                DefaultGroup.Orphan,
                DefaultProbe.Orphan,
                DefaultProperty.Orphan,
                PrtgOrphan.PropertyCollection((PropertyOrphan) DefaultProperty.Orphan),
                PrtgOrphan.TriggerCollection((TriggerOrphan) DefaultTrigger.Orphan),
                PrtgOrphan.Grouping(DefaultSensor.Orphan)
            };

            foreach (var orphan in valid)
            {
                collection.WithChildren(orphan);
            }

            foreach (var orphan in invalid)
            {
                AssertEx.Throws<InvalidOperationException>(
                    () => collection.WithChildren(orphan),
                    $"Cannot add child of type '{orphan.Type}' to 'TriggerOrphanCollection': child must be of type 'Trigger'."
                );
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_ValidChildren_PropertyCollection()
        {
            var collection = PrtgOrphan.PropertyCollection();

            var valid = new[]
            {
                DefaultProperty.Orphan
            };

            var invalid = new PrtgOrphan[]
            {
                DefaultSensor.Orphan,
                DefaultDevice.Orphan,
                DefaultGroup.Orphan,
                DefaultProbe.Orphan,
                DefaultTrigger.Orphan,
                PrtgOrphan.PropertyCollection((PropertyOrphan) DefaultProperty.Orphan),
                PrtgOrphan.TriggerCollection((TriggerOrphan) DefaultTrigger.Orphan),
                PrtgOrphan.Grouping(DefaultSensor.Orphan)
            };

            foreach (var orphan in valid)
            {
                collection.WithChildren(orphan);
            }

            foreach (var orphan in invalid)
            {
                AssertEx.Throws<InvalidOperationException>(
                    () => collection.WithChildren(orphan),
                    $"Cannot add child of type '{orphan.Type}' to 'PropertyOrphanCollection': child must be of type 'Property'."
                );
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_ValidChildren_Grouping()
        {
            var valid = new PrtgOrphan[]
            {
                DefaultSensor.Orphan,
                DefaultDevice.Orphan,
                DefaultGroup.Orphan,
                DefaultProbe.Orphan,
                DefaultTrigger.Orphan,
                DefaultProperty.Orphan,
                PrtgOrphan.PropertyCollection((PropertyOrphan) DefaultProperty.Orphan),
                PrtgOrphan.Grouping(DefaultSensor.Orphan)
            };

            var grouping = PrtgOrphan.Grouping();

            foreach (var orphan in valid)
            {
                AssertEx.Throws<NotSupportedException>(
                    () => grouping.WithChildren(orphan),
                    "Modifying the children of a PrtgOrphanGrouping is not supported"
                );
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_ValidChildren_RootGroup()
        {
            var valid = new PrtgOrphan[]
            {
                DefaultProbe.Orphan,
                DefaultTrigger.Orphan,
                DefaultProperty.Orphan,
                PrtgOrphan.PropertyCollection(PrtgOrphan.Property(Device(), ObjectProperty.Name, "test"))
            };

            var invalid = new PrtgOrphan[]
            {
                
                DefaultSensor.Orphan,
                PrtgOrphan.Grouping(DefaultSensor.Orphan)
            };

            var specialInvalid = new PrtgOrphan[]
            {
                DefaultDevice.Orphan,
                DefaultGroup.Orphan,
            };

            var root = PrtgOrphan.Group(Group(id: 0));

            ValidateChildren(root, valid, invalid);

            foreach(var i in specialInvalid)
            {
                AssertEx.Throws<InvalidOperationException>(
                    () => root.WithChildren(i),
                    $"Node '{i} (ID: {i.Value.Id})' of type '{i.Type}' cannot be a child of the PRTG Root Node. Only probes may directly descend from the root node."
                );
            }
        }

        private void ValidateChildren(PrtgOrphan parent, PrtgOrphan[] valid, PrtgOrphan[] invalid)
        {
            foreach (var i in invalid)
            {
                if (i.Type == PrtgNodeType.Grouping)
                {
                    var grouping = i as PrtgOrphanGrouping;

                    AssertEx.Throws<InvalidOperationException>(
                        () => parent.WithChildren(i),
                        $"Node '{i} (ID: {grouping.Group[0].Value.Id})' of type '{grouping.Group[0].Type}' cannot be a child of a node of type '{parent.Type}'"
                    );
                }
                else
                {
                    AssertEx.Throws<InvalidOperationException>(
                        () => parent.WithChildren(i),
                        $"Node '{i} (ID: {i.Value.Id})' of type '{i.Type}' cannot be a child of a node of type '{parent.Type}'"
                    );
                }
            }

            foreach (var v in valid)
            {
                parent.WithChildren(v);
            }
        }

            #endregion
            #region PrtgNode

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ValidChildren_Sensor()
        {
            var valid = new PrtgNode[]
            {
                DefaultTrigger,
                DefaultProperty,
                PrtgNode.PropertyCollection(DefaultProperty)
            };

            var invalid = new PrtgNode[]
            {
                DefaultSensor,
                DefaultDevice,
                DefaultGroup,
                DefaultProbe,
                PrtgNode.Grouping(DefaultSensor)
            };

            ValidateChildren(DefaultSensor, valid, invalid);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ValidChildren_Device()
        {
            var valid = new PrtgNode[]
            {
                DefaultSensor,
                DefaultTrigger,
                DefaultProperty,
                PrtgNode.PropertyCollection(DefaultProperty)
            };

            var invalid = new PrtgNode[]
            {
                DefaultDevice,
                DefaultGroup,
                DefaultProbe,
                PrtgNode.Grouping(DefaultDevice)
            };

            ValidateChildren(DefaultDevice, valid, invalid);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ValidChildren_Group()
        {
            var valid = new PrtgNode[]
            {
                DefaultDevice,
                DefaultGroup,
                DefaultTrigger,
                DefaultProperty,
                PrtgNode.PropertyCollection(PrtgNode.Property(Device(), ObjectProperty.Name, "test"))
            };

            var invalid = new PrtgNode[]
            {
                DefaultSensor,
                PrtgNode.Grouping(DefaultSensor)
            };

            AssertEx.Throws<InvalidOperationException>(
                () => DefaultGroup.WithChildren(DefaultProbe),
                "Node 'Local Probe (ID: 1001)' of type 'Probe' cannot be a child of a group that is not the PRTG Root Node."
            );

            AssertEx.Throws<InvalidOperationException>(
                () => DefaultGroup.WithChildren(PrtgNode.Group(Group(id: 0))),
                "Cannot add the PRTG Root Node as the child of another group."
            );

            ValidateChildren(DefaultGroup, valid, invalid);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ValidChildren_Probe()
        {
            var valid = new PrtgNode[]
            {
                DefaultDevice,
                DefaultGroup,
                PrtgNode.PropertyCollection(DefaultProperty)
            };

            var invalid = new PrtgNode[]
            {
                DefaultSensor,
                PrtgNode.Grouping(DefaultSensor)
            };

            AssertEx.Throws<InvalidOperationException>(
                () => DefaultProbe.WithChildren(PrtgNode.Group(Group(id: 0))),
                "Cannot add the PRTG Root Node as the child of a probe."
            );

            ValidateChildren(DefaultProbe, valid, invalid);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ValidChildren_Trigger()
        {
            var invalid = new PrtgNode[]
            {
                DefaultSensor,
                DefaultDevice,
                DefaultGroup,
                DefaultProbe,
                DefaultTrigger,
                DefaultProperty,
                PrtgNode.PropertyCollection(DefaultProperty),
                PrtgNode.Grouping(DefaultSensor)
            };

            var parent = DefaultTrigger;

            foreach (var node in invalid)
            {
                AssertEx.Throws<InvalidOperationException>(
                    () => parent.WithChildren(node),
                    "Cannot add children to node of type 'Trigger': node does not support children."
                );
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ValidChildren_Property()
        {
            var invalid = new PrtgNode[]
            {
                DefaultSensor,
                DefaultDevice,
                DefaultGroup,
                DefaultProbe,
                DefaultTrigger,
                DefaultProperty,
                PrtgNode.PropertyCollection(DefaultProperty),
                PrtgNode.Grouping(DefaultSensor)
            };

            var parent = DefaultProperty;

            foreach (var node in invalid)
            {
                AssertEx.Throws<InvalidOperationException>(
                    () => parent.WithChildren(node),
                    "Cannot add children to node of type 'Property': node does not support children."
                );
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ValidChildren_TriggerCollection()
        {
            var collection = PrtgNode.TriggerCollection();

            var valid = new[]
            {
                DefaultTrigger
            };

            var invalid = new PrtgNode[]
            {
                DefaultSensor,
                DefaultDevice,
                DefaultGroup,
                DefaultProbe,
                DefaultProperty,
                PrtgNode.PropertyCollection(DefaultProperty),
                PrtgNode.TriggerCollection(DefaultTrigger),
                PrtgNode.Grouping(DefaultSensor)
            };

            foreach (var node in valid)
            {
                collection.WithChildren(node);
            }

            foreach (var node in invalid)
            {
                AssertEx.Throws<InvalidOperationException>(
                    () => collection.WithChildren(node),
                    $"Cannot add child of type '{node.Type}' to 'TriggerNodeCollection': child must be of type 'Trigger'."
                );
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ValidChildren_PropertyCollection()
        {
            var collection = PrtgNode.PropertyCollection();

            var valid = new[]
            {
                DefaultProperty
            };

            var invalid = new PrtgNode[]
            {
                DefaultSensor,
                DefaultDevice,
                DefaultGroup,
                DefaultProbe,
                DefaultTrigger,
                PrtgNode.PropertyCollection(DefaultProperty),
                PrtgNode.TriggerCollection(DefaultTrigger),
                PrtgNode.Grouping(DefaultSensor)
            };

            foreach (var node in valid)
            {
                collection.WithChildren(node);
            }

            foreach (var node in invalid)
            {
                AssertEx.Throws<InvalidOperationException>(
                    () => collection.WithChildren(node),
                    $"Cannot add child of type '{node.Type}' to 'PropertyNodeCollection': child must be of type 'Property'."
                );
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_ValidChildren_Grouping()
        {
            var valid = new PrtgNode[]
            {
                DefaultSensor,
                DefaultDevice,
                DefaultGroup,
                DefaultProbe,
                DefaultTrigger,
                DefaultProperty,
                PrtgNode.PropertyCollection(DefaultProperty),
                PrtgNode.Grouping(DefaultSensor)
            };

            var grouping = PrtgNode.Grouping();

            foreach (var node in valid)
            {
                AssertEx.Throws<NotSupportedException>(
                    () => grouping.WithChildren(node),
                    "Modifying the children of a PrtgNodeGrouping is not supported"
                );
            }
        }

        private void ValidateChildren(PrtgNode parent, PrtgNode[] valid, PrtgNode[] invalid)
        {
            foreach (var i in invalid)
            {
                if (i.Type == PrtgNodeType.Grouping)
                {
                    var grouping = i as PrtgNodeGrouping;

                    AssertEx.Throws<InvalidOperationException>(
                        () => parent.WithChildren(i),
                        $"Node '{i} (ID: {grouping.Group[0].Value.Id})' of type '{grouping.Group[0].Type}' cannot be a child of a node of type '{parent.Type}'"
                    );
                }
                else
                {
                    AssertEx.Throws<InvalidOperationException>(
                        () => parent.WithChildren(i),
                        $"Node '{i} (ID: {i.Value.Id})' of type '{i.Type}' cannot be a child of a node of type '{parent.Type}'"
                    );
                }
            }

            foreach (var v in valid)
            {
                parent.WithChildren(v);
            }
        }

            #endregion
        #endregion
        #region Dynamic

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Dynamic_GetsValidChild()
        {
            dynamic probe = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group())
            );

            var group = probe.Servers;
            Assert.IsInstanceOfType(group, typeof(GroupNode));
            Assert.AreEqual(probe.Children[0], group);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Dynamic_GetsInvalidChild()
        {
            dynamic probe = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group())
            );

            AssertEx.Throws<RuntimeBinderException>(
                () => Console.WriteLine(probe.Tomato),
                "'PrtgAPI.Tree.ProbeNode' does not contain a definition for 'Tomato'"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Dynamic_GetsValidGrandChild()
        {
            dynamic probe = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.Device(Device("dc"))
                )
            );

            var device = probe.Servers.dc;
            Assert.IsInstanceOfType(device, typeof(DeviceNode));
            Assert.AreEqual(probe["Servers"]["dc"], device);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Dynamic_SetsChild_Throws()
        {
            dynamic probe = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group())
            );

            AssertEx.Throws<RuntimeBinderException>(
                () => probe.Servers = PrtgNode.Group(Group("Linux")),
                "'PrtgAPI.Tree.ProbeNode' does not contain a definition for 'Servers'"
            );
        }

        #endregion
        #region TreeDifference

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Difference_None()
        {
            var first = PrtgNode.Probe(Probe());
            var second = PrtgNode.Probe(Probe());
            var comparison = first.CompareTo(second);

            Assert.IsTrue(TreeNodeDifference.None == comparison.Difference);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Difference_Type()
        {
            var first = PrtgNode.Probe(Probe());
            var second = PrtgNode.Device(Device("Local Probe", 1001, 1));
            var comparison = first.CompareTo(second);

            Assert.IsTrue(TreeNodeDifference.Type == comparison.Difference, $"Difference was actually '{comparison.Difference}'");
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Difference_Child_Type()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group())
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("Servers", 2001, 2))
            );

            var comparison = first.CompareTo(second);

            Assert.IsTrue(TreeNodeDifference.Type == comparison.TreeDifference);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Difference_ParentId()
        {
            var first = PrtgNode.Probe(Probe());
            var second = PrtgNode.Probe(Probe(parentId: 0));
            var comparison = first.CompareTo(second);

            Assert.IsTrue(TreeNodeDifference.ParentId == comparison.Difference);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Difference_Name()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );
            var second = PrtgNode.Probe(Probe("Cluster Probe"),
                PrtgNode.Device(Device("dc-2"))
            );
            var comparison = first.CompareTo(second);

            Assert.IsTrue(TreeNodeDifference.Name == comparison.Difference);
            Assert.IsTrue(TreeNodeDifference.Name == comparison["dc-1"].Difference);
            Assert.IsTrue(TreeNodeDifference.Name == comparison["dc-2"].Difference);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Difference_HasChildren()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var second = PrtgNode.Probe(Probe());

            var comparison = first.CompareTo(second);

            Assert.IsTrue((TreeNodeDifference.NumberOfChildren | TreeNodeDifference.HasChildren) == comparison.Difference);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Difference_NumberOfChildren()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device()),
                PrtgNode.Device(Device(id: 3002))
            );

            var comparison = first.CompareTo(second);

            Assert.IsTrue((TreeNodeDifference.NumberOfChildren) == comparison.Difference);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Difference_Added()
        {
            var first = PrtgNode.Probe(Probe());

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var comparison = first.CompareTo(second);

            Assert.IsTrue(TreeNodeDifference.Added == comparison["dc-1"].Difference);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Difference_Removed()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var second = PrtgNode.Probe(Probe());

            var comparison = first.CompareTo(second);

            Assert.IsTrue(TreeNodeDifference.Removed == comparison["dc-1"].Difference);
        }

        #endregion

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_TreeDifference()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device("dc-2"))
            );

            var comparison = first.CompareTo(second);

            Assert.IsTrue(TreeNodeDifference.None == comparison.Difference);
            Assert.IsTrue(TreeNodeDifference.Name == comparison.TreeDifference);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareNode_Type()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var second = PrtgNode.Probe(Probe(id: 1002),
                PrtgNode.Device(Device())
            );

            var comparison = first.CompareTo(second);

            Assert.AreEqual(TreeNodeType.Collection, comparison.Type);
            Assert.AreEqual(TreeNodeType.Node, comparison.Children[0].Type);
            Assert.AreEqual(TreeNodeType.Grouping, comparison["Local Probe"].Type);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Contains_Null()
        {
            var probe = PrtgNode.Probe(Probe());

            Assert.IsFalse(probe.Contains(null));
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Contains_GrandChild()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device(),
                    PrtgNode.Sensor(Sensor())
                )
            );

            var sensor = probe["dc-1"]["VMware Datastore"];

            Assert.IsTrue(probe.Contains(sensor));
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Contains_DoesNotContain()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device(),
                    PrtgNode.Sensor(Sensor())
                )
            );

            var sensor = PrtgNode.Sensor(Sensor());

            Assert.IsFalse(probe.Contains(sensor));
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_NullValues_Throws()
        {
            var createNodes = new Func<PrtgNode>[]
            {
                () => PrtgNode.Sensor(null),
                () => PrtgNode.Device(null),
                () => PrtgNode.Group(null),
                () => PrtgNode.Probe(null),
                () => PrtgNode.Trigger(null),
                () => PrtgNode.Property(null)
            };

            var createCollections = new Func<PrtgNode>[]
            {
                () => PrtgNode.PropertyCollection(null),
                () => PrtgNode.TriggerCollection(null),
                () => PrtgNode.Grouping(null)
            };

            foreach (var createNode in createNodes)
            {
                AssertEx.Throws<ArgumentNullException>(
                    () => createNode(),
                    "Value cannot be null"
                );
            }

            foreach (var createNode in createCollections)
            {
                var collection = createNode();
                Assert.AreEqual(0, collection.Children.Count);
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Children_ContainsNull_Throws()
        {
            AssertEx.Throws<ArgumentNullException>(
                () => {
                    PrtgNode.Probe(Probe(),
                        new PrtgNode[] {null}
                    );
                },
                "List contained a null element."
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_CompareOrphan_NullNodes()
        {
            var orphan = new CompareOrphan(null, null, new[] { new CompareOrphan(null, null, null) });
            Assert.IsTrue(TreeNodeDifference.None == orphan.Difference);
            Assert.IsTrue(TreeNodeDifference.None == orphan.TreeDifference);
            Assert.IsNull(orphan["dc-1"]);
        }

        /*[UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_Collection_Name_WhenEmpty_IsCorrect()
        {
            var collection = PrtgOrphan.TriggerCollection();

            Assert.AreEqual("Empty", collection.Name);
        }*/

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_Collection_Name_WhenSingleValue_IsCorrect()
        {
            var first = PrtgNode.Probe(Probe());
            var second = PrtgNode.Probe(Probe(id: 1002));

            var comparison = first.CompareTo(second).Orphan;

            Assert.AreEqual("Local Probe", comparison.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_Collection_Name_WhenMultipleValues_IsCorrect()
        {
            var node = PrtgOrphan.Probe(Probe(),
                PrtgOrphan.Device(Device()),
                PrtgOrphan.Device(Device("DC-1"))
            );

            var device = node["dc-1", true];

            Assert.AreEqual("dc-1 / DC-1", device.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgOrphan_Collection_Name_Specialized_IsCorrect()
        {
            var triggers = PrtgOrphan.TriggerCollection();
            var properties = PrtgOrphan.PropertyCollection();

            Assert.AreEqual("Triggers", triggers.Name);
            Assert.AreEqual("Properties", properties.Name);
        }

        /*[UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Collection_Name_WhenEmpty_IsCorrect()
        {
            var collection = PrtgNode.TriggerCollection();

            Assert.AreEqual("Empty", collection.Name);
        }*/

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Collection_Name_WhenSingleValue_IsCorrect()
        {
            var first = PrtgNode.Probe(Probe());
            var second = PrtgNode.Probe(Probe(id: 1002));

            var comparison = first.CompareTo(second);

            Assert.AreEqual("Local Probe", comparison.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Collection_Name_WhenMultipleValues_IsCorrect()
        {
            var node = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device()),
                PrtgNode.Device(Device("DC-1"))
            );

            var device = node["dc-1", true];

            Assert.AreEqual("dc-1 / DC-1", device.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrtgNode_Collection_Name_Specialized_IsCorrect()
        {
            var triggers = PrtgNode.TriggerCollection();
            var properties = PrtgNode.PropertyCollection();

            Assert.AreEqual("Triggers", triggers.Name);
            Assert.AreEqual("Properties", properties.Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_TreeNode_AllTypesHaveDebugView()
        {
            var exclusionMap = new Dictionary<Type, string[]>
            {
                [typeof(PrtgOrphanCollectionDebugView)] = new[] {"Value"},
                [typeof(PrtgOrphanGroupingDebugView)] = new[] {"Value"},
                [typeof(PrtgNodeCollectionDebugView)] = new[] {"Value"},
                [typeof(PrtgNodeGroupingDebugView)] = new[] {"Value"},

                [typeof(CompareOrphanCollectionDebugView)] = new[] {"First","Second"},
                [typeof(CompareOrphanGroupingDebugView)] = new[] {"First", "Second"},
                [typeof(CompareNodeCollectionDebugView)] = new[] {"First", "Second"},
                [typeof(CompareNodeGroupingDebugView)] = new[] {"First", "Second"}
            };

            var assemblyTypes = typeof(TreeNode).Assembly.GetTypes();

            var types = assemblyTypes.Where(t => !t.IsAbstract && (typeof(TreeNode).IsAssignableFrom(t) || typeof(TreeOrphan).IsAssignableFrom(t))).ToList();

            foreach (var type in types)
            {
                var attrib = type.GetCustomAttributes<DebuggerTypeProxyAttribute>().FirstOrDefault();

                if (attrib == null)
                {
                    var properties = type.GetProperties().Select(p => p.Name).Where(v => v != "Item").Distinct().ToList();

                    Assert.Fail($"'{type}' is missing a '{nameof(DebuggerTypeProxyAttribute)}'. Proxy should have properties {string.Join(", ", properties)}");
                }
                else
                {
                    var typeProperties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(p => p.Name).Where(v => v != "Item" && v != "Orphan" && !v.Contains("DebuggerDisplay")).ToList();
                    var proxyType = assemblyTypes.First(t => t.AssemblyQualifiedName == attrib.ProxyTypeName);
                    var proxyProperties = proxyType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Select(p => p.Name).ToList();

                    //We're a list proxy, so we don't expect to show our actual members
                    if (proxyProperties.Count == 1 && proxyProperties.Single() == "Items")
                        continue;

                    var missing = typeProperties.Except(proxyProperties).ToList();

                    if (missing.Count > 0)
                    {
                        string[] exclusions;

                        if (exclusionMap.TryGetValue(proxyType, out exclusions))
                            missing = missing.Except(exclusions).ToList();

                        if (missing.Count > 0)
                            Assert.Fail($"{proxyType.FullName} is missing properties {string.Join(", ", missing)}");
                    }
                }
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_NodeExtensions_AllUniqueMethodsHaveETSDefinitions()
        {
            var exclusions = new[]
            {
                "PrtgNode_PrettyPrint",
                "CompareNode_PrettyPrint"
            };

            var src = TestHelpers.GetProjectRoot(true);
            var ps1xml = Path.Combine(src, "PrtgAPI.PowerShell/PowerShell/Resources/PrtgAPI.Types.ps1xml");

            if (!File.Exists(ps1xml))
                throw new InvalidOperationException($"File '{ps1xml}' does not exist.");

            var xml = XDocument.Load(ps1xml);

            var expectedMethods = typeof(NodeExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Select(m =>
            {
                string prefix;

                if (m.IsGenericMethod)
                    prefix = m.GetGenericArguments()[0].GetGenericParameterConstraints()[0].Name;
                else
                    prefix = m.GetParameters()[0].ParameterType.Name;

                return prefix + "_" + m.Name;
            }).Distinct().ToArray();
            var actualMethods = xml.Descendants("ScriptMethod").Select(d => d.Parent.Parent.Element("Name").Value.Replace("PrtgAPI.Tree.", "") + "_" + d.Element("Name").Value).ToArray();

            var missing = expectedMethods.Except(actualMethods).Except(exclusions).ToArray();

            if (missing.Length > 0)
                Assert.Fail($"Methods {(string.Join(", ", missing))} are missing from PrtgAPI.Types.ps1xml.");
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PropertyValuePair_MembersCorrect_TypedProperty()
        {
            var pair = new PropertyValuePair(Device(), ObjectProperty.Host, "dc-1.contoso.local");
            Assert.AreEqual(3001, pair.ParentId);
            Assert.IsNull(pair.Id);

            Assert.AreEqual(ObjectProperty.Host, pair.Property);
            Assert.IsNull(pair.Property.Right);
            Assert.AreEqual("dc-1.contoso.local", pair.Value);
            Assert.AreEqual("Host", ((ITreeValue) pair).Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PropertyValuePair_MembersCorrect_RawProperty()
        {
            var pair = new PropertyValuePair(Device(), "host_", "dc-1.contoso.local");
            Assert.AreEqual(3001, pair.ParentId);
            Assert.IsNull(pair.Id);

            Assert.AreEqual("host_", pair.Property.Right);
            Assert.AreEqual("dc-1.contoso.local", pair.Value);
            Assert.AreEqual("host_", ((ITreeValue)pair).Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PropertyValuePair_NullProperty_Throws()
        {
            AssertEx.Throws<ArgumentNullException>(
                () => new PropertyValuePair(Device(), null, "1"),
                "Value of type 'String' cannot be null"
            );
        }
    }
}
