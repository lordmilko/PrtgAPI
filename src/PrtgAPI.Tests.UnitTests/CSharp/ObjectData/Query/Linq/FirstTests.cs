using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class FirstTests : BaseQueryTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_First_NoPredicate()
        {
            ExecuteNow(q => q.First(), string.Empty, r => Assert.AreEqual("Volume IO _Total0", r.Name));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_First_WithPredicate()
        {
            ExecuteNow(q => q.First(s => s.Id == 4002), "filter_objid=4002", r => Assert.AreEqual("Volume IO _Total2", r.Name));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_First_NoPredicate_WithWhere()
        {
            ExecuteNow(q => q.Where(s => s.Name.Contains("Volume")).First(), "filter_name=@sub(Volume)", r => Assert.AreEqual("Volume IO _Total0", r.Name));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_First_WithPredicate_WithWhere()
        {
            ExecuteNow(q => q.Where(s => s.Name.Contains("Volume")).First(s => s.Id == 4001), "filter_name=@sub(Volume)", r => Assert.AreEqual("Volume IO _Total1", r.Name));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_First_NoPredicate_AfterTake()
        {
            ExecuteNow(q => q.Take(2).First(), "count=2", r => Assert.AreEqual("Volume IO _Total0", r.Name), UrlFlag.Columns);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_First_Predicate_AfterTake()
        {
            ExecuteNow(q => q.Take(2).First(s => s.Id == 4001), "count=2", r => Assert.AreEqual("Volume IO _Total1", r.Name), UrlFlag.Columns);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_First_Backwards()
        {
            ExecuteNow(q => q.OrderByDescending(s => s.Name).First(), "sortby=-name", r => Assert.AreEqual("Volume IO _Total2", r.Name));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_FirstOrDefault_NoPredicate()
        {
            ExecuteNow(q => q.FirstOrDefault(), string.Empty, r => Assert.AreEqual(null, r), count: 0);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_FirstOrDefault_WithPredicate()
        {
            ExecuteNow(q => q.FirstOrDefault(s => s.Name == "Ping"), "filter_name=Ping", r => Assert.AreEqual(null, r));
        }
    }
}
