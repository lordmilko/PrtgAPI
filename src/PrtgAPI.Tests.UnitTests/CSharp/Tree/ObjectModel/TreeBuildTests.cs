using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;
using PrtgAPI.Tree;

namespace PrtgAPI.Tests.UnitTests.Tree
{
    [TestClass]
    public class TreeBuildTests : BaseTreeTest
    {
        [UnitTest]
        [TestMethod]
        public void Tree_Builds()
        {
            var response = new TreeRequestResponse(TreeRequestScenario.MultiLevelContainer);

            var client = BaseTest.Initialize_Client(response);

            var tree = client.GetTree(1001);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_Fast()
        {
            var response = new TreeRequestResponse(TreeRequestScenario.FastPath);

            var client = BaseTest.Initialize_Client(response);

            var tree = client.GetTree(options: TreeParseOption.Common | TreeParseOption.Triggers);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_BuildsAsync()
        {
            var response = new TreeRequestResponse(TreeRequestScenario.MultiLevelContainer, true);

            var client = BaseTest.Initialize_Client(response);

            var tree = await client.GetTreeAsync(1001, options: TreeParseOption.Common | TreeParseOption.Triggers);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_FastAsync()
        {
            var response = new TreeRequestResponse(TreeRequestScenario.FastPath, true);

            var client = BaseTest.Initialize_Client(response);

            var tree = await client.GetTreeAsync(options: TreeParseOption.Common | TreeParseOption.Triggers);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_UntypedObject()
        {
            var probe = new PrtgObject {Id = 1001};

            var response = new TreeRequestResponse(TreeRequestScenario.ContainerWithChild);

            var client = BaseTest.Initialize_Client(response);

            AssertEx.Throws<NotSupportedException>(
                () => client.GetTree(probe),
                "Cannot process value"
            );
        }

        #region Sensor

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_SensorsOnly_Fast()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathSensorsOnly));

            var tree = client.GetTree(options: TreeParseOption.Sensors);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(1, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_SensorsOnly_FastAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathSensorsOnly));

            var tree = await client.GetTreeAsync(options: TreeParseOption.Sensors);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(1, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_SensorsOnly_Slow()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathSensorsOnly));

            var tree = client.GetTree(3001, TreeParseOption.Sensors);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(2, desc.Length);
            Assert.AreEqual(PrtgNodeType.Device, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Sensor, desc[1].Type);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_SensorsOnly_SlowAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathSensorsOnly));

            var tree = await client.GetTreeAsync(3001, TreeParseOption.Sensors);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(2, desc.Length);
            Assert.AreEqual(PrtgNodeType.Device, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Sensor, desc[1].Type);
        }

        #endregion
        #region Device

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_DevicesOnly_Fast()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathDevicesOnly));

            var tree = client.GetTree(options: TreeParseOption.Devices);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(1, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_DevicesOnly_FastAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathDevicesOnly));

            var tree = await client.GetTreeAsync(options: TreeParseOption.Devices);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(1, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_DevicesOnly_Slow()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathDevicesOnly));

            var tree = client.GetTree(1001, TreeParseOption.Devices);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(2, desc.Length);
            Assert.AreEqual(PrtgNodeType.Probe, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Device, desc[1].Type);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_DevicesOnly_SlowAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathDevicesOnly));

            var tree = await client.GetTreeAsync(1001, TreeParseOption.Devices);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(2, desc.Length);
            Assert.AreEqual(PrtgNodeType.Probe, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Device, desc[1].Type);
        }

        #endregion
        #region Group

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_GroupsOnly_Fast()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathGroupsOnly));

            var tree = client.GetTree(options: TreeParseOption.Groups);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(1, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_GroupsOnly_FastAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathGroupsOnly));

            var tree = await client.GetTreeAsync(options: TreeParseOption.Groups);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(1, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_GroupsOnly_Slow()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathGroupsOnly));

            var tree = client.GetTree(1001, TreeParseOption.Groups);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(2, desc.Length);
            Assert.AreEqual(PrtgNodeType.Probe, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Group, desc[1].Type);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_GroupsOnly_SlowAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathGroupsOnly));

            var tree = await client.GetTreeAsync(1001, TreeParseOption.Groups);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(2, desc.Length);
            Assert.AreEqual(PrtgNodeType.Probe, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Group, desc[1].Type);
        }

        #endregion
        #region Probe

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_ProbesOnly_Fast()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathProbesOnly));

            var tree = client.GetTree(options: TreeParseOption.Probes);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(2, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Probe, desc[1].Type);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_ProbesOnly_FastAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathProbesOnly));

            var tree = await client.GetTreeAsync(options: TreeParseOption.Probes);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(2, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Probe, desc[1].Type);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_ProbesOnly_Slow()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathProbesOnly));

            var tree = client.GetTree(1001, TreeParseOption.Probes);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(1, desc.Length);
            Assert.AreEqual(PrtgNodeType.Probe, desc[0].Type);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_ProbesOnly_SlowAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathProbesOnly));

            var tree = await client.GetTreeAsync(1001, TreeParseOption.Probes);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(1, desc.Length);
            Assert.AreEqual(PrtgNodeType.Probe, desc[0].Type);
        }

        #endregion
        #region Trigger

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_TriggersOnly_Fast()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathTriggersOnly));

            var tree = client.GetTree(options: TreeParseOption.Triggers);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(3, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Collection, desc[1].Type);
            Assert.AreEqual(PrtgNodeType.Trigger, desc[2].Type);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_TriggersOnly_FastAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathTriggersOnly, true));

            var tree = await client.GetTreeAsync(options: TreeParseOption.Triggers);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(3, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Collection, desc[1].Type);
            Assert.AreEqual(PrtgNodeType.Trigger, desc[2].Type);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_TriggersOnly_Slow()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathTriggersOnly));

            var tree = client.GetTree(1001, TreeParseOption.Triggers);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(3, desc.Length);
            Assert.AreEqual(PrtgNodeType.Probe, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Collection, desc[1].Type);
            Assert.AreEqual(PrtgNodeType.Trigger, desc[2].Type);
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_TriggersOnly_SlowAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathTriggersOnly, true));

            var tree = await client.GetTreeAsync(1001, TreeParseOption.Triggers);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(3, desc.Length);
            Assert.AreEqual(PrtgNodeType.Probe, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Collection, desc[1].Type);
            Assert.AreEqual(PrtgNodeType.Trigger, desc[2].Type);
        }

        #endregion
        #region Property

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_PropertiesOnly_Fast()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathPropertiesOnly));

            var tree = client.GetTree(options: TreeParseOption.Properties);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(28, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Collection, desc[1].Type);

            for (var i = 2; i < 28; i++)
            {
                Assert.AreEqual(PrtgNodeType.Property, desc[i].Type);
            }
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_PropertiesOnly_FastAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.FastPathPropertiesOnly, true));

            var tree = await client.GetTreeAsync(options: TreeParseOption.Properties);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(28, desc.Length);
            Assert.AreEqual(PrtgNodeType.Group, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Collection, desc[1].Type);

            for (var i = 2; i < 28; i++)
            {
                Assert.AreEqual(PrtgNodeType.Property, desc[i].Type);
            }
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Builds_PropertiesOnly_Slow()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathPropertiesOnly));

            var tree = client.GetTree(1001, TreeParseOption.Properties);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(28, desc.Length);
            Assert.AreEqual(PrtgNodeType.Probe, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Collection, desc[1].Type);

            for (var i = 2; i < 28; i++)
            {
                Assert.AreEqual(PrtgNodeType.Property, desc[i].Type);
            }
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_Builds_PropertiesOnly_SlowAsync()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.SlowPathPropertiesOnly, true));

            var tree = await client.GetTreeAsync(1001, TreeParseOption.Properties);

            var desc = tree.DescendantNodesAndSelf().ToArray();

            Assert.AreEqual(28, desc.Length);
            Assert.AreEqual(PrtgNodeType.Probe, desc[0].Type);
            Assert.AreEqual(PrtgNodeType.Collection, desc[1].Type);

            for (var i = 2; i < 28; i++)
            {
                Assert.AreEqual(PrtgNodeType.Property, desc[i].Type);
            }
        }

        #endregion
    }
}
