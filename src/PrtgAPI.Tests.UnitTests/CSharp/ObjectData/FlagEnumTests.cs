using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tree;
using System;
using System.Linq;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class FlagEnumTests
    {
        [UnitTest]
        [TestMethod]
        public void FlagEnum_DefaultConstructor_ShouldBeDefaultValue()
        {
            var value = new FlagEnum<TreeNodeDifference>();
            Assert.IsTrue(value.Equals(TreeNodeDifference.None));
            Assert.IsFalse(TreeNodeDifference.None.Equals(value));
            Assert.IsTrue(value == TreeNodeDifference.None);
            Assert.IsTrue(TreeNodeDifference.None == value);

            Assert.IsTrue(value.GetValues().Single() == TreeNodeDifference.None);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ValueConstructor_DefaultValue()
        {
            var value = new FlagEnum<TreeNodeDifference>(TreeNodeDifference.None);
            Assert.IsTrue(value == TreeNodeDifference.None);
            Assert.IsTrue(value.GetValues().Single() == TreeNodeDifference.None);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ValueConstructor_SingleValue()
        {
            var value = new FlagEnum<TreeNodeDifference>(TreeNodeDifference.Name);
            Assert.IsTrue(value == TreeNodeDifference.Name);
            Assert.IsTrue(value.GetValues().Single() == TreeNodeDifference.Name);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ValueConstructor_FlagValue()
        {
            var value = new FlagEnum<TreeNodeDifference>(TreeNodeDifference.Name | TreeNodeDifference.NumberOfChildren);
            Assert.AreEqual(2, value.GetValues().Count);
            Assert.IsTrue(value.Equals(TreeNodeDifference.Name | TreeNodeDifference.NumberOfChildren));
            Assert.IsTrue(value.Contains(TreeNodeDifference.Name));
            Assert.IsTrue(value.Contains(TreeNodeDifference.NumberOfChildren));
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ArrayConstructor_DefaultValue()
        {
            var value = new FlagEnum<TreeNodeDifference>(new[] { TreeNodeDifference.None });
            Assert.IsTrue(value.Equals(TreeNodeDifference.None));
            Assert.IsTrue(value.GetValues().Single() == TreeNodeDifference.None);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ArrayConstructor_SingleValue()
        {
            var value = new FlagEnum<TreeNodeDifference>(new[] { TreeNodeDifference.Name });
            Assert.IsTrue(value.Equals(TreeNodeDifference.Name));
            Assert.IsTrue(value.GetValues().Single() == TreeNodeDifference.Name);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ArrayConstructor_FlagValue()
        {
            var both = TreeNodeDifference.Name | TreeNodeDifference.NumberOfChildren;

            var value = new FlagEnum<TreeNodeDifference>(new[] { both });
            Assert.IsTrue(both == value);
            Assert.IsTrue(value.GetValues().Single() == both);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ArrayConstructor_IndividualValues()
        {
            var value = new FlagEnum<TreeNodeDifference>(new[] { TreeNodeDifference.Name, TreeNodeDifference.NumberOfChildren });
            Assert.IsTrue(value == (TreeNodeDifference.Name | TreeNodeDifference.NumberOfChildren));
            Assert.AreEqual(2, value.GetValues().Count);
            Assert.IsTrue(TreeNodeDifference.Name == value.GetValues()[0]);
            Assert.IsTrue(TreeNodeDifference.NumberOfChildren == value.GetValues()[1]);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ArrayConstructor_Null()
        {
            var value = new FlagEnum<TreeNodeDifference>(null);
            Assert.IsTrue(value == TreeNodeDifference.None);
            Assert.IsTrue(value.GetValues().Single() == TreeNodeDifference.None);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ArrayConstructor_EmptyList()
        {
            var value = new FlagEnum<TreeNodeDifference>(new TreeNodeDifference[] { });
            Assert.IsTrue(value == TreeNodeDifference.None);
            Assert.IsTrue(value.GetValues().Single() == TreeNodeDifference.None);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Implicit_ToFlagEnum()
        {
            FlagEnum<TreeNodeDifference> value = TreeNodeDifference.Name | TreeNodeDifference.NumberOfChildren;
            Assert.IsTrue(value == (TreeNodeDifference.Name | TreeNodeDifference.NumberOfChildren));
            Assert.AreEqual(2, value.GetValues().Count);

            Assert.IsTrue(TreeNodeDifference.Name == value.GetValues()[0]);
            Assert.IsTrue(TreeNodeDifference.NumberOfChildren == value.GetValues()[1]);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Implicit_FromFlagEnum()
        {
            var value = new FlagEnum<TreeNodeDifference>(TreeNodeDifference.Name | TreeNodeDifference.NumberOfChildren);

            TreeNodeDifference enumValue = value;

            Assert.IsTrue((enumValue & TreeNodeDifference.Name) == TreeNodeDifference.Name);
            Assert.IsTrue((enumValue & TreeNodeDifference.NumberOfChildren) == TreeNodeDifference.NumberOfChildren);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Operators_BitwiseOr()
        {
            FlagEnum<TreeNodeDifference> first = TreeNodeDifference.Name;
            FlagEnum<TreeNodeDifference> second = TreeNodeDifference.NumberOfChildren;

            var value = first | second;

            Assert.AreEqual(2, value.GetValues().Count);
            Assert.IsTrue(value.Equals(TreeNodeDifference.Name | TreeNodeDifference.NumberOfChildren));
            Assert.IsTrue(value.Contains(TreeNodeDifference.Name));
            Assert.IsTrue(value.Contains(TreeNodeDifference.NumberOfChildren));
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Operators_BitwiseAnd()
        {
            FlagEnum<TreeNodeDifference> first = TreeNodeDifference.Name | TreeNodeDifference.NumberOfChildren;

            var value = first & TreeNodeDifference.Name;

            Assert.IsTrue(value == TreeNodeDifference.Name);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_NonEnumValue_Throws()
        {
            AssertEx.Throws<ArgumentException>(
                () => new FlagEnum<DateTime>(DateTime.Now),
                "Value must be an enum"
            );
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_NonEnumArray_Throws()
        {
            AssertEx.Throws<ArgumentException>(
                () => new FlagEnum<DateTime>(new[] { DateTime.Now }),
                "Value must be an enum"
            );
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ToString_SingleFlag()
        {
            var value = new FlagEnum<TreeNodeDifference>(TreeNodeDifference.Name);

            Assert.AreEqual("Name", value.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ToString_MultipleFlags()
        {
            var value = new FlagEnum<TreeNodeDifference>(TreeNodeDifference.Name | TreeNodeDifference.NumberOfChildren);

            Assert.AreEqual("Name, NumberOfChildren", value.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_GetHashCode_EqualsEnum()
        {
            var value = new FlagEnum<TreeNodeDifference>(TreeNodeDifference.Name);

            Assert.IsTrue(value.Equals((object) TreeNodeDifference.Name));
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_GetHashCode_EqualsFlagEnum()
        {
            var value = new FlagEnum<TreeNodeDifference>(TreeNodeDifference.Name);

            Assert.IsTrue(value.Equals((object) new FlagEnum<TreeNodeDifference>(TreeNodeDifference.Name)));
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_GetHashCode_EqualsEnumHashCode()
        {
            var value = new FlagEnum<TreeNodeDifference>(TreeNodeDifference.Name);

            Assert.AreEqual(value.GetHashCode(), TreeNodeDifference.Name.GetHashCode());
        }
    }
}
