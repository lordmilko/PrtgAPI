using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class LastTests : BaseQueryTests
    {
        [UnitTest]
        [TestMethod]
        public void Query_Last_NoPredicate()
        {
            ExecuteNow(q => q.Last(), string.Empty, r => Assert.AreEqual("Volume IO _Total2", r.Name));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Last_WithPredicate()
        {
            ExecuteNow(q => q.Last(s => s.Id == 4002), "filter_objid=4002", r => Assert.AreEqual("Volume IO _Total2", r.Name));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Last_NoPredicate_WithWhere()
        {
            ExecuteNow(q => q.Where(s => s.Name.Contains("Volume")).Last(), "filter_name=@sub(Volume)", r => Assert.AreEqual("Volume IO _Total2", r.Name));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Last_WithPredicate_WithWhere()
        {
            ExecuteNow(q => q.Where(s => s.Name.Contains("Volume")).Last(s => s.Id == 4001), "filter_name=@sub(Volume)", r => Assert.AreEqual("Volume IO _Total1", r.Name));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Last_NoPredicate_AfterTake()
        {
            ExecuteNow(q => q.Take(2).Last(), "count=2", r => Assert.AreEqual("Volume IO _Total1", r.Name), UrlFlag.Columns);
        }

        [UnitTest]
        [TestMethod]
        public void Query_Last_Predicate_AfterTake()
        {
            ExecuteNow(q => q.Take(2).Last(s => s.Id == 4000), "count=2", r => Assert.AreEqual("Volume IO _Total0", r.Name), UrlFlag.Columns);
        }

        [UnitTest]
        [TestMethod]
        public void Query_LastOrDefault_NoPredicate()
        {
            ExecuteNow(q => q.LastOrDefault(), string.Empty, r => Assert.AreEqual(null, r), count: 0);
        }

        [UnitTest]
        [TestMethod]
        public void Query_LastOrDefault_WithPredicate()
        {
            ExecuteNow(q => q.LastOrDefault(s => s.Name == "Ping"), "filter_name=Ping", r => Assert.AreEqual(null, r));
        }
    }
}
