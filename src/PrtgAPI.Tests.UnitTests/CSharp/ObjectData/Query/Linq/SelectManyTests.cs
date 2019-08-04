using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class SelectManyTests : BaseQueryTests
    {
        #region Overloads

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_Overload_TSourceTResult_SingleProperty()
        {
            Execute(q => q.SelectMany(s => s.Tags), "content=sensors&columns=tags&count=500", s =>
            {
                Assert.AreEqual(6, s.Count);
                Assert.AreEqual("wmilogicalsensor", s[0]);
                Assert.AreEqual("C_OS_Win", s[1]);
                Assert.AreEqual("wmilogicalsensor", s[2]);
                Assert.AreEqual("C_OS_Win", s[3]);
                Assert.AreEqual("wmilogicalsensor", s[4]);
                Assert.AreEqual("C_OS_Win", s[5]);
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_Overload_TSourceIntTResult_SelectIndex()
        {
            Execute(q => q.SelectMany((s, i) => new[] { i }), $"content=sensors&columns={UnitRequest.DefaultSensorProperties()}&count=500", s =>
            {
                Assert.AreEqual(0, s[0]);
                Assert.AreEqual(1, s[1]);
                Assert.AreEqual(2, s[2]);
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_Overload_TSourceIntTResult_SelectIndexAndProperty()
        {
            Execute(q => q.SelectMany((s, i) => new[] {new
            {
                Fake1 = s.Name,
                Fake2 = i
            }}), "content=sensors&columns=name&count=500", s =>
            {
                Assert.AreEqual("Volume IO _Total0", s[0].Fake1);
                Assert.AreEqual("Volume IO _Total1", s[1].Fake1);
                Assert.AreEqual("Volume IO _Total2", s[2].Fake1);
                Assert.AreEqual(0, s[0].Fake2);
                Assert.AreEqual(1, s[1].Fake2);
                Assert.AreEqual(2, s[2].Fake2);
            });
        }

        #endregion

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_ArrayOfAnonymousType_From_ArrayOfAnonymousType()
        {
            Execute(q => q.SelectMany(s => new[] { new
                {
                    CustomName = s.Name,
                    Type = s.BaseType
                }}).SelectMany(a => new[] {new
                {
                    Val = a.CustomName
                }}),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].Val);
                    Assert.AreEqual("Volume IO _Total1", s[1].Val);
                    Assert.AreEqual("Volume IO _Total2", s[2].Val);
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_ArrayOfAnonymousType_From_CastedArrayOfAnonymousType()
        {
            Execute(q => q.SelectMany(s => new RealTypeProperty[] { new RealTypeProperty
                {
                    RealName = s.Name,
                    RealBaseType = s.BaseType
                }}).SelectMany(a => new RealTypeProperty[] {new RealTypeProperty
                {
                    RealName = a.RealName
                }}),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].RealName);
                    Assert.AreEqual("Volume IO _Total1", s[1].RealName);
                    Assert.AreEqual("Volume IO _Total2", s[2].RealName);
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_ArrayOfAnonymousType_From_ArrayOfAnonymousType_WithSubMember()
        {
            Execute(q => q.SelectMany(s => new[] { new
                {
                    CustomTypes = s.NotificationTypes,
                    Type = s.BaseType
                }}).SelectMany(a => new[] {new
                {
                    Val = a.CustomTypes.ChangeTriggers
                }}),
                "content=sensors&columns=notifiesx,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual(0, s[0].Val);
                    Assert.AreEqual(0, s[1].Val);
                    Assert.AreEqual(0, s[2].Val);
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_ListOfRealType_From_ListOfRealType()
        {
            Execute(q => q.SelectMany(s => new List<RealTypeProperty> { new RealTypeProperty
                {
                    RealName = s.Name,
                    RealBaseType = s.BaseType
                }}).SelectMany(a => new List<RealTypeProperty> {new RealTypeProperty
                {
                    RealName = a.RealName,
                    RealBaseType = BaseType.Device
                }}),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].RealName);
                    Assert.AreEqual("Volume IO _Total1", s[1].RealName);
                    Assert.AreEqual("Volume IO _Total2", s[2].RealName);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_ListOfRealType_From_CastedListOfRealType()
        {
            Execute(q => q.SelectMany(s => (IEnumerable<object>)new List<RealTypeProperty> { new RealTypeProperty
                {
                    RealName = s.Name,
                    RealBaseType = s.BaseType
                }}).SelectMany(a => new List<RealTypeProperty> {new RealTypeProperty
                {
                    RealName = ((RealTypeProperty)a).RealName,
                    RealBaseType = BaseType.Device
                }}),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].RealName);
                    Assert.AreEqual("Volume IO _Total1", s[1].RealName);
                    Assert.AreEqual("Volume IO _Total2", s[2].RealName);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_ArrayOfAnonymousType_From_ArrayOfAnonymousType_WithMultipleArrayElements()
        {
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
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(6, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].Val);
                    Assert.AreEqual("Volume IO _Total0", s[1].Val);
                    Assert.AreEqual("Volume IO _Total1", s[2].Val);
                    Assert.AreEqual("Volume IO _Total1", s[3].Val);
                    Assert.AreEqual("Volume IO _Total2", s[4].Val);
                    Assert.AreEqual("Volume IO _Total2", s[5].Val);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_ArrayOfAnonymousType_From_ArrayOfRealType_WithRandom()
        {
            Execute(q => q.SelectMany(s => new object[] {
                    new RealTypeProperty
                    {
                        RealName = s.Name,
                        RealBaseType = s.BaseType
                    },
                    "hello"
                }).SelectMany(a => new[] {new
                {
                    Val = a is RealTypeProperty ? ((RealTypeProperty)a).RealName : a
                }}),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(6, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].Val);
                    Assert.AreEqual("hello", s[1].Val);
                    Assert.AreEqual("Volume IO _Total1", s[2].Val);
                    Assert.AreEqual("hello", s[3].Val);
                    Assert.AreEqual("Volume IO _Total2", s[4].Val);
                    Assert.AreEqual("hello", s[5].Val);

                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_WithResultSelector_ToSelect()
        {
            Execute(q => q.SelectMany(s => s.Tags, (s, c) => s.Id).Select(s => s), "content=sensors&columns=objid,tags&count=500", s => s.ToList());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_WithResultSelector_UsingBothArgs_ToSelect()
        {
            Execute(q => q.SelectMany(s => s.Tags, (s, c) => new
            {
                First = s.Id,
                Second = c
            }).Select(s => s.Second), "content=sensors&columns=objid,tags&count=500", s =>
            {
                Assert.AreEqual(6, s.Count);
                Assert.AreEqual("wmilogicalsensor", s[0]);
                Assert.AreEqual("C_OS_Win", s[1]);
                Assert.AreEqual("wmilogicalsensor", s[2]);
                Assert.AreEqual("C_OS_Win", s[3]);
                Assert.AreEqual("wmilogicalsensor", s[4]);
                Assert.AreEqual("C_OS_Win", s[5]);
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_SelectMany_WithResultSelector_UsingBothArgs_ToWhere()
        {
            Execute(q => q.SelectMany(s => s.Tags, (s, c) => new
            {
                First = s,
                Second = c
            }).Where(s => s.First.Id == 4000), "content=sensors&columns=objid,tags&count=500&filter_objid=4000", s =>
            {
                Assert.AreEqual(2, s.Count);
                Assert.AreEqual("wmilogicalsensor", s[0].Second);
                Assert.AreEqual("C_OS_Win", s[1].Second);
                Assert.AreEqual(4000, s[0].First.Id);
                Assert.AreEqual(4000, s[1].First.Id);
            });
        }
    }
}
