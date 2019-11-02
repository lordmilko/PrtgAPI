using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;
using Expr = System.Linq.Expressions.Expression;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    public class IllegalInt
    {
        public int Value { get; set; }

        public static explicit operator int(IllegalInt val)
        {
            return val.Value;
        }

        public static explicit operator IllegalInt(int val)
        {
            return new IllegalInt
            {
                Value = val
            };
        }

        public static bool operator ==(IllegalInt first, double second)
        {
            return first.Value == second;
        }

        public static bool operator !=(IllegalInt first, double second)
        {
            return first.Value == second;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    [TestClass]
    public class WhereTests : BaseQueryTests
    {
        #region Overloads

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceBool_SingleCondition()
        {
            ExecuteFilter(s => s.Name == "Volume IO _Total1", "filter_name=Volume+IO+_Total1", s =>
            {
                Assert.AreEqual(1, s.Count);
                Assert.AreEqual("Volume IO _Total1", s.Single().Name);
            });
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_SingleCondition_IgnoreIndex()
        {
            ExecuteQuery(q => q.Where((s, i) => s.Name == "Volume IO _Total1"), "filter_name=Volume+IO+_Total1", s =>
            {
                Assert.AreEqual(1, s.Count);
                Assert.AreEqual("Volume IO _Total1", s.Single().Name);
            });
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_SingleCondition_UseIndex()
        {
            ExecuteQuery(q => q.Where((s, i) => s.Id == 4000 + i), string.Empty, s =>
            {
                Assert.AreEqual(3, s.Count);
            });
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_FirstConditionIgnoreIndex_SecondConditionUseIndex()
        {
            ExecuteQuery(q => q.Where((s, i) => s.Name == "Volume IO _Total1" && s.Id == 4000 + i), "filter_name=Volume+IO+_Total1", s =>
            {
                Assert.AreEqual(1, s.Count);
                Assert.AreEqual("Volume IO _Total1", s.Single().Name);
            });
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_TwoMaybeIndex_YesNo()
        {
            ExecuteQuery(
                q => q.Where((s, i) => s.Name.Contains("Vol") && i == 0).Where(s => s.Id == 4000),
                "filter_name=@sub(Vol)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_TwoMaybeIndex_NoYes()
        {
            ExecuteQuery(
                q => q.Where(s => s.Name.Contains("Vol")).Where((s, i) => s.Id == 4000 && i == 0),
                "filter_name=@sub(Vol)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_TwoMaybeIndex_YesYes()
        {
            ExecuteQuery(
                q => q.Where((s, i) => s.Name.Contains("Vol") && i == 0).Where((s, j) => s.Id == 4000 && j == 0),
                "filter_name=@sub(Vol)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_YesNoNo()
        {
            ExecuteQuery(
                q => q.Where((s, i) => s.Name.Contains("Vol") && i == 0)
                      .Where(s => s.Id == 4000)
                      .Where(s => s.Name.Contains("ume")),
                "filter_name=@sub(Vol)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_NoYesNo()
        {
            ExecuteQuery(
                q => q.Where(s => s.Name.Contains("Vol"))
                    .Where((s, j) => s.Id == 4000 && j == 0)
                    .Where(s => s.Name.Contains("ume")),
                "filter_name=@sub(Vol)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_NoNoYes()
        {
            ExecuteQuery(
                q => q.Where(s => s.Name.Contains("Vol"))
                    .Where(s => s.Id == 4000)
                    .Where((s, k) => s.Name.Contains("ume") && k == 0),
                "filter_name=@sub(Vol)&filter_objid=4000",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_YesYesNo()
        {
            ExecuteQuery(
                q => q.Where((s, i) => s.Name.Contains("Vol") && i == 0)
                    .Where((s, j) => s.Id == 4000 && j == 0)
                    .Where(s => s.Name.Contains("ume")),
                "filter_name=@sub(Vol)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_NoYesYes()
        {
            ExecuteQuery(
                q => q.Where(s => s.Name.Contains("Vol"))
                    .Where((s, j) => s.Id == 4000 && j == 0)
                    .Where((s, k) => s.Name.Contains("ume") && k == 0),
                "filter_name=@sub(Vol)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_YesNoYes()
        {
            ExecuteQuery(
                q => q.Where((s, i) => s.Name.Contains("Vol") && i == 0)
                    .Where(s => s.Id == 4000)
                    .Where((s, k) => s.Name.Contains("ume") && k == 0),
                "filter_name=@sub(Vol)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_YesYesYes()
        {
            ExecuteQuery(
                q => q.Where((s, i) => s.Name.Contains("Vol") && i == 0)
                    .Where((s, j) => s.Id == 4000 && j == 0)
                    .Where((s, k) => s.Name.Contains("ume") && k == 0),
                "filter_name=@sub(Vol)",
                s => Assert.AreEqual(1, s.Count)
            );
        }

        #endregion
        #region Serialization

        [UnitTest]
        [TestMethod]
        public void Query_Where_Serialize_TimeSpan()
        {
            ExecuteFilter(s => s.TotalDowntime > new TimeSpan(0, 20, 21), "filter_downtimetime=@above(000000000001221)", s => Assert.AreEqual("Volume IO _Total2", s.Single().Name));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Serialize_DateTime()
        {
            ExecuteFilter(s => s.LastUp > new DateTime(2000, 10, 2, 19, 2, 1, DateTimeKind.Utc), "filter_lastup=@above(36801.7930671296)", s => Assert.AreEqual("Volume IO _Total2", s.Single().Name));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Serialize_NullableValue()
        {
            ExecuteFilter(s => s.LastUp.Value == new DateTime(2000, 10, 2, 12, 10, 5, DateTimeKind.Utc), "filter_lastup=36801.5070023148", s => Assert.AreEqual(0, s.Count));
        }

        #endregion
        #region Operators

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators()
        {
            ExecuteFilter(s => s.Name == "Volume IO _Total1",      "filter_name=Volume+IO+_Total1", s => Assert.AreEqual("Volume IO _Total1", s.Single().Name));
            ExecuteFilter(s => s.Name.Equals("Volume IO _Total1"), "filter_name=Volume+IO+_Total1", s => Assert.AreEqual("Volume IO _Total1", s.Single().Name));
            ExecuteFilter(s => s.Id != 4000,                       "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => s.Id > 4000,                        "filter_objid=@above(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => s.Id >= 4000,                       "filter_objid=@above(4000)&filter_objid=4000", s => Assert.AreEqual(3, s.Count));
            ExecuteFilter(s => s.Id < 4001,                        "filter_objid=@below(4001)", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => s.Id <= 4001,                       "filter_objid=@below(4001)&filter_objid=4001", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => s.Name.Contains("Total2"),          "filter_name=@sub(Total2)", s => Assert.AreEqual("Volume IO _Total2", s.Single().Name));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators_Backwards()
        {
            ExecuteFilter(s => "Volume IO _Total1" == s.Name,        "filter_name=Volume+IO+_Total1", s => Assert.AreEqual("Volume IO _Total1", s.Single().Name));
            ExecuteFilter(s => "Volume IO _Total1".Equals(s.Name),   "filter_name=Volume+IO+_Total1", s => Assert.AreEqual("Volume IO _Total1", s.Single().Name));
            ExecuteFilter(s => 4000 != s.Id,                         "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => 4000 < s.Id,                          "filter_objid=@above(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => 4000 <= s.Id,                         "filter_objid=@above(4000)&filter_objid=4000", s => Assert.AreEqual(3, s.Count));
            ExecuteFilter(s => 4001 > s.Id,                          "filter_objid=@below(4001)", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => 4001 >= s.Id,                         "filter_objid=@below(4001)&filter_objid=4001", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => "Volume IO _Total2".Contains(s.Name), "filter_name=@sub(Volume+IO+_Total2)", s => Assert.AreEqual("Volume IO _Total2", s.Single().Name));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators_Equals_EmptyString()
        {
            ExecuteFilter(s => s.Name == string.Empty, string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators_Contains_IntToString()
        {
            ExecuteFilter(s => s.Id.ToString().Contains("40"), "filter_objid=@sub(40)", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators_Equals_IntToString()
        {
            ExecuteFilter(s => s.Id.ToString() == "4000", "filter_objid=4000", s => Assert.AreEqual(1, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators_Equals_NullableDoubleToString()
        {
            ExecuteFilter(s => s.LastValue.ToString() == "3", "filter_lastvalue=0000000000000030.0000", s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators_Contains_StructToString()
        {
            ExecuteFilter(s => s.LastUp.ToString().Contains("2018"), string.Empty, s => Assert.AreEqual(0, s.Count));
            ExecuteFilter(s => s.UpDuration.ToString().Contains("30"), string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators_Contains_ClassToString()
        {
            ExecuteFilter(s => s.NotificationTypes.ToString().Contains("Volume"), string.Empty, s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators_Contains_EnumToString()
        {
            ExecuteFilter(s => s.Status.ToString().Contains("Up"), string.Empty, s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_StartsWith()
        {
            ExecuteFilter(s => s.Name.StartsWith("Vol"), "filter_name=@sub(Vol)", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators_Fake_EndsWith()
        {
            ExecuteFilter(s => s.Name.EndsWith("0"), "filter_name=@sub(0)", s => Assert.AreEqual(1, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators_Fake_StartsWith_Take()
        {
            ExecuteClient(
                c => c.QuerySensors(s => s.Name.StartsWith("Vol")).Take(2),
                new[] { UnitRequest.Sensors("count=500&filter_name=@sub(Vol)", UrlFlag.Columns), },
                s => Assert.AreEqual(2, s.Count())
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Operators_Fake_EndsWith_Take()
        {
            ExecuteClient(
                c => c.QuerySensors(s => s.Name.EndsWith("0")).Take(2),
                new[] { UnitRequest.Sensors("count=500&filter_name=@sub(0)", UrlFlag.Columns) },
                s => Assert.AreEqual(1, s.Count())
            );
        }

        #endregion
        #region Server Logic

        [UnitTest]
        [TestMethod]
        public void Query_Where_ServerLogic_DifferentProperties_And_SingleQuery()
        {
            ExecuteFilter(s => s.Name == "Volume IO _Total1" && s.ParentId == 2193,
                "filter_name=Volume+IO+_Total1&filter_parentid=2193", s =>
                {
                    Assert.AreEqual(1, s.Count);
                    Assert.AreEqual("Volume IO _Total1", s.Single().Name);
                });
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_ServerLogic_DifferentProperties_Or()
        {
            ExecuteFilter(
                s => s.Name == "Volume IO _Total1" || s.ParentId == 2193,
                new[] {"filter_name=Volume+IO+_Total1", "filter_parentid=2193"},
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_ServerLogic_SameProperty_And_SingleQuery()
        {
            ExecuteFilter(s => s.Name == "Volume IO _Total0" && s.Name == "Volume IO _Total1",
                "filter_name=Volume+IO+_Total0", s =>
                {
                    Assert.AreEqual(0, s.Count);
                });

            ExecuteFilter(s => s.Name.Contains("Volume IO") && s.Name.Contains("Total"),
                "filter_name=@sub(Volume+IO)", s =>
                {
                    Assert.AreEqual(3, s.Count);
                });
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_ServerLogic_SameProperty_Or()
        {
            ExecuteFilter(
                s => s.Name == "Volume IO _Total0" || s.Name == "Volume IO _Total1",
                "filter_name=Volume+IO+_Total0&filter_name=Volume+IO+_Total1",
                s =>
                {
                    Assert.AreEqual(2, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s.First().Name);
                    Assert.AreEqual("Volume IO _Total1", s.Last().Name);
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_ServerLogic_SamePropertyOr_AndDifferentProperty()
        {
            ExecuteFilter(
                    s => (s.Name == "Volume IO _Total0" || s.Name == "Volume IO _Total1") && s.ParentId == 2193,
                "filter_name=Volume+IO+_Total0&filter_name=Volume+IO+_Total1&filter_parentid=2193",
                s =>
                {
                    Assert.AreEqual(2, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s.First().Name);
                    Assert.AreEqual("Volume IO _Total1", s.Last().Name);
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_ServerLogic_SameProperty_And_TwoQueries()
        {
            ExecuteQuery(
                q => q.Where(s => s.Name.Contains("Volume IO")).Where(t => t.Name.Contains("Total1")),
                "filter_name=@sub(Volume+IO)",
                s =>
                {
                    Assert.AreEqual(1, s.Count);
                    Assert.AreEqual("Volume IO _Total1", s.Single().Name);
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_ServerLogic_DifferentProperties_And_TwoQueries()
        {
            ExecuteQuery(
                q => q.Where(s => s.Name == "Volume IO _Total1").Where(s => s.ParentId == 2193),
                "filter_name=Volume+IO+_Total1&filter_parentid=2193",
                s =>
                {
                    Assert.AreEqual(1, s.Count);
                    Assert.AreEqual("Volume IO _Total1", s.Single().Name);
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_ServerLogic_DifferentProperties_And_ThreeQueries()
        {
            ExecuteQuery(
                q => q.Where(s => s.Name == "Volume IO _Total1").Where(s => s.ParentId == 2193).Where(s => s.Id == 4001),
                "filter_name=Volume+IO+_Total1&filter_parentid=2193&filter_objid=4001",
                s =>
                {
                    Assert.AreEqual(1, s.Count);
                    Assert.AreEqual("Volume IO _Total1", s.Single().Name);
                }
            );
        }

        #endregion
        #region Boolean Property Logic

        [UnitTest]
        [TestMethod]
        public void Query_Where_Boolean_IsExplicitlyTrue()
        {
            ExecuteFilter(s => s.Active == true, "filter_active=-1", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_BooleanExpression_IsExplicitlyTrue_OnLeft()
        {
            ExecuteFilter(s => true == (s.Id == 4000), "filter_objid=4000", s => Assert.AreEqual(1, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Boolean_IsExplicitlyFalse()
        {
            ExecuteFilter(s => s.Active == false, "filter_active=0", s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Boolean_IsImplicitlyTrue()
        {
            ExecuteFilter(s => s.Active, "filter_active=-1", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Boolean_IsImplicitlyTrue_AndCondition()
        {
            ExecuteFilter(s => s.Active && s.Id == 4001, "filter_active=-1&filter_objid=4001", s => Assert.AreEqual(1, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Boolean_IsImplicitlyTrue_OrCondition()
        {
            ExecuteFilter(s => s.Active || !s.Active, "filter_active=-1&filter_active=0", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Boolean_NotEqualsTrue()
        {
            ExecuteFilter(s => s.Active != true, "filter_active=@neq(-1)", s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Boolean_NotEqualsFalse()
        {
            ExecuteFilter(s => s.Active != false, "filter_active=@neq(0)", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Boolean_IsNotTrue()
        {
            ExecuteFilter(s => s.Active == !true, "filter_active=0", s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Boolean_IsNotNotTrue()
        {
            ExecuteFilter(s => s.Active == !!true, "filter_active=-1", s => Assert.AreEqual(3, s.Count));
        }

        #endregion
        #region Enum Property Logic

        [UnitTest]
        [TestMethod]
        public void Query_Where_Enum_Normal()
        {
            ExecuteFilter(s => s.Status == Status.Up, "filter_status=3", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Enum_ToString()
        {
            ExecuteFilter(s => s.Status.ToString() == "Up", string.Empty, s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Enum_ToString_IllegalString()
        {
            ExecuteFilter(s => s.Status.ToString() == "Banana", string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Enum_AgainstDifferentEnumType_DifferentNumber()
        {
            ExecuteFilter(s => ((Enum)s.Status).Equals((Enum)RetryMode.Retry), string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Enum_AgainstDifferentEnumType_ManualExpression()
        {
            var lambda = BaseExpressionTest.CreateLambda(Property.Status, s =>
            {
                var retry = Expr.Constant(RetryMode.Retry);
                var methodInfo = typeof(object).GetMethod(nameof(Equals), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                
                var method = Expr.Call(methodInfo, Expr.Convert(retry, typeof(object)), Expr.Convert(s, typeof(object)));

                return method;
            });

            ExecuteFilter(lambda, string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Enum_AgainstDifferentEnumType_DifferentNumber_RightToLeft()
        {
            ExecuteFilter(s => ((Enum)RetryMode.Retry).Equals((Enum)s.BaseType), string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Enum_AgainstDifferentEnumType_Method()
        {
            ExecuteFilter(s => ((Enum)s.Status).Equals(SomeEnum(s)), string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Enum_AgainstDifferentEnumType_NoCast()
        {
            ExecuteFilter(s => s.Status.Equals(RetryMode.Retry), string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        private Enum SomeEnum(Sensor s)
        {
            return RetryMode.Retry;
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Enum_AgainstDifferentEnumType_SameNumber()
        {
            ExecuteFilter(s => ((Enum)s.BaseType).Equals((Enum)AuthMode.PassHash), string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Enum_AgainstDifferentEnumType_ToString()
        {
            ExecuteFilter(s => (((Enum)s.Status).ToString()).Equals(((Enum)RetryMode.Retry).ToString()), string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        #endregion
        #region Illegal Logic

        [UnitTest]
        [TestMethod]
        public void Query_Where_IllegalLogic_Property_EqualsNull()
        {
            ExecuteFilter(s => s.LastUp == null, string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_IllegalLogic_PropertyOrDifferent_And_Property()
        {
            //Cannot OR different properties, cannot AND the same property
            ExecuteFilter(
                    s => (s.Name == "Volume IO _Total0" || s.ParentId == 2193) && s.Name == "Volume IO _Total1",
                new[] {"filter_name=Volume+IO+_Total0", "filter_parentid=2193"},
                s =>
                {
                    Assert.AreEqual(1, s.Count);
                    Assert.AreEqual("Volume IO _Total1", s.Single().Name);
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_IllegalLogic_PropertyAndProperty_Or_Different()
        {
            //Cannot AND the same property, cannot OR different properties
            ExecuteFilter(
                s => (s.Name.Contains("Volume IO") && s.Name.Contains("Total")) || s.ParentId == 2193,
                new[] {"filter_name=@sub(Volume+IO)", "filter_parentid=2193"},
                s => Assert.AreEqual(3, s.Count)
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_IllegalLogic_Property1_Against_Property2()
        {
            ExecuteFilter(s => s.Name == s.Message, string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_IllegalLogic_IllegalMethod_EqualsValue()
        {
            ExecuteFilter(s => s.Name.Substring(0, 3) == "Vol", string.Empty, s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_IllegalLogic_InternalLambda()
        {
            ExecuteFilter(s => s.Tags.Where(t => t.Contains("wmi")).Any(), string.Empty, s =>
            {
                Assert.AreEqual(3, s.Count);
            });
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_IllegalLogic_PropertyMember()
        {
            ExecuteFilter(s => s.LastUp.Value.DayOfWeek == DayOfWeek.Tuesday, string.Empty, s => Assert.AreEqual("Volume IO _Total2", s.Single().Name));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_IllegalLogic_PropertyMethod()
        {
            ExecuteFilter(s => s.LastUp.Value.AddHours(1) == DateTime.Now, string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        #endregion
        #region Negated Conditions

        [UnitTest]
        [TestMethod]
        public void Query_Where_Negation_EqualsFalse_Supported()
        {
            ExecuteFilter(s => s.Id == 4000 == false,                       "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => s.Id.ToString().Equals("4000") == false,     "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => "Volume IO _Total1" != s.Name == false,      "filter_name=Volume+IO+_Total1", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => 4000 < s.Id == false,                        "filter_objid=@below(4000)&filter_objid=4000", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => 4001 <= s.Id == false,                       "filter_objid=@below(4001)", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => 4001 > s.Id == false,                        "filter_objid=@above(4001)&filter_objid=4001", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => 4001 >= s.Id == false,                       "filter_objid=@above(4001)", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => "Volume IO _Total2".Contains(s.Name) == false, string.Empty, s => Assert.AreEqual(2, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Negation_EqualsFalse_RightToLeft()
        {
            ExecuteFilter(s => false == (s.Id == 4000), "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Negation_NotContains_EqualsFalse()
        {
            ExecuteFilter(s => !s.Name.Contains("Vol") == false, "filter_name=@sub(Vol)", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Negation_NotContains_EqualsCastedFalse()
        {
            //Construct an expression like s => !s.Contains("Vol") == (bool)false
            var parameter = Expr.Parameter(typeof(Sensor), "s");
            var member = Expr.MakeMemberAccess(parameter, typeof(Sensor).GetProperty("Name"));
            var method = typeof(string).GetMethod("Contains", new[] {typeof(string)});
            var methodCall = Expr.Call(member, method, Expr.Constant("Vol"));
            var notMethodCall = Expr.Not(methodCall);
            var boolVal = Expr.Constant(false);
            var cast = Expr.Convert(boolVal, typeof(bool));
            var equals = Expr.Equal(notMethodCall, cast);

            var lambda = Expr.Lambda<Func<Sensor, bool>>(
                equals,
                parameter
            );

            //Expression will be replaced by partial evaluator
            ExecuteFilter(lambda, "filter_name=@sub(Vol)", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Negation_NotNotContains_EqualsFalse()
        {
            ExecuteFilter(s => !!s.Name.Contains("Vol") == false, string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Negation_NotNotContains_EqualsFalse_RightToLeft()
        {
            ExecuteFilter(s => false == !!s.Name.Contains("Vol"), string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Negation_EqualsFalse_Twice_Supported()
        {
            ExecuteFilter(s => s.Name == "Volume IO _Total1" == false == false, "filter_name=Volume+IO+_Total1", s => Assert.AreEqual(1, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Negation_Not_Supported()
        {
            ExecuteFilter(s => !(s.Id == 4000),                       "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => !(s.Id.ToString().Equals("4000")),     "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => !("Volume IO _Total1" != s.Name),      "filter_name=Volume+IO+_Total1", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => !(4000 < s.Id),                        "filter_objid=@below(4000)&filter_objid=4000", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => !(4001 <= s.Id),                       "filter_objid=@below(4001)", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => !(4001 > s.Id),                        "filter_objid=@above(4001)&filter_objid=4001", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => !(4001 >= s.Id),                       "filter_objid=@above(4001)", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => !("Volume IO _Total2".Contains(s.Name)), string.Empty, s => Assert.AreEqual(2, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Negation_NotEqualsTrue_Supported()
        {
            ExecuteFilter(s => s.Id == 4000 != true,                       "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => s.Id.ToString().Equals("4000") != true,     "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => "Volume IO _Total1" != s.Name != true,      "filter_name=Volume+IO+_Total1", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => 4000 < s.Id != true,                        "filter_objid=@below(4000)&filter_objid=4000", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => 4001 <= s.Id != true,                       "filter_objid=@below(4001)", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => 4001 > s.Id != true,                        "filter_objid=@above(4001)&filter_objid=4001", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => 4001 >= s.Id != true,                       "filter_objid=@above(4001)", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => "Volume IO _Total2".Contains(s.Name) != true, string.Empty, s => Assert.AreEqual(2, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Negation_NotEqualsFalse_Supported()
        {
            ExecuteFilter(s => s.Name == "Volume IO _Total1" != false,        "filter_name=Volume+IO+_Total1", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => s.Name.Equals("Volume IO _Total1") != false,   "filter_name=Volume+IO+_Total1", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => 4000 != s.Id != false,                         "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => 4000 < s.Id != false,                          "filter_objid=@above(4000)", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => 4001 <= s.Id != false,                         "filter_objid=@above(4001)&filter_objid=4001", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => 4001 > s.Id != false,                          "filter_objid=@below(4001)", s => Assert.AreEqual(1, s.Count));
            ExecuteFilter(s => 4001 >= s.Id != false,                         "filter_objid=@below(4001)&filter_objid=4001", s => Assert.AreEqual(2, s.Count));
            ExecuteFilter(s => "Volume IO _Total2".Contains(s.Name) != false, "filter_name=@sub(Volume+IO+_Total2)", s => Assert.AreEqual(1, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Negation_NotEqualsFalse_RightToLeft()
        {
            ExecuteFilter(s => false != (s.Name == "Volume IO _Total1"), "filter_name=Volume+IO+_Total1", s => Assert.AreEqual(1, s.Count));
        }

        #endregion
        #region Nested Queries

        [UnitTest]
        [TestMethod]
        public void Query_Where_Sensors_MatchAnyDevices()
        {
            var urls = new List<string>
            {
                UnitRequest.Sensors("count=500", UrlFlag.Columns)
            };

            var filters = new[]
            {
                "filter_objid=3000",
                "filter_objid=3001",
                "filter_objid=3002"
            };

            urls.AddRange(filters.SelectMany(f => new[]
                {
                    UnitRequest.Devices($"count=500&{f}", UrlFlag.Columns)
                }
            ));

            var client = GetClient(urls.ToArray());

            var sensors = client.Item1.QuerySensors().Where(s => client.Item1.QueryDevices().Any(d => d.Id == s.Id - 1000)).ToList();

            Assert.AreEqual(3, sensors.Count);

            client.Item2.AssertFinished();
        }

        #endregion
        #region Cast

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Enum_To_TypedEnum() => ExecuteFilter(s => s.Status == (Status)Status.Up, "filter_status=3", s => Assert.AreEqual(3, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Enum_To_UntypedEnum() => ExecuteFilter(s => ((Enum)s.Status).Equals((Enum)Status.Up), "filter_status=3", s => Assert.AreEqual(3, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Int_To_Enum() => ExecuteFilter(s => s.Status == (Status)8, "filter_status=3", s => Assert.AreEqual(3, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_NullableDouble_To_Int() => ExecuteFilter(s => (int)s.LastValue == 69, "filter_lastvalue=0000000000000690.0000", s => Assert.AreEqual(3, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Int_To_Double() => ExecuteFilter(s => (double)s.Id == 4000.0, "filter_objid=4000", s => Assert.AreEqual(1, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Int_To_Illegal() => ExecuteFilter(s => (IllegalInt)s.Id == 4000, string.Empty, s => Assert.AreEqual(1, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Int_To_NullableInt() => ExecuteFilter(s => (int?) s.Id == 4000, "filter_objid=4000", s => Assert.AreEqual(1, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_NullableBool_To_Bool() => ExecuteFilter(s => (bool)s.Active == true, "filter_active=-1", s => Assert.AreEqual(3, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Object_To_Type() => ExecuteFilter(s => (bool)(object)s.Active == true, "filter_active=-1", s => Assert.AreEqual(3, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Object_To_NullableType() => ExecuteFilter(s => (int?)(object)s.Id == 4001, "filter_objid=4001", s => Assert.AreEqual(1, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Type_To_Object() => ExecuteFilter(s => ((object)s.Id).Equals((object)4001), "filter_objid=4001", s => Assert.AreEqual(1, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Type_To_Object_OperatorEquality() => ExecuteFilter(s => (object)s.Id == (object)4001, "filter_objid=4001", s => Assert.AreEqual(0, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_NullableType_To_Object() => ExecuteFilter(s => ((object) s.LastValue).Equals((object) 69.0), "filter_lastvalue=0000000000000690.0000", s => Assert.AreEqual(3, s.Count));

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_IsImplicitlyTrue()
        {
            ExecuteFilter(s => (bool)(object)s.Active, "filter_active=-1", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Condition_EqualsFalse()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == 4000)) == false, "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Condition_EqualsTrue()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == 4000)) == true, "filter_objid=4000", s => Assert.AreEqual(1, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Property_EqualsFalse()
        {
            ExecuteFilter(s => ((bool)(object)(s.Active)) == false, "filter_active=0", s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Property_EqualsTrue()
        {
            ExecuteFilter(s => ((bool)(object)(s.Active)) == true, "filter_active=-1", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Condition_ToNullableBool()
        {
            ExecuteFilter(s => ((bool?)(object)(s.Id == 4000)) == false, string.Empty, s => Assert.AreEqual(2, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Condition_Bool()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == 4000)) == false, "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Condition_EqualsBool()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == 4000)).Equals(false), "filter_objid=@neq(4000)", s => Assert.AreEqual(2, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Condition_EqualsInt()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == 4000)).Equals(3), string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Condition_ContainsString()
        {
            //Can't reduce further since Contains("False") would require the FALSE ones, which could
            //have only been calculated if all results were returned
            ExecuteFilter(s => ((bool)(object)(s.Id == 4000)).ToString().Contains("test"), string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Condition_ContainsCondition()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == 4000)).ToString().Contains((s.ParentId == 4000).ToString()), string.Empty, s => Assert.AreEqual(2, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Cast_Property_Enum()
        {
            ExecuteFilter(s => ((Enum)(object)(s.Status)).Equals((Enum)Status.Up), "filter_status=3", s => Assert.AreEqual(3, s.Count));
        }

        #endregion
        #region AsEnumerable / AsQueryable

        [UnitTest]
        [TestMethod]
        public void Query_Where_WrapProperty_Add()
        {
            ExecuteFilter(s => s.Id + 3 == 4, string.Empty, s =>
            {
                Assert.AreEqual(0, s.Count);
            });
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_AsEnumerable_AsQueryable()
        {
            ExecuteClient(
                c => c.QuerySensors().Where(s => s.Id == 4000).AsEnumerable().AsQueryable(),
                new[] { UnitRequest.Sensors("count=500&filter_objid=4000", UrlFlag.Columns) },
                s => Assert.AreEqual(1, s.Count())
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_AsEnumerable_AsQueryable_QueryableMethod()
        {
            ExecuteClient(
                c => c.QuerySensors().Where(s => s.Id == 4000).AsEnumerable().AsQueryable().Where(n => n.Name == "Volume IO _Total0"),
                new[] { UnitRequest.Sensors("count=500&filter_objid=4000&filter_name=Volume+IO+_Total0", UrlFlag.Columns) },
                s => Assert.AreEqual(1, s.Count())
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_AsEnumerable_EnumerableMethod_AsQueryable()
        {
            ExecuteClient(
                c => c.QuerySensors().Where(s => s.Id == 4000).AsEnumerable().Where(s => s.Name == "Volume IO _Total0").AsQueryable(),
                new[] { UnitRequest.Sensors("count=500&filter_objid=4000", UrlFlag.Columns) },
                s => Assert.AreEqual(1, s.Count())
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_AsEnumerable_EnumerableMethod_AsQueryable_QueryableMethod()
        {
            ExecuteClient(
                c => c.QuerySensors()
                    .Where(s => s.Id == 4000).AsEnumerable()
                    .Where(s => s.Name == "Volume IO _Total0").AsQueryable()
                    .Select(s => s.Name),
                new[] { UnitRequest.Sensors("count=500&filter_objid=4000", UrlFlag.Columns) },
                s => Assert.AreEqual(1, s.Count())
            );
        }

        #endregion
        #region Null Access

        [UnitTest]
        [TestMethod]
        public void Query_Where_NullAccess_BoolMethod_OnNullProperty()
        {
            ExecuteNullable(q => q.Where(s => s.Message.Contains("Test")), "filter_message=@sub(Test)", i => i.MessageRaw = null);
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_NullAccess_NonBoolMethod_OnNullProperty()
        {
            ExecuteNullable(q => q.Where(s => s.Message.ToString() == "Test"), string.Empty, i => i.MessageRaw = null);
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_NullAccess_BoolMethod_OnMethod_OnNullProperty()
        {
            ExecuteNullable(q => q.Where(s => s.Message.ToString().Contains("Test")), string.Empty, i => i.MessageRaw = null);
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_NullAccess_NonBoolMethod_OnMethod_OnNullProperty()
        {
            AssertEx.Throws<NullReferenceException>(() =>
            {
                ExecuteNullable(q => q.Select(s => s.Message.ToString().Substring(0, 3)), "columns=message", i => i.MessageRaw = null);
            }, "Object reference not set to an instance of an object calling method 'Substring' in expression 's.Message.ToString().Substring(0, 3)'. Consider using a ternary expression to specify conditional access.");

        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_NullAccess_NonBoolPropertyAccess_OnNullProperty()
        {
            AssertEx.Throws<NullReferenceException>(() =>
            {
                ExecuteNullable(q => q.Where(s => s.Message.Length == 3), string.Empty, i => i.MessageRaw = null);
            }, "Object reference not set to an instance of an object.");
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_NullAccess_NonBoolPropertyAccess_OnMethod_OnNullProperty()
        {
            AssertEx.Throws<NullReferenceException>(() =>
            {
                ExecuteNullable(q => q.Select(s => s.Message.Substring(0, 3).Length), "columns=message", i => i.MessageRaw = null);
            }, "Object reference not set to an instance of an object calling method 'Substring' in expression 's.Message.Substring(0, 3)'. Consider using a ternary expression to specify conditional access.");
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_NullAccess_Value_OfNullProperty()
        {
            AssertEx.Throws<InvalidOperationException>(() =>
            {
                ExecuteNullable(q => q.Where(l => l.LastValue.Value == 3), "filter_lastvalue=0000000000000030.0000", s => s.LastValueRaw = null);
            }, "Nullable object must have a value");

            AssertEx.Throws<InvalidOperationException>(() =>
            {
                ExecuteNullable(q => q.Select(l => l.LastValue.Value), "columns=lastvalue", s => s.LastValueRaw = null);
            }, "Nullable object must have a value");
        }

        #endregion
        #region Intermediate Types

        [UnitTest]
        [TestMethod]
        public void Query_Where_Intermediate_AnonymousType()
        {
            ExecuteFilter(s => new
            {
                FakeId = s.Id
            }.FakeId == 4000, "filter_objid=4000", s => Assert.AreEqual(1, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Intermediate_NewSensorType_Single()
        {
            ExecuteFilter(s => new Sensor
            {
                Message = s.Name
            }.Message == "test", "filter_name=test", s => s.ToList());
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Intermediate_NewSensor_ToWhere()
        {
            ExecuteClient(c => c.QuerySensors().Where(s => new Sensor
            {
                Message = s.Name
            }.Message == "test").Where(s => s.Id == 4000), new[] { UnitRequest.Sensors("count=500&filter_name=test&filter_objid=4000", UrlFlag.Columns) }, s => s.ToList());
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_Intermediate_NewSensor_Select_FromSource()
        {
            ExecuteClient(c => c.QuerySensors().Where(s => new Sensor
            {
                Message = s.Name
            }.Message == "test").Select(s => s.Name), new[] { UnitRequest.Sensors("columns=name&count=500&filter_name=test", null) }, s => s.ToList());
        }

        #endregion
        #region Split Requests

        [UnitTest]
        [TestMethod]
        public void Query_Where_SplitRequests_Logs_Select_SingleSet_ReduceParameters()
        {
            ExecuteClient(
                c => c.QueryLogs().Where(l => l.Id == 1001).Select(l => l.Name),
                new[] { UnitRequest.Logs("columns=objid,name&count=500&start=1&id=1001", null) },
                l => l.ToList()
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_SplitRequests_Logs_Select_MultipleSets_AllParameters()
        {
            ExecuteClient(
                c => c.QueryLogs().Where(l => l.Id == 1001 || l.Id == 1002).Select(l => l.Name),
                new[] { UnitRequest.Logs("count=500&start=1&id=1001", UrlFlag.Columns), UnitRequest.Logs("count=500&start=1&id=1002", UrlFlag.Columns) },
                l => l.ToList()
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_SplitRequests_OmitsId_WithSingleRequest()
        {
            ExecuteClient(
                c => c.QuerySensors(s => s.Name == "Ping").Select(s => s.Name),
                new[] { UnitRequest.Sensors("columns=name&count=500&filter_name=Ping", null) },
                s => s.ToList()
            );
        }

        [UnitTest]
        [TestMethod]
        public void Query_Where_SplitRequests_IncludesId_WithMultipleRequests()
        {
            ExecuteClient(
                c => c.QuerySensors(s => s.Name == "Ping" || s.Device == "dc-1").Select(s => s.Name),
                new[]
                {
                    UnitRequest.Sensors("columns=objid,name,device&count=500&filter_name=Ping", null),
                    UnitRequest.Sensors("columns=objid,name,device&count=500&filter_device=dc-1", null)
                },
                s => s.ToList()
            );
        }

        #endregion
        #region Type Constraints

        [UnitTest]
        [TestMethod]
        public void Where_TypeConstraints_SinglePredicate_ConstraintPropertiesOnly()
        {
            ExecuteFilter(s => ((IObject) s).Name.Contains("Vol"), "filter_name=@sub(Vol)", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Where_TypeConstraints_SinglePredicate_BaseType()
        {
            ExecuteFilter(s => ((SensorOrDeviceOrGroupOrProbe)s).Name.Contains("Vol"), "filter_name=@sub(Vol)", s => Assert.AreEqual(3, s.Count));
        }

        [UnitTest]
        [TestMethod]
        public void Where_TypeConstraints_TwoInterfaces()
        {
            Execute(q => q.Where(s => ((IObject) s).Name.Contains("Vol")).Select(s => ((ITableObject) s).Id), "content=sensors&columns=objid,name&count=500&filter_name=@sub(Vol)", s => Assert.AreEqual(3, s.Count));
        }

        #endregion
    }
}
