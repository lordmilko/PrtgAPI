using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Request;

namespace PrtgAPI.Tests.UnitTests.Infrastructure
{
    [TestClass]
    public class PrtgRequestMessageTests
    {
        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_Server_Prefix_Http()
        {
            Server_Prefix("http://prtg.example.com", "http://prtg.example.com");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_Server_Prefix_Https()
        {
            Server_Prefix("https://prtg.example.com", "https://prtg.example.com");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_Server_Prefix_None()
        {
            Server_Prefix("prtg.example.com", "https://prtg.example.com");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_SpecifiedNoPass_Yields_PassHash()
        {
            var url = CreateUrl(new BaseParameters(), false);

            Assert.IsTrue(url.Contains("passhash=12345678"));
            Assert.IsFalse(url.Contains("password=12345678"));
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_SpecifiedPassHash_Yields_PassHash()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.PassHash] = "password"
            }, false);

            Assert.IsTrue(url.Contains("passhash=password"));
            Assert.IsFalse(url.Contains("password=password"));
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_SpecifiedPassword_Yields_Password()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.Password] = "password"
            }, false);

            Assert.IsFalse(url.Contains("passhash=password"));
            Assert.IsTrue(url.Contains("password=password"));
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_MultiParameter_WithoutIEnumerable_Equals_WithIEnumerable()
        {
            var urlWithoutArray = CreateUrl(new BaseParameters
            {
                [Parameter.FilterXyz] = new SearchFilter(Property.Name, "dc1")
            });

            var urlWithArray = CreateUrl(new BaseParameters
            {
                [Parameter.FilterXyz] = new[] {new SearchFilter(Property.Name, "dc1")}
            });

            var expected = "filter_name=dc1";
            Assert.AreEqual(expected, urlWithoutArray, "URL without array was not correct");
            Assert.AreEqual(expected, urlWithArray, "URL with array was not correct");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_MultiValue_WithoutIEnumerable_Equals_WithIEnumerable()
        {
            var urlWithoutArray = CreateUrl(new BaseParameters
            {
                [Parameter.Columns] = Property.Id
            });

            var urlWithArray = CreateUrl(new BaseParameters
            {
                [Parameter.Columns] = new[] {Property.Id }
            });

            var expected = "columns=objid";
            Assert.AreEqual(expected, urlWithoutArray, "URL without array was not correct");
            Assert.AreEqual(expected, urlWithArray, "URL with array was not correct");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_CustomParameter_WithoutIEnumerable_Equals_WithIEnumerable()
        {
            var urlWithoutArray = CreateUrl(new BaseParameters
            {
                [Parameter.Custom] = new CustomParameter("foo", "bar")
            });

            var urlWithArray = CreateUrl(new BaseParameters
            {
                [Parameter.Custom] = new[] { new CustomParameter("foo", "bar") }
            });

            var expected = "foo=bar";
            Assert.AreEqual(expected, urlWithoutArray, "URL without array was not correct");
            Assert.AreEqual(expected, urlWithArray, "URL with array was not correct");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_SearchFilter_FromParameters_With_EnumFlags()
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

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_FilterValue_With_EnumFlags()
        {
            //Specifying FilterXyz doesn't actually make sense here (we should be using a SearchFilter) however
            //the point of the test is to validate flag parsing

            var flagsUrl = CreateUrl(new BaseParameters
            {
                [Parameter.FilterXyz] = new SearchFilter(Property.Status, Status.Paused)
            });

            var manualUrl = CreateUrl(new BaseParameters
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

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_SearchFilter_With_EnumFlags()
        {
            var flagsUrl = CreateUrl(new BaseParameters
            {
                [Parameter.FilterXyz] = new SearchFilter(Property.Status, new[] { Status.Paused })
            });

            var manualUrl = CreateUrl(new BaseParameters
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

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_SearchFilter_With_TimeSpan()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.FilterXyz] = new[] {new SearchFilter(Property.UpDuration, new TimeSpan(1, 2, 3))}
            });

            Assert.AreEqual("filter_uptimesince=000000000003723", url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_Parameter_WithBool()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.Action] = true
            });

            Assert.AreEqual("action=1", url);

            url = CreateUrl(new BaseParameters
            {
                [Parameter.Action] = false
            });

            Assert.AreEqual("action=0", url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_SearchFilter_With_DateTime()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.FilterXyz] = new[] { new SearchFilter(Property.LastUp, new DateTime(2000, 10, 2, 12, 10, 5, DateTimeKind.Utc)) }
            });

            Assert.AreEqual("filter_lastup=36801.5070023148", url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_Throws_WhenCustomParameterValueIsWrongType()
        {
            AssertEx.Throws<ArgumentException>(() =>
            {
                var url = CreateUrl(new BaseParameters
                {
                    [Parameter.Custom] = 3
                });
            }, "Expected parameter 'Custom' to contain one or more objects of type 'CustomParameter', however value was of type 'System.Int32'");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_IgnoresCustomParameterValue_WhenValueIsNull()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.Custom] = null
            });

            Assert.AreEqual(string.Empty, url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_IgnoresCustomParameterValue_WhenValueIsEmptyList()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.Custom] = new CustomParameter[] {}
            });

            Assert.AreEqual(string.Empty, url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_MultiValue_CustomParameter_FormatsCorrectly()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.Custom] = new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiValue)
            });

            Assert.AreEqual("name=first,second", url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_MultiParameter_CustomParameter_FormatsCorrectly()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.Custom] = new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiParameter)
            });

            Assert.AreEqual("name=first&name=second", url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_MultiValue_CustomParameterList_FormatsCorrectly()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.Custom] = new List<CustomParameter>
                {
                    new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiValue),
                    new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiValue)
                }
            });

            Assert.AreEqual("name=first,second&name=first,second", url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_MultiParameter_CustomParameterList_FormatsCorrectly()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.Custom] = new List<CustomParameter>
                {
                    new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiParameter),
                    new CustomParameter("name", new[] { "first", "second" }, ParameterType.MultiParameter)
                }
            });

            Assert.AreEqual("name=first&name=second&name=first&name=second", url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_MultiParameter_With_Enum()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.Service] = Status.Up
            });

            var expected = "service__check=3";

            Assert.AreEqual(expected, url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_MultiParameter_With_EnumFlags()
        {
            var url = CreateUrl(new BaseParameters
            {
                [Parameter.Service] = Status.Paused
            });

            var expected = "service__check=7&service__check=8&service__check=9&service__check=11&service__check=12";

            Assert.AreEqual(expected, url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_Throws_UsingList_With_SingleParameter()
        {
            AssertEx.Throws<ArgumentException>(() =>
            {
                var url = CreateUrl(new BaseParameters
                {
                    [Parameter.Name] = new[] {1, 2}
                });
            }, "Parameter 'Name' is of type SingleValue, however a list of elements was specified");
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_Null_SearchFilter()
        {
            var url = CreateUrl(new SensorParameters
            {
                SearchFilters = null,
                Properties = new[] { Property.Name }
            });

            Assert.AreEqual("content=sensors&columns=name&count=*", url);
        }

        [UnitTest]
        [TestMethod]
        public void PrtgRequestMessage_Empty_SearchFilters()
        {
            var url = CreateUrl(new SensorParameters
            {
                SearchFilters = new List<SearchFilter>(),
                Properties = new[] { Property.Name }
            });

            Assert.AreEqual("content=sensors&columns=name&count=*", url);
        }

        public static string CreateUrl(IParameters parameters, bool truncate = true)
        {
            var request = new PrtgRequestMessage(new ConnectionDetails("prtg.example.com", "username", "12345678"), XmlFunction.TableData, parameters);

            if (truncate)
            {
                var suffix = "https://prtg.example.com/api/table.xml?";
                var prefix = $"&username=username&passhash=12345678";
                try
                {
                    Assert.IsTrue(request.Url.StartsWith(suffix), "URL did not start with suffix");
                }
                catch
                {
                    Assert.IsTrue(request.Url.StartsWith(suffix.Substring(0, suffix.Length - 1) + "&"));
                }
                Assert.IsTrue(request.Url.EndsWith(prefix), "URL did not end with prefix");

                var length = request.Url.Length - (suffix.Length + prefix.Length);

                if (length == -1)
                    return string.Empty;

                return request.Url.Substring(suffix.Length, length);
            }

            return request.Url;
        }

        private void Server_Prefix(string server, string prefixedServer)
        {
            var request = new PrtgRequestMessage(new ConnectionDetails(server, "username", "password"), XmlFunction.TableData, new BaseParameters());

            Assert.IsTrue(request.Url.StartsWith(prefixedServer));
        }
    }
}
