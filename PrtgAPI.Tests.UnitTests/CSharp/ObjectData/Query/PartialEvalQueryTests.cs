using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class PartialEvalQueryTests : BaseQueryTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_FromVariable()
        {
            var val = "Volume IO _Total0";

            ExecuteFilter(
                s => s.Name.Contains(val),
                "filter_name=@sub(Volume+IO+_Total0)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_AnonymousType_WithVariable()
        {
            var val = "Volume IO _Total0";

            ExecuteFilter(
                s => new
                {
                    Prop1 = s.Name,
                    Prop2 = val
                }.Prop2.Contains(new
                {
                    Prop1 = s.Name,
                    Prop2 = val
                }.Prop1),
                "filter_name=@sub(Volume+IO+_Total0)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_FromArray()
        {
            var arr = new[]
            {
                "Volume IO _Total0"
            };

            ExecuteFilter(
                s => s.Name.Contains(arr[0]),
                "filter_name=@sub(Volume+IO+_Total0)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_FromConst()
        {
            const string var = "Volume IO _Total0";

            ExecuteFilter(
                s => s.Name.Contains(var),
                "filter_name=@sub(Volume+IO+_Total0)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        private string value = "Volume IO _Total0";

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_FromThis()
        {
            ExecuteFilter(
                s => s.Name.Contains(value),
                "filter_name=@sub(Volume+IO+_Total0)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_LocalLambda()
        {
            Func<Sensor, bool> lambda = s => s.Name == "Volume IO _Total1";

            ExecuteFilter(s => lambda(s), string.Empty, s => Assert.AreEqual("Volume IO _Total1", s.Single().Name));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_LocalMethod()
        {
            ExecuteFilter(s => LocalMethod(s), string.Empty, s => Assert.AreEqual("Volume IO _Total1", s.Single().Name));
        }

        private bool LocalMethod(Sensor sensor)
        {
            return sensor.Name == "Volume IO _Total1";
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_CastToSpecific()
        {
            ExecuteFilter(s => (DateTime)(object)s.LastUp > (DateTime)(object)new DateTime(2000, 10, 2, 19, 2, 1, DateTimeKind.Utc), "filter_lastup=@above(36801.7930671296)", s => Assert.AreEqual("Volume IO _Total2", s.Single().Name));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_CastToObject()
        {
            ExecuteFilter(s => ((object)s.Name).Equals((object)"Volume IO _Total1"), "filter_name=Volume+IO+_Total1", s => Assert.AreEqual("Volume IO _Total1", s.Single().Name));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_WithVariableLambda()
        {
            Expression<Func<Sensor, bool>> expr = s => s.Name == "Volume IO _Total0";

            var countOverride = new Dictionary<Content, int>
            {
                [Content.Sensors] = 3
            };

            Execute(
                c =>
                {
                    var result = c.QuerySensors().Where(expr).ToList();

                    Assert.AreEqual(1, result.Count);
                },
                UnitRequest.Sensors("count=500&filter_name=Volume+IO+_Total0", UrlFlag.Columns),
                countOverride
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_WithVariableMember()
        {
            Sensor t = new Sensor { Name = "test" };
            ExecuteFilter(s => t.Name == "test", string.Empty, s => Assert.AreEqual(3, s.Count));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_PartialEval_AgainstVariableMember()
        {
            Sensor t = new Sensor { Name = "Volume IO _Total2" };
            ExecuteFilter(s => t.Name == s.Name, "filter_name=Volume+IO+_Total2", s => Assert.AreEqual("Volume IO _Total2", s.Single().Name));
        }
    }
}
