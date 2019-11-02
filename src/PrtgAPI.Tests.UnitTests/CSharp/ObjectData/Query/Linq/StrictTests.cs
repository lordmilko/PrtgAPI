using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class StrictTests : BaseQueryTests
    {
        [UnitTest]
        [TestMethod]
        public void Query_Strict_UnsupportedMethod()
        {
            ExecuteUnsupported(
                q => q.TakeWhile(s => true),
                "'TakeWhile' is not supported",

                q => q.AsEnumerable().TakeWhile(s => true),
                string.Empty,
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_SensorType()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Type == ObjectType.Sensor),
                "PRTG only supports filters where the most 'derived' object type is specified.",

                q => q.AsEnumerable().Where(s => s.Type == ObjectType.Sensor),
                string.Empty,
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_StartsWith()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Name.StartsWith("Vol")),
                "Cannot call method 'StartsWith' when using strict evaluation semantics.",

                q => q.AsEnumerable().Where(s => s.Name.StartsWith("Vol")),
                string.Empty,
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_EndsWith()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Name.EndsWith("0")),
                "Cannot call method 'EndsWith' when using strict evaluation semantics.",

                q => q.AsEnumerable().Where(s => s.Name.EndsWith("0")),
                string.Empty,
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_NotContains()
        {
            ExecuteUnsupported(
                q => q.Where(s => !s.Name.Contains("Volume")),
                "Cannot evaluate multi-part expression 's.Name.Contains(\"Volume\") == False'.",

                q => q.AsEnumerable().Where(s => !s.Name.Contains("Volume")),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_InvalidCondition()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.LastUp == null),
                "Expression 's.LastUp == null' could not be translated to a valid SearchFilter as a valid value could not be found.",

                q => q.AsEnumerable().Where(s => s.LastUp == null),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_LastUnsupported_NoPredicate()
        {
            ExecuteUnsupportedNow(
                q => q.Last(),
                "Method 'Last' is not supported.",
                
                q => q.AsEnumerable().Last(),
                string.Empty,
                s => Assert.AreEqual(4002, s.Id)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_LastUnsupported_Predicate()
        {
            ExecuteUnsupportedNow(
                q => q.Last(s => s.Id > 4000),
                "Method 'Last' is not supported. Consider changing your expression to 'Query(PrtgAPI.Sensor).Where(s => (s.Id > 4000)).AsEnumerable().Last()'.",

                q => q.Where(s => s.Id > 4000).AsEnumerable().Last(),
                "filter_objid=@above(4000)",
                s => Assert.AreEqual(4002, s.Id)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_InvalidEnumComparison()
        {
            ExecuteUnsupported(
                q => q.Where(s => ((Enum) s.Status).Equals((Enum) RetryMode.Retry)),
                $"Cannot evaluate condition '({Cast(Cast("<root>.Status", "Enum"), "Object")} == {Cast("Retry", "Object")})' between enums of type 'PrtgAPI.Status' and 'PrtgAPI.RetryMode'.",

                q => q.AsEnumerable().Where(s => ((Enum) s.Status).Equals((Enum) RetryMode.Retry)),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_MultiPartExpression()
        {
            ExecuteUnsupported(
                q => q.Where(s => (bool?)(s.Id == 4000) == false),
                $"Cannot evaluate multi-part expression '{Cast("(s.Id == 4000)", "Nullable`1")} == False'.",

                q => q.AsEnumerable().Where(s => (bool?)(s.Id == 4000) == false),
                string.Empty,
                s => Assert.AreEqual(2, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_MergeMultipleParameters()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Id == 4000).Where((s, i) => s.Name.Contains("Vol") && i == 0),
                "Cannot merge method \'Where\' in expression \'Query(PrtgAPI.Sensor).Where(s => (s.Id == 4000)).Where((s, i) => (s.Name.Contains(\"Vol\") AndAlso (i == 0)))\': one or more lambda expressions reference multiple lambda parameters.",

                q => q.Where(s => s.Id > 4000).AsEnumerable().Where((s, i) => s.Name.Contains("Vol") && i == 0),
                "filter_objid=@above(4000)",
                s => Assert.AreEqual("Volume IO _Total1", s.Single().Name)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_UnsupportedProperty_NoPropertyEnumMember()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.DisplayLastValue == "test"),
                "Property 'DisplayLastValue' cannot be evaluated server side.",

                q => q.AsEnumerable().Where(s => s.DisplayLastValue == "test"),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_UnsupportedProperty_WithPropertyEnumMember()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.NotificationTypes.Equals(new NotificationTypes("Inherited"))),
                "Cannot filter by property 'NotificationTypes': filter is not supported by PRTG.",

                q => q.AsEnumerable().Where(s => s.NotificationTypes.Equals(new NotificationTypes("Inherited"))),
                string.Empty,
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_UnsupportedValue()
        {
            ExecuteUnsupported(
                q => q.Where(s => !s.Favorite),
                "Cannot filter where property 'Favorite' equals 'False'.",

                q => q.AsEnumerable().Where(s => !s.Favorite),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_AmbiguousIntermediate()
        {
            ExecuteUnsupported(
                q => q.Select(s => new RealTypeConstructor(s.Name, s.BaseType)).Where(r => r.RealName == "Volume IO _Total0"),
                "Cannot resolve source property of member 'r.RealName', possibly due to the use of a constructor or the member not having been assigned a value.",

                q => q.AsEnumerable().Select(s => new RealTypeConstructor(s.Name, s.BaseType)).Where(r => r.RealName == "Volume IO _Total0"),
                string.Empty,
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_AmbiguousCondition()
        {
            ExecuteUnsupported(
                q => q.Where(s => ((bool)(object)(s.Id == 4000)).ToString().Contains("Volume")),
                $"Cannot evaluate expression '{Cast(Cast("(s.Id == 4000)", "Object"), "Boolean")}.ToString().Contains(\"Volume\")' containing multiple sub-conditions ('{Cast(Cast("(s.Id == 4000)", "Object"), "Boolean")}.ToString().Contains(\"Volume\")', 's.Id == 4000').",

                q => q.AsEnumerable().Where(s => ((bool)(object)(s.Id == 4000)).ToString().Contains("Volume")),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_UnconsecutiveCall()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Id == 4000).Take(2).Where(s => s.Name.Contains("test")),
                "'Where' cannot be called again",

                q => q.Where(s => s.Id == 4000).Take(2).AsEnumerable().Where(s => s.Name.Contains("Volume")),
                "count=2&filter_objid=4000",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_LeftRightSameProperty_Standalone()
        {
            //The regular exception won't be thrown because there is no AND/OR condition to cause
            //LegalFilterParser to inspect the expression
            ExecuteUnsupported(
                q => q.Where(s => s.Id == s.Id + 1),
                "Condition 's.Id == (s.Id + 1)' references multiple source properties (s.Id, s.Id).",

                q => q.AsEnumerable().Where(s => s.Id == s.Id + 1),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_LeftRightSameProperty_WithAnotherCondition()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Id == s.Id + 1 && s.ParentId == 12),
                "Condition 's.Id == (s.Id + 1)' references multiple source properties (s.Id, s.Id).",

                q => q.AsEnumerable().Where(s => s.Id == s.Id + 1),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_LeftRightSameProperty_AndAlso()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Id == s.Id + 1 && s.ParentId == 1001),
                "Condition 's.Id == (s.Id + 1)' references multiple source properties (s.Id, s.Id).",

                q => q.AsEnumerable().Where(s => s.Id == s.Id + 1 && s.ParentId == 1001),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_LeftRightDifferentProperties()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Name == s.Message),
                "Condition 's.Name == s.Message' references multiple source properties (s.Name, s.Message).",

                q => q.AsEnumerable().Where(s => s.Name == s.Message),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_LocalEval_Left()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Id == 4000 && false),
                "The expression 'False' did not contain any property references and cannot be evaluated server side.",

                q => q.Where(s => s.Id == 4000).AsEnumerable().Where(s => false),
                "filter_objid=4000",
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_LocalEval_Right()
        {
            ExecuteUnsupported(
                q => q.Where(s => false && s.Id == 4000),
                "The expression 'False' did not contain any property references and cannot be evaluated server side.",

                q => q.Where(s => s.Id == 4000).AsEnumerable().Where(s => false),
                "filter_objid=4000",
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_Or_DifferentProperties()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Id == 4000 || s.Name.Contains("Volume")),
                "Cannot perform logical OR between properties 'Id' and 'Name'.",

                q => q.AsEnumerable().Where(s => s.Id == 4000 || s.Name.Contains("Volume")),
                string.Empty,
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_Log_DoubleStart()
        {
            ExecuteUnsupportedLog(
                q => q.Where(l => l.DateTime < Time.Today && l.DateTime < Time.Yesterday),
                $"Cannot specify multiple upper DateTime bounds in a single request. One of '{Time.Today}', '{Time.Yesterday}' must be specified.",

                q => q.Where(l => l.DateTime < Time.Today).AsEnumerable().Where(l => l.DateTime < Time.Yesterday),
                $"start=1&filter_dend={Time.TodayStr}",
                l => Assert.AreEqual(5, l.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_Log_DoubleEnd()
        {
            ExecuteUnsupportedLog(
                q => q.Where(l => l.DateTime > Time.TwoDaysAgo && l.DateTime > Time.Yesterday),
                $"Cannot specify multiple lower DateTime bounds in a single request. One of '{Time.TwoDaysAgo}', '{Time.Yesterday}' must be specified.",

                q => q.Where(l => l.DateTime > Time.TwoDaysAgo).AsEnumerable().Where(l => l.DateTime > Time.Yesterday),
                $"start=1&filter_dstart={Time.TwoDaysAgoStr}",
                l => Assert.AreEqual(0, l.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_Log_UnsupportedFilters()
        {
            ExecuteUnsupportedLog(
                q => q.Where(l => l.Device == "dc-1" && l.Group == "Servers" && l.Name == "Ping" && l.Id == 4000),
                "Cannot filter logs by properties 'Device', 'Group', 'Name': specified filters are not supported.",

                q => q.Where(l => l.Id == 4000).AsEnumerable().Where(l => l.Device == "dc-1" && l.Group == "Servers" && l.Name == "Ping"),
                "start=1&id=4000",
                l => Assert.AreEqual(0, l.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_Log_DoubleId()
        {
            ExecuteUnsupportedLog(
                q => q.Where(l => l.Id == 4000 || l.Id == 4001),
                "Cannot filter logs by multiple IDs in a single request. One of '4000', '4001' must be specified.",

                q => q.AsEnumerable().Where(l => l.Id == 4000 || l.Id == 4001),
                "start=1",
                l => Assert.AreEqual(0, l.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_Log_OrRange()
        {
            ExecuteUnsupportedLog(
                q => q.Where(l => l.DateTime < Time.Today && l.DateTime > Time.Yesterday || l.DateTime < Time.Yesterday),
                "Cannot perform logical OR against property 'DateTime': property can only be specified once in an expression.",

                q => q.AsEnumerable().Where(l => l.DateTime < Time.Today && l.DateTime > Time.Yesterday || l.DateTime < Time.Yesterday),
                "start=1",
                l => Assert.AreEqual(5, l.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_And_SameProperty()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Name.Contains("Vol") && s.Name.Contains("ume")),
                "Cannot perform multiple AND conditions against the property 'Name'.",

                q => q.Where(s => s.Name.Contains("Vol")).AsEnumerable().Where(s => s.Name.Contains("ume")),
                "filter_name=@sub(Vol)",
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_And_SameProperty_SplitGroup()
        {                   
            ExecuteUnsupported(
                q => q.Where(s => (s.Name == "Volume IO _Total0" && s.ParentId == 2193) && s.Name == "Volume IO _Total1"),
                "Cannot perform multiple AND conditions against the property 'Name'.",

                q => q.Where(s => s.Name == "Volume IO _Total0" && s.ParentId == 2193).AsEnumerable().Where(s => s.Name == "Volume IO _Total1"),
                "filter_name=Volume+IO+_Total0&filter_parentid=2193",
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_And_SameProperty_SplitWhere()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Name == "Volume IO _Total0" && s.ParentId == 2193).Where(s => s.Name == "Volume IO _Total1"),
                "Cannot perform multiple AND conditions against the property 'Name'.",

                q => q.Where(s => s.Name == "Volume IO _Total0" && s.ParentId == 2193).AsEnumerable().Where(s => s.Name == "Volume IO _Total1"),
                "filter_name=Volume+IO+_Total0&filter_parentid=2193",
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_Or_DifferentProperty_SplitGroup()
        {
            ExecuteUnsupported(
                q => q.Where(s => (s.Name.Contains("Volume IO") || s.Name.Contains("Total")) || s.ParentId == 2193),
                "Cannot perform logical OR between properties 'Name' and 'ParentId'.",

                q => q.AsEnumerable().Where(s => (s.Name.Contains("Volume IO") || s.Name.Contains("Total")) || s.ParentId == 2193),
                string.Empty,
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_MethodCall_Property()
        {
            ExecuteUnsupported(
                q => q.Where(s => StrictMethodProperty(s.Id)),
                "Method 'StrictMethodProperty'",

                q => q.AsEnumerable().Where(s => StrictMethodProperty(s.Id)),
                string.Empty,
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_MethodCall_Source()
        {
            ExecuteUnsupported(
                q => q.Where(s => StrictMethodSource(s)),
                "Condition 'value(PrtgAPI.Tests.UnitTests.ObjectData.Query.StrictTests).StrictMethodSource(s)' did not contain a property expression.",

                q => q.AsEnumerable().Where(StrictMethodSource),
                string.Empty,
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_IllegalCast()
        {
            ExecuteUnsupported(
                q => q.Where(s => ((IllegalInt)s.Id) == 4000),
                "Expression 's.Id' of type 'System.Int32' cannot be casted to type 'IllegalInt'.",

                q => q.AsEnumerable().Where(s => ((IllegalInt)s.Id) == 4000),
                string.Empty,
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_IllegalToString()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.LastUp.Value.ToString().Contains("18")),
                "Method 'ToString' cannot be used on bool, class, enum or struct type 'DateTime'",

                q => q.AsEnumerable().Where(s => s.LastUp.Value.ToString().Contains("18")),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_BoolToString()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Active.ToString() == "True"),
                "Method 'ToString' cannot be used on bool, class, enum or struct type 'Boolean' in condition 's.Active.ToString() == \"True\"'.",

                q => q.AsEnumerable().Where(s => s.Active.ToString() == "True"),
                string.Empty,
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_IllegalExpressionType()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.Tags.Length == 2),
                "Expression type 'ArrayLength' cannot be used as part of a property expression in condition 'ArrayLength(s.Tags) == 2'.",

                q => q.AsEnumerable().Where(s => s.Tags.Length == 2),
                string.Empty,
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_IllegalParent()
        {
            ExecuteUnsupported(
                q => q.Where(s => -s.Id == -4000),
                "Expression type 'Negate' cannot be used as the parent of a property expression in condition '-s.Id == -4000'.",

                q => q.AsEnumerable().Where(s => -s.Id == -4000),
                string.Empty,
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_MissingProperty()
        {
            ExecuteUnsupported(
                q => q.Select(s => new { First = 3 }).Where(f => f.First == 3),
                "Condition 'f.First == 3' did not contain a property expression. A single source property must be referenced.",

                q => q.AsEnumerable().Select(s => new { First = 3 }).Where(f => f.First == 3),
                string.Empty,
                s => Assert.IsTrue(s.All(e => e.First == 3))
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Strict_ExtraMembers_Where()
        {
            ExecuteUnsupported(
                q => q.Where(s => s.LastUp.Value.Day == 30),
                "Condition 's.LastUp.Value.Day == 30' references one or more sub-members of a property expression (s.LastUp.Value.Day). Property sub-members cannot be evaluated when specified in a condition.",

                q => q.AsEnumerable().Where(s => s.LastUp.Value.Day == 30),
                string.Empty,
                s => Assert.AreEqual(0, s.Count)
            );
        }

        private bool StrictMethodProperty(int id)
        {
            return true;
        }

        private bool StrictMethodSource(Sensor s)
        {
            return true;
        }

        protected void ExecuteUnsupported<TResult>(
            Func<IQueryable<Sensor>, IQueryable<TResult>> illegalAction,
            string message,
            Func<IQueryable<Sensor>, IEnumerable<TResult>> legalAction,
            string legalUrl,
            Action<List<TResult>> legalValidator
        )
        {
            ExecuteUnsupportedInternal(
                c => c.QuerySensors(true),
                illegalAction,
                message,
                legalAction,
                legalUrl,
                legalValidator
            );
        }

        protected void ExecuteUnsupportedLog<TResult>(
            Func<IQueryable<Log>, IQueryable<TResult>> illegalAction,
            string message,
            Func<IQueryable<Log>, IEnumerable<TResult>> legalAction,
            string legalUrl,
            Action<List<TResult>> legalValidator
        )
        {
            ExecuteUnsupportedInternal(
                c => c.QueryLogs(true),
                illegalAction,
                message,
                legalAction,
                legalUrl,
                legalValidator
            );
        }

        protected void ExecuteUnsupportedInternal<TSource, TResult>(
            Func<PrtgClient, IQueryable<TSource>> query,
            Func<IQueryable<TSource>, IQueryable<TResult>> illegalAction,
            string message,
            Func<IQueryable<TSource>, IEnumerable<TResult>> legalAction,
            string legalUrl,
            Action<List<TResult>> legalValidator
            )
        {
            AssertEx.Throws<NotSupportedException>(
                () => ExecuteClient(c => illegalAction(query(c)), new string[] { }, s => s.ToList()),
                message
            );

            var flags = UnitRequest.DefaultObjectFlags & ~UrlFlag.Count;

            if (!legalUrl.Contains("count"))
                legalUrl = "count=500" + (string.IsNullOrEmpty(legalUrl) ? legalUrl : $"&{legalUrl}");

            var urls = new[]
            {
                typeof(TSource) == typeof(Log) ? UnitRequest.Logs(legalUrl, flags) : UnitRequest.Sensors(legalUrl, flags)
            };

            ExecuteClient(c => legalAction(query(c)), urls, s => legalValidator(s.ToList()));
        }

        protected void ExecuteUnsupportedNow<TResult>(
            Func<IQueryable<Sensor>, TResult> illegalAction,
            string message,
            Func<IQueryable<Sensor>, TResult> legalAction,
            string legalUrl,
            Action<TResult> legalValidator
        )
        {
            AssertEx.Throws<NotSupportedException>(
                () => ExecuteClient(c => illegalAction(c.QuerySensors(true)), new string[] { }, null),
                message
            );

            var flags = UnitRequest.DefaultObjectFlags & ~UrlFlag.Count;

            if (!legalUrl.Contains("count"))
                legalUrl = "count=500" + (string.IsNullOrEmpty(legalUrl) ? legalUrl : $"&{legalUrl}");

            var urls = new[]
            {
                UnitRequest.Sensors(legalUrl, flags)
            };

            ExecuteClient(c => legalAction(c.QuerySensors(true)), urls, legalValidator);
        }
    }
}
