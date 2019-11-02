using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class CountTests : BaseQueryTests
    {
        [UnitTest]
        [TestMethod]
        public void Query_Count_NoPredicate()
        {
            ExecuteNow(q => q.Count(), string.Empty, r => Assert.AreEqual(3, r));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Count_WithPredicate()
        {
            ExecuteNow(q => q.Count(s => s.Id == 4002), "filter_objid=4002", r => Assert.AreEqual(1, r));
        }
    }
}
