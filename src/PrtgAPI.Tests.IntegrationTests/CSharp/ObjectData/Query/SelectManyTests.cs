using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unit = PrtgAPI.Tests.UnitTests.ObjectData.Query;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData.Query
{
    [TestClass]
    public class SelectManyTests : BaseQueryTest
    {
        #region Overloads

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_Overload_TSourceTResult_SingleProperty()
        {
            var tags = client.GetSensors().SelectMany(s => s.Tags).OrderBy(t => t).ToList();

            Execute(q => q.SelectMany(s => s.Tags), s => AssertEx.AreEqualLists(tags, s.OrderBy(t => t).ToList(), "Did not get all tags"));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_Overload_TSourceIntTResult_SelectIndex()
        {
            Execute(q => q.SelectMany((s, i) => new[] { i }), s =>
            {
                Assert.AreEqual(Settings.SensorsInTestServer, s.Count);

                for (var i = 0; i < s.Count; i++)
                    Assert.AreEqual(i, s[i]);
            });
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_Overload_TSourceIntTResult_SelectIndexAndProperty()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Name).ToList();

            Execute(q => q.SelectMany((s, i) => new[] {new
            {
                Fake1 = s.Name,
                Fake2 = i
            }}), s =>
            {
                s = s.OrderBy(o => o.Fake1).ToList();

                for (var i = 0; i < s.Count; i++)
                {
                    Assert.AreEqual(sensors[i].Name, s[i].Fake1);
                    Assert.AreEqual(i, s[i].Fake2);
                }
            });
        }

        #endregion

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_ArrayOfAnonymousType_From_ArrayOfAnonymousType()
        {
            var sensors = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.SelectMany(s => new[] { new
                {
                    CustomName = s.Name,
                    Type = s.BaseType
                }}).SelectMany(a => new[] {new
                {
                    Val = a.CustomName
                }}),
                s =>
                {
                    AssertEx.AreEqualLists(s.Select(o => o.Val).OrderBy(v => v).ToList(), sensors, "Lists were not equal");
                }
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_ArrayOfAnonymousType_From_CastedArrayOfAnonymousType()
        {
            var sensors = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.SelectMany(s => new Unit.RealTypeProperty[] { new Unit.RealTypeProperty
                {
                    RealName = s.Name,
                    RealBaseType = s.BaseType
                }}).SelectMany(a => new Unit.RealTypeProperty[] {new Unit.RealTypeProperty
                {
                    RealName = a.RealName
                }}),
                s => AssertEx.AreEqualLists(s.Select(o => o.RealName).OrderBy(v => v).ToList(), sensors, "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_ArrayOfAnonymousType_From_ArrayOfAnonymousType_WithSubMember()
        {
            Execute(q => q.SelectMany(s => new[] { new
                {
                    CustomTypes = s.NotificationTypes,
                    Type = s.BaseType
                }}).SelectMany(a => new[] {new
                {
                    Val = a.CustomTypes.ChangeTriggers
                }}),
                s =>
                {
                    Assert.AreEqual(Settings.SensorsInTestServer, s.Count);

                    Assert.IsTrue(s.All(o => o.Val == 0));
                }
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_ListOfRealType_From_ListOfRealType()
        {
            var sensors = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.SelectMany(s => new List<Unit.RealTypeProperty> { new Unit.RealTypeProperty
                {
                    RealName = s.Name,
                    RealBaseType = s.BaseType
                }}).SelectMany(a => new List<Unit.RealTypeProperty> {new Unit.RealTypeProperty
                {
                    RealName = a.RealName,
                    RealBaseType = BaseType.Device
                }}),
                s => AssertEx.AreEqualLists(s.Select(o => o.RealName).OrderBy(v => v).ToList(), sensors, "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_ListOfRealType_From_CastedListOfRealType()
        {
            var sensors = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.SelectMany(s => (IEnumerable<object>)new List<Unit.RealTypeProperty> { new Unit.RealTypeProperty
                {
                    RealName = s.Name,
                    RealBaseType = s.BaseType
                }}).SelectMany(a => new List<Unit.RealTypeProperty> {new Unit.RealTypeProperty
                {
                    RealName = ((Unit.RealTypeProperty)a).RealName,
                    RealBaseType = BaseType.Device
                }}),
                s => AssertEx.AreEqualLists(s.Select(o => o.RealName).OrderBy(v => v).ToList(), sensors, "Lists were not equal"));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_ArrayOfAnonymousType_From_ArrayOfAnonymousType_WithMultipleArrayElements()
        {
            var sensors = client.GetSensors().Select(s => s.Name).OrderBy(n => n).SelectMany(v => Enumerable.Repeat(v, 2)).ToList();

            Execute(q => q.SelectMany(s => new[] {
                    new
                    {
                        CustomName = s.Name,
                        Type = s.BaseType
                    },
                    new
                    {
                        CustomName = s.Name,
                        Type = s.BaseType
                    }
                }).SelectMany(a => new[] {new
                {
                    Val = a.CustomName
                }}),
                s => AssertEx.AreEqualLists(s.Select(o => o.Val).OrderBy(v => v).ToList(), sensors, "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_ArrayOfAnonymousType_From_ArrayOfRealType_WithRandom()
        {
            var sensors = client.GetSensors().Select(s => s.Name).OrderBy(n => n).SelectMany(v => new[] {v, "hello"}).ToList();

            Execute(q => q.SelectMany(s => new object[] {
                    new Unit.RealTypeProperty
                    {
                        RealName = s.Name,
                        RealBaseType = s.BaseType
                    },
                    "hello"
                }).SelectMany(a => new[] {new
                {
                    Val = a is Unit.RealTypeProperty ? ((Unit.RealTypeProperty)a).RealName : a
                }}),
                s => AssertEx.AreEqualLists(s.Select(o => o.Val).Cast<string>().ToList(), sensors, "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_WithResultSelector_ToSelect()
        {
            Execute(q => q.SelectMany(s => s.Tags, (s, c) => s.Id).Select(s => s), s => s.ToList());
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_WithResultSelector_UsingBothArgs_ToSelect()
        {
            var tags = client.GetSensors().SelectMany(s => s.Tags).OrderBy(t => t).ToList();

            Execute(q => q.SelectMany(s => s.Tags, (s, c) => new
            {
                First = s.Id,
                Second = c
            }).Select(s => s.Second), s => AssertEx.AreEqualLists(tags, s.OrderBy(t => t).ToList(), "Did not get all tags"));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_SelectMany_WithResultSelector_UsingBothArgs_ToWhere()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            Execute(q => q.SelectMany(s => s.Tags, (s, c) => new
            {
                First = s,
                Second = c
            }).Where(s => s.First.Id == Settings.UpSensor), s =>
            {
                AssertEx.AreEqualLists(upSensor.Tags.ToList(), s.Select(v => v.Second).ToList(), "Tags were not equal");
                Assert.IsTrue(s.All(v => v.First.Id == upSensor.Id));
            });
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SelectManyTests_HasAllTests()
        {
            HasAllTests(typeof(Unit.SelectManyTests));
        }
    }
}
