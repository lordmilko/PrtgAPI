using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class SelectTests : BaseQueryTests
    {
        #region Overload

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_Overload_TSourceTResult_SingleProperty()
        {
            Execute(q => q.Select(s => s.Name), "content=sensors&columns=name&count=500", s =>
            {
                Assert.AreEqual("Volume IO _Total0", s[0]);
                Assert.AreEqual("Volume IO _Total1", s[1]);
                Assert.AreEqual("Volume IO _Total2", s[2]);
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_Overload_TSourceIntTResult_SelectIndex()
        {
            Execute(q => q.Select((s, i) => i), $"content=sensors&columns={UnitRequest.DefaultSensorProperties()}&count=500", s =>
            {
                Assert.AreEqual(0, s[0]);
                Assert.AreEqual(1, s[1]);
                Assert.AreEqual(2, s[2]);
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_Overload_TSourceIntTResult_SelectIndexAndProperty()
        {
            Execute(q => q.Select((s, i) => new
            {
                Fake1 = s.Name,
                Fake2 = i
            }), "content=sensors&columns=name&count=500", s =>
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
        #region Illegal Predecessors

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_FromUnsupported()
        {
            Execute(q => q.SkipWhile((s, i) => i == 0).Select(s => s.Name), $"content=sensors&columns={UnitRequest.DefaultSensorProperties()}&count=500", s =>
            {
                Assert.AreEqual("Volume IO _Total1", s[0]);
                Assert.AreEqual("Volume IO _Total2", s[1]);
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_FromUnsupported_FromSupported()
        {
            Execute(q => q.Where(s => s.Active).SkipWhile((s, i) => i == 0).Select(s => s.Name), $"content=sensors&columns={UnitRequest.DefaultSensorProperties()}&count=500&filter_active=-1", s =>
            {
                Assert.AreEqual("Volume IO _Total1", s[0]);
                Assert.AreEqual("Volume IO _Total2", s[1]);
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_ToUnsupported()
        {
            Execute(q => q.Select(s => s.Name).SkipWhile((s, i) => i == 0), "content=sensors&columns=name&count=500", s =>
            {
                Assert.AreEqual("Volume IO _Total1", s[0]);
                Assert.AreEqual("Volume IO _Total2", s[1]);
            });
        }

        #endregion
        #region Anonymous Type

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousType()
        {
            Execute(q => q.Select(s => new
                {
                    CustomName = s.Name,
                    Type = s.BaseType
                }),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual("Volume IO _Total0", s[0].CustomName);
                    Assert.AreEqual(BaseType.Sensor, s[0].Type);
                    Assert.AreEqual("Volume IO _Total1", s[1].CustomName);
                    Assert.AreEqual(BaseType.Sensor, s[1].Type);
                    Assert.AreEqual("Volume IO _Total2", s[2].CustomName);
                    Assert.AreEqual(BaseType.Sensor, s[2].Type);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousType_Where()
        {
            Execute(q => q.Select(s => new
                {
                    CustomName = s.Name,
                    Type = s.BaseType
                }).Where(s1 => s1.CustomName == "Volume IO _Total1"),
                "content=sensors&columns=name,basetype&count=500&filter_name=Volume+IO+_Total1",
                s =>
                {
                    Assert.AreEqual(1, s.Count);
                    Assert.AreEqual("Volume IO _Total1", s.Single().CustomName);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousType_WithPropertyMember_ToWhere()
        {
            Execute(q => q.Select(s => new
                {
                    CustomName = s.Name,
                    Intermediate = new
                    {
                        First = s.NotificationTypes.ChangeTriggers,
                        Second = s.Active
                    }
                }).Where(a => a.Intermediate.First == 0),
                "content=sensors&columns=name,notifiesx,active&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual(0, s[0].Intermediate.First);
                    Assert.AreEqual(0, s[1].Intermediate.First);
                    Assert.AreEqual(0, s[2].Intermediate.First);
                }
            );
        }

        #endregion
        #region Type Constraints

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Select_TypeConstraints_CastType_ToSelect()
        {
            Execute(q => q.Select(s => (ITableObject)s).Select(v => v.Tags), "content=sensors&columns=tags&count=500", s => Assert.AreEqual(3, s.Count));
        }

        #endregion

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_PropertyMember_ToWhere()
        {
            Execute(q => q.Select(s => s.NotificationTypes.ChangeTriggers).Where(a => a == 0),
                "content=sensors&columns=notifiesx&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual(0, s[0]);
                    Assert.AreEqual(0, s[1]);
                    Assert.AreEqual(0, s[2]);
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousLiteral_ToWhere()
        {
            Execute(q => q.Select(s => new
            {
                First = 3
            }).Where(f => f.First == 3), $"content=sensors&columns={UnitRequest.DefaultSensorProperties()}&count=500", s => Assert.IsTrue(s.All(e => e.First == 3)));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_Anonymous_Where_ToAnonymous()
        {
            Execute(q => q.Select(s => new
            {
                First = s.Id
            }).Where(a => a.First == 3000).Select(a => a.First), "content=sensors&columns=objid&count=500&filter_objid=3000", s => s.ToList());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnotherQuery()
        {
            var urls = new[]
            {
                UnitRequest.Sensors("count=500", UrlFlag.Columns),

                UnitRequest.Devices("count=500", UrlFlag.Columns),
                UnitRequest.Devices("count=500", UrlFlag.Columns),
                UnitRequest.Devices("count=500", UrlFlag.Columns)
            };

            ExecuteClient(
                c => c.QuerySensors().Select(s => c.QueryDevices()).SelectMany(s => s).ToList(),
                urls,
                d => Assert.AreEqual(12, d.Count)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnotherQuery_InAnAnonymousType()
        {
            var urls = new[]
            {
                UnitRequest.Sensors("columns=name&count=500", null),

                UnitRequest.Devices("count=500&filter_active=-1", UrlFlag.Columns),
                UnitRequest.Devices("count=500&filter_active=-1", UrlFlag.Columns),
                UnitRequest.Devices("count=500&filter_active=-1", UrlFlag.Columns),
            };

            ExecuteClient(
                c => c.QuerySensors().Select(s => new
                    {
                        First = s.Name,
                        Devices = c.QueryDevices().Any(d => d.Active)
                    }).ToList(),
                urls,
                d => Assert.AreEqual(3, d.Count)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnotherSelectQuery()
        {
            var urls = new[]
            {
                UnitRequest.Sensors("count=500", UrlFlag.Columns),

                UnitRequest.Devices("columns=favorite&count=500", null),
                UnitRequest.Devices("columns=favorite&count=500", null),
                UnitRequest.Devices("columns=favorite&count=500", null)
            };

            ExecuteClient(
                c => c.QuerySensors().Select(s => c.QueryDevices().Select(d => d.Favorite)).SelectMany(s => s).ToList(),
                urls,
                d => Assert.AreEqual(12, d.Count)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousType_From_AnonymousType()
        {
            Execute(q => q.Select(s => new
            {
                CustomName = s.Name,
                Type = s.BaseType
            }).Select(a => new
            {
                Val = a.CustomName
            }),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].Val);
                    Assert.AreEqual("Volume IO _Total1", s[1].Val);
                    Assert.AreEqual("Volume IO _Total2", s[2].Val);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousType_FromRealTypeViaProperty()
        {
            Execute(q => q.Select(s =>
                    new RealTypeProperty
                    {
                        RealName = s.Name,
                        RealBaseType = s.BaseType
                    }
                ).Select(a =>new
                {
                    Val = a.RealName
                }),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].Val);
                    Assert.AreEqual("Volume IO _Total1", s[1].Val);
                    Assert.AreEqual("Volume IO _Total2", s[2].Val);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousType_FromRealTypeViaConstructor()
        {
            Execute(q => q.Select(s => new RealTypeConstructor(s.Name, s.BaseType)
                ).Select(a => new
                {
                    Val = a.RealName
                }),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].Val);
                    Assert.AreEqual("Volume IO _Total1", s[1].Val);
                    Assert.AreEqual("Volume IO _Total2", s[2].Val);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousType_FromRealTypeViaConstructorGetOnly()
        {
            Execute(q => q.Select(s =>
                    new RealTypeConstructorGetOnly(s.Name, s.BaseType)
                ).Select(a => new
                {
                    Val = a.RealName
                }),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].Val);
                    Assert.AreEqual("Volume IO _Total1", s[1].Val);
                    Assert.AreEqual("Volume IO _Total2", s[2].Val);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousType_FromRealTypeViaConstructorAndProperty()
        {
            Execute(q => q.Select(s =>
                    new RealTypeHybrid(s.Name, s.BaseType)
                    {
                        RealActive = s.Active
                    }
                ).Select(a => new
                {
                    Val = a.RealName
                }),
                "content=sensors&columns=name,basetype,active&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].Val);
                    Assert.AreEqual("Volume IO _Total1", s[1].Val);
                    Assert.AreEqual("Volume IO _Total2", s[2].Val);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_ArrayOfAnonymousType_From_ArrayOfAnonymousType()
        {
            Execute(q => q.Select(s => new[] {new
                {
                    CustomName = s.Name,
                    Type = s.BaseType
                }}).Select(a => new[] { new
                {
                    Val = a[0].CustomName
                }}),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0][0].Val);
                    Assert.AreEqual("Volume IO _Total1", s[1][0].Val);
                    Assert.AreEqual("Volume IO _Total2", s[2][0].Val);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_ListOfAnonymousType_From_ListOfAnonymousType()
        {
            Execute(q => q.Select(s => new[] {new
                {
                    CustomName = s.Name,
                    Type = s.BaseType
                }}.ToList()).Select(a => new[] { new
                {
                    Val = a[0].CustomName
                }}.ToList()),
                "content=sensors&columns=name,basetype&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0][0].Val);
                    Assert.AreEqual("Volume IO _Total1", s[1][0].Val);
                    Assert.AreEqual("Volume IO _Total2", s[2][0].Val);
                });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousType_From_AnonymousType_WithLiteral()
        {
            Execute(q => q.Select(s => new
                {
                    CustomName = s.Name,
                    Type = 3
                }).Select(a => new
                {
                    Val1 = a.CustomName,
                    Val2 = a.Type
                }),
                "content=sensors&columns=name&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual("Volume IO _Total0", s[0].Val1);
                    Assert.AreEqual("Volume IO _Total1", s[1].Val1);
                    Assert.AreEqual("Volume IO _Total2", s[2].Val1);
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousType_From_AnonymousTypeWithTwoLevels()
        {
            Execute(q => q.Select(s => new
            {
                CustomName = s.Name,
                Intermediate = new
                {
                    First = s.Id,
                    Second = s.Active
                }
            }).Select(a => new
            {
                Val = a.Intermediate.First
            }),
                "content=sensors&columns=objid,name,active&count=500",
                s =>
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual(4000, s[0].Val);
                    Assert.AreEqual(4001, s[1].Val);
                    Assert.AreEqual(4002, s[2].Val);
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_AnonymousType_From_AnonymousType_IncludingGetOnlyProperty()
        {
            Execute(q => q.Select(s => new
            {
                CustomName = s.Name,
                Intermediate = new
                {
                    First = s.NotificationTypes,
                    Second = s.Active
                }
            }).Select(a => new
            {
                Val = a.Intermediate.First
            }),
                "content=sensors&columns=name,notifiesx,active&count=500",
                s =>
                {
                    var str = "Inheritance: True, State: 0, Threshold: 0, Change: 0, Speed: 0, Volume: 0";

                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual(str, s[0].Val.ToString());
                    Assert.AreEqual(str, s[1].Val.ToString());
                    Assert.AreEqual(str, s[2].Val.ToString());
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_IncludeAllPropertyExpressions()
        {
            Execute(
                q => q.Where(s => s.Name == "Volume IO _Total0").Select(s => s.Device),
                "content=sensors&columns=name,device&count=500&filter_name=Volume+IO+_Total0",
                s => Assert.AreEqual("dc1", s.Single())
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_ToWhere()
        {
            Execute(q => q.Select(s => s.Name).Where(n => n.Contains("Vol")), "content=sensors&columns=name&count=500&filter_name=@sub(Vol)", s => Assert.AreEqual(3, s.Count));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_SubMemberCondition()
        {
            Execute(q => q.Select(s => s.LastUp.Value.Day == 30), "content=sensors&columns=lastup&count=500", s => Assert.IsTrue(s.All(v => v == false)));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_NewSensor_FromSource()
        {
            Execute(q => q.Select(s => new Sensor {Message = s.Name}), "content=sensors&columns=name&count=500", s =>
            {
                Assert.AreEqual(3, s.Count);
                Assert.IsTrue(s.All(v => v.Name == null));
                Assert.AreEqual("Volume IO _Total0", s[0].Message);
                Assert.AreEqual("Volume IO _Total1", s[1].Message);
                Assert.AreEqual("Volume IO _Total2", s[2].Message);
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_NewSensorProperty_FromSource()
        {
            Execute(q => q.Select(s => new Sensor { Message = s.Name }.Message), "content=sensors&columns=name&count=500", s =>
            {
                Assert.AreEqual(3, s.Count);
                Assert.AreEqual("Volume IO _Total0", s[0]);
                Assert.AreEqual("Volume IO _Total1", s[1]);
                Assert.AreEqual("Volume IO _Total2", s[2]);
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_Anonymous_FromNewSensor_FromAnonymous_FromSource()
        {
            Execute(q => q.Select(s => new
            {
                Fake1 = s.Id,
                Fake2 = s.Name
            }).Select(a => new Sensor
            {
                Message = a.Fake2
            }).Select(s => new
            {
                Fake3 = s.ParentId,
                Fake4 = s.Message
            }), "content=sensors&columns=objid,name&count=500", s => s.ToList());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_NewSensor_Where_FromSource()
        {
            ExecuteClient(c => c.QuerySensors().Select(s => new Sensor
            {
                Id = s.ParentId,
                Message = s.Name
            }.Message).Where(v => v == "test"), new[] { UnitRequest.Sensors("columns=name&count=500&filter_name=test", null) }, s => s.ToList());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_NewSensor_WhereInternalProperty_FromSource()
        {
            ExecuteClient(c => c.QuerySensors().Select(s => new Sensor
            {
                Id = s.ParentId,
                Message = s.Name
            }.Message).Where(v => v == "test"), new[] { UnitRequest.Sensors("columns=name&count=500&filter_name=test", null) }, s => s.ToList());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_NewSensor_Where_FromNothing()
        {
            ExecuteClient(c => c.QuerySensors().Select(s => new Sensor
            {
                Id = s.ParentId,
                Message = "hello"
            }.Message).Where(v => v == "test"), new[] { UnitRequest.Sensors("count=500", UrlFlag.Columns) }, s => s.ToList());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Query_Select_NewSensor_Where_FromOperation()
        {
            ExecuteClient(c => c.QuerySensors().Select(s => new Sensor
            {
                Id = s.ParentId,
                Message = s.Name + s.Type
            }.Message).Where(v => v == "test"), new[] { UnitRequest.Sensors("columns=name,type&count=500", null) }, s => s.ToList());
        }
    }
}
