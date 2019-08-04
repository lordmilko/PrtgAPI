using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unit = PrtgAPI.Tests.UnitTests.ObjectData.Query;
using Expr = System.Linq.Expressions.Expression;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData.Query
{
    [TestClass]
    public class WhereTests : BaseQueryTest
    {
        #region Overloads

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceBool_SingleCondition()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteFilter(s => s.Name == upSensor.Name, s =>
            {
                AssertEx.AreEqual(1, s.Count, "Did not retrieve correct number of sensors");
                AssertEx.AreEqual(upSensor.Name, s.Single().Name, "Sensor did not have the right name");
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_SingleCondition_IgnoreIndex()
        {
            var name = "Ping";

            var pingSensors = client.GetSensors(Property.Name, "Ping");
            AssertEx.AreEqual(2, pingSensors.Count, $"Server did not have the correct number of '{name}' sensors");

            ExecuteQuery(q => q.Where((s, i) => s.Name == name), s =>
            {
                AssertEx.AreEqual(2, s.Count, "Did not return two sensors");
                AssertEx.AreEqual(name, s.First().Name, "Name of first sensor was incorrect");
                AssertEx.AreEqual(name, s.Last().Name, "Name of second sensor was incorrect");
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_SingleCondition_UseIndex()
        {
            var sensors = client.GetSensors();

            var first = sensors.First().Id;

            ExecuteQuery(q => q.Where((s, i) => s.Id == first + i), s =>
            {
                AssertEx.AreEqual(3, s.Count, "Either the first three sensors returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work");
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_FirstConditionIgnoreIndex_SecondConditionUseIndex()
        {
            var sensors = client.GetSensors(Property.Name, "Ping");

            ExecuteQuery(q => q.Where((s, i) => s.Name == sensors.First().Name && s.Id == sensors.First().Id + i), s =>
            {
                AssertEx.AreEqual(1, s.Count, "Either the first sensor returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work");
                AssertEx.AreEqual("Ping", s.Single().Name, "Sensor did not have the right name");
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_TwoMaybeIndex_YesNo()
        {
            var sensor = client.GetSensors().First();

            ExecuteQuery(
                q => q.AsEnumerable().Where((s, i) => s.Name.Contains(sensor.Name) && i == 0).Where(s => s.Id == sensor.Id).AsQueryable(),
                s => AssertEx.AreEqual(1, s.Count, "Either the first sensor returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_TwoMaybeIndex_NoYes()
        {
            var sensor = client.GetSensors().First();

            ExecuteQuery(
                q => q.Where(s => s.Name.Contains(sensor.Name)).Where((s, i) => s.Id == sensor.Id && i == 0),
                s => AssertEx.AreEqual(1, s.Count, "Either the first sensor returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_TwoMaybeIndex_YesYes()
        {
            var sensor = client.GetSensors().First();

            ExecuteQuery(
                q => q.Where((s, i) => s.Name.Contains(sensor.Name) && i >= 0).Where((s, j) => s.Id >= sensor.Id && j == 0),
                s => AssertEx.AreEqual(1, s.Count, "Either the first sensor returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_YesNoNo()
        {
            var sensor = client.GetSensors().First();
            var subLength = sensor.Name.Length / 2;
            var first = sensor.Name.Substring(0, subLength);
            var second = sensor.Name.Substring(subLength);

            ExecuteQuery(
                q => q.Where((s, i) => s.Name.Contains(first) && i == 0)
                      .Where(s => s.Id == sensor.Id)
                      .Where(s => s.Name.Contains(second)),
                s => AssertEx.AreEqual(1, s.Count, "Either the first sensor returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_NoYesNo()
        {
            var sensor = client.GetSensors().First();
            var subLength = sensor.Name.Length / 2;
            var first = sensor.Name.Substring(0, subLength);
            var second = sensor.Name.Substring(subLength);

            ExecuteQuery(
                q => q.Where(s => s.Name.Contains(first))
                    .Where((s, j) => s.Id == sensor.Id && j == 0)
                    .Where(s => s.Name.Contains(second)),
                s => AssertEx.AreEqual(1, s.Count, "Either the first sensor returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_NoNoYes()
        {
            var sensor = client.GetSensors().First();
            var subLength = sensor.Name.Length / 2;
            var first = sensor.Name.Substring(0, subLength);
            var second = sensor.Name.Substring(subLength);

            ExecuteQuery(
                q => q.Where(s => s.Name.Contains(first))
                    .Where(s => s.Id == sensor.Id)
                    .Where((s, k) => s.Name.Contains(second) && k == 0),
                s => AssertEx.AreEqual(1, s.Count, "Either the first sensor returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_YesYesNo()
        {
            var sensor = client.GetSensors().First();
            var subLength = sensor.Name.Length / 2;
            var first = sensor.Name.Substring(0, subLength);
            var second = sensor.Name.Substring(subLength);

            ExecuteQuery(
                q => q.Where((s, i) => s.Name.Contains(first) && i == 0)
                    .Where((s, j) => s.Id == sensor.Id && j == 0)
                    .Where(s => s.Name.Contains(second)),
                s => AssertEx.AreEqual(1, s.Count, "Either the first sensor returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_NoYesYes()
        {
            var sensor = client.GetSensors().First();
            var subLength = sensor.Name.Length / 2;
            var first = sensor.Name.Substring(0, subLength);
            var second = sensor.Name.Substring(subLength);

            ExecuteQuery(
                q => q.Where(s => s.Name.Contains(first))
                    .Where((s, j) => s.Id == sensor.Id && j == 0)
                    .Where((s, k) => s.Name.Contains(second) && k == 0),
                s => AssertEx.AreEqual(1, s.Count, "Either the first sensor returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_YesNoYes()
        {
            var sensor = client.GetSensors().First();
            var subLength = sensor.Name.Length / 2;
            var first = sensor.Name.Substring(0, subLength);
            var second = sensor.Name.Substring(subLength);

            ExecuteQuery(
                q => q.Where((s, i) => s.Name.Contains(first) && i == 0)
                    .Where(s => s.Id == sensor.Id)
                    .Where((s, k) => s.Name.Contains(second) && k == 0),
                s => AssertEx.AreEqual(1, s.Count, "Either the first sensor returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Overload_TSourceIntBool_ThreeMaybeIndex_YesYesYes()
        {
            var sensor = client.GetSensors().First();
            var subLength = sensor.Name.Length / 2;
            var first = sensor.Name.Substring(0, subLength);
            var second = sensor.Name.Substring(subLength);

            ExecuteQuery(
                q => q.Where((s, i) => s.Name.Contains(first) && i == 0)
                    .Where((s, j) => s.Id == sensor.Id && j == 0)
                    .Where((s, k) => s.Name.Contains(second) && k == 0),
                s => AssertEx.AreEqual(1, s.Count, "Either the first sensor returned from a normal request didn't match the sensor sensor returned from a query, or the query simply did not work")
            );
        }

        #endregion
        #region Serialization

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Serialize_TimeSpan()
        {
            var sensors = client.GetSensors();

            var haveDowntime = sensors
                .Where(s => s.TotalDowntime != null).ToList();

            var down = haveDowntime
                .OrderBy(s => s.TotalDowntime)
                .Average(s => s.TotalDowntime.Value.TotalSeconds);

            var secs = TimeSpan.FromSeconds(down);

            var expected = sensors.Where(s => s.TotalDowntime > secs).ToList();

            AssertEx.AreNotEqual(haveDowntime.Count, expected.Count, "Expected should have fewer elements than all down");

            ExecuteFilter(
                s => s.TotalDowntime > secs,
                s => AssertEx.AreEqualLists(expected, s, new PrtgObjectComparer(), "Query sensors did not match expected")
             );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Serialize_DateTime()
        {
            var sensors = client.GetSensors();

            var haveLastUp = sensors.Where(s => s.LastUp != null).OrderBy(s => s.LastUp).ToList();

            var median = haveLastUp.Skip(haveLastUp.Count / 2).First().LastUp;

            var expected = haveLastUp.Where(s => s.LastUp > median).ToList();

            ExecuteFilter(
                s => s.LastUp > median,
                s => AssertEx.AreEqualLists(expected.OrderBy(e => e.Id).ToList(), s.OrderBy(e => e.Id).ToList(), new PrtgObjectComparer(), "Query sensors did not match expected")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Serialize_NullableValue()
        {
            var unknown = client.GetSensor(Settings.UnknownSensor);
            AssertEx.AreEqual(null, unknown.LastUp, "Unknown sensor did not have a null LastUp");

            ExecuteFilter(
                s => s.LastUp.Value > DateTime.Now.AddYears(-1),
                s => AssertEx.IsTrue(!s.Select(o => o.Id).Contains(Settings.UnknownSensor), "Sensors with a null LastUp did not contain the unknown sensor")
            );
        }

        #endregion
        #region Operators

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Id).ToList();
            var upSensor = sensors.Single(s => s.Id == Settings.UpSensor);
            var middle = sensors.Skip(sensors.Count / 2).First();

            ExecuteFilter(s => s.Name == upSensor.Name,      s => AssertEx.AreEqual(upSensor.Name, s.Single().Name, "Name retrieved from == was incorrect"));
            ExecuteFilter(s => s.Name.Equals(upSensor.Name), s => AssertEx.AreEqual(upSensor.Name, s.Single().Name, "Name retrieved from Equals() was incorrect"));
            ExecuteFilter(s => s.Name != upSensor.Name,      s => AssertEx.AreEqual(sensors.Count - 1, s.Count, "Name retrieved from != was incorrect"));
            ExecuteFilter(s => s.Id > middle.Id,             s => AssertEx.AreEqual(sensors.Count(o => o.Id > middle.Id), s.Count, "Ids retrieved from > was incorrect"));
            ExecuteFilter(s => s.Id >= middle.Id,            s => AssertEx.AreEqual(sensors.Count(o => o.Id >= middle.Id), s.Count, "Ids retrieved from >= was incorrect"));
            ExecuteFilter(s => s.Id < middle.Id,             s => AssertEx.AreEqual(sensors.Count(o => o.Id < middle.Id), s.Count, "Ids retrieved from < was incorrect"));
            ExecuteFilter(s => s.Id <= middle.Id,            s => AssertEx.AreEqual(sensors.Count(o => o.Id <= middle.Id), s.Count, "Ids retrieved from <= was incorrect"));
            ExecuteFilter(s => s.Name.Contains("Pi"),        s => AssertEx.AreEqual(sensors.Count(o => o.Name.Contains("Pi")), s.Count, "Name retrieved from Contains() was incorrect"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators_Backwards()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Id).ToList();
            var upSensor = sensors.Single(s => s.Id == Settings.UpSensor);
            var middle = sensors.Skip(sensors.Count / 2).First();

            ExecuteFilter(s => upSensor.Name == s.Name,      s => AssertEx.AreEqual(upSensor.Name, s.Single().Name, "Name retrieved from == was incorrect"));
            ExecuteFilter(s => upSensor.Name.Equals(s.Name), s => AssertEx.AreEqual(upSensor.Name, s.Single().Name, "Name retrieved from Equals() was incorrect"));
            ExecuteFilter(s => upSensor.Name != s.Name,      s => AssertEx.AreEqual(sensors.Count - 1, s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id < s.Id,             s => AssertEx.AreEqual(sensors.Count(o => o.Id > middle.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id <= s.Id,            s => AssertEx.AreEqual(sensors.Count(o => o.Id >= middle.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id > s.Id,             s => AssertEx.AreEqual(sensors.Count(o => o.Id < middle.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id >= s.Id,            s => AssertEx.AreEqual(sensors.Count(o => o.Id <= middle.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => "Ping".Contains(s.Name),        s => AssertEx.AreEqual(sensors.Count(o => o.Name == "Ping"), s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators_Equals_EmptyString()
        {
            ExecuteFilter(s => s.Name == string.Empty, s => Assert.AreEqual(0, s.Count));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators_Contains_IntToString()
        {
            var expected = client.GetSensors().Count(s => s.Id.ToString().Contains("20"));

            ExecuteFilter(s => s.Id.ToString().Contains("20"), s => AssertEx.AreEqual(expected, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators_Equals_IntToString()
        {
            ExecuteFilter(s => s.Id.ToString() == Settings.DownSensor.ToString(), s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators_Equals_NullableDoubleToString()
        {
            var mostCommon = client.GetSensors().Where(s => s.LastValue > 0).GroupBy(s => s.LastValue).OrderBy(s => s.Key).First();

            ExecuteFilter(s => s.LastValue.ToString() == mostCommon.Key.ToString(), s => AssertEx.AreEqual(mostCommon.Count(), s.Count, $"Number of sensors with LastValue {mostCommon.Key} was incorrect"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators_Contains_StructToString()
        {
            FilterTests.Retry(retry =>
            {
                var lastUp = client.GetSensors().Select(s => s.LastUp?.ToString()).First(u => u != null);
                ExecuteFilter(s => s.LastUp.ToString().Contains(lastUp), s => AssertEx.AreEqual(1, s.Count, $"Number of sensors with LastUp {lastUp} was incorrect", retry));
            });

            FilterTests.Retry(retry =>
            {
                var upDuration = client.GetSensors().Select(s => s.UpDuration?.ToString()).First(u => u != null);
                ExecuteFilter(s => s.UpDuration.ToString().Contains(upDuration), s => AssertEx.AreEqual(1, s.Count, $"Number of sensors with UpDuration {upDuration} was incorrect", retry));
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators_Contains_ClassToString()
        {
            ExecuteFilter(s => s.NotificationTypes.ToString().Contains("Volume"), s => AssertEx.AreEqual(Settings.SensorsInTestServer, s.Count, "Did not match all sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators_Contains_EnumToString()
        {
            var expected = client.GetSensors().Count(s => s.Status == Status.Up);

            ExecuteFilter(s => s.Status.ToString().Contains("Up"), s => AssertEx.AreEqual(expected, s.Count, "Did not return all up sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_StartsWith()
        {
            var expected = client.GetSensors().Count(s => s.Name.StartsWith("Pi"));

            ExecuteFilter(s => s.Name.StartsWith("Pi"), s => AssertEx.AreEqual(expected, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators_Fake_EndsWith()
        {
            var expected = client.GetSensors().Count(s => s.Name.EndsWith("ng"));

            ExecuteFilter(s => s.Name.EndsWith("ng"), s => AssertEx.AreEqual(expected, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators_Fake_StartsWith_Take()
        {
            ExecuteClient(
                c => c.QuerySensors(s => s.Name.StartsWith("Pi")).Take(2),
                s => AssertEx.AreEqual(2, s.Count(), "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Operators_Fake_EndsWith_Take()
        {
            ExecuteClient(
                c => c.QuerySensors(s => s.Name.EndsWith("ng")).Take(2),
                s => AssertEx.AreEqual(2, s.Count(), "Did not return correct number of sensors")
            );
        }

        #endregion
        #region Server Logic

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_ServerLogic_DifferentProperties_And_SingleQuery()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteFilter(s => s.Name == upSensor.Name && s.ParentId == upSensor.ParentId,
                s =>
                {
                    AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors");
                    AssertEx.AreEqual(upSensor.Name, s.Single().Name, "Sensor name was incorrect");
                });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_ServerLogic_DifferentProperties_Or()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);
            var count = client.GetSensors(Property.ParentId, upSensor.ParentId).Count;

            ExecuteFilter(
                s => s.Name == upSensor.Name || s.ParentId == upSensor.ParentId,
                s => AssertEx.AreEqual(count, s.Count, "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_ServerLogic_SameProperty_And_SingleQuery()
        {
            var first = client.GetSensor(Settings.UpSensor);
            var second = client.GetSensor(Settings.DownSensor);

            ExecuteFilter(
                s => s.Name == first.Name && s.Name == second.Name,
                s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors")
            );

            var pingSensors = client.GetSensors().Count(s => s.Name.Contains("Pi") && s.Name.Contains("ng"));

            AssertEx.IsTrue(pingSensors > 0, "Did not retrieve any sensors");

            ExecuteFilter(
                s => s.Name.Contains("Pi") && s.Name.Contains("ng"),
                s => AssertEx.AreEqual(pingSensors, s.Count, "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_ServerLogic_SameProperty_Or()
        {
            var first = client.GetSensor(Settings.UpSensor);
            var second = client.GetSensor(Settings.DownSensor);

            ExecuteFilter(
                s => s.Name == first.Name || s.Name == second.Name,
                s =>
                {
                    AssertEx.AreEqual(2, s.Count, "Did not return correct number of sensors");
                    AssertEx.AreEqual(first.Name, s.First().Name, "First sensor name was incorrect");
                    AssertEx.AreEqual(second.Name, s.Last().Name, "Second sensor name was incorrect");
                }
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_ServerLogic_SamePropertyOr_AndDifferentProperty()
        {
            var first = client.GetSensor(Settings.UpSensor);
            var second = client.GetSensors().First(s => s.ParentId != first.ParentId);

            ExecuteFilter(
                    s => (s.Name == first.Name || s.Name == second.Name) && s.ParentId == second.ParentId,
                s =>
                {
                    AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors");
                    AssertEx.AreEqual(second.Name, s.First().Name, "Sensor name was incorrect");
                }
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_ServerLogic_SameProperty_And_TwoQueries()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            var first = upSensor.Name.Substring(0, upSensor.Name.Length / 2);
            var second = upSensor.Name.Substring(upSensor.Name.Length / 2);

            ExecuteQuery(
                q => q.Where(s => s.Name.Contains(first)).Where(t => t.Name.Contains(second)),
                s =>
                {
                    AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors");
                    AssertEx.AreEqual(upSensor.Name, s.Single().Name, "Sensor name was incorrect");
                }
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_ServerLogic_DifferentProperties_And_TwoQueries()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteQuery(
                q => q.Where(s => s.Name == upSensor.Name).Where(s => s.ParentId == upSensor.ParentId),
                s =>
                {
                    AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors");
                    AssertEx.AreEqual(upSensor.Name, s.Single().Name, "Sensor name was incorrect");
                }
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_ServerLogic_DifferentProperties_And_ThreeQueries()
        {
            var sensor = client.GetSensors(Property.Name, "Ping").First();

            ExecuteQuery(
                q => q.Where(s => s.Name == "Ping").Where(s => s.ParentId == sensor.ParentId).Where(s => s.Id == sensor.Id),
                s =>
                {
                    AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors");
                    AssertEx.AreEqual("Ping", s.Single().Name, "Sensor name was incorrect");
                }
            );
        }

        #endregion
        #region Boolean Property Logic

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Boolean_IsExplicitlyTrue()
        {
            var activeCount = client.GetSensors().Count(s => s.Active);

            ExecuteFilter(s => s.Active == true, s => AssertEx.AreEqual(activeCount, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_BooleanExpression_IsExplicitlyTrue_OnLeft()
        {
            ExecuteFilter(s => true == (s.Id == Settings.UpSensor), s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Boolean_IsExplicitlyFalse()
        {
            var inactiveCount = client.GetSensors().Count(s => !s.Active);

            ExecuteFilter(s => s.Active == false, s => AssertEx.AreEqual(inactiveCount, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Boolean_IsImplicitlyTrue()
        {
            var activeCount = client.GetSensors().Count(s => s.Active);

            ExecuteFilter(s => s.Active, s => AssertEx.AreEqual(activeCount, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Boolean_IsImplicitlyTrue_AndCondition()
        {
            ExecuteFilter(s => s.Active && s.Id == Settings.UpSensor, s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Boolean_IsImplicitlyTrue_OrCondition()
        {
            ExecuteFilter(s => s.Active || !s.Active, s => AssertEx.AreEqual(Settings.SensorsInTestServer, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Boolean_NotEqualsTrue()
        {
            var inactiveCount = client.GetSensors().Count(s => !s.Active);

            ExecuteFilter(s => s.Active != true, s => AssertEx.AreEqual(inactiveCount, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Boolean_NotEqualsFalse()
        {
            var activeCount = client.GetSensors().Count(s => s.Active);

            ExecuteFilter(s => s.Active != false, s => AssertEx.AreEqual(activeCount, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Boolean_IsNotTrue()
        {
            var inactiveCount = client.GetSensors().Count(s => !s.Active);

            ExecuteFilter(s => s.Active == !true, s => AssertEx.AreEqual(inactiveCount, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Boolean_IsNotNotTrue()
        {
            var activeCount = client.GetSensors().Count(s => s.Active);

            ExecuteFilter(s => s.Active == !!true, s => AssertEx.AreEqual(activeCount, s.Count, "Did not return correct number of sensors"));
        }

        #endregion
        #region Enum Property Logic

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Enum_Normal()
        {
            var upSensors = client.GetSensors().Count(s => s.Status == Status.Up);

            ExecuteFilter(s => s.Status == Status.Up, s => AssertEx.AreEqual(upSensors, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Enum_ToString()
        {
            var upSensors = client.GetSensors().Where(s => s.Status == Status.Up).Count();

            ExecuteFilter(s => s.Status.ToString() == "Up", s => AssertEx.AreEqual(upSensors, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Enum_ToString_IllegalString()
        {
            ExecuteFilter(s => s.Status.ToString() == "Banana", s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Enum_AgainstDifferentEnumType_DifferentNumber()
        {
            ExecuteFilter(s => ((Enum)s.Status).Equals((Enum)RetryMode.Retry), s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Enum_AgainstDifferentEnumType_ManualExpression()
        {
            var lambda = Unit.BaseExpressionTest.CreateLambda(Property.Status, s =>
            {
                var retry = Expr.Constant(RetryMode.Retry);
                var methodInfo = typeof(object).GetMethod(nameof(Equals), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                var method = Expr.Call(methodInfo, Expr.Convert(retry, typeof(object)), Expr.Convert(s, typeof(object)));

                return method;
            });

            ExecuteFilter(lambda, s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Enum_AgainstDifferentEnumType_DifferentNumber_RightToLeft()
        {
            ExecuteFilter(s => ((Enum)RetryMode.Retry).Equals((Enum)s.BaseType), s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Enum_AgainstDifferentEnumType_Method()
        {
            ExecuteFilter(s => ((Enum)s.Status).Equals(SomeEnum(s)), s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Enum_AgainstDifferentEnumType_NoCast()
        {
            ExecuteFilter(s => s.Status.Equals(RetryMode.Retry), s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));
        }

        private Enum SomeEnum(Sensor s)
        {
            return RetryMode.Retry;
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Enum_AgainstDifferentEnumType_SameNumber()
        {
            ExecuteFilter(s => ((Enum)s.BaseType).Equals((Enum)AuthMode.PassHash), s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Enum_AgainstDifferentEnumType_ToString()
        {
            ExecuteFilter(s => (((Enum)s.Status).ToString()).Equals(((Enum)RetryMode.Retry).ToString()), s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));
        }

        #endregion
        #region Illegal Logic

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_IllegalLogic_Property_EqualsNull()
        {
            var unknown = client.GetSensors().Where(s => s.LastUp == null).OrderBy(s => s.Id).ToList();

            ExecuteFilter(
                s => s.LastUp == null,
                s => AssertEx.AreEqualLists(unknown, s.OrderBy(o => o.Id).ToList(), new PrtgObjectComparer(), "Did not retrieve the correct number of objects with a null property")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_IllegalLogic_PropertyOrDifferent_And_Property()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);
            var downSensor = client.GetSensor(Settings.DownSensor);

            AssertEx.AreEqual(upSensor.ParentId, downSensor.ParentId, "Up and down sensors did not have the same parent ID");

            //Cannot OR different properties, cannot AND the same property
            ExecuteFilter(
                    s => (s.Name == upSensor.Name || s.ParentId == upSensor.ParentId) && s.Name == downSensor.Name,
                s =>
                {
                    AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors");
                    AssertEx.AreEqual(downSensor.Name, s.Single().Name, "Sensor name was incorrect");
                }
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_IllegalLogic_PropertyAndProperty_Or_Different()
        {
            var count = client.GetSensors().Where(s => (s.Name.Contains("Pi") && s.Name.Contains("ng")) || s.ParentId == Settings.Device).Count();

            //Cannot AND the same property, cannot OR different properties
            ExecuteFilter(
                s => (s.Name.Contains("Pi") && s.Name.Contains("ng")) || s.ParentId == Settings.Device,
                s => AssertEx.AreEqual(count, s.Count, "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_IllegalLogic_Property1_Against_Property2()
        {
            ExecuteFilter(s => s.Name == s.Message, s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_IllegalLogic_IllegalMethod_EqualsValue()
        {
            var startsWithPi = client.GetSensors().Count(s => s.Name.Substring(0, 2) == "Pi");

            AssertEx.IsTrue(startsWithPi > 0, "Did not retrieve any sensors");

            ExecuteFilter(s => s.Name.Substring(0, 2) == "Pi", s => AssertEx.AreEqual(startsWithPi, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_IllegalLogic_InternalLambda()
        {
            var count = client.GetSensors().Where(s => s.Tags.Where(t => t.Contains("wmi")).Any()).Count();

            ExecuteFilter(
                s => s.Tags.Where(t => t.Contains("wmi")).Any(),
                s => AssertEx.AreEqual(count, s.Count, "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_IllegalLogic_PropertyMember()
        {
            var upToday = client.GetSensors().Where(s => s.LastUp?.DayOfWeek == DateTime.Now.DayOfWeek).OrderBy(s => s.Id).ToList();

            ExecuteFilter(
                s => s.LastUp != null && s.LastUp.Value.DayOfWeek == DateTime.Now.DayOfWeek,
                s => AssertEx.AreEqualLists(upToday, s.OrderBy(o => o.Id).ToList(), new PrtgObjectComparer(), "Lists were not equal")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_IllegalLogic_PropertyMethod()
        {
            var startsWithPi = client.GetSensors().Where(s => s.Name.StartsWith("Pi")).Count();

            ExecuteFilter(
                s => s.Name.IndexOf("Pi") == 0,
                s => AssertEx.AreEqual(startsWithPi, s.Count, "Did not return correct number of sensors")
            );
        }

        #endregion
        #region Negated Conditions

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Negation_EqualsFalse_Supported()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Id).ToList();
            var middle = sensors.Skip(sensors.Count / 2).First();

            ExecuteFilter(s => s.Name == "Ping" == false,      s => AssertEx.AreEqual(sensors.Count(o => o.Name != "Ping"), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => s.Name.Equals("Ping") == false, s => AssertEx.AreEqual(sensors.Count(o => o.Name != "Ping"), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => "Ping" != s.Name == false,      s => AssertEx.AreEqual(sensors.Count(o => o.Name == "Ping"), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id < s.Id == false,      s => AssertEx.AreEqual(sensors.Count(o => middle.Id >= o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id <= s.Id == false,     s => AssertEx.AreEqual(sensors.Count(o => middle.Id > o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id > s.Id == false,      s => AssertEx.AreEqual(sensors.Count(o => middle.Id <= o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id >= s.Id == false,     s => AssertEx.AreEqual(sensors.Count(o => middle.Id < o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => "Ping".Contains(s.Name) == false, s => AssertEx.AreEqual(sensors.Count(o => o.Name != "Ping"), s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Negation_EqualsFalse_RightToLeft()
        {
            ExecuteFilter(
                s => false == (s.Id == Settings.UpSensor),
                s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Negation_NotContains_EqualsFalse()
        {
            var containsPi = client.GetSensors().Where(s => s.Name.Contains("Pi")).Count();

            ExecuteFilter(
                s => !s.Name.Contains("Pi") == false,
                s => AssertEx.AreEqual(containsPi, s.Count, "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Negation_NotContains_EqualsCastedFalse()
        {
            var containsPi = client.GetSensors().Where(s => s.Name.Contains("Pi")).Count();

            //Construct an expression like s => !s.Contains("Vol") == (bool)false
            var parameter = Expr.Parameter(typeof(Sensor), "s");
            var member = Expr.MakeMemberAccess(parameter, typeof(Sensor).GetProperty("Name"));
            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var methodCall = Expr.Call(member, method, Expr.Constant("Pi"));
            var notMethodCall = Expr.Not(methodCall);
            var boolVal = Expr.Constant(false);
            var cast = Expr.Convert(boolVal, typeof(bool));
            var equals = Expr.Equal(notMethodCall, cast);

            var lambda = Expr.Lambda<Func<Sensor, bool>>(
                equals,
                parameter
            );

            //Expression will be replaced by partial evaluator
            ExecuteFilter(lambda, s => AssertEx.AreEqual(containsPi, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Negation_NotNotContains_EqualsFalse()
        {
            var sensors = client.GetSensors().Where(s => !s.Name.Contains("Pi")).Count();

            ExecuteFilter(s => !!s.Name.Contains("Pi") == false, s => AssertEx.AreEqual(sensors, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Negation_NotNotContains_EqualsFalse_RightToLeft()
        {
            var sensors = client.GetSensors().Where(s => !s.Name.Contains("Pi")).Count();

            ExecuteFilter(s => false == !!s.Name.Contains("Pi"), s => AssertEx.AreEqual(sensors, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Negation_EqualsFalse_Twice_Supported()
        {
            var sensors = client.GetSensors().Where(s => s.Name == "Ping").Count();

            ExecuteFilter(s => s.Name == "Ping" == false == false, s => AssertEx.AreEqual(sensors, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Negation_Not_Supported()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Id).ToList();
            var upSensor = sensors.Single(s => s.Id == Settings.UpSensor);
            var middle = sensors.Skip(sensors.Count / 2).First();

            ExecuteFilter(s => !(s.Name == upSensor.Name),      s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => !(s.Name.Equals(upSensor.Name)), s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => !(upSensor.Name != s.Name),      s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => !(middle.Id < s.Id),             s => AssertEx.AreEqual(sensors.Count(o => middle.Id >= o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => !(middle.Id <= s.Id),            s => AssertEx.AreEqual(sensors.Count(o => middle.Id > o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => !(middle.Id > s.Id),             s => AssertEx.AreEqual(sensors.Count(o => middle.Id <= o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => !(middle.Id >= s.Id),            s => AssertEx.AreEqual(sensors.Count(o => middle.Id < o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => !(upSensor.Name.Contains(s.Name)), s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Negation_NotEqualsTrue_Supported()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Id).ToList();
            var upSensor = sensors.Single(s => s.Id == Settings.UpSensor);
            var middle = sensors.Skip(sensors.Count / 2).First();

            ExecuteFilter(s => s.Name == upSensor.Name != true,      s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => s.Name.Equals(upSensor.Name) != true, s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => upSensor.Name != s.Name != true,      s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id < s.Id != true,             s => AssertEx.AreEqual(sensors.Count(o => middle.Id >= o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id <= s.Id != true,            s => AssertEx.AreEqual(sensors.Count(o => middle.Id > o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id > s.Id != true,             s => AssertEx.AreEqual(sensors.Count(o => middle.Id <= o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id >= s.Id != true,            s => AssertEx.AreEqual(sensors.Count(o => middle.Id < o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => upSensor.Name.Contains(s.Name) != true, s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Negation_NotEqualsFalse_Supported()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Id).ToList();
            var upSensor = sensors.Single(s => s.Id == Settings.UpSensor);
            var middle = sensors.Skip(sensors.Count / 2).First();

            ExecuteFilter(s => s.Name == upSensor.Name != false,      s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => s.Name.Equals(upSensor.Name) != false, s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => upSensor.Name != s.Name != false,      s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id < s.Id != false,             s => AssertEx.AreEqual(sensors.Count(o => middle.Id < o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id <= s.Id != false,            s => AssertEx.AreEqual(sensors.Count(o => middle.Id <= o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id > s.Id != false,             s => AssertEx.AreEqual(sensors.Count(o => middle.Id > o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => middle.Id >= s.Id != false,            s => AssertEx.AreEqual(sensors.Count(o => middle.Id >= o.Id), s.Count, "Did not return correct number of sensors"));
            ExecuteFilter(s => upSensor.Name.Contains(s.Name) != false, s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Negation_NotEqualsFalse_RightToLeft()
        {
            var sensors = client.GetSensors().Count(s => s.Name == "Ping");

            ExecuteFilter(s => false != (s.Name == "Ping"), s => AssertEx.AreEqual(sensors, s.Count, "Did not return correct number of sensors"));
        }

        #endregion
        #region Nested Queries

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Sensors_MatchAnyDevices()
        {
            var sensor = client.GetSensor(Settings.UpSensor);
            var diff = sensor.ParentId - sensor.Id;

            var sensors = client.QuerySensors().Where(s => client.QueryDevices().Any(d => d.Id == s.Id - diff)).ToList();

            AssertEx.AreEqual(2, sensors.Count, "Did not return correct number of sensors");
        }

        #endregion
        #region Cast

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Enum_To_TypedEnum()
        {
            var upSensors = client.GetSensors(Status.Up).Count;

            ExecuteFilter(s => s.Status == (Status)Status.Up, s => AssertEx.AreEqual(upSensors, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Enum_To_UntypedEnum()
        {
            var upSensors = client.GetSensors(Status.Up).Count;

            ExecuteFilter(s => ((Enum)s.Status).Equals((Enum)Status.Up), s => AssertEx.AreEqual(upSensors, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Int_To_Enum()
        {
            var upSensors = client.GetSensors(Status.Up).Count;

            ExecuteFilter(s => s.Status == (Status)8, s => AssertEx.AreEqual(upSensors, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_NullableDouble_To_Int()
        {
            FilterTests.Retry(retry =>
            {
                var sensor = new FilterTests().GetLastValueSensor(Property.Id, Settings.UpSensor);

                int value = (int)sensor.LastValue;

                ExecuteFilter(s => (int)s.LastValue == value, s => AssertEx.AreEqual(1, s.Count, $"Did not return correct number of sensors with value '{value}'"));
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Int_To_Double() => ExecuteFilter(s => (double)s.Id == 1001.0, s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Int_To_Illegal() => ExecuteFilter(s => (Unit.IllegalInt)s.Id == Settings.UpSensor, s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Int_To_NullableInt() => ExecuteFilter(s => (int?)s.Id == Settings.UpSensor, s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_NullableBool_To_Bool()
        {
            var active = client.GetSensors().Where(s => s.Active).Count();

            ExecuteFilter(s => (bool)s.Active == true, s => AssertEx.AreEqual(active, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Object_To_Type()
        {
            var active = client.GetSensors().Where(s => s.Active).Count();

            ExecuteFilter(s => (bool)(object)s.Active == true, s => AssertEx.AreEqual(active, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Object_To_NullableType() => ExecuteFilter(s => (int?)(object)s.Id == Settings.UpSensor, s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Type_To_Object() => ExecuteFilter(s => ((object)s.Id).Equals((object)Settings.UpSensor), s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Type_To_Object_OperatorEquality() => ExecuteFilter(s => (object)s.Id == (object)Settings.UpSensor, s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_NullableType_To_Object()
        {
            var uptime = client.GetSensors(Property.Tags, FilterOperator.Contains, "wmiuptime").Single();

            ExecuteFilter(s => ((object)s.LastValue).Equals((object)uptime.LastValue), s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_IsImplicitlyTrue()
        {
            var active = client.GetSensors().Where(s => s.Active).Count();

            ExecuteFilter(s => (bool)(object)s.Active, s => AssertEx.AreEqual(active, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Condition_EqualsFalse()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == Settings.UpSensor)) == false, s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Condition_EqualsTrue()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == Settings.UpSensor)) == true, s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Property_EqualsFalse()
        {
            var inactive = client.GetSensors().Where(s => !s.Active).Count();

            ExecuteFilter(s => ((bool)(object)(s.Active)) == false, s => AssertEx.AreEqual(inactive, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Property_EqualsTrue()
        {
            var active = client.GetSensors().Where(s => s.Active).Count();

            ExecuteFilter(s => ((bool)(object)(s.Active)) == true, s => AssertEx.AreEqual(active, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Condition_ToNullableBool()
        {
            ExecuteFilter(s => ((bool?)(object)(s.Id == Settings.UpSensor)) == false, s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Condition_Bool()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == Settings.UpSensor)) == false, s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Condition_EqualsBool()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == Settings.UpSensor)).Equals(false), s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Condition_EqualsInt()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == Settings.UpSensor)).Equals(3), s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Condition_ContainsString()
        {
            //Can't reduce further since Contains("False") would require the FALSE ones, which could
            //have only been calculated if all results were returned
            ExecuteFilter(s => ((bool)(object)(s.Id == Settings.UpSensor)).ToString().Contains("test"), s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Condition_ContainsCondition()
        {
            ExecuteFilter(s => ((bool)(object)(s.Id == Settings.UpSensor)).ToString().Contains((s.ParentId == Settings.UpSensor).ToString()), s => AssertEx.AreEqual(Settings.SensorsInTestServer - 1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Cast_Property_Enum()
        {
            var up = client.GetSensors(Status.Up).Count;

            ExecuteFilter(s => ((Enum)(object)(s.Status)).Equals((Enum)Status.Up), s => AssertEx.AreEqual(up, s.Count, "Did not return correct number of sensors"));
        }

        #endregion
        #region AsEnumerable / AsQueryable

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_WrapProperty_Add()
        {
            ExecuteFilter(s => s.Id + 3 == 4, s =>
            {
                AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors");
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_AsEnumerable_AsQueryable()
        {
            ExecuteClient(
                c => c.QuerySensors().Where(s => s.Id == Settings.UpSensor).AsEnumerable().AsQueryable(),
                s => AssertEx.AreEqual(1, s.Count(), "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_AsEnumerable_AsQueryable_QueryableMethod()
        {
            var sensor = client.GetSensor(Settings.UpSensor);

            ExecuteClient(
                c => c.QuerySensors().Where(s => s.Id == Settings.UpSensor).AsEnumerable().AsQueryable().Where(n => n.Name == sensor.Name),
                s => AssertEx.AreEqual(1, s.Count(), "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_AsEnumerable_EnumerableMethod_AsQueryable()
        {
            var sensor = client.GetSensor(Settings.UpSensor);

            ExecuteClient(
                c => c.QuerySensors().Where(s => s.Id == Settings.UpSensor).AsEnumerable().Where(s => s.Name == sensor.Name).AsQueryable(),
                s => AssertEx.AreEqual(1, s.Count(), "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_AsEnumerable_EnumerableMethod_AsQueryable_QueryableMethod()
        {
            var sensor = client.GetSensor(Settings.UpSensor);

            ExecuteClient(
                c => c.QuerySensors()
                    .Where(s => s.Id == Settings.UpSensor).AsEnumerable()
                    .Where(s => s.Name == sensor.Name).AsQueryable()
                    .Select(s => s.Name),
                s => AssertEx.AreEqual(1, s.Count(), "Did not return correct number of sensors")
            );
        }

        #endregion
        #region Null Access

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_NullAccess_BoolMethod_OnNullProperty()
        {
            ExecuteNullable(
                q => q.Where(s => s.Comments.Contains("Blah")),
                () => new List<Sensor>(),
                s => s.Comments,
                s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_NullAccess_NonBoolMethod_OnNullProperty()
        {
            ExecuteNullable(
                q => q.Where(s => s.Comments.ToString() == "Blah"),
                () => new List<Sensor>(),
                s => s.Comments,
                s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_NullAccess_BoolMethod_OnMethod_OnNullProperty()
        {
            ExecuteNullable(
                q => q.Where(s => s.Comments.ToString().Contains("Blah")),
                () => new List<Sensor>(),
                s => s.Comments,
                s => AssertEx.AreEqual(0, s.Count, "Did not return correct number of sensors")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_NullAccess_NonBoolMethod_OnMethod_OnNullProperty()
        {
            AssertEx.Throws<NullReferenceException>(() =>
            {
                ExecuteNullable<string, string>(
                    q => q.Select(s => s.Comments.ToString().Substring(0, 3)),
                    () => new List<string>(),
                    s => s,
                    null
                );
            }, "Object reference not set to an instance of an object calling method 'Substring' in expression 's.Comments.ToString().Substring(0, 3)'. Consider using a ternary expression to specify conditional access.");

        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_NullAccess_NonBoolPropertyAccess_OnNullProperty()
        {
            AssertEx.Throws<NullReferenceException>(() =>
            {
                ExecuteNullable<Sensor, int>(q => q.Where(s => s.Comments.Length == 3), null, null, null);
            }, "Object reference not set to an instance of an object.");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_NullAccess_NonBoolPropertyAccess_OnMethod_OnNullProperty()
        {
            AssertEx.Throws<NullReferenceException>(() =>
            {
                ExecuteNullable<int, int>(
                    q => q.Select(s => s.Comments.Substring(0, 3).Length),
                    () => new List<int>(),
                    s => s,
                    null
                );
            }, "Object reference not set to an instance of an object calling method 'Substring' in expression 's.Comments.Substring(0, 3)'. Consider using a ternary expression to specify conditional access.");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_NullAccess_Value_OfNullProperty()
        {
            ExecuteNullable(
                q => q.Where(l => l.LastValue.Value == 3),
                () => client.GetSensors().Where(l => l.LastValue == null).ToList(),
                s => s.LastValue,
                s => s.ToList()
            );

            AssertEx.Throws<InvalidOperationException>(() =>
            {
                ExecuteNullable(
                    q => q.Select(l => l.LastValue.Value).Cast<double?>(),
                    () => client.GetSensors().Where(s => s.LastValue == null).Select(l => l.LastValue).ToList(),
                    s => s,
                    null
                 );
            }, "Nullable object must have a value");
        }

        #endregion
        #region Intermediate Types

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Intermediate_AnonymousType()
        {
            ExecuteFilter(s => new
            {
                FakeId = s.Id
            }.FakeId == Settings.UpSensor, s => AssertEx.AreEqual(1, s.Count, "Did not return correct number of sensors"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Intermediate_NewSensorType_Single()
        {
            var sensor = client.GetSensor(Settings.UpSensor);

            ExecuteFilter(s => new Sensor
            {
                Message = s.Name
            }.Message == sensor.Name, s => AssertEx.AreEqual(sensor.Name, s.Single().Name, "Sensor name was incorrect"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Intermediate_NewSensor_ToWhere()
        {
            var sensor = client.GetSensor(Settings.UpSensor);

            ExecuteClient(c => c.QuerySensors().Where(s => new Sensor
            {
                Message = s.Name
            }.Message == sensor.Name).Where(s => s.Id == Settings.UpSensor), s => AssertEx.AreEqual(sensor.Name, s.Single().Name, "Sensor name was incorrect"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_Intermediate_NewSensor_Select_FromSource()
        {
            var sensor = client.GetSensor(Settings.UpSensor);

            ExecuteClient(c => c.QuerySensors().Where(s => new Sensor
            {
                Message = s.Name
            }.Message == sensor.Name).Select(s => s.Name), s => AssertEx.AreEqual(sensor.Name, s.Single(), "Sensor name was incorrect"));
        }

        #endregion
        #region Split Requests

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_SplitRequests_Logs_Select_SingleSet_ReduceParameters()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteClient(
                c => c.QueryLogs().Where(l => l.Id == Settings.UpSensor).Select(l => l.Name),
                l => AssertEx.IsTrue(l.All(v => v == upSensor.Name), $"All sensors did not have the name '{upSensor.Name}'")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_SplitRequests_Logs_Select_MultipleSets_AllParameters()
        {
            var first = client.GetSensor(Settings.UpSensor);
            var second = client.GetSensor(Settings.DownSensor);

            ExecuteClient(
                c => c.QueryLogs().Where(l => l.Id == Settings.UpSensor || l.Id == Settings.DownSensor).Select(l => l.Name),
                l => AssertEx.IsTrue(l.Contains(first.Name) && l.Contains(second.Name), $"Response did not contain both {first.Name} and {second.Name}")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_SplitRequests_OmitsId_WithSingleRequest()
        {
            var count = client.GetSensors(Property.Name, "Ping").Count;

            ExecuteClient(
                c => c.QuerySensors(s => s.Name == "Ping").Select(s => s.Name),
                s => {
                    var list = s.ToList();

                    AssertEx.IsTrue(list.Any() && list.All(v => v == "Ping") && list.Count() == count, "Response did not contain all ping sensors");
                }
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Where_SplitRequests_IncludesId_WithMultipleRequests()
        {
            var sensors = client.GetSensors().Where(s => s.Name == "Ping" || s.Device == Settings.DeviceName);

            ExecuteClient(
                c => c.QuerySensors(s => s.Name == "Ping" || s.Device == Settings.DeviceName).Select(s => s.Name),
                s => AssertEx.AreEqual(sensors.Count(), s.Count(), "Did not return correct number of sensors")
            );
        }

        #endregion
        #region Type Constraints

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Where_TypeConstraints_SinglePredicate_ConstraintPropertiesOnly()
        {
            var pingCount = client.GetSensors().Count(s => s.Name.Contains("Ping"));

            ExecuteFilter(s => ((IObject)s).Name.Contains("Ping"), s => Assert.AreEqual(pingCount, s.Count));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Where_TypeConstraints_SinglePredicate_BaseType()
        {
            var pingCount = client.GetSensors().Count(s => s.Name.Contains("Ping"));

            ExecuteFilter(s => ((SensorOrDeviceOrGroupOrProbe)s).Name.Contains("Ping"), s => Assert.AreEqual(pingCount, s.Count));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Where_TypeConstraints_TwoInterfaces()
        {
            var pingCount = client.GetSensors().Count(s => s.Name.Contains("Ping"));

            Execute(q => q.Where(s => ((IObject)s).Name.Contains("Ping")).Select(s => ((ITableObject)s).Id), s => Assert.AreEqual(pingCount, s.Count));
        }

        #endregion

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_WhereTests_HasAllTests()
        {
            HasAllTests(typeof(Unit.WhereTests));
        }
    }
}
