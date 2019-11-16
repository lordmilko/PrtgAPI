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

        protected void Validate(
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
