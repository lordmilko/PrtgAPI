using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;
using PrtgAPI.Tests.UnitTests.Support.Tree;
using PrtgAPI.Tree;

namespace PrtgAPI.Tests.UnitTests.Tree
{
    public class Validation
    {
        public TreeNodeDifference Expected { get; set; }

        public Func<CompareNode, TreeNodeDifference> GetActual { get; set; }

        public Validation(TreeNodeDifference expected, Func<CompareNode, TreeNodeDifference> getActual)
        {
            Expected = expected;
            GetActual = getActual;
        }
    }

    [TestClass]
    public class TreeCompareTests : BaseTreeTest
    {
        [UnitTest]
        [TestMethod]
        public void Tree_Compare_SingleNode()
        {
            var first = PrtgNode.Probe(Probe());
            var second = PrtgNode.Probe(Probe());

            Validate(first, second, new[]
            {
                new Validation(TreeNodeDifference.None, c => c.Difference)
            });
        }

        /*[TestMethod]
        public void Tree_Compare_RealWithDescriptor()
        {
            var first = PrtgNode.Probe(Probe());
            var second = PrtgNode.Probe(new ProbeDescriptor("Local Probe", 1001, 1));

            Validate(first, second, new[]
            {
                new Validation(TreeNodeDifference.None, c => c.Difference)
            });
        }*/

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_Different_SingleNode()
        {
            var first = PrtgNode.Probe(Probe());
            var second = PrtgNode.Probe(Probe("New York"));

            Validate(first, second, new[]
            {
                new Validation(TreeNodeDifference.Name, c => c.Difference)
            });

            ValidatePretty(first, second, new[]
            {
                "<Yellow>Local Probe (Renamed 'New York')</Yellow>"
            }, new[]
            {
                "<Yellow>New York (Renamed 'Local Probe')</Yellow>"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_HasChildren()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group())
            );

            var second = PrtgNode.Probe(Probe());

            Validate(first, second, new[]
            {
                new Validation(TreeNodeDifference.HasChildren | TreeNodeDifference.NumberOfChildren, c => c.Difference)
            });

            ValidatePretty(first, second, new[]
            {
                "<Yellow>Local Probe</Yellow>",
                "└──<Red>Servers</Red>"
            }, new[]
            {
                "<Yellow>Local Probe</Yellow>",
                "└──<Green>Servers</Green>"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_HasMoreChildren()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group()),
                PrtgNode.Group(Group("Linux"))
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group())
            );

            Validate(first, second, new[]
            {
                new Validation(TreeNodeDifference.NumberOfChildren, c => c.Difference),
                new Validation(TreeNodeDifference.Removed, c => c["Linux"].Difference)
            }, new[]
            {
                new Validation(TreeNodeDifference.NumberOfChildren, c => c.Difference),
                new Validation(TreeNodeDifference.Added, c => c["Linux"].Difference)
            });

            ValidatePretty(first, second, new[]
            {
                "<Yellow>Local Probe</Yellow>",
                "├──Servers",
                "└──<Red>Linux</Red>"
            }, new[]
            {
                "<Yellow>Local Probe</Yellow>",
                "├──Servers",
                "└──<Green>Linux</Green>"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_ChildHasDifference()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group())
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group("Linux"))
            );

            Validate(
                first, second,
                new[] { new Validation(TreeNodeDifference.Name, c => c["Servers"].Difference) },
                new[] { new Validation(TreeNodeDifference.Name, c => c["Linux"].Difference) }
            );

            ValidatePretty(
                first, second,
                new[] {
                    "Local Probe",
                    "└──<Yellow>Servers (Renamed 'Linux')</Yellow>"
                },
                new[] {
                    "Local Probe",
                    "└──<Yellow>Linux (Renamed 'Servers')</Yellow>"
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_HasMoreChildren_AndAGrandChild()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group()),
                PrtgNode.Group(Group("Linux"),
                    PrtgNode.Device(Device("arch-1"))
                )
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group())
            );

            Validate(first, second, new[]
            {
                new Validation(TreeNodeDifference.NumberOfChildren, c => c.Difference),
                new Validation(TreeNodeDifference.Removed, c => c["Linux"].Difference),
                new Validation(TreeNodeDifference.Removed, c => c["Linux"]["arch-1"].Difference)
            }, new[]
            {
                new Validation(TreeNodeDifference.NumberOfChildren, c => c.Difference),
                new Validation(TreeNodeDifference.Added, c => c["Linux"].Difference),
                new Validation(TreeNodeDifference.Added, c => c["Linux"]["arch-1"].Difference)
            });

            ValidatePretty(first, second, new[]
            {
                "<Yellow>Local Probe</Yellow>",
                "├──Servers",
                "└──<Red>Linux</Red>",
                "   └──<Red>arch-1</Red>"
            }, new[]
            {
                "<Yellow>Local Probe</Yellow>",
                "├──Servers",
                "└──<Green>Linux</Green>",
                "   └──<Green>arch-1</Green>"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_ReplaceChild_AndAddAGrandChild()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group()),
                PrtgNode.Group(Group("Windows"))
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group()),
                PrtgNode.Group(Group("Linux", 2002),
                    PrtgNode.Device(Device("arch-1"))
                )
            );

            Validate(first, second, new[]
            {
                new Validation(TreeNodeDifference.None, c => c.Difference),
                new Validation(TreeNodeDifference.Added, c => c["Linux"].Difference),
                new Validation(TreeNodeDifference.Added, c => c["Linux"]["arch-1"].Difference)
            }, new[]
            {
                new Validation(TreeNodeDifference.None, c => c.Difference),
                new Validation(TreeNodeDifference.Removed, c => c["Linux"].Difference),
                new Validation(TreeNodeDifference.Removed, c => c["Linux"]["arch-1"].Difference)
            });

            ValidatePretty(first, second, new[]
            {
                "Local Probe",
                "├──Servers",
                "├──<Red>Windows</Red>",
                "└──<Green>Linux</Green>",
                "   └──<Green>arch-1</Green>"
            }, new[]
            {
                "Local Probe",
                "├──Servers",
                "├──<Red>Linux</Red>",
                "│  └──<Red>arch-1</Red>",
                "└──<Green>Windows</Green>"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_ReplaceChildWithGrandChild_With_ChildWithGrandChild()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group()),
                PrtgNode.Group(Group("Windows"),
                    PrtgNode.Device(Device())
                )
            );

            var second = PrtgNode.Probe(Probe(),
                PrtgNode.Group(Group()),
                PrtgNode.Group(Group("Linux", 2002),
                    PrtgNode.Device(Device("arch-1"))
                )
            );

            Validate(first, second, new[]
            {
                new Validation(TreeNodeDifference.None, c => c.Difference),
                new Validation(TreeNodeDifference.Removed, c => c["Windows"].Difference),
                new Validation(TreeNodeDifference.Added, c => c["Linux"].Difference),
                new Validation(TreeNodeDifference.Added, c => c["Linux"]["arch-1"].Difference)
            }, new[]
            {
                new Validation(TreeNodeDifference.None, c => c.Difference),
                new Validation(TreeNodeDifference.Added, c => c["Windows"].Difference),
                new Validation(TreeNodeDifference.Removed, c => c["Linux"].Difference),
                new Validation(TreeNodeDifference.Removed, c => c["Linux"]["arch-1"].Difference)
            });

            ValidatePretty(first, second, new[]
            {
                "Local Probe",
                "├──Servers",
                "├──<Red>Windows</Red>",
                "│  └──<Red>dc-1</Red>",
                "└──<Green>Linux</Green>",
                "   └──<Green>arch-1</Green>"
            }, new[]
            {
                "Local Probe",
                "├──Servers",
                "├──<Red>Linux</Red>",
                "│  └──<Red>arch-1</Red>",
                "└──<Green>Windows</Green>",
                "   └──<Green>dc-1</Green>",
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_Root()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device()),
                PrtgNode.Device(Device(id: 3002))
            );

            var second = PrtgNode.Probe(Probe(id: 1002));

            ValidatePretty(first, second, new[]
            {
                "<Red>Local Probe</Red>",
                "├──<Red>dc-1</Red>",
                "└──<Red>dc-1</Red>",
                "<Green>Local Probe</Green>"
            }, new[]
            {
                "<Red>Local Probe</Red>",
                "<Green>Local Probe</Green>",
                "├──<Green>dc-1</Green>",
                "└──<Green>dc-1</Green>"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_Grouping_Throws()
        {
            var first = DefaultProbe;
            var second = PrtgNode.Device(Device(),
                DefaultSensor,
                DefaultSensor
            );

            var grouping = second["VMware Datastore"];
            Assert.AreEqual(PrtgNodeType.Grouping, grouping.Type);

            AssertEx.Throws<ArgumentException>(
                () => first.CompareTo(grouping),
                "Cannot create a comparison containing a node of type Grouping."
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_SecondLastNodeRemoved()
        {
            var first = PrtgNode.Device(Device(),
                PrtgNode.Sensor(Sensor("First Sensor")),
                PrtgNode.Sensor(Sensor("Second Sensor", 4002))
            );

            var second = PrtgNode.Device(Device(),
                PrtgNode.Sensor(Sensor("Second Sensor", 4002))
            );

            ValidatePretty(first, second, new[]
            {
                "<Yellow>dc-1</Yellow>",
                "├──<Red>First Sensor</Red>",
                "└──Second Sensor"
            }, new[]
            {
                "<Yellow>dc-1</Yellow>",
                "├──Second Sensor",
                "└──<Green>First Sensor</Green>"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_SecondLastNodeRemoved_LastNodeTriggerCollection()
        {
            var first = PrtgNode.Device(Device(),
                PrtgNode.Sensor(Sensor("First Sensor")),
                PrtgNode.TriggerCollection(DefaultTrigger)
            );

            var second = PrtgNode.Device(Device(),
                PrtgNode.TriggerCollection(DefaultTrigger)
            );

            ValidatePretty(first, second, new[]
            {
                "<Yellow>dc-1</Yellow>",
                "├──<Red>First Sensor</Red>",
                "└──Triggers",
                "   └──Email to Admin"
            }, new[]
            {
                "<Yellow>dc-1</Yellow>",
                "├──Triggers",
                "│  └──Email to Admin",
                "└──<Green>First Sensor</Green>"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_SecondLastNodeRemoved_LastNodePropertyCollection()
        {
            var first = PrtgNode.Device(Device(),
                PrtgNode.Sensor(Sensor("First Sensor")),
                PrtgNode.PropertyCollection(PrtgNode.Property(3001, "name_", "test"))
            );

            var second = PrtgNode.Device(Device(),
                PrtgNode.PropertyCollection(PrtgNode.Property(3001, "name_", "test"))
            );

            ValidatePretty(first, second, new[]
            {
                "<Yellow>dc-1</Yellow>",
                "├──<Red>First Sensor</Red>",
                "└──Properties",
                "   └──name_"
            }, new[]
            {
                "<Yellow>dc-1</Yellow>",
                "├──Properties",
                "│  └──name_",
                "└──<Green>First Sensor</Green>",
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_SecondLastNodeRemoved_LastNodeTrigger()
        {
            var first = PrtgNode.Device(Device(),
                PrtgNode.Trigger(Trigger("First Trigger")),
                PrtgNode.Trigger(Trigger("Second Trigger", 2))
            );

            var second = PrtgNode.Device(Device(),
                PrtgNode.Trigger(Trigger("Second Trigger", 2))
            );

            ValidatePretty(first, second, new[]
            {
                "<Yellow>dc-1</Yellow>",
                "├──<Red>First Trigger</Red>",
                "└──Second Trigger"
            }, new[]
            {
                "<Yellow>dc-1</Yellow>",
                "├──Second Trigger",
                "└──<Green>First Trigger</Green>"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_SecondLastNodeRemoved_LastNodeProperty()
        {
            var first = PrtgNode.Device(Device(),
                PrtgNode.Property(Property("First Property")),
                PrtgNode.Property(Property("Second Property"))
            );

            var second = PrtgNode.Device(Device(),
                PrtgNode.Property(Property("Second Property"))
            );

            ValidatePretty(first, second, new[]
            {
                "<Yellow>dc-1</Yellow>",
                "├──<Red>First Property</Red>",
                "└──Second Property"
            }, new[]
            {
                "<Yellow>dc-1</Yellow>",
                "├──Second Property",
                "└──<Green>First Property</Green>"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_PropertyValue_Same()
        {
            var first = PrtgNode.Device(Device(),
                PrtgNode.Property(Property("First Property"))
            );

            var second = PrtgNode.Device(Device(),
                PrtgNode.Property(Property("First Property"))
            );

            ValidatePretty(first, second, new[]
            {
                "dc-1",
                "└──First Property"
            }, new[]
            {
                "dc-1",
                "└──First Property"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_PropertyValue_Different()
        {
            var first = PrtgNode.Device(Device(),
                PrtgNode.Property(Property("First Property"))
            );

            var second = PrtgNode.Device(Device(),
                PrtgNode.Property(Property("First Property", null))
            );

            ValidatePretty(first, second, new[]
            {
                "dc-1",
                "└──<Yellow>First Property ('dc-1' -> '')</Yellow>"
            }, new[]
            {
                "dc-1",
                "└──<Yellow>First Property ('' -> 'dc-1')</Yellow>"
            });
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_TriggerValue_Different()
        {
            //Don't need a test for Same because having a different action on the same trigger
            //is considered a rename, but a value change

            var first = PrtgNode.Device(Device(),
                PrtgNode.Trigger(Trigger("Original Action"))
            );

            var second = PrtgNode.Device(Device(),
                PrtgNode.Trigger(Trigger("Renamed Trigger"))
            );

            ValidatePretty(first, second, new[]
            {
                "dc-1",
                "└──<Yellow>Original Action (Renamed 'Renamed Trigger')</Yellow>"
            }, new[]
            {
                "dc-1",
                "└──<Yellow>Renamed Trigger (Renamed 'Original Action')</Yellow>"
            });
        }
        
        [UnitTest]
        [TestMethod]
        public void Tree_Compare_Position_Different()
        {
            var first = PrtgNode.Device(Device(),
                PrtgNode.Sensor(Sensor("Sensor 1", 4001, position: 1)),
                PrtgNode.Sensor(Sensor("Sensor 2", 4002, position: 2))
            );

            var second = PrtgNode.Device(Device(),
                PrtgNode.Sensor(Sensor("Sensor 2", 4002, position: 1)),
                PrtgNode.Sensor(Sensor("Sensor 1", 4001, position: 2))
            );

            ValidatePretty(first, second, new[]
            {
                "dc-1",
                "├──<Yellow>Sensor 1 (Position)</Yellow>",
                "└──<Yellow>Sensor 2 (Position)</Yellow>"
            }, new[]
            {
                "dc-1",
                "├──<Yellow>Sensor 2 (Position)</Yellow>",
                "└──<Yellow>Sensor 1 (Position)</Yellow>"
            });
        }

        internal static void Validate(
            PrtgNode first,
            PrtgNode second,
            Validation[] validateFirst,
            Validation[] validateSecond = null,
            Action<CompareNode, CompareNode> additionalValidations = null)
        {
            if (validateSecond == null)
                validateSecond = validateFirst;

            var firstComparison = first.CompareTo(second);

            foreach (var comparison in validateFirst)
            {
                Assert.AreEqual(comparison.Expected, comparison.GetActual(firstComparison), "Validation of first comparison failed");
            }

            var secondComparison = second.CompareTo(first);

            foreach (var comparison in validateSecond)
            {
                Assert.AreEqual(comparison.Expected, comparison.GetActual(secondComparison), "Validation of second comparison failed");
            }

            additionalValidations?.Invoke(firstComparison, secondComparison);
        }

        private void ValidatePretty(PrtgNode first, PrtgNode second, string[] validateFirst, string[] validateSecond = null)
        {
            if (validateSecond == null)
                validateSecond = validateFirst;

            var firstComparison = first.CompareTo(second);
            firstComparison.PrettyPrint(new ComparePrettyValidator(validateFirst));

            var secondComparison = second.CompareTo(first);
            secondComparison.PrettyPrint(new ComparePrettyValidator(validateSecond));
        }
    }
}
