using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.Tree;
using PrtgAPI.Tree;

namespace PrtgAPI.Tests.UnitTests.Tree
{
    [TestClass]
    public class TreePrettyPrintTests : BaseTreeTest
    {
        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_SingleNode()
        {
            var tree = DefaultProbe;

            var expected = new[]
            {
                "Local Probe"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_SingleChild()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group())
            );

            var expected = new[]
            {
                "Local Probe",
                "└──Servers"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_SingleChildCollection()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.TriggerCollection(PrtgNode.Trigger(Trigger()))
            );

            var expected = new[]
            {
                "Local Probe",
                "└──Email to Admin"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_TwoChildren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group()),
                PrtgNode.Group(Group("Linux", 2002))
            );

            var expected = new[]
            {
                "Local Probe",
                "├──Servers",
                "└──Linux"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_TwoCollectionChildren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.TriggerCollection(
                    PrtgNode.Trigger(Trigger()),
                    PrtgNode.Trigger(Trigger("Ticket Notification"))
                )
            );

            var expected = new[]
            {
                "Local Probe",
                "├──Email to Admin",
                "└──Ticket Notification"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_SingleGrandChild()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.Device(Device())
                )
            );

            var expected = new[]
            {
                "Local Probe",
                "└──Servers",
                "   └──dc-1"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_SingleGrandChildCollection()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.TriggerCollection(PrtgNode.Trigger(Trigger()))
                )
            );

            var expected = new[]
            {
                "Local Probe",
                "└──Servers",
                "   └──Email to Admin"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_TwoChildrenOneGrandChild()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.Device(Device())
                ),
                PrtgNode.Group(Group("Linux", 2002))
            );

            var expected = new[]
            {
                "Local Probe",
                "├──Servers",
                "│  └──dc-1",
                "└──Linux"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_TwoChildrenOneGrandChildCollection()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.TriggerCollection(PrtgNode.Trigger(Trigger()))
                ),
                PrtgNode.Group(Group("Linux", 2002))
            );

            var expected = new[]
            {
                "Local Probe",
                "├──Servers",
                "│  └──Email to Admin",
                "└──Linux"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_TwoGrandChildren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.Device(Device()),
                    PrtgNode.Device(Device("dc-2", 3002))
                )
            );

            var expected = new[]
            {
                "Local Probe",
                "└──Servers",
                "   ├──dc-1",
                "   └──dc-2"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_TwoChildren_TwoGrandChildren()
        {
            var tree = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group(),
                    PrtgNode.Device(Device()),
                    PrtgNode.Device(Device("dc-2", 3002))
                ),
                PrtgNode.Group(Group("Linux", 2002))
            );

            var expected = new[]
            {
                "Local Probe",
                "├──Servers",
                "│  ├──dc-1",
                "│  └──dc-2",
                "└──Linux"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_TwoChildren_OneHasTwoChildren_WithOneGrandChildEach()
        {
            var tree = PrtgNode.Group(Group("PRTG Network Monitor", 0),
                PrtgNode.Probe(Probe("Probe 1"),
                    PrtgNode.Group(Group(),
                        PrtgNode.Device(Device())
                    ),
                    PrtgNode.Group(Group("Linux"),
                        PrtgNode.Device(Device("arch-1"))
                    )
                ),
                PrtgNode.Probe(Probe("Probe 2"))
            );

            var expected = new[]
            {
                "PRTG Network Monitor",
                "├──Probe 1",
                "│  ├──Servers",
                "│  │  └──dc-1",
                "│  └──Linux",
                "│     └──arch-1",
                "└──Probe 2"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_TwoChildren_OneHasTwoChildren_WithOneGrandChildCollectionEach()
        {
            var tree = PrtgNode.Group(Group("PRTG Network Monitor", 0),
                PrtgNode.Probe(Probe("Probe 1"),
                    PrtgNode.Group(Group(),
                        PrtgNode.TriggerCollection(PrtgNode.Trigger(Trigger()))
                    ),
                    PrtgNode.Group(Group("Linux"),
                        PrtgNode.TriggerCollection(PrtgNode.Trigger(Trigger("Ticket Notification")))
                    )
                ),
                PrtgNode.Probe(Probe("Probe 2"))
            );

            var expected = new[]
            {
                "PRTG Network Monitor",
                "├──Probe 1",
                "│  ├──Servers",
                "│  │  └──Email to Admin",
                "│  └──Linux",
                "│     └──Ticket Notification",
                "└──Probe 2"
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_TwoTriggers()
        {
            var group = Group();

            var tree = PrtgNode.Group(group,
                PrtgNode.Trigger(Trigger()),
                PrtgNode.TriggerCollection(PrtgNode.Trigger(Trigger("Ticket Notification")))
            );

            var expected = new[]
            {
                "Servers",
                "├──Email to Admin",
                "└──Ticket Notification",
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_NodeAndTwoTriggers()
        {
            var tree = PrtgNode.Group(Group(),
                PrtgNode.Device(Device()),
                PrtgNode.TriggerCollection(
                    PrtgNode.Trigger(Trigger()),
                    PrtgNode.Trigger(Trigger("Ticket Notification"))
                )
            );

            var expected = new[]
            {
                "Servers",
                "├──dc-1",
                "├──Email to Admin",
                "└──Ticket Notification",
            };

            Validate(tree, expected);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_PrettyPrint_Grouping()
        {
            var tree = PrtgNode.Device(Device(),
                PrtgNode.Sensor(Sensor(),
                    PrtgNode.Trigger(Trigger())
                ),
                PrtgNode.Sensor(Sensor())
            );

            var grouping = tree["VMware Datastore"];

            var expected = new[]
            {
                "VMware Datastore",
                "└──Email to Admin",
                "VMware Datastore"
            };

            Validate(grouping, expected);
        }

        private void Validate(PrtgNode node, string[] expected)
        {
            node.PrettyPrint(new PrtgPrettyValidator(expected));
        }
    }
}
