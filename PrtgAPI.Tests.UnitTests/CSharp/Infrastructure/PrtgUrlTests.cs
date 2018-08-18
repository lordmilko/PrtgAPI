using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Request;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests
{
    [TestClass]
    public class PrtgUrlTests
    {
        [TestMethod]
        public void PrtgUrl_Server_Prefix_Http()
        {
            Server_Prefix("http://prtg.example.com", "http://prtg.example.com");
        }

        [TestMethod]
        public void PrtgUrl_Server_Prefix_Https()
        {
            Server_Prefix("https://prtg.example.com", "https://prtg.example.com");
        }

        [TestMethod]
        public void PrtgUrl_Server_Prefix_None()
        {
            Server_Prefix("prtg.example.com", "https://prtg.example.com");
        }

        [TestMethod]
        public void PrtgUrl_SpecifiedNoPass_Yields_PassHash()
        {
            var url = CreateUrl(new Parameters.Parameters(), false);

            Assert.IsTrue(url.Contains("passhash=password"));
            Assert.IsFalse(url.Contains("password=password"));
        }

        [TestMethod]
        public void PrtgUrl_SpecifiedPassHash_Yields_PassHash()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.PassHash] = "password"
            }, false);

            Assert.IsTrue(url.Contains("passhash=password"));
            Assert.IsFalse(url.Contains("password=password"));
        }

        [TestMethod]
        public void PrtgUrl_SpecifiedPassword_Yields_Password()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Password] = "password"
            }, false);

            Assert.IsFalse(url.Contains("passhash=password"));
            Assert.IsTrue(url.Contains("password=password"));
        }

        [TestMethod]
        public void PrtgUrl_MultiParameter_WithoutIEnumerable_Equals_WithIEnumerable()
        {
            var urlWithoutArray = CreateUrl(new Parameters.Parameters
            {
                [Parameter.FilterXyz] = new SearchFilter(Property.Name, "dc1")
            });

            var urlWithArray = CreateUrl(new Parameters.Parameters
            {
                [Parameter.FilterXyz] = new[] {new SearchFilter(Property.Name, "dc1")}
            });

            var expected = "filter_name=dc1";
            Assert.AreEqual(expected, urlWithoutArray, "URL without array was not correct");
            Assert.AreEqual(expected, urlWithArray, "URL with array was not correct");
        }

        [TestMethod]
        public void PrtgUrl_MultiValue_WithoutIEnumerable_Equals_WithIEnumerable()
        {
            var urlWithoutArray = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Columns] = Property.Id
            });

            var urlWithArray = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Columns] = new[] {Property.Id }
            });

            var expected = "columns=objid";
            Assert.AreEqual(expected, urlWithoutArray, "URL without array was not correct");
            Assert.AreEqual(expected, urlWithArray, "URL with array was not correct");
        }

        [TestMethod]
        public void PrtgUrl_CustomParameter_WithoutIEnumerable_Equals_WithIEnumerable()
        {
            var urlWithoutArray = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Custom] = new CustomParameter("foo", "bar")
            });

            var urlWithArray = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Custom] = new[] { new CustomParameter("foo", "bar") }
            });

            var expected = "foo=bar";
            Assert.AreEqual(expected, urlWithoutArray, "URL without array was not correct");
            Assert.AreEqual(expected, urlWithArray, "URL with array was not correct");
        }

        [TestMethod]
        public void PrtgUrl_SearchFilter_FromParameters_With_EnumFlags()
        {
            var flagsParameters = new SensorParameters
            {
                Status = new[] {Status.Paused}
            };

            flagsParameters.GetParameters().Remove(Parameter.Content);
            flagsParameters.GetParameters().Remove(Parameter.Columns);
            flagsParameters.GetParameters().Remove(Parameter.Count);

            var flagsUrl = CreateUrl(flagsParameters);

            var manualParameters = new SensorParameters
            {
                Status = new[]
                {
                    Status.PausedByUser, Status.PausedByDependency, Status.PausedBySchedule, Status.PausedByLicense,
                    Status.PausedUntil
                }
            };

            manualParameters.GetParameters().Remove(Parameter.Content);
            manualParameters.GetParameters().Remove(Parameter.Columns);
            manualParameters.GetParameters().Remove(Parameter.Count);

            var manualUrl = CreateUrl(manualParameters);

            var expected = "filter_status=7&filter_status=8&filter_status=9&filter_status=11&filter_status=12";
            Assert.AreEqual(expected, flagsUrl, "Flags URL was not correct");
            Assert.AreEqual(expected, manualUrl, "Manual URL was not correct");
        }

        [TestMethod]
        public void PrtgUrl_FilterValue_With_EnumFlags()
        {
            //Specifying FilterXyz doesn't actually make sense here (we should be using a SearchFilter) however
            //the point of the test is to validate flag parsing

            var flagsUrl = CreateUrl(new Parameters.Parameters
            {
                [Parameter.FilterXyz] = new SearchFilter(Property.Status, Status.Paused)
            });

            var manualUrl = CreateUrl(new Parameters.Parameters
            {
                [Parameter.FilterXyz] =
                    new[]
                    {
                        Status.PausedByUser, Status.PausedByDependency, Status.PausedBySchedule, Status.PausedByLicense, 
                        Status.PausedUntil
                    }.ToList().Select(s => new SearchFilter(Property.Status, s)).ToArray()
            });

            var expected = "filter_status=7&filter_status=8&filter_status=9&filter_status=11&filter_status=12";
            Assert.AreEqual(expected, flagsUrl, "Flags URL was not correct");
            Assert.AreEqual(expected, manualUrl, "Manual URL was not correct");
        }

        [TestMethod]
        public void PrtgUrl_SearchFilter_With_EnumFlags()
        {
            var flagsUrl = CreateUrl(new Parameters.Parameters
            {
                [Parameter.FilterXyz] = new SearchFilter(Property.Status, new[] { Status.Paused })
            });

            var manualUrl = CreateUrl(new Parameters.Parameters
            {
                [Parameter.FilterXyz] = new SearchFilter(Property.Status,
                    new[]
                    {
                        Status.PausedByUser, Status.PausedByDependency, Status.PausedBySchedule,
                        Status.PausedByLicense, Status.PausedUntil
                    })

            });

            var expected = "filter_status=7&filter_status=8&filter_status=9&filter_status=11&filter_status=12";
            Assert.AreEqual(expected, flagsUrl, "Flags URL was not correct");
            Assert.AreEqual(expected, manualUrl, "Manual URL was not correct");
        }

        [TestMethod]
        public void PrtgUrl_SearchFilter_With_TimeSpan()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.FilterXyz] = new[] {new SearchFilter(Property.UpDuration, new TimeSpan(1, 2, 3))}
            });

            Assert.AreEqual("filter_uptimesince=3723", url);
        }

        [TestMethod]
        public void PrtgUrl_SearchFilter_With_DateTime()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.FilterXyz] = new[] { new SearchFilter(Property.LastUp, new DateTime(2000, 10, 2, 12, 10, 5, DateTimeKind.Utc)) }
            });

            Assert.AreEqual("filter_lastup=36801.5070023148", url);
        }

        [TestMethod]
        public void PrtgUrl_Throws_WhenCustomParameterValueIsWrongType()
        {
            AssertEx.Throws<ArgumentException>(() =>
            {
                var url = CreateUrl(new Parameters.Parameters
                {
                    [Parameter.Custom] = 3
                });
            }, "Expected parameter 'Custom' to contain one or more objects of type 'CustomParameter', however value was of type 'System.Int32'");
        }

        [TestMethod]
        public void PrtgUrl_IgnoresCustomParameterValue_WhenValueIsNull()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Custom] = null
            });

            Assert.AreEqual(string.Empty, url);
        }

        [TestMethod]
        public void PrtgUrl_IgnoresCustomParameterValue_WhenValueIsEmptyList()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Custom] = new CustomParameter[] {}
            });

            Assert.AreEqual(string.Empty, url);
        }

        [TestMethod]
        public void PrtgUrl_MultiValue_CustomParameter_FormatsCorrectly()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Custom] = new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiValue)
            });

            Assert.AreEqual("name=first,second", url);
        }

        [TestMethod]
        public void PrtgUrl_MultiParameter_CustomParameter_FormatsCorrectly()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Custom] = new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiParameter)
            });

            Assert.AreEqual("name=first&name=second", url);
        }

        [TestMethod]
        public void PrtgUrl_MultiValue_CustomParameterList_FormatsCorrectly()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Custom] = new List<CustomParameter>
                {
                    new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiValue),
                    new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiValue)
                }
            });

            Assert.AreEqual("name=first,second&name=first,second", url);
        }

        [TestMethod]
        public void PrtgUrl_MultiParameter_CustomParameterList_FormatsCorrectly()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Custom] = new List<CustomParameter>
                {
                    new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiParameter),
                    new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiParameter)
                }
            });

            Assert.AreEqual("name=first&name=second&name=first&name=second", url);
        }

        [TestMethod]
        public void PrtgUrl_MultiParameter_With_Enum()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Service] = Status.Up
            });

            var expected = "service__check=3";

            Assert.AreEqual(expected, url);
        }

        [TestMethod]
        public void PrtgUrl_MultiParameter_With_EnumFlags()
        {
            var url = CreateUrl(new Parameters.Parameters
            {
                [Parameter.Service] = Status.Paused
            });

            var expected = "service__check=7&service__check=8&service__check=9&service__check=11&service__check=12";

            Assert.AreEqual(expected, url);
        }

        [TestMethod]
        public void PrtgUrl_Throws_UsingList_With_SingleParameter()
        {
            AssertEx.Throws<ArgumentException>(() =>
            {
                var url = CreateUrl(new Parameters.Parameters
                {
                    [Parameter.Name] = new[] {1, 2}
                });
            }, "Parameter 'Name' is of type SingleValue, however a list of elements was specified");
        }

        [TestMethod]
        public void PrtgUrl_Null_SearchFilter()
        {
            var url = CreateUrl(new SensorParameters
            {
                SearchFilter = null,
                Properties = new[] { Property.Name }
            });

            Assert.AreEqual("content=sensors&columns=name&count=*", url);
        }

        [TestMethod]
        public void PrtgUrl_Empty_SearchFilters()
        {
            var url = CreateUrl(new SensorParameters
            {
                SearchFilter = new SearchFilter[] { },
                Properties = new[] { Property.Name }
            });

            Assert.AreEqual("content=sensors&columns=name&count=*", url);
        }

        public static string CreateUrl(Parameters.Parameters parameters, bool truncate = true)
        {
            var url = new PrtgUrl(new ConnectionDetails("prtg.example.com", "username", "password"), XmlFunction.TableData, parameters);

            if (truncate)
            {
                var suffix = "https://prtg.example.com/api/table.xml?";
                var prefix = $"&username=username&passhash=password";
                try
                {
                    Assert.IsTrue(url.Url.StartsWith(suffix), "URL did not start with suffix");
                }
                catch
                {
                    Assert.IsTrue(url.Url.StartsWith(suffix.Substring(0, suffix.Length - 1) + "&"));
                }
                Assert.IsTrue(url.Url.EndsWith(prefix), "URL did not end with prefix");

                var length = url.Url.Length - (suffix.Length + prefix.Length);

                if (length == -1)
                    return string.Empty;

                return url.Url.Substring(suffix.Length, length);
            }

            return url.Url;
        }

        private void Server_Prefix(string server, string prefixedServer)
        {
            var url = new PrtgUrl(new ConnectionDetails(server, "username", "password"), XmlFunction.TableData, new Parameters.Parameters());

            Assert.IsTrue(url.Url.StartsWith(prefixedServer));
        }
    }
}
