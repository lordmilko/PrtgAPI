using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.Tree;
using PrtgAPI.Tree;

namespace PrtgAPI.Tests.UnitTests.Tree
{
    [TestClass]
    public class TreeCompareReduceTests : TreeCompareTests
    {
        [UnitTest]
        [TestMethod]
        public void Tree_Compare_Reduce_NoDifference()
        {
            var node = PrtgNode.Probe(Probe("Parent"),
                PrtgNode.Group(Group("Child1"))
            );

            var comparison = node.CompareTo(node).Reduce();

            Assert.IsNull(comparison);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_Reduce_Root()
        {
            var first = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device()),
                PrtgNode.Device(Device("exch-1")
            ));

            var second = PrtgNode.Probe(Probe(id: 1002),
                PrtgNode.Device(Device()),
                PrtgNode.Device(Device("sql-1", id: 3002))
            );

            ValidateReduce(
                first, second,
                new[] {new Validation(TreeNodeDifference.None, c => c.Difference)},
                new[]
                {
                    "<Red>Local Probe</Red>",
                    "├──<Red>dc-1</Red>",
                    "└──<Red>exch-1</Red>",
                    "<Green>Local Probe</Green>",
                    "├──<Green>dc-1</Green>",
                    "└──<Green>sql-1</Green>"
                },
                null,
                new[]
                {
                    "<Red>Local Probe</Red>",
                    "├──<Red>dc-1</Red>",
                    "└──<Red>sql-1</Red>",
                    "<Green>Local Probe</Green>",
                    "├──<Green>dc-1</Green>",
                    "└──<Green>exch-1</Green>",
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_Reduce_AlreadyReduced_ReturnsSameTree()
        {
            var first = PrtgNode.Probe(Probe("Parent"),
                PrtgNode.Group(Group("Child1")),
                PrtgNode.Group(Group("Child2"))
            );

            var second = PrtgNode.Probe(Probe("Parent"),
                PrtgNode.Group(Group("Child1"))
            );

            var reduced = first.CompareTo(second).Reduce();

            var reducedAgain = reduced.Reduce();

            Assert.IsTrue(ReferenceEquals(reduced, reducedAgain));
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_Reduce_Root_NoDifference()
        {
            var node = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device()),
                PrtgNode.Device(Device("exch-1")
            ));

            var comparison = node.CompareTo(node).Reduce();

            Assert.IsNull(comparison);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_Reduce_ParentTwoChildren_OneChildRemoved()
        {
            var first = PrtgNode.Probe(Probe("Parent"),
                PrtgNode.Group(Group("Child1")),
                PrtgNode.Group(Group("Child2"))
            );

            var second = PrtgNode.Probe(Probe("Parent"),
                PrtgNode.Group(Group("Child1"))
            );

            ValidateReduce(
                first, second,
                new[] { new Validation(TreeNodeDifference.NumberOfChildren, c => c.Difference) },
                new[]
                {
                    "<Yellow>Parent</Yellow>",
                    "└──<Red>Child2</Red>"
                },
                null,
                new[]
                {
                    "<Yellow>Parent</Yellow>",
                    "└──<Green>Child2</Green>"
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_Reduce_GrandParentParentTwoChildren_OneChildRemoved()
        {
            var first = PrtgNode.Probe(Probe("GrandParent"),
                PrtgNode.Group(Group("Parent"),
                    PrtgNode.Device(Device("Child1")),
                    PrtgNode.Device(Device("Child2"))
                )
            );

            var second = PrtgNode.Probe(Probe("GrandParent"),
                PrtgNode.Group(Group("Parent"),
                    PrtgNode.Device(Device("Child1"))
                )
            );

            ValidateReduce(
                first, second,
                new[] {new Validation(TreeNodeDifference.None, c => c.Difference)},
                new[]
                {
                    "GrandParent",
                    "└──<Yellow>Parent</Yellow>",
                    "   └──<Red>Child2</Red>"
                },
                null,
                new[]
                {
                    "GrandParent",
                    "└──<Yellow>Parent</Yellow>",
                    "   └──<Green>Child2</Green>"
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Compare_Reduce_GrandParentTwoParentsOneChildEach_OneParentAndChildRemoved()
        {
            var first = PrtgNode.Probe(Probe("GrandParent"),
                PrtgNode.Group(Group("Parent1"),
                    PrtgNode.Device(Device("Child1"))
                ),
                PrtgNode.Group(Group("Parent2"),
                    PrtgNode.Device(Device("Child2"))
                )
            );

            var second = PrtgNode.Probe(Probe("GrandParent"),
                PrtgNode.Group(Group("Parent1"),
                    PrtgNode.Device(Device("Child1"))
                )
            );

            ValidateReduce(
                first, second,
                new[] {new Validation(TreeNodeDifference.NumberOfChildren, c => c.Difference)},
                new[]
                {
                    "<Yellow>GrandParent</Yellow>",
                    "└──<Red>Parent2</Red>",
                    "   └──<Red>Child2</Red>"
                },
                null,
                new[]
                {
                    "<Yellow>GrandParent</Yellow>",
                    "└──<Green>Parent2</Green>",
                    "   └──<Green>Child2</Green>"
                }
            );
        }

        protected void ValidateReduce(
            PrtgNode first,
            PrtgNode second,
            Validation[] validateFirst,
            string[] firstReduced,
            Validation[] validateSecond = null,
            string[] secondReduced = null)
        {
            if (secondReduced == null)
                secondReduced = firstReduced;

            Validate(first, second, validateFirst, validateSecond, (c1, c2) =>
            {
                var r1 = c1.Reduce();
                var r2 = c2.Reduce();

                r1.PrettyPrint(new ComparePrettyValidator(firstReduced));
                r2.PrettyPrint(new ComparePrettyValidator(secondReduced));
            });
        }
    }
}
