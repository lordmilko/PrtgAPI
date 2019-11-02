using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class OrderByTests : BaseQueryTests
    {
        [UnitTest]
        [TestMethod]
        public void Query_OrderBy_MultipleProperties()
        {
            ExecuteQuery(q => q.OrderBy(s => s.Id).OrderBy(s => s.Name), "sortby=name", s =>
            {
                Assert.AreEqual("Volume IO _Total0", s[0].Name);
                Assert.AreEqual("Volume IO _Total1", s[1].Name);
                Assert.AreEqual("Volume IO _Total2", s[2].Name);
            });
        }

        [UnitTest]
        [TestMethod]
        public void Query_OrderBy_ThreeTimes()
        {
            ExecuteQuery(q => q.OrderBy(s => s.Id).OrderBy(s => s.Name).OrderBy(s => s.Id), "sortby=objid", s =>
            {
                Assert.AreEqual("Volume IO _Total0", s[0].Name);
                Assert.AreEqual("Volume IO _Total1", s[1].Name);
                Assert.AreEqual("Volume IO _Total2", s[2].Name);
            });
        }

        [UnitTest]
        [TestMethod]
        public void Query_OrderBy_ForwardsThenBackwards()
        {
            ExecuteQuery(q => q.OrderBy(s => s.Id).OrderByDescending(s => s.Name), "sortby=-name", s =>
            {
                Assert.AreEqual("Volume IO _Total2", s[0].Name);
                Assert.AreEqual("Volume IO _Total1", s[1].Name);
                Assert.AreEqual("Volume IO _Total0", s[2].Name);
            });
        }

        [UnitTest]
        [TestMethod]
        public void Query_OrderBy_KeyComparer()
        {
            ExecuteQuery(q => q.OrderBy(s => s.Name, StringComparer.CurrentCulture), "sortby=name", s => s.ToList());
        }

        [UnitTest]
        [TestMethod]
        public void Query_OrderBy_WithComparer_ToOrderBy_WithoutComparer()
        {
            ExecuteQuery(q => q.OrderBy(s => s.Name, StringComparer.CurrentCulture).OrderBy(s => s.Id), "sortby=objid", s => s.ToList());
        }

        [UnitTest]
        [TestMethod]
        public void Query_OrderBy_WithoutComparer_ToOrderBy_WithComparer()
        {
            ExecuteQuery(q => q.OrderBy(s => s.Id).OrderBy(s => s.Name, StringComparer.CurrentCulture), "sortby=name", s => s.ToList());
        }
    }
}
