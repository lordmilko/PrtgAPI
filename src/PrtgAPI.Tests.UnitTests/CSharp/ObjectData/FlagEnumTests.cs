using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tree;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class FlagEnumTests
    {
        #region Create

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Create_SingleValue()
        {
            var value = FlagEnum.Create(TreeNodeDifference.ParentId);

            Assert.AreEqual(TreeNodeDifference.ParentId, value.Value);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Create_Array_Params()
        {
            var value = FlagEnum.Create(TreeNodeDifference.ParentId, TreeNodeDifference.Position);

            Assert.AreEqual(TreeNodeDifference.ParentId | TreeNodeDifference.Position, value.Value);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Create_List()
        {
            List<TreeNodeDifference> list = new List<TreeNodeDifference>
            {
                TreeNodeDifference.ParentId,
                TreeNodeDifference.Position
            };

            var value = FlagEnum.Create(list);

            Assert.AreEqual(TreeNodeDifference.ParentId | TreeNodeDifference.Position, value.Value);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Create_IEnumerable()
        {
            IEnumerable<TreeNodeDifference> enumerable = new[] {TreeNodeDifference.ParentId, TreeNodeDifference.Position};

            var value = FlagEnum.Create(enumerable);

            Assert.AreEqual(TreeNodeDifference.ParentId | TreeNodeDifference.Position, value.Value);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Create_ReadOnlyCollection()
        {
            ReadOnlyCollection<TreeNodeDifference> list = new List<TreeNodeDifference>
            {
                TreeNodeDifference.ParentId,
                TreeNodeDifference.Position
            }.AsReadOnly();

            var value = FlagEnum.Create(list);

            Assert.AreEqual(TreeNodeDifference.ParentId | TreeNodeDifference.Position, value.Value);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Create_IList()
        {
            IList<TreeNodeDifference> list = new List<TreeNodeDifference>
            {
                TreeNodeDifference.ParentId,
                TreeNodeDifference.Position
            };

            var value = FlagEnum.Create(list);

            Assert.AreEqual(TreeNodeDifference.ParentId | TreeNodeDifference.Position, value.Value);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Create_IReadOnlyCollection()
        {
            IReadOnlyCollection<TreeNodeDifference> list = new List<TreeNodeDifference>
            {
                TreeNodeDifference.ParentId,
                TreeNodeDifference.Position
            }.AsReadOnly();

            var value = FlagEnum.Create(list);

            Assert.AreEqual(TreeNodeDifference.ParentId | TreeNodeDifference.Position, value.Value);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_Create_IReadOnlyList()
        {
            IReadOnlyList<TreeNodeDifference> list = new List<TreeNodeDifference>
            {
                TreeNodeDifference.ParentId,
                TreeNodeDifference.Position
            }.AsReadOnly();

            var value = FlagEnum.Create(list);

            Assert.AreEqual(TreeNodeDifference.ParentId | TreeNodeDifference.Position, value.Value);
        }

        #endregion

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
            Assert.IsTrue(value.GetValues().Count() == 2);
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
        public void FlagEnum_ArrayConstructor_NegateCombined_One()
        {
            var value = new FlagEnum<TreeParseOption>(TreeParseOption.Common, ~TreeParseOption.Probes);

            Assert.AreEqual(3, value.GetValues().Count);
            Assert.IsTrue(value == (TreeParseOption.Sensors | TreeParseOption.Devices | TreeParseOption.Groups));
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ArrayConstructor_NegateCombined_Two()
        {
            var value = new FlagEnum<TreeParseOption>(TreeParseOption.Common, ~TreeParseOption.Sensors, ~TreeParseOption.Probes);

            Assert.AreEqual(2, value.GetValues().Count);
            Assert.IsTrue(value == (TreeParseOption.Devices | TreeParseOption.Groups));
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ArrayConstructor_NegationOnly_One()
        {
            var value = new FlagEnum<TreeParseOption>(~TreeParseOption.Probes);

            var expected = TreeParseOption.Sensors | TreeParseOption.Devices | TreeParseOption.Groups | TreeParseOption.Triggers | TreeParseOption.Properties;

            Assert.IsTrue(expected == value);
            Assert.AreEqual(5, value.GetValues().Count);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ArrayConstructor_NegationOnly_Two()
        {
            var value = new FlagEnum<TreeParseOption>(~TreeParseOption.Probes & ~TreeParseOption.Sensors);

            var expected = TreeParseOption.Devices | TreeParseOption.Groups | TreeParseOption.Triggers | TreeParseOption.Properties;

            Assert.IsTrue(expected == value);
            Assert.AreEqual(4, value.GetValues().Count);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ArrayConstructor_NegationOnly_One_Array()
        {
            var value = new FlagEnum<TreeParseOption>(new[]{~TreeParseOption.Probes});

            var expected = TreeParseOption.Sensors | TreeParseOption.Devices | TreeParseOption.Groups | TreeParseOption.Triggers | TreeParseOption.Properties;

            Assert.IsTrue(expected == value);
            Assert.AreEqual(5, value.GetValues().Count);
        }

        [UnitTest]
        [TestMethod]
        public void FlagEnum_ArrayConstructor_NegationOnly_Two_Array()
        {
            var value = new FlagEnum<TreeParseOption>(~TreeParseOption.Probes, ~TreeParseOption.Sensors);

            var expected = TreeParseOption.Devices | TreeParseOption.Groups | TreeParseOption.Triggers | TreeParseOption.Properties;

            Assert.IsTrue(expected == value);
            Assert.AreEqual(4, value.GetValues().Count);
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
        public void FlagEnum_Operators_BitwiseComplement()
        {
            var original = new FlagEnum<TreeParseOption>(TreeParseOption.Common);

            var complement = ~original;

            Assert.IsTrue((TreeParseOption.Triggers | TreeParseOption.Properties) == complement);
            Assert.AreEqual(2, complement.GetValues().Count);
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
