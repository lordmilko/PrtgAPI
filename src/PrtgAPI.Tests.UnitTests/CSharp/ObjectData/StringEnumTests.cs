using System;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    enum SimpleEnum
    {
        First,
        Second
    }

    enum DescriptionEnum
    {
        [System.ComponentModel.Description("CustomFirst")]
        First,
        Second
    }

    enum XmlEnum
    {
        [XmlEnum("XmlFirst")]
        First,
        Second
    }

    [TestClass]
    public class StringEnumTests
    {
        [UnitTest]
        [TestMethod]
        public void StringEnum_ParsesEnum_WithoutAttribute()
        {
            var e = new StringEnum<SimpleEnum>(SimpleEnum.First);

            Assert.AreEqual(SimpleEnum.First, e.Value, "Enum value was incorrect");
            Assert.AreEqual("first", e.StringValue, "String value was incorrect");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Throws_WithInvalidEnumValue()
        {
            AssertEx.Throws<ArgumentException>(() => new StringEnum<SimpleEnum>((SimpleEnum)3), "'3' is not a valid value for type 'SimpleEnum'.");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_ParsesString_WithoutCorrespondingAttribute()
        {
            var e = new StringEnum<SimpleEnum>("first");

            Assert.AreEqual(SimpleEnum.First, e.Value, "Enum value was incorrect");
            Assert.AreEqual(SimpleEnum.First, e, "Implicit enum value was incorrect");
            Assert.AreEqual("first", e.StringValue, "String value was incorrect");
            Assert.AreEqual("first", e, "Implicit string value was incorrect");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_ParsesEnum_WithDescription()
        {
            var e = new StringEnum<DescriptionEnum>(DescriptionEnum.First);

            Assert.AreEqual(DescriptionEnum.First, e.Value, "Enum value was incorrect");
            Assert.AreEqual(DescriptionEnum.First, e, "Implicit enum value was incorrect");
            Assert.AreEqual("CustomFirst", e.StringValue, "String value was incorrect");
            Assert.AreEqual("CustomFirst", e, "Implicit string value was incorrect");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_ParsesString_ForDescription()
        {
            var e = new StringEnum<DescriptionEnum>("CustomFirst");

            Assert.AreEqual(DescriptionEnum.First, e.Value, "Enum value was incorrect");
            Assert.AreEqual(DescriptionEnum.First, e, "Implicit enum value was incorrect");
            Assert.AreEqual("CustomFirst", e.StringValue, "String value was incorrect");
            Assert.AreEqual("CustomFirst", e, "Implicit string value was incorrect");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_ParsesEnum_WithXmlEnum()
        {
            var e = new StringEnum<XmlEnum>(XmlEnum.First);

            Assert.AreEqual(XmlEnum.First, e.Value, "Enum value was incorrect");
            Assert.AreEqual(XmlEnum.First, e, "Implicit enum value was incorrect");
            Assert.AreEqual("XmlFirst", e.StringValue, "String value was incorrect");
            Assert.AreEqual("XmlFirst", e, "Implicit string value was incorrect");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_ParsesString_ForXmlEnum()
        {
            var e = new StringEnum<XmlEnum>("XmlFirst");

            Assert.AreEqual(XmlEnum.First, e.Value, "Enum value was incorrect");
            Assert.AreEqual(XmlEnum.First, e, "Implicit enum value was incorrect");
            Assert.AreEqual("XmlFirst", e.StringValue, "String value was incorrect");
            Assert.AreEqual("XmlFirst", e, "Implicit string value was incorrect");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_ParsesString_Invalid()
        {
            var e = new StringEnum<SimpleEnum>("invalid");

            Assert.AreEqual(null, e.Value, "Enum value was not null");
            Assert.AreEqual("invalid", e.StringValue, "String value was incorrect");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Assigns_FromEnum()
        {
            StringEnum<SimpleEnum> e = SimpleEnum.Second;

            Assert.AreEqual(SimpleEnum.Second, e);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Assigns_FromString()
        {
            StringEnum<SimpleEnum> e = "second";

            Assert.AreEqual(SimpleEnum.Second, e);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Equals_StringEnum_SameEnumType()
        {
            StringEnum<SimpleEnum> first = SimpleEnum.First;
            StringEnum<SimpleEnum> second = "first";

            Assert.IsTrue(first == second);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Equals_StringEnum_SameEnumType_DifferentValues()
        {
            StringEnum<SimpleEnum> first = SimpleEnum.First;
            StringEnum<SimpleEnum> second = "second";

            Assert.IsFalse(first == second);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Equals_StringEnum_DifferentEnumType()
        {
            StringEnum<SimpleEnum> first = SimpleEnum.First;
            StringEnum<DescriptionEnum> second = DescriptionEnum.First;

            Assert.IsFalse(first.Equals(second));
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_NotEquals_StringEnum_SameEnumType()
        {
            StringEnum<SimpleEnum> first = SimpleEnum.First;
            StringEnum<SimpleEnum> second = "second";

            Assert.IsTrue(first != second);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Equals_Null()
        {
            StringEnum<SimpleEnum> first = SimpleEnum.First;
            StringEnum<SimpleEnum> second = "second";
            StringEnum<SimpleEnum> third = "invalid";
            StringEnum<SimpleEnum> fourth = null;

            Assert.AreNotEqual(first, null);
            Assert.AreNotEqual(second, null);
            Assert.AreNotEqual(third, null);
            Assert.AreEqual(fourth, null);

            Assert.IsFalse(first == null);
            Assert.IsFalse(second == null);
            Assert.IsFalse(third == null);
            Assert.IsFalse(fourth != null);

            Assert.IsTrue(first != null);
            Assert.IsTrue(second != null);
            Assert.IsTrue(third != null);
            Assert.IsTrue(fourth == null);
            Assert.ReferenceEquals(fourth, null);
            Assert.IsFalse(first.Equals(null));
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Equals_Enum_WithSplitValue()
        {
            var val = new StringEnum<DescriptionEnum>(DescriptionEnum.Second, "str");

            Assert.IsTrue(val == DescriptionEnum.Second);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_NotEquals_Enum_WithCustomString()
        {
            var val = new StringEnum<DescriptionEnum>("str");

            Assert.IsFalse(val == DescriptionEnum.Second);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Equals_StringAgainstSplitValue()
        {
            var first = new StringEnum<DescriptionEnum>("str");
            var second = new StringEnum<DescriptionEnum>(DescriptionEnum.Second, "str");

            Assert.IsTrue(first == second);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_NotEquals_DifferentEnum()
        {
            var first = new StringEnum<SimpleEnum>(SimpleEnum.First);
            var second = new StringEnum<SimpleEnum>(SimpleEnum.Second);

            Assert.IsTrue(first != second);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_NotEquals_DifferentEnumSameStringSplitValue()
        {
            var first = new StringEnum<SimpleEnum>(SimpleEnum.First, "str");
            var second = new StringEnum<SimpleEnum>(SimpleEnum.Second, "str");

            Assert.IsFalse(first == second);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_NotEquals_DifferentStringAgainstSplitValue()
        {
            var first = new StringEnum<DescriptionEnum>("str1");
            var second = new StringEnum<DescriptionEnum>(DescriptionEnum.Second, "str");

            Assert.IsFalse(first == second);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Equals_MatchingString()
        {
            StringEnum<DescriptionEnum> first = DescriptionEnum.First;

            Assert.IsTrue(first == "customfirst");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_NotEquals_MatchingString()
        {
            StringEnum<DescriptionEnum> first = DescriptionEnum.First;

            Assert.IsFalse(first != "customfirst");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Equals_InvalidString()
        {
            StringEnum<SimpleEnum> first = SimpleEnum.First;

            Assert.IsFalse(first == "invalid");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Equals_SameEnumAndValue()
        {
            var first = new StringEnum<SimpleEnum>(SimpleEnum.First, "str");
            var second = new StringEnum<SimpleEnum>(SimpleEnum.First, "str");

            Assert.IsTrue(first == second);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_NotEquals_SameEnumDifferentString()
        {
            var first = new StringEnum<SimpleEnum>(SimpleEnum.First, "str");
            var second = new StringEnum<SimpleEnum>(SimpleEnum.First, "str1");

            Assert.IsFalse(first == second);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_NotEquals_InvalidString()
        {
            StringEnum<SimpleEnum> first = SimpleEnum.First;

            Assert.IsTrue(first != "invalid");
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_ChangesString_WhenStringDoesntMatchDescription()
        {
            var e = new StringEnum<ObjectType>("probe");
            Assert.AreEqual(ObjectType.Probe, e.Value);
            Assert.AreEqual("probenode", e.StringValue);
        }

        [UnitTest]
        [TestMethod]
        public void StringEnum_Validates_ConstructorArguments()
        {
            AssertEx.Throws<ArgumentNullException>(() => new StringEnum<SimpleEnum>(null), "Value cannot be null.");
            AssertEx.Throws<ArgumentException>(() => new StringEnum<SimpleEnum>(string.Empty), "stringValue cannot be empty or whitespace.");
        }
    }
}
