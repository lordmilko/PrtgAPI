using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests
{
    [TestClass]
    public class SearchFilterTests
    {
        [TestMethod]
        public void SearchFilter_OperatorEquals_ToString_CorrectFormat()
        {
            Operator_ToString_CorrectFormat(FilterOperator.Equals, s => s);
        }

        [TestMethod]
        public void SearchFilter_OperatorContains_ToString_CorrectFormat()
        {
            Operator_ToString_CorrectFormat(FilterOperator.Contains, s => $"@sub({s})");
        }

        [TestMethod]
        public void SearchFilter_OperatorNotEquals_ToString_CorrectFormat()
        {
            Operator_ToString_CorrectFormat(FilterOperator.NotEquals, s => $"@neq({s})");
        }

        [TestMethod]
        public void SearchFilter_OperatorGreaterThan_ToString_CorrectFormat()
        {
            Operator_ToString_CorrectFormat(FilterOperator.GreaterThan, s => $"@above({s})");
        }

        [TestMethod]
        public void SearchFilter_OperatorLessThan_ToString_CorrectFormat()
        {
            Operator_ToString_CorrectFormat(FilterOperator.LessThan, s => $"@below({s})");
        }

        [TestMethod]
        public void SearchFilter_EnumValue_WithXmlEnum_CorrectFormat()
        {
            ToString_CorrectFormat(Property.Priority, FilterOperator.Equals, Priority.Four, "priority", "4");
        }

        //Helpers

        private void Operator_ToString_CorrectFormat(FilterOperator op, Func<string,string> formatted)
        {
            ToString_CorrectFormat(Property.Name, op, "test", "name", formatted("test"));
        }

        private void ToString_CorrectFormat(Property property, FilterOperator op, object value, string filterName, string filterValue)
        {
            var filter = new SearchFilter(property, op, value);

            var str = filter.ToString();

            Assert.IsTrue(str == $"filter_{filterName}={filterValue}");
        }
    }
}
