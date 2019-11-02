using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unit = PrtgAPI.Tests.UnitTests.ObjectData.Query;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData.Query
{
    [TestClass]
    public class SelectTests : BaseQueryTest
    {
        #region Overload

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_Overload_TSourceTResult_SingleProperty()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.Select(s => s.Name), s => AssertEx.AreEqualLists(names, s, "Lists were not equal"));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_Overload_TSourceIntTResult_SelectIndex()
        {
            Execute(q => q.Select((s, i) => i), s =>
            {
                for (var i = 0; i < Settings.SensorsInTestServer; i++)
                    Assert.AreEqual(i, s[i]);
            });
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_Overload_TSourceIntTResult_SelectIndexAndProperty()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.Select((s, i) => new
            {
                Fake1 = s.Name,
                Fake2 = i
            }), s =>
            {
                for(var i = 0; i < s.Count; i++)
                {
                    Assert.AreEqual(names[i], s[i].Fake1);
                    Assert.AreEqual(i, s[i].Fake2);
                }
            });
        }

        #endregion
        #region Illegal Predecessors

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_FromUnsupported()
        {
            var names = client.GetSensors().Select(s => s.Name).Skip(1).ToList();

            Execute(
                q => q.SkipWhile((s, i) => i == 0).Select(s => s.Name),
                s => AssertEx.AreEqualLists(s, names, "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_FromUnsupported_FromSupported()
        {
            var names = client.GetSensors().Where(s => s.Active).Select(s => s.Name).Skip(1).ToList();

            Execute(
                q => q.Where(s => s.Active).SkipWhile((s, i) => i == 0).Select(s => s.Name),
                s => AssertEx.AreEqualLists(s, names, "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_ToUnsupported()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).Skip(1).ToList();

            Execute(
                q => q.Select(s => s.Name).SkipWhile((s, i) => i == 0),
                s => AssertEx.AreEqualLists(s, names, "Lists were not equal")
            );
        }

        #endregion
        #region Anonymous Type

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousType()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(
                q => q.Select(s => new
                {
                    CustomName = s.Name,
                    Type = s.BaseType
                }),
                s =>
                {
                    AssertEx.AreEqualLists(names, s.Select(v => v.CustomName).ToList(), "List of names were not equal");
                    Assert.IsTrue(s.All(v => v.Type == BaseType.Sensor));
                }
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousType_Where()
        {
            var sensor = client.GetSensor(Settings.UpSensor);

            Execute(q => q.Select(s => new
            {
                CustomName = s.Name,
                Type = s.BaseType
            }).Where(s1 => s1.CustomName == sensor.Name),
            s =>
            {
                Assert.AreEqual(1, s.Count);
                Assert.AreEqual(sensor.Name, s.Single().CustomName);
            });
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousType_WithPropertyMember_ToWhere()
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
            s =>
            {
                Assert.AreEqual(Settings.SensorsInTestServer, s.Count);
                Assert.IsTrue(s.All(v => v.Intermediate.First == 0));
            });
        }

        #endregion
        #region Type Constraints

        [TestMethod]
        [IntegrationTest]
        public void Data_Select_TypeConstraints_CastType_ToSelect()
        {
            Execute(q => q.Select(s => (ITableObject)s).Select(v => v.Tags), s => Assert.AreEqual(Settings.SensorsInTestServer, s.Count));
        }

        #endregion

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_PropertyMember_ToWhere()
        {
            Execute(q => q.Select(s => s.NotificationTypes.ChangeTriggers).Where(a => a == 0),
            s =>
            {
                Assert.AreEqual(Settings.SensorsInTestServer, s.Count);
                Assert.IsTrue(s.All(v => v == 0));
            }
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousLiteral_ToWhere()
        {
            Execute(
                q => q.Select(s => new
                {
                    First = 3
                }).Where(f => f.First == 3),
                s =>
                {
                    Assert.AreEqual(Settings.SensorsInTestServer, s.Count);
                    Assert.IsTrue(s.All(e => e.First == 3));
                }
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_Anonymous_Where_ToAnonymous()
        {
            Execute(q => q.Select(s => new
            {
                First = s.Id
            }).Where(a => a.First == Settings.UpSensor).Select(a => a.First), s => Assert.AreEqual(Settings.UpSensor, s.Single()));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnotherQuery()
        {
            ExecuteClient(
                c => c.QuerySensors().Select(s => c.QueryDevices()).SelectMany(s => s).ToList(),
                d => Assert.AreEqual(Settings.DevicesInTestServer * Settings.SensorsInTestServer, d.Count)
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnotherQuery_InAnAnonymousType()
        {
            ExecuteClient(
                c => c.QuerySensors().Select(s => new
                {
                    First = s.Name,
                    Devices = c.QueryDevices().Any(d => d.Active)
                }).ToList(),
                d => Assert.AreEqual(Settings.SensorsInTestServer, d.Count)
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnotherSelectQuery()
        {
            ExecuteClient(
                c => c.QuerySensors().Select(s => c.QueryDevices().Select(d => d.Favorite)).SelectMany(s => s).ToList(),
                d => Assert.AreEqual(Settings.DevicesInTestServer * Settings.SensorsInTestServer, d.Count)
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousType_From_AnonymousType()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.Select(s => new
            {
                CustomName = s.Name,
                Type = s.BaseType
            }).Select(a => new
            {
                Val = a.CustomName
            }),
            s => AssertEx.AreEqualLists(names, s.Select(v => v.Val).ToList(), "Lists were not equal"));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousType_FromRealTypeViaProperty()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.Select(s =>
                    new Unit.RealTypeProperty
                    {
                        RealName = s.Name,
                        RealBaseType = s.BaseType
                    }
                ).Select(a => new
                {
                    Val = a.RealName
                }),
                s => AssertEx.AreEqualLists(names, s.Select(v => v.Val).ToList(), "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousType_FromRealTypeViaConstructor()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.Select(s => new Unit.RealTypeConstructor(s.Name, s.BaseType)
                ).Select(a => new
                {
                    Val = a.RealName
                }),
                s => AssertEx.AreEqualLists(names, s.Select(v => v.Val).ToList(), "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousType_FromRealTypeViaConstructorGetOnly()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.Select(s =>
                    new Unit.RealTypeConstructorGetOnly(s.Name, s.BaseType)
                ).Select(a => new
                {
                    Val = a.RealName
                }),
                s => AssertEx.AreEqualLists(names, s.Select(v => v.Val).ToList(), "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousType_FromRealTypeViaConstructorAndProperty()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.Select(s =>
                    new Unit.RealTypeHybrid(s.Name, s.BaseType)
                    {
                        RealActive = s.Active
                    }
                ).Select(a => new
                {
                    Val = a.RealName
                }),
                s => AssertEx.AreEqualLists(names, s.Select(v => v.Val).ToList(), "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_ArrayOfAnonymousType_From_ArrayOfAnonymousType()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.Select(s => new[] {new
                {
                    CustomName = s.Name,
                    Type = s.BaseType
                }}).Select(a => new[] { new
                {
                    Val = a[0].CustomName
                }}),
                s => AssertEx.AreEqualLists(names, s.SelectMany(v => v.Select(v1 => v1.Val)).ToList(), "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_ListOfAnonymousType_From_ListOfAnonymousType()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.Select(s => new[] {new
                {
                    CustomName = s.Name,
                    Type = s.BaseType
                }}.ToList()).Select(a => new[] { new
                {
                    Val = a[0].CustomName
                }}.ToList()),
                s => AssertEx.AreEqualLists(names, s.SelectMany(v => v.Select(v1 => v1.Val)).ToList(), "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousType_From_AnonymousType_WithLiteral()
        {
            var names = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(
                q => q.Select(s => new
                {
                    CustomName = s.Name,
                    Type = 3
                }).Select(a => new
                {
                    Val1 = a.CustomName,
                    Val2 = a.Type
                }),
                s => AssertEx.AreEqualLists(names, s.Select(v => v.Val1).ToList(), "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousType_From_AnonymousTypeWithTwoLevels()
        {
            var ids = client.GetSensors().Select(s => s.Id).ToList();

            Execute(
                q => q.Select(s => new
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
                s => AssertEx.AreEqualLists(ids, s.Select(v => v.Val).ToList(), "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_AnonymousType_From_AnonymousType_IncludingGetOnlyProperty()
        {
            var triggers = client.GetSensors().Select(s => s.NotificationTypes.ToString()).OrderBy(v => v).ToList();

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
            s =>
            {
                var list = s.Select(v => v.Val.ToString()).OrderBy(o => o).ToList();

                AssertEx.AreEqualLists(list, triggers, "Lists were not equal");
            });
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_IncludeAllPropertyExpressions()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            Execute(
                q => q.Where(s => s.Name == upSensor.Name).Select(s => s.Device),
                s => Assert.AreEqual(upSensor.Device, s.Single())
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_ToWhere()
        {
            var count = client.GetSensors().Count(s => s.Name.Contains("Pi"));

            Execute(q => q.Select(s => s.Name).Where(n => n.Contains("Pi")), s => Assert.AreEqual(count, s.Count));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_SubMemberCondition()
        {
            Execute(q => q.Select(s => s.LastUp != null && s.LastUp.Value.Day == 32), s => Assert.IsTrue(s.All(v => v == false)));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_NewSensor_FromSource()
        {
            var sensors = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(q => q.Select(s => new Sensor { Message = s.Name }), s =>
            {
                Assert.AreEqual(Settings.SensorsInTestServer, s.Count);
                Assert.IsTrue(s.All(v => v.Name == null));
                AssertEx.AreEqualLists(sensors, s.Select(v => v.Message).ToList(), "Lists were not equal");
            });
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_NewSensorProperty_FromSource()
        {
            var sensors = client.GetSensors().Select(s => s.Name).OrderBy(n => n).ToList();

            Execute(
                q => q.Select(s => new Sensor { Message = s.Name }.Message),
                s => AssertEx.AreEqualLists(sensors, s, "Lists were not equal")
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_Anonymous_FromNewSensor_FromAnonymous_FromSource()
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
            }), s => s.ToList());
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_NewSensor_Where_FromSource()
        {
            ExecuteClient(c => c.QuerySensors().Select(s => new Sensor
            {
                Id = s.ParentId,
                Message = s.Name
            }.Message).Where(v => v == "test"), s => s.ToList());
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_NewSensor_WhereInternalProperty_FromSource()
        {
            ExecuteClient(c => c.QuerySensors().Select(s => new Sensor
            {
                Id = s.ParentId,
                Message = s.Name
            }.Message).Where(v => v == "test"), s => s.ToList());
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_NewSensor_Where_FromNothing()
        {
            ExecuteClient(c => c.QuerySensors().Select(s => new Sensor
            {
                Id = s.ParentId,
                Message = "hello"
            }.Message).Where(v => v == "test"), s => s.ToList());
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Select_NewSensor_Where_FromOperation()
        {
            ExecuteClient(c => c.QuerySensors().Select(s => new Sensor
            {
                Id = s.ParentId,
                Message = s.Name + s.Type
            }.Message).Where(v => v == "test"), s => s.ToList());
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SelectTests_HasAllTests()
        {
            HasAllTests(typeof(Unit.SelectTests));
        }
    }
}
