using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.Infrastructure
{
    [TestClass]
    public class SearchFilterTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilter_OperatorEquals_EmptyString()
        {
            AssertEx.Throws<ArgumentException>(() => new SearchFilter(Property.Name, string.Empty), "Search Filter value cannot be an empty string.");

            var filter = new SearchFilter(Property.Name, FilterOperator.Equals, string.Empty, FilterMode.Illegal);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilter_OperatorEquals_ToString_CorrectFormat()
        {
            Operator_ToString_CorrectFormat(FilterOperator.Equals, s => s);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilter_OperatorContains_ToString_CorrectFormat()
        {
            Operator_ToString_CorrectFormat(FilterOperator.Contains, s => $"@sub({s})");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilter_OperatorNotEquals_ToString_CorrectFormat()
        {
            AssertEx.Throws<NotSupportedException>(
                () => Operator_ToString_CorrectFormat(FilterOperator.NotEquals, s => $"@neq({s})"),
                "Cannot filter where property 'Name' notequals 'test'"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilter_OperatorGreaterThan_ToString_CorrectFormat()
        {
            Operator_ToString_CorrectFormat(FilterOperator.GreaterThan, s => $"@above({s})", FilterMode.Illegal);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilter_OperatorLessThan_ToString_CorrectFormat()
        {
            Operator_ToString_CorrectFormat(FilterOperator.LessThan, s => $"@below({s})", FilterMode.Illegal);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilter_EnumValue_WithXmlEnum_CorrectFormat()
        {
            ToString_CorrectFormat(Property.Priority, FilterOperator.Equals, Priority.Four, "priority", "4");
        }

        //Helpers

        private void Operator_ToString_CorrectFormat(FilterOperator op, Func<string,string> formatted, FilterMode filterMode = FilterMode.Normal)
        {
            ToString_CorrectFormat(Property.Name, op, "test", "name", formatted("test"), filterMode);
        }

        private void ToString_CorrectFormat(Property property, FilterOperator op, object value, string filterName, string filterValue, FilterMode filterMode = FilterMode.Normal)
        {
            var filter = new SearchFilter(property, op, value, filterMode);

            var str = filter.ToString();

            Assert.IsTrue(str == $"filter_{filterName}={filterValue}");
        }
    }
}
