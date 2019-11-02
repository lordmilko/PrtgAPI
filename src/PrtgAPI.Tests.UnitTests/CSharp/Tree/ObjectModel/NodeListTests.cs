using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tree;

namespace PrtgAPI.Tests.UnitTests.Tree
{
    [TestClass]
    public class NodeListTests : BaseTreeTest
    {
        #region Add

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_Add_NoExistingChildren()
        {
            var probe = PrtgNode.Probe(Probe());

            var newList = probe.Children.Add(PrtgNode.Device(Device()));

            Assert.AreNotEqual(probe.Children, newList);

            //The list is standalone until it's added as the child
            Assert.IsNull(newList[0].Parent);

            var newProbe = probe.WithChildren(newList);

            Assert.IsNotNull(newProbe.Children[0].Parent);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_Add_ExistingChildren()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var newList = probe.Children.Add(PrtgNode.Group(Group()));

            Assert.AreNotEqual(probe.Children, newList);

            Assert.IsTrue(newList.All(i => i.Parent == null));

            var newProbe = probe.WithChildren(newList);

            Assert.IsTrue(newProbe.Children.All(c => c != null));
        }

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_AddRange_NoExistingChildren()
        {
            var probe = PrtgNode.Probe(Probe());

            var newList = probe.Children.AddRange(new PrtgNode[]
            {
                PrtgNode.Device(Device()),
                PrtgNode.Group(Group())
            });

            Assert.AreNotEqual(probe.Children, newList);

            Assert.AreEqual(2, newList.Count);
            Assert.IsTrue(newList.All(v => v.Parent == null));

            var newProbe = probe.WithChildren(newList);

            Assert.IsTrue(newProbe.Children.All(v => v.Parent != null));
        }

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_AddRange_ExistingChildren()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var newList = probe.Children.AddRange(new PrtgNode[]
            {
                PrtgNode.Device(Device("dc-2")),
                PrtgNode.Group(Group())
            });

            Assert.AreNotEqual(probe.Children, newList);

            Assert.AreEqual(3, newList.Count);
            Assert.IsTrue(newList.All(v => v.Parent == null));

            var newProbe = probe.WithChildren(newList);

            Assert.IsTrue(newProbe.Children.All(v => v.Parent != null));
        }

        #endregion
        #region Insert

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_Insert_Null_Throws()
        {
            var probe = PrtgNode.Probe(Probe());

            AssertEx.Throws<ArgumentNullException>(
                () => probe.Children.Insert(0, null),
                "Value cannot be null"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_InsertRange_Null_Throws()
        {
            var probe = PrtgNode.Probe(Probe());

            AssertEx.Throws<ArgumentNullException>(
                () => probe.Children.InsertRange(0, null),
                "Value cannot be null"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_InsertRange_InvalidIndex_Throws()
        {
            var probe = PrtgNode.Probe(Probe());

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => probe.Children.InsertRange(-1, new[] { PrtgNode.Device(Device()) }),
                "Specified argument was out of the range of valid values"
            );

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => probe.Children.InsertRange(1, new[] { PrtgNode.Device(Device()) }),
                "Specified argument was out of the range of valid values"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_InsertRange_EmptyList()
        {
            var probe = PrtgNode.Probe(Probe());

            var newList = probe.Children.InsertRange(0, Enumerable.Empty<PrtgNode>());

            Assert.AreEqual(probe.Children, newList);
        }

        #endregion
        #region Remove

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_RemoveAt()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device()),
                PrtgNode.Group(Group())
            );

            var newList = probe.Children.RemoveAt(0);
            Assert.AreEqual(1, newList.Count);

            Assert.AreEqual(newList[0].Name, "Servers");
        }

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_Remove_Null_Throws()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            AssertEx.Throws<ArgumentNullException>(
                () => probe.Children.Remove(null),
                "Value cannot be null"
            );
        }

        #endregion
        #region Replace

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_Replace()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var newChild = PrtgNode.Group(Group());

            var newList = probe.Children.Replace(probe["dc-1"], newChild);

            Assert.AreNotEqual(probe.Children, newList);
            Assert.AreEqual(1, newList.Count);
            Assert.AreEqual("Servers", newList[0].Name);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_ReplaceRange_OldNode_Null_Throws()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            AssertEx.Throws<ArgumentNullException>(
                () => probe.Children.ReplaceRange(null, Enumerable.Empty<PrtgNode>()),
                "Value cannot be null"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_ReplaceRange_NewNodes_Null_Throws()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            AssertEx.Throws<ArgumentNullException>(
                () => probe.Children.ReplaceRange(probe["dc-1"], null),
                "Value cannot be null"
            );
        }

        #endregion
        #region Item

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_Item_InBounds()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            var device = probe.Children[0];
            Assert.IsNotNull(device);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_NodeList_Item_OutOfBounds()
        {
            var probe = PrtgNode.Probe(Probe(),
                PrtgNode.Device(Device())
            );

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Console.WriteLine(probe.Children[-1]),
                "Specified argument was out of the range of valid values"
            );

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Console.WriteLine(probe.Children[1]),
                "Specified argument was out of the range of valid values"
            );
        }

        #endregion
    }
}
