using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Request;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class SetObjectPropertyTests : BaseTest
    {
        #region Type Parsing

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Enum_With_Int()
        {
            AssertEx.Throws<ArgumentException>(
                () => SetObjectProperty(ObjectProperty.IntervalErrorMode, 1),
                "'1' is not a valid value for type 'PrtgAPI.IntervalErrorMode'. Please specify one of 'DownImmediately'"
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Enum_With_Enum()
        {
            SetObjectProperty(ObjectProperty.IntervalErrorMode, IntervalErrorMode.DownImmediately, ((int)IntervalErrorMode.DownImmediately).ToString());
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Bool_With_Bool()
        {
            SetObjectProperty(ObjectProperty.InheritInterval, false, "0");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Bool_With_Int()
        {
            SetObjectProperty(ObjectProperty.InheritInterval, true, "1");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Int_With_Enum()
        {
            AssertEx.Throws<InvalidTypeException>(
                () => SetObjectProperty(ObjectProperty.DBPort, Status.Up, "8"),
                "Expected type: 'System.Int32'. Actual type: 'PrtgAPI.Status'"
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Int_With_Int()
        {
            SetObjectProperty(ObjectProperty.DBPort, 8080);
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Double_With_Int()
        {
            SetObjectProperty(ChannelProperty.ScalingDivision, 10);
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Int_With_Double()
        {
            double val = 8080.0;
            SetObjectProperty(ObjectProperty.DBPort, val, "8080");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Int_With_Bool()
        {
            AssertEx.Throws<InvalidTypeException>(
                () => SetObjectProperty(ObjectProperty.DBPort, true, "1"),
                "Expected type: 'System.Int32'. Actual type: 'System.Boolean'"
            );
        }

        #endregion
        #region Normal

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_ReverseDependencyProperty()
        {
            SetObjectProperty(ChannelProperty.ColorMode, AutoMode.Manual, "1");
        }

        [UnitTest]
        [TestMethod]
        public async Task SetObjectProperty_CanExecuteAsync()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ObjectProperty>(ObjectProperty.DBPort, "8080"));

            await client.SetObjectPropertyAsync(1001, ObjectProperty.DBPort, 8080);
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_CanSetLocation()
        {
            SetObjectProperty(ObjectProperty.Location, "23 Fleet Street, Boston", "23 Fleet St, Boston, MA 02113, USA");
        }

        #region Google Location

        [UnitTest]
        [TestMethod]
        public void Location_Google_Deserializes()
        {
            var client = GetLocationClient(RequestVersion.v14_4);

            var result = client.ResolveAddress("Google", CancellationToken.None);

            Assert.AreEqual("23 Fleet St, Boston, MA 02113, USA", result.Address);
            Assert.AreEqual(42.3643847, result.Latitude);
            Assert.AreEqual(-71.0527997, result.Longitude);
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Google_DeserializesAsync()
        {
            var client = GetLocationClient(RequestVersion.v14_4);

            var result = await client.ResolveAddressAsync("Google", CancellationToken.None);

            Assert.AreEqual("23 Fleet St, Boston, MA 02113, USA", result.Address);
            Assert.AreEqual(42.3643847, result.Latitude);
            Assert.AreEqual(-71.0527997, result.Longitude);
        }

        [UnitTest]
        [TestMethod]
        public void Location_Google_CorrectUrl()
        {
            Execute(
                c => c.ResolveAddress("google google", CancellationToken.None),
                "geolocator.htm?cache=false&dom=0&path=google%2Bgoogle&username",
                version: RequestVersion.v14_4
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Google_ResolvesNothing()
        {
            var client = GetLocationClient(RequestVersion.v14_4);

            var location = client.ResolveAddress(null, CancellationToken.None);

            Assert.AreEqual(null, location.ToString());
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Google_ResolvesNothingAsync()
        {
            var client = GetLocationClient(RequestVersion.v14_4);

            var location = await client.ResolveAddressAsync(null, CancellationToken.None);

            Assert.AreEqual(null, location.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Location_Google_FailsToResolve()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoResults), RequestVersion.v14_4);

            AssertEx.Throws<PrtgRequestException>(() => client.ResolveAddress("something", CancellationToken.None), "Could not resolve 'something' to an actual address");
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Google_FailsToResolveAsync()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoResults), RequestVersion.v14_4);

            await AssertEx.ThrowsAsync<PrtgRequestException>(async () => await client.ResolveAddressAsync("something", CancellationToken.None), "Could not resolve 'something' to an actual address");
        }

        [UnitTest]
        [TestMethod]
        public void Location_Google_ProviderUnavailable()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoProvider), RequestVersion.v14_4);

            AssertEx.Throws<PrtgRequestException>(() => client.ResolveAddress("something", CancellationToken.None), "the PRTG map provider is not currently available");
        }

        [UnitTest]
        [TestMethod]
        public void Location_Google_NoAPIKey()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoAPIKey), RequestVersion.v14_4);

            AssertEx.Throws<PrtgRequestException>(() => client.ResolveAddress("google", CancellationToken.None), "Could not resolve 'google' to an actual address: server responded with 'Keyless access to Google Maps Platform is deprecated. Please use an API key with all your API calls to avoid service interruption. For further details please refer to http://g.co/dev/maps-no-account. OVER_QUERY_LIMIT'");
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Google_NoAPIKeyAsync()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoAPIKey), RequestVersion.v14_4);

            await AssertEx.ThrowsAsync<PrtgRequestException>(async () => await client.ResolveAddressAsync("google", CancellationToken.None), "Could not resolve 'google' to an actual address: server responded with 'Keyless access to Google Maps Platform is deprecated. Please use an API key with all your API calls to avoid service interruption. For further details please refer to http://g.co/dev/maps-no-account. OVER_QUERY_LIMIT'");
        }

        #endregion
        #region Here Location

        [UnitTest]
        [TestMethod]
        public void Location_Here_Deserializes()
        {
            var client = GetLocationClient(RequestVersion.v18_1);

            var result = client.ResolveAddress("HERE", CancellationToken.None);

            Assert.AreEqual("100 HERE Lane", result.Address);
            Assert.AreEqual(62.3643847, result.Latitude);
            Assert.AreEqual(-91.0527997, result.Longitude);
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Here_DeserializesAsync()
        {
            var client = GetLocationClient(RequestVersion.v18_1);

            var result = await client.ResolveAddressAsync("HERE", CancellationToken.None);

            Assert.AreEqual("100 HERE Lane", result.Address);
            Assert.AreEqual(62.3643847, result.Latitude);
            Assert.AreEqual(-91.0527997, result.Longitude);
        }

        [UnitTest]
        [TestMethod]
        public void Location_Here_CorrectUrl()
        {
            Execute(
                c => c.ResolveAddress("here here", CancellationToken.None),
                "geolocator.htm?cache=false&dom=2&path=here%2Bhere&username",
                version: RequestVersion.v17_4
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Here_ResolvesNothing()
        {
            var client = GetLocationClient(RequestVersion.v17_4);

            var location = client.ResolveAddress(null, CancellationToken.None);

            Assert.AreEqual(null, location.ToString());
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Here_ResolvesNothingAsync()
        {
            var client = GetLocationClient(RequestVersion.v17_4);

            var location = await client.ResolveAddressAsync(null, CancellationToken.None);

            Assert.AreEqual(null, location.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void Location_Here_FailsToResolve()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoResults), RequestVersion.v18_1);

            AssertEx.Throws<PrtgRequestException>(() => client.ResolveAddress("something", CancellationToken.None), "Could not resolve 'something' to an actual address");
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Here_FailsToResolveAsync()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoResults), RequestVersion.v18_1);

            await AssertEx.ThrowsAsync<PrtgRequestException>(async () => await client.ResolveAddressAsync("something", CancellationToken.None), "Could not resolve 'something' to an actual address");
        }

        [UnitTest]
        [TestMethod]
        public void Location_Here_ProviderUnavailable()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoProvider), RequestVersion.v18_1);

            AssertEx.Throws<PrtgRequestException>(() => client.ResolveAddress("something", CancellationToken.None), "the PRTG map provider is not currently available");
        }

        #endregion
        #region Coordinates Location

        [UnitTest]
        [TestMethod]
        public void Location_Coordinates_Matches_InputString()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            var url = $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username";

            Execute(c =>
            {
                var location = c.ResolveAddress($"{lat}, {lon}", CancellationToken.None);

                Assert.AreEqual(lat, location.Latitude.ToString(), "Latitude was incorrect");
                Assert.AreEqual(lon, location.Longitude.ToString(), "Longitude was incorrect");
                Assert.AreEqual($"{lat}, {lon}", location.Address, "Address was incorrect");

                c.SetObjectProperty(1001, ObjectProperty.Location, location);
            }, url);
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Coordinates_Matches_InputStringAsync()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            var url = $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username";

            await ExecuteAsync(async c =>
            {
                var location = await c.ResolveAddressAsync($"{lat}, {lon}", CancellationToken.None);

                Assert.AreEqual(lat, location.Latitude.ToString(), "Latitude was incorrect");
                Assert.AreEqual(lon, location.Longitude.ToString(), "Longitude was incorrect");
                Assert.AreEqual($"{lat}, {lon}", location.Address, "Address was incorrect");

                await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, location);
            }, url);
        }

        [UnitTest]
        [TestMethod]
        public void Location_Coordinates_Array()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, new[] {40.71455, -74.00714}),
                $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Coordinates_ArrayAsync()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, new[] { 40.71455, -74.00714 }),
                $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Coordinates_Removes_InvalidCharacters()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            Action<string, string, string> validate = (str, expected, message) =>
            {
                var location = client.ResolveAddress(str, CancellationToken.None);

                Assert.AreEqual(expected, location.Address, message);
            };

            var lat = "40.71455";
            var lon = "-74.00714";

            var expectedStr = $"{lat}, {lon}";

            validate($"{lat}\r{lon}",          expectedStr, "\\r");
            validate($"{lat}\n{lon}",          expectedStr, "\\n");
            validate($"{lat}\r\n{lon}",        expectedStr, "\\r\\n");
            validate($"{lat}\r\n\r{lon}",      expectedStr, "\\r\\n\\r");
            validate($"{lat}, {lon} {{blah}}", expectedStr, "{{blah}}");
        }

        [UnitTest]
        [TestMethod]
        public void Location_Coordinates_Truncates_MultiplePeriods()
        {
            var lat = "40.7145.5";
            var lon = "-74.00714";

            var client = Initialize_Client(new MultiTypeResponse());

            var location = client.ResolveAddress($"{lat}, {lon}", CancellationToken.None);

            Assert.AreEqual("40.7145", location.Latitude.ToString(), "Latitude was incorrect");
            Assert.AreEqual(lon, location.Longitude.ToString(), "Longitude was incorrect");
        }

        [UnitTest]
        [TestMethod]
        public void Location_Coordinates_Matches_WithoutComma()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, $"{lat} {lon}"),
                $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Coordinates_Matches_WithoutCommaOrSpace()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, $"{lat}{lon}"),
                $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Coordinates_Ignores_ThreeOrMoreGroups()
        {
            var lat = "40.7145";
            var lon = "-74.00714";
            var other = "101.6262";

            Execute(
                c => c.ResolveAddress($"{lat}, {lon}, {other}", CancellationToken.None),
                new[]
                {
                    UnitRequest.Status(),
                    UnitRequest.Get("api/geolocator.htm?cache=false&dom=0&path=40.7145%2C%2B-74.00714%2C%2B101.6262")
                }
            );
        }

        #endregion
        #region Location Label

        [UnitTest]
        [TestMethod]
        public void Location_Label_NewLine_Coordinates()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\n12.3456, -7.8910"),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Label_NewLine_CoordinatesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\n12.3456, -7.8910"),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Label_CarriageReturn_Coordinates()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\r12.3456, -7.8910"),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Label_CarriageReturn_CoordinatesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\r12.3456, -7.8910"),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Label_NewLineCarriageReturn_Coordinates()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\r\n12.3456, -7.8910"),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Label_NewLineCarriageReturn_CoordinatesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\r\n12.3456, -7.8910"),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Label_NewLine_Address()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\n23 Fleet Street"),
                new[]
                {
                    UnitRequest.Status(),
                    UnitRequest.Get("api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet"),
                    UnitRequest.EditSettings("id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Label_NewLine_AddressAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\n23 Fleet Street"),
                new[]
                {
                    UnitRequest.Status(),
                    UnitRequest.Get("api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet"),
                    UnitRequest.EditSettings("id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Label_CarriageReturn_Address()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\r23 Fleet Street"),
                new[]
                {
                    UnitRequest.Status(),
                    UnitRequest.Get("api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet"),
                    UnitRequest.EditSettings("id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Label_CarriageReturn_AddressAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\r23 Fleet Street"),
                new[]
                {
                    UnitRequest.Status(),
                    UnitRequest.Get("api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet"),
                    UnitRequest.EditSettings("id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Label_NewLineCarriageReturn_Address()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\r\n23 Fleet Street"),
                new[]
                {
                    UnitRequest.Status(),
                    UnitRequest.Get("api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet"),
                    UnitRequest.EditSettings("id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Label_NewLineCarriageReturn_AddressAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\n23 Fleet Street"),
                new[]
                {
                    UnitRequest.Status(),
                    UnitRequest.Get("api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet"),
                    UnitRequest.EditSettings("id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Label_Parameter_Coordinates()
        {
            Execute(
                c => c.SetObjectProperty(
                    1001,
                    new PropertyParameter(ObjectProperty.Location, "12.3456, -7.8910"),
                    new PropertyParameter(ObjectProperty.LocationName, "Headquarters")
                ),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Label_Parameter_CoordinatesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(
                    1001,
                    new PropertyParameter(ObjectProperty.Location, "12.3456, -7.8910"),
                    new PropertyParameter(ObjectProperty.LocationName, "Headquarters")
                ),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Label_Parameter_Address()
        {
            Execute(
                c => c.SetObjectProperty(
                    1001,
                    new PropertyParameter(ObjectProperty.Location, "23 Fleet Street"),
                    new PropertyParameter(ObjectProperty.LocationName, "Headquarters")
                ),
                new[]
                {
                    UnitRequest.Status(),
                    UnitRequest.Get("api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet"),
                    UnitRequest.EditSettings("id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Label_Parameter_AddressAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(
                    1001,
                    new PropertyParameter(ObjectProperty.Location, "23 Fleet Street"),
                    new PropertyParameter(ObjectProperty.LocationName, "Headquarters")
                ),
                new[]
                {
                    UnitRequest.Status(),
                    UnitRequest.Get("api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet"),
                    UnitRequest.EditSettings("id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Label_WithoutLocation()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            AssertEx.Throws<InvalidOperationException>(
                () => client.SetObjectProperty(1001, ObjectProperty.LocationName, "Test"),
                "ObjectProperty 'LocationName' must be used in conjunction with property 'Location'"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Label_WithoutLocationAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.SetObjectPropertyAsync(1001, ObjectProperty.LocationName, "Test"),
                "ObjectProperty 'LocationName' must be used in conjunction with property 'Location'"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Label_Coordinates_EndsWithNewline()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "1.1, 2.2\n"),
                "editsettings?id=1001&location_=1.1%2C+2.2&lonlat_=2.2%2C1.1&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Label_Coordinates_EndsWithNewlineAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "1.1, 2.2\n"),
                "editsettings?id=1001&location_=1.1%2C+2.2&lonlat_=2.2%2C1.1&locationgroup=0&username"
            );
        }

        [UnitTest]
        [TestMethod]
        public void Location_Label_Address_EndsWithNewline()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "23 Fleet Street\n"),
                new[]
                {
                    UnitRequest.Status(),
                    UnitRequest.Get("api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet%0A"),
                    UnitRequest.EditSettings("id=1001&location_=23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Location_Label_Address_EndsWithNewlineAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "23 Fleet Street\n"),
                new[]
                {
                    UnitRequest.Status(),
                    UnitRequest.Get("api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet%0A"),
                    UnitRequest.EditSettings("id=1001&location_=23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0")
                }
            );
        }

        #endregion

        private PrtgClient GetLocationClient(RequestVersion version)
        {
            var client = Initialize_Client(
                new SetObjectPropertyResponse<ObjectProperty>(ObjectProperty.Location, null),
                version
            );

            return client;
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_CanExecute() =>
            Execute(c => c.SetObjectPropertyRaw(1001, "name_", "testName"), "editsettings?id=1001&name_=testName");

        [UnitTest]
        [TestMethod]
        public async Task SetObjectPropertyRaw_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.SetObjectPropertyRawAsync(1001, "name_", "testName"), "editsettings?id=1001&name_=testName");

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_CanExecuteAsync()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ChannelProperty>(ChannelProperty.LimitsEnabled, "1"));

            await client.SetChannelPropertyAsync(1001, 1, ChannelProperty.LimitsEnabled, true);
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_CanNullifyValue()
        {
            SetObjectProperty(ChannelProperty.UpperErrorLimit, null, string.Empty);
        }

        [UnitTest]
        [TestMethod]
        public async Task SetObjectProperty_CanSetLocationAsync()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ObjectProperty>(ObjectProperty.Location, "23 Fleet St, Boston, MA 02113, USA"));

            await client.SetObjectPropertyAsync(1, ObjectProperty.Location, "23 Fleet Street");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Array_WithArray()
        {
            var channels = new[]
            {
                "#1: First Channel",
                "channel(1001,1)",
                "#2: Second Channel",
                "channel(2001,1)"
            };

            SetObjectProperty(ObjectProperty.ChannelDefinition, channels, string.Join("\n", channels));
        }

        private void SetObjectProperty(ObjectProperty property, object value, string expectedSerializedValue = null)
        {
            if (expectedSerializedValue == null)
                expectedSerializedValue = value?.ToString();

            var response = new SetObjectPropertyResponse<ObjectProperty>(property, expectedSerializedValue);

            var client = Initialize_Client(response);

            client.SetObjectProperty(1, property, value);
        }

        private void SetObjectProperty(ChannelProperty property, object value, string expectedSerializedValue = null)
        {
            if (expectedSerializedValue == null)
                expectedSerializedValue = value.ToString();

            var client = Initialize_Client(new SetObjectPropertyResponse<ChannelProperty>(property, expectedSerializedValue));
            client.SetChannelProperty(1, 1, property, value);
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_DecimalPoint_AmericanCulture()
        {
            TestCustomCulture(() => SetObjectProperty(ChannelProperty.ScalingDivision, "1.1", "1.1"), new CultureInfo("en-US"));
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_DecimalPoint_EuropeanCulture()
        {
            TestCustomCulture(() =>
            {
                SetObjectProperty(ChannelProperty.ScalingDivision, "1,1", "1,1");
                SetObjectProperty(ChannelProperty.ScalingDivision, 1.1, "1,1");
            }, new CultureInfo("de-DE"));
        }

        private void TestCustomCulture(Action action, CultureInfo newCulture)
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = newCulture;

                action();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        #endregion
        #region Multiple

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Modifies_MultipleProperties()
        {
            Execute(
                c => c.SetObjectProperty(
                    1001,
                    new PropertyParameter(ObjectProperty.WindowsUserName, "username"),
                    new PropertyParameter(ObjectProperty.WindowsPassword, "password")
                ),
                "id=1001&windowsloginusername_=username&windowsconnection=0&windowsloginpassword_=password"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetObjectProperty_Modifies_MultiplePropertiesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(
                    1001,
                    new PropertyParameter(ObjectProperty.WindowsUserName, "username"),
                    new PropertyParameter(ObjectProperty.WindowsPassword, "password")
                ),
                "id=1001&windowsloginusername_=username&windowsconnection=0&windowsloginpassword_=password"
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Modifies_MultipleProperties()
        {
            var urls = new[]
            {
                UnitRequest.Channels(1001),
                UnitRequest.ChannelProperties(1001, 1),
                UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=20&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
            };

            Execute(c => c.SetChannelProperty(
                1001,
                1,
                new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                new ChannelParameter(ChannelProperty.LowerErrorLimit, 20)
            ), urls);
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_Modifies_MultiplePropertiesAsync()
        {
            var urls = new[]
            {
                UnitRequest.Channels(1001),
                UnitRequest.ChannelProperties(1001, 1),
                UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=20&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
            };

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(
                    1001,
                    1,
                    new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                    new ChannelParameter(ChannelProperty.LowerErrorLimit, 20)
                ),
                urls
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_Modifies_MultipleRawProperties()
        {
            Execute(
                c => c.SetObjectPropertyRaw(
                    1001,
                    new CustomParameter("windowsloginusername_", "username"),
                    new CustomParameter("windowsloginpassword_", "password"),
                    new CustomParameter("windowsconnection", 0)
                ),
                "id=1001&windowsloginusername_=username&windowsloginpassword_=password&windowsconnection=0"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetObjectProperty_Modifies_MultipleRawPropertiesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyRawAsync(
                    1001,
                    new CustomParameter("windowsloginusername_", "username"),
                    new CustomParameter("windowsloginpassword_", "password"),
                    new CustomParameter("windowsconnection", 0)
                ),
                "id=1001&windowsloginusername_=username&windowsloginpassword_=password&windowsconnection=0"
            );
        }

        #endregion
        #region Version Differences
            #region Single

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_SingleValue_OnlyUpperErrorLimit()
        {
            //When we specify an ErrorLimitMessage, we also include our UpperErrorLimit
            var matrix = new[]
            {
                //UpperError/LowerError/UpperWarning/LowerWarning
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_SingleValue_OnlyLowerErrorLimit()
        {
            //When we specify an ErrorLimitMessage, we also include our LowerErrorLimit
            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 1, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitminerror_1=1&limitminerror_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_SingleValue_OnlyUpperWarningLimit()
        {
            //When we specify an ErrorLimitMessage, we also include our UpperWarningLimit
            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 1, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitmaxwarning_1=1&limitmaxwarning_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_SingleValue_OnlyLowerWarningLimit()
        {
            //When we specify an ErrorLimitMessage, we also include our LowerWarningLimit
            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 1}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitminwarning_1=1&limitminwarning_1_factor=0.1")
            );
        }

            #endregion
            #region Multiple

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_MultipleValues_OnlyUpperErrorLimit()
        {
            //When we specify our ErrorLimitMessage, we split the request in two:
            //All of the channels with an UpperErrorLimit of 1 are grouped together,
            //while all of the channels with an UpperErrorLimit of 2 are grouped together

            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {2, null, null, null},
                new int?[] {2, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001,3001&limiterrormsg_1=test&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                UnitRequest.EditSettings("id=2001,3001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=2&limitmaxerror_1_factor=0.1"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1")
            });
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_MultipleValues_OnlyLowerErrorLimit()
        {
            //When we specify our ErrorLimitMessage, we split the request in two:
            //All of the channels with an LowerErrorLimit of 1 are grouped together,
            //while all of the channels with an LowerErrorLimit of 2 are grouped together

            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 2, null, null},
                new int?[] {null, 2, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            //todo: does this actually group together the sensor IDs to execute against for the request?

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001,3001&limiterrormsg_1=test&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                UnitRequest.EditSettings("id=2001,3001&limiterrormsg_1=test&limitmode_1=1&limitminerror_1=2&limitminerror_1_factor=0.1"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitminerror_1=1&limitminerror_1_factor=0.1")
            });
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_MultipleValues_OnlyUpperWarningLimit()
        {
            //When we specify our ErrorLimitMessage, we split the request in two:
            //All of the channels with an UpperWarningLimit of 1 are grouped together,
            //while all of the channels with an UpperWarningLimit of 2 are grouped together

            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 2, null},
                new int?[] {null, null, 2, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001,3001&limiterrormsg_1=test&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                UnitRequest.EditSettings("id=2001,3001&limiterrormsg_1=test&limitmode_1=1&limitmaxwarning_1=2&limitmaxwarning_1_factor=0.1"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxwarning_1=1&limitmaxwarning_1_factor=0.1")
            });
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_MultipleValues_OnlyLowerWarningLimit()
        {
            //When we specify our ErrorLimitMessage, we split the request in two:
            //All of the channels with an LowerWarningLimit of 1 are grouped together,
            //while all of the channels with an LowerWarningLimit of 2 are grouped together

            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 2},
                new int?[] {null, null, null, 2}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001,3001&limiterrormsg_1=test&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                UnitRequest.EditSettings("id=2001,3001&limiterrormsg_1=test&limitmode_1=1&limitminwarning_1=2&limitminwarning_1_factor=0.1"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitminwarning_1=1&limitminwarning_1_factor=0.1")
                
            });
        }

            #endregion
            #region Version Properties

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_VersionProperty_ErrorLimitMessage()
        {
            //Basically the same test as SetChannelProperty_SingleValue_OnlyUpperErrorLimit

            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_VersionProperty_WarningLimitMessage()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.WarningLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limitwarningmsg_1=test&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limitwarningmsg_1=test&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_VersionProperty_LimitsEnabled_True()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)true,
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_VersionProperty_LimitsEnabled_False()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)false,
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limitmode_1=0&limitmaxerror_1=&limitmaxwarning_1=&limitminerror_1=&limitminwarning_1=&limiterrormsg_1=&limitwarningmsg_1=")
            );
            SetChannelProperty(config, RequestVersion.v18_1,
                new[] {UnitRequest.EditSettings("id=1001,2001&limitmode_1=0&limitmaxerror_1=&limitmaxwarning_1=&limitminerror_1=&limitminwarning_1=&limiterrormsg_1=&limitwarningmsg_1=")},
                true
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_VersionProperty_NormalProperty()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.SpikeFilterMax,
                (object)100,
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&spikemax_1=100&spikemode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1,
                new[] {UnitRequest.EditSettings("id=1001,2001&spikemax_1=100&spikemode_1=1")},
                true
            );
        }

            #endregion
            #region Miscellaneous

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ThreeValues()
        {
            /* We have a collection of 6 channels, with a variety of different limit property/value combinations.
             * From these channels, we can see the following:
             * 2 have the same UpperErrorLimit ("2") - 1001,2001
             * 3 have the same LowerErrorLimit ("3") - 4001,5001,6001
             * 1 has the same UpperWarningLimit ("2") - 1001 (defaults to first channel)
             * 1 has the same LowerWarningLimit ("1") - 1001 (defaults to first channel)
             *
             * We then iterate over our analysis, removing channels until all channels have been processed,
             * or the highest limit property analysis found contains no channels (in which case channels must exist
             * that have NO limit property)
             *
             * For example, since LowerErrorLimit has 3 channels, we set those as the first request and remove them from consideration.
             * Next is upperErrorLimit with 2 channels, leaving one channel, which defaults to using its UpperErrorLimit.
             *
             * As a result, we get three requests
             */

            var matrix = new[]
            {
                new int?[] {1,    null, 2,    1}, //1001 - 2
                new int?[] {1,    null, null, 4}, //2001 - 2
                new int?[] {5,    null, 8,    7}, //3001 - 4
                new int?[] {2,    3,    15,   6}, //4001 - 1
                new int?[] {null, 3,    20,   5}, //5001 - 1
                new int?[] {4,    3,    null, 8}  //6001 - 1
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001,3001,4001,5001,6001&limiterrormsg_1=test&limitmode_1=1")
            );
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                UnitRequest.EditSettings("id=4001,5001,6001&limiterrormsg_1=test&limitmode_1=1&limitminerror_1=3&limitminerror_1_factor=0.1"),
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1"),
                UnitRequest.EditSettings("id=3001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=5&limitmaxerror_1_factor=0.1")
            });
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_SomeNull()
        {
            var matrix = new[]
            {
                new int?[] {null, 2, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Action<RequestVersion> action = version =>
                SetChannelProperty(config, version,
                    UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
                );

            action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 1: ");
            builder.Append("Sensor ID 2001 does not have a limit value defined on it. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            AssertEx.Throws<InvalidOperationException>(() => action(RequestVersion.v18_1), builder.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_AllNull()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Action<RequestVersion> action = version =>
                SetChannelProperty(config, version,
                    UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
                );

            action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 1: ");
            builder.Append("Sensor IDs 1001 and 2001 do not have a limit value defined on them. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            AssertEx.Throws<InvalidOperationException>(() => action(RequestVersion.v18_1), builder.ToString());
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_VersionSpecific_ResolvesChannels()
        {
            var addresses = new[]
            {
                UnitRequest.Channels(1001),
                UnitRequest.ChannelProperties(1001, 1),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=hello&limitmode_1=1&limitmaxerror_1=100&limitmaxerror_1_factor=1")
            };

            var property = ChannelProperty.ErrorLimitMessage;
            var val = "hello";

#pragma warning disable 618
            var response = new AddressValidatorResponse(addresses, true);
#pragma warning restore 618

            var client = Initialize_Client(response, RequestVersion.v18_1);

            var versionClient = client.GetVersionClient(new object[] { property });

            versionClient.SetChannelProperty(new[] { 1001 }, 1, new[] { new ChannelParameter(property, val) }, CancellationToken.None);

            response.AssertFinished();
        }

        #endregion
        #region Helpers

        private void SetChannelProperty(Tuple<ChannelProperty, object, int?[][]> config, RequestVersion version, params string[] addresses) =>
            SetChannelProperty(config, version, addresses, false);

        private void SetChannelProperty(Tuple<ChannelProperty, object, int?[][]> config, RequestVersion version, string[] addresses, bool noChannels)
        {
            var property = config.Item1;
            var val = config.Item2;
            var matrix = config.Item3;

            var channels = matrix.Select(CreateChannel).ToArray();

            var parameters = new[] { new ChannelParameter(property, val) };

            try
            {
                SetChannelPropertyInternal(
                    channels,
                    property,
                    version,
                    addresses,
                    c => c.SetChannelProperty(channels, parameters, CancellationToken.None)
                );
            }
            catch (AssertFailedException ex)
            {
                throw new AssertFailedException($"Channel {version} test failed: {ex.Message}", ex);
            }

            addresses = GetSetChannelPropertyManualUrls(channels, version, addresses, noChannels);

            try
            {
                SetChannelPropertyInternal(
                    channels,
                    property,
                    version,
                    addresses,
                    c => c.SetChannelProperty(channels.Select(ch => ch.SensorId).ToArray(), 1, parameters, CancellationToken.None)
                );
            }
            catch (AssertFailedException ex)
            {
                throw new AssertFailedException($"Manual {version} test failed: {ex.Message}", ex);
            }
        }

        private void SetChannelPropertyInternal(Channel[] channels, ChannelProperty property, RequestVersion version, string[] addresses, Action<VersionClient> action)
        {
#pragma warning disable 618
            var response = new AddressValidatorResponse(addresses, true);
            response.ResponseTextManipulator = (r, a) => FixupChannelLimits(r, a, channels);
#pragma warning restore 618

            var client = Initialize_Client(response, version);

            var versionClient = client.GetVersionClient(new object[] { property });

            action(versionClient);

            response.AssertFinished();
        }

        #endregion

        private string[] GetSetChannelPropertyManualUrls(Channel[] channels, RequestVersion version, string[] addresses, bool noChannels)
        {
            var addressList = new List<string>();

            if (version == RequestVersion.v18_1 && !noChannels)
            {
                addressList.AddRange(channels.SelectMany(c =>
                {
                    return new[]
                    {
                        UnitRequest.Channels(c.SensorId),
                        UnitRequest.ChannelProperties(c.SensorId, 1)
                    };
                }));
            }

            addressList.AddRange(addresses);

            return addressList.ToArray();
        }

        private string FixupChannelLimits(string response, string address, Channel[] channels)
        {
            var function = MultiTypeResponse.GetFunction(address);

            if (function != nameof(HtmlFunction.ChannelEdit))
                return response;

            var components = UrlUtilities.CrackUrl(address);

            var channel = channels.First(c => c.SensorId.ToString() == components["id"]);

            response = FixupChannelLimit("limitmaxerror_1", channel.UpperErrorLimit, response);
            response = FixupChannelLimit("limitminerror_1", channel.LowerErrorLimit, response);
            response = FixupChannelLimit("limitmaxwarning_1", channel.UpperWarningLimit, response);
            response = FixupChannelLimit("limitminwarning_1", channel.LowerWarningLimit, response);

            response = FixupChannelLimit("limitmaxerror_1_factor", channel.UpperErrorLimitFactor, response);
            response = FixupChannelLimit("limitminerror_1_factor", channel.LowerErrorLimitFactor, response);
            response = FixupChannelLimit("limitmaxwarning_1_factor", channel.UpperWarningLimitFactor, response);
            response = FixupChannelLimit("limitminwarning_1_factor", channel.LowerWarningLimitFactor, response);

            response = FixupChannelLimit("ref100percent_1_factor", channel.PercentFactor, response);

            return response;
        }

        private string FixupChannelLimit(string property, object value, string response)
        {
            var pattern = $"(<input.+?name=\".+?\".+?value=\")(.*?)(\".*?>)";

            var match = Regex.Matches(response, pattern).Cast<Match>().First(m => m.Value.Contains($"name=\"{property}\""));

            Assert.IsTrue(match.Success, $"Failed to find property '{property}' in the response");

            var withNewValue = Regex.Replace(match.Value, "value=\".*?\"", $"value=\"{value}\"");

            return response.Replace(match.Value, withNewValue);
        }

        #endregion
        #region Version Differences Async
            #region Single

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_SingleValue_OnlyUpperErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_SingleValue_OnlyLowerErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 1, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitminerror_1=1&limitminerror_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_SingleValue_OnlyUpperWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 1, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitmaxwarning_1=1&limitmaxwarning_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_SingleValue_OnlyLowerWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 1}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitminwarning_1=1&limitminwarning_1_factor=0.1")
            );
        }

            #endregion
            #region Multiple

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_MultipleValues_OnlyUpperErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {2, null, null, null},
                new int?[] {2, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001,3001&limiterrormsg_1=test&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                UnitRequest.EditSettings("id=2001,3001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=2&limitmaxerror_1_factor=0.1"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1")
            });
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_MultipleValues_OnlyLowerErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 2, null, null},
                new int?[] {null, 2, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            //todo: does this actually group together the sensor IDs to execute against for the request?

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001,3001&limiterrormsg_1=test&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                UnitRequest.EditSettings("id=2001,3001&limiterrormsg_1=test&limitmode_1=1&limitminerror_1=2&limitminerror_1_factor=0.1"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitminerror_1=1&limitminerror_1_factor=0.1")
            });
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_MultipleValues_OnlyUpperWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 2, null},
                new int?[] {null, null, 2, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001,3001&limiterrormsg_1=test&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                UnitRequest.EditSettings("id=2001,3001&limiterrormsg_1=test&limitmode_1=1&limitmaxwarning_1=2&limitmaxwarning_1_factor=0.1"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxwarning_1=1&limitmaxwarning_1_factor=0.1")
            });
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_MultipleValues_OnlyLowerWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 2},
                new int?[] {null, null, null, 2}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001,3001&limiterrormsg_1=test&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                UnitRequest.EditSettings("id=2001,3001&limiterrormsg_1=test&limitmode_1=1&limitminwarning_1=2&limitminwarning_1_factor=0.1"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitminwarning_1=1&limitminwarning_1_factor=0.1")

            });
        }

            #endregion
            #region Version Properties

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_VersionProperty_ErrorLimitMessageAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_VersionProperty_WarningLimitMessageAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.WarningLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limitwarningmsg_1=test&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limitwarningmsg_1=test&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_VersionProperty_LimitsEnabled_TrueAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)true,
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1,
                UnitRequest.EditSettings("id=1001,2001&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1")
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_VersionProperty_LimitsEnabled_FalseAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)false,
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&limitmode_1=0&limitmaxerror_1=&limitmaxwarning_1=&limitminerror_1=&limitminwarning_1=&limiterrormsg_1=&limitwarningmsg_1=")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1,
                new[] { UnitRequest.EditSettings("id=1001,2001&limitmode_1=0&limitmaxerror_1=&limitmaxwarning_1=&limitminerror_1=&limitminwarning_1=&limiterrormsg_1=&limitwarningmsg_1=") },
                true
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_VersionProperty_NormalPropertyAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.SpikeFilterMax,
                (object)100,
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001&spikemax_1=100&spikemode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1,
                new[] { UnitRequest.EditSettings("id=1001,2001&spikemax_1=100&spikemode_1=1") },
                true
            );
        }

            #endregion
            #region Miscellaneous

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ThreeValuesAsync()
        {
            var matrix = new[]
            {
                new int?[] {1,    null, 2,    1}, //1001 - 2
                new int?[] {1,    null, null, 4}, //2001 - 2
                new int?[] {5,    null, 8,    7}, //3001 - 4
                new int?[] {2,    3,    15,   6}, //4001 - 1
                new int?[] {null, 3,    20,   5}, //5001 - 1
                new int?[] {4,    3,    null, 8}  //6001 - 1
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4,
                UnitRequest.EditSettings("id=1001,2001,3001,4001,5001,6001&limiterrormsg_1=test&limitmode_1=1")
            );
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                UnitRequest.EditSettings("id=4001,5001,6001&limiterrormsg_1=test&limitmode_1=1&limitminerror_1=3&limitminerror_1_factor=0.1"),
                UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=1&limitmaxerror_1_factor=0.1"),
                UnitRequest.EditSettings("id=3001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=5&limitmaxerror_1_factor=0.1")
            });
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_SomeNullAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, 2, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Func<RequestVersion, Task> action = async version =>
                await SetChannelPropertyAsync(config, version,
                    UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
                );

            await action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 1: ");
            builder.Append("Sensor ID 2001 does not have a limit value defined on it. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await action(RequestVersion.v18_1), builder.ToString());
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_AllNullAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Func<RequestVersion, Task> action = async version =>
                await SetChannelPropertyAsync(config, version,
                    UnitRequest.EditSettings("id=1001,2001&limiterrormsg_1=test&limitmode_1=1")
                );

            await action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 1: ");
            builder.Append("Sensor IDs 1001 and 2001 do not have a limit value defined on them. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await action(RequestVersion.v18_1), builder.ToString());
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_VersionSpecific_ResolvesChannelsAsync()
        {
            var addresses = new[]
            {
                UnitRequest.Channels(1001),
                UnitRequest.ChannelProperties(1001, 1),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=hello&limitmode_1=1&limitmaxerror_1=100&limitmaxerror_1_factor=1")
            };

            var property = ChannelProperty.ErrorLimitMessage;
            var val = "hello";

#pragma warning disable 618
            var response = new AddressValidatorResponse(addresses, true);
#pragma warning restore 618

            var client = Initialize_Client(response, RequestVersion.v18_1);

            await client.GetVersionClient(new object[] { property }).SetChannelPropertyAsync(new[] { 1001 }, 1, new[] { new ChannelParameter(property, val) }, CancellationToken.None);

            response.AssertFinished();
        }

            #endregion
            #region Helpers

        private async Task SetChannelPropertyAsync(Tuple<ChannelProperty, object, int?[][]> config, RequestVersion version, params string[] addresses) =>
            await SetChannelPropertyAsync(config, version, addresses, false);

        private async Task SetChannelPropertyAsync(Tuple<ChannelProperty, object, int?[][]> config, RequestVersion version, string[] addresses, bool noChannels)
        {
            var property = config.Item1;
            var val = config.Item2;
            var matrix = config.Item3;

            var channels = matrix.Select(CreateChannel).ToArray();

            var parameters = new[] { new ChannelParameter(property, val) };

            try
            {
                await SetChannelPropertyInternalAsync(
                    channels,
                    property,
                    version,
                    addresses,
                    async c => await c.SetChannelPropertyAsync(channels, parameters, CancellationToken.None)
                );
            }
            catch (AssertFailedException ex)
            {
                throw new AssertFailedException($"Channel {version} test failed: {ex.Message}", ex);
            }

            addresses = GetSetChannelPropertyManualUrls(channels, version, addresses, noChannels);

            try
            {
                await SetChannelPropertyInternalAsync(
                    channels,
                    property,
                    version,
                    addresses,
                    async c => await c.SetChannelPropertyAsync(channels.Select(ch => ch.SensorId).ToArray(), 1, parameters, CancellationToken.None)
                );
            }
            catch (AssertFailedException ex)
            {
                throw new AssertFailedException($"Manual {version} test failed: {ex.Message}", ex);
            }
        }

        private async Task SetChannelPropertyInternalAsync(Channel[] channels, ChannelProperty property, RequestVersion version, string[] addresses, Func<VersionClient, Task> action)
        {
#pragma warning disable 618
            var response = new AddressValidatorResponse(addresses, true);
            response.ResponseTextManipulator = (r, a) => FixupChannelLimits(r, a, channels);
#pragma warning restore 618

            var client = Initialize_Client(response, version);

            var versionClient = client.GetVersionClient(new object[] { property });

            await action(versionClient);

            response.AssertFinished();
        }

            #endregion
        #endregion
        #region Factor
            #region Manual (No Factor)
                #region Manual: Same Channel/Sensor

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Manual_SameID_AndSensor_ExecutesSingleRequest()
        {
            Execute(
                c => c.SetChannelProperty(new[] { 1001, 1001 }, 1, ChannelProperty.SpikeFilterEnabled, true),
                UnitRequest.EditSettings("id=1001&spikemode_1=1")
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_Manual_SameID_AndSensor_ExecutesSingleRequestAsync()
        {
            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(new[] { 1001, 1001 }, 1, ChannelProperty.SpikeFilterEnabled, true),
                UnitRequest.EditSettings("id=1001&spikemode_1=1")
            );
        }

                #endregion
            #endregion
            #region Manual (Factor)
                #region ManualFactor: Same Channel/Sensor

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ManualFactor_SameID_AndSensor_ExecutesSingleRequest()
        {
            Execute(
                c => c.SetChannelProperty(new[] { 1001, 1001 }, 1, ChannelProperty.UpperErrorLimit, 100),
                new[]
                {
                    UnitRequest.Channels(1001),
                    UnitRequest.ChannelProperties(1001, 1),
                    UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=1")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ManualFactor_SameID_AndSensor_ExecutesSingleRequestAsync()
        {
            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(new[] { 1001, 1001 }, 1, ChannelProperty.UpperErrorLimit, 100),
                new[]
                {
                    UnitRequest.Channels(1001),
                    UnitRequest.ChannelProperties(1001, 1),
                    UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=1")
                }
            );
        }

                #endregion
                #region ManualFactor: Same Channel, Different Sensors

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ManualFactor_SameID_DifferentSensors_SingleFactor_ExecutesSingleRequest()
        {
            Execute(
                c => c.SetChannelProperty(new[] { 1001, 1002 }, 1, ChannelProperty.UpperErrorLimit, 100),
                new[]
                {
                    UnitRequest.Channels(1001),
                    UnitRequest.ChannelProperties(1001, 1),
                    UnitRequest.Channels(1002),
                    UnitRequest.ChannelProperties(1002, 1),
                    UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=1")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ManualFactor_SameID_DifferentSensors_SingleFactor_ExecutesSingleRequestAsync()
        {
            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(new[] { 1001, 1002 }, 1, ChannelProperty.UpperErrorLimit, 100),
                new[]
                {
                    UnitRequest.Channels(1001),
                    UnitRequest.ChannelProperties(1001, 1),
                    UnitRequest.Channels(1002),
                    UnitRequest.ChannelProperties(1002, 1),
                    UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=1")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ManualFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleRequest()
        {
            Execute(
                c => c.SetChannelProperty(
                    new[] { 1001, 1002 },
                    1,
                    new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                    new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
                ),
                new[]
                {
                    UnitRequest.Channels(1001),
                    UnitRequest.ChannelProperties(1001, 1),
                    UnitRequest.Channels(1002),
                    UnitRequest.ChannelProperties(1002, 1),
                    UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=50&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ManualFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleRequestAsync()
        {
            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(
                    new[] { 1001, 1002 },
                    1,
                    new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                    new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
                ),
                new[]
                {
                    UnitRequest.Channels(1001),
                    UnitRequest.ChannelProperties(1001, 1),
                    UnitRequest.Channels(1002),
                    UnitRequest.ChannelProperties(1002, 1),
                    UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=50&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
                }
            );
        }

                #endregion
            #endregion
            #region Channel (No Factor)
                #region Channel: Same Channel/Sensor

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequest_v14() =>
            SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequest(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequest_v18() =>
            SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequest(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequestAsync_v14() =>
            await SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequestAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequestAsync_v18() =>
            await SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequestAsync(RequestVersion.v18_1);

        private void SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequest(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1001,       1,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.SpikeFilterEnabled, true),
                UnitRequest.EditSettings("id=1001&spikemode_1=1"),
                version: version
            );
        }

        private async Task SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequestAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1001,       1,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.SpikeFilterEnabled, true),
                UnitRequest.EditSettings("id=1001&spikemode_1=1"),
                version: version
            );
        }

                #endregion
                #region Channel: Different Channels, Same Sensor

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequest_v14() =>
            SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequest(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequest_v18() =>
            SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequest(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequestAsync_v14() =>
            await SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequestAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequestAsync_v18() =>
            await SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequestAsync(RequestVersion.v18_1);

        private void SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequest(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  null},
                new double?[] {1001,       2,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.SpikeFilterEnabled, true),
                UnitRequest.EditSettings("id=1001&spikemode_1=1&spikemode_2=1"),
                version: version
            );
        }

        private async Task SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequestAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  null},
                new double?[] {1001,       2,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.SpikeFilterEnabled, true),
                UnitRequest.EditSettings("id=1001&spikemode_1=1&spikemode_2=1"),
                version: version
            );
        }

                #endregion
                #region Channel: Same Channel, Different Sensors

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequest_v14() =>
            SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequest(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequest_v18() =>
            SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequest(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequestAsync_v14() =>
            await SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequestAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequestAsync_v18() =>
            await SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequestAsync(RequestVersion.v18_1);

        private void SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequest(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.SpikeFilterEnabled, true),
                UnitRequest.EditSettings("id=1001,1002&spikemode_1=1"),
                version: version
            );
        }

        private async Task SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequestAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.SpikeFilterEnabled, true),
                UnitRequest.EditSettings("id=1001,1002&spikemode_1=1"),
                version: version
            );
        }

                #endregion
                #region Channel: Different Channels, Different Sensors

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequests_v14() =>
            SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequests(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequests_v18() =>
            SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequests(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync_v14() =>
            await SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync_v18() =>
            await SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync(RequestVersion.v18_1);

        private void SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequests(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       2,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.SpikeFilterEnabled, true),
                new[]
                {
                    UnitRequest.EditSettings("id=1001&spikemode_1=1"),
                    UnitRequest.EditSettings("id=1002&spikemode_2=1")
                },
                version: version
            );
        }

        private async Task SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       2,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.SpikeFilterEnabled, true),
                new[]
                {
                    UnitRequest.EditSettings("id=1001&spikemode_1=1"),
                    UnitRequest.EditSettings("id=1002&spikemode_2=1")
                },
                version: version
            );
        }

                #endregion
            #endregion
            #region Channel (Factor)
                #region ChannelFactor: Same Channel/Sensor

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequest_v14() =>
            SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequest(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequest_v18() =>
            SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequest(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequestAsync_v14() =>
            await SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequestAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequestAsync_v18() =>
            await SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequestAsync(RequestVersion.v18_1);

        private void SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequest(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1001,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.UpperErrorLimit, 100),
                UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.1"),
                version: version
            );
        }

        private async Task SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequestAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1001,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.UpperErrorLimit, 100),
                UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.1"),
                version: version
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_SameSensor_NeedsLimit()
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1001,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            channels[0].UpperErrorLimit = 50;
            channels[1].LowerErrorLimit = 25;

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.ErrorLimitMessage, "test"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=50&limitmaxerror_1_factor=0.1"),
                version: RequestVersion.v18_1
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_SameSensor_NeedsLimitAsync()
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1001,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            channels[0].UpperErrorLimit = 50;
            channels[1].LowerErrorLimit = 25;

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.ErrorLimitMessage, "test"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=50&limitmaxerror_1_factor=0.1"),
                version: RequestVersion.v18_1
            );
        }

                #endregion
                #region ChannelFactor: Different Channels, Same Sensor

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequest_v14() =>
            SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequest(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequest_v18() =>
            SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequest(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequestAsync_v14() =>
            await SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequestAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequestAsync_v18() =>
            await SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequestAsync(RequestVersion.v18_1);

        private void SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequest(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1001,       2,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.UpperErrorLimit, 100),
                UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.1&limitmaxerror_2=100&limitmode_2=1&limitmaxerror_2_factor=0.2"),
                version: version
            );
        }

        private async Task SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequestAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1001,       2,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.UpperErrorLimit, 100),
                UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.1&limitmaxerror_2=100&limitmode_2=1&limitmaxerror_2_factor=0.2"),
                version: version
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_NeedsLimit()
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1001,       2,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            channels[0].UpperErrorLimit = 50;
            channels[1].LowerErrorLimit = 25;

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.ErrorLimitMessage, "test"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=50&limitmaxerror_1_factor=0.1&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=25&limitminerror_2_factor=0.2"),
                version: RequestVersion.v18_1
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_NeedsLimitAsync()
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1001,       2,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            channels[0].UpperErrorLimit = 50;
            channels[1].LowerErrorLimit = 25;

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.ErrorLimitMessage, "test"),
                UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=50&limitmaxerror_1_factor=0.1&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=25&limitminerror_2_factor=0.2"),
                version: RequestVersion.v18_1
            );
        }

                #endregion
                #region ChannelFactor: Same Channel, Different Sensors
                    #region Single Factor / Single Factor Property

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactor_v14() =>
            SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactor(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactor_v18() =>
            SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactor(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactorAsync_v14() =>
            await SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactorAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactorAsync_v18() =>
            await SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactorAsync(RequestVersion.v18_1);

        private void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactor(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.UpperErrorLimit, 100),
                UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.1"),
                version: version
            );
        }

        private async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactorAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.UpperErrorLimit, 100),
                UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.1"),
                version: version
            );
        }

                    #endregion
                    #region Single Factor / Multiple Factor Properties

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactor_v14() =>
            SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactor(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactor_v18() =>
            SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactor(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactorAsync_v14() =>
            await SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactorAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactorAsync_v18() =>
            await SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactorAsync(RequestVersion.v18_1);

        private void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactor(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(
                    channels,
                    new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                    new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
                ),
                UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=50&limitmaxerror_1_factor=0.1&limitminerror_1_factor=0.1"),
                version: version
            );
        }

        private async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactorAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.1}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(
                    channels,
                    new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                    new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
                ),
                UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=50&limitmaxerror_1_factor=0.1&limitminerror_1_factor=0.1"),
                version: version
            );
        }

                    #endregion
                    #region Multiple Factors / Single Factor Property

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequests_v14() =>
            SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequests(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequests_v18() =>
            SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequests(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequestsAsync_v14() =>
            await SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequestsAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequestsAsync_v18() =>
            await SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequestsAsync(RequestVersion.v18_1);

        private void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequests(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.UpperErrorLimit, 100),
                new[]
                {
                    UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.1"),
                    UnitRequest.EditSettings("id=1002&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.2")
                },
                version: version
            );
        }

        private async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequestsAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.UpperErrorLimit, 100),
                new[]
                {
                    UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.1"),
                    UnitRequest.EditSettings("id=1002&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.2")
                },
                version: version
            );
        }

                    #endregion
                    #region Multiple Factors / Multiple Factor Properties

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequests_v14() =>
            SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequests(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequests_v18() =>
            SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequests(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequestsAsync_v14() =>
            await SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequestsAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequestsAsync_v18() =>
            await SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequestsAsync(RequestVersion.v18_1);

        private void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequests(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(
                    channels,
                    new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                    new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
                ),
                new[]
                {
                    UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=50&limitmaxerror_1_factor=0.1&limitminerror_1_factor=0.1"),
                    UnitRequest.EditSettings("id=1002&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=50&limitmaxerror_1_factor=0.2&limitminerror_1_factor=0.2")
                },
                version: version
            );
        }

        private async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequestsAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(
                    channels,
                    new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                    new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
                ),
                new[]
                {
                    UnitRequest.EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=50&limitmaxerror_1_factor=0.1&limitminerror_1_factor=0.1"),
                    UnitRequest.EditSettings("id=1002&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=50&limitmaxerror_1_factor=0.2&limitminerror_1_factor=0.2")
                },
                version: version
            );
        }

                    #endregion
                    #region Multiple Factors / ValueNullOrEmpty

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequest_v14() =>
            SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequest(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequest_v18() =>
            SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequest(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequestAsync_v14() =>
            await SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequestAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequestAsync_v18() =>
            await SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequestAsync(RequestVersion.v18_1);

        private void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequest(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.UpperErrorLimit, null),
                new[]
                {
                    UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=&limitmode_1=1"),
                },
                version: version
            );
        }

        private async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequestAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.UpperErrorLimit, null),
                new[]
                {
                    UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=&limitmode_1=1"),
                },
                version: version
            );
        }

                    #endregion

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_SameID_DifferentSensors_NeedsLimit()
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            channels[0].UpperErrorLimit = 50;
            channels[1].LowerErrorLimit = 25;

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.ErrorLimitMessage, "test"),
                new[]
                {
                    UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=50&limitmaxerror_1_factor=0.1"),
                    UnitRequest.EditSettings("id=1002&limiterrormsg_1=test&limitmode_1=1&limitminerror_1=25&limitminerror_1_factor=0.2")
                },
                version: RequestVersion.v18_1
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_SameID_DifferentSensors_NeedsLimitAsync()
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            channels[0].UpperErrorLimit = 50;
            channels[1].LowerErrorLimit = 25;

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.ErrorLimitMessage, "test"),
                new[]
                {
                    UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=50&limitmaxerror_1_factor=0.1"),
                    UnitRequest.EditSettings("id=1002&limiterrormsg_1=test&limitmode_1=1&limitminerror_1=25&limitminerror_1_factor=0.2")
                },
                version: RequestVersion.v18_1
            );
        }

                #endregion
                #region ChannelFactor: Different Channels, Different Sensors

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequests_v14() =>
            SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequests(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequests_v18() =>
            SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequests(RequestVersion.v18_1);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync_v14() =>
            await SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync(RequestVersion.v14_4);

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync_v18() =>
            await SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync(RequestVersion.v14_4);

        private void SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequests(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.1},
                new double?[] {1003,       2,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.UpperErrorLimit, 100),
                new[]
                {
                    UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.1"),
                    UnitRequest.EditSettings("id=1003&limitmaxerror_2=100&limitmode_2=1&limitmaxerror_2_factor=0.2")
                },
                version: version
            );
        }

        private async Task SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync(RequestVersion version)
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       1,  0.1},
                new double?[] {1003,       2,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.UpperErrorLimit, 100),
                new[]
                {
                    UnitRequest.EditSettings("id=1001,1002&limitmaxerror_1=100&limitmode_1=1&limitmaxerror_1_factor=0.1"),
                    UnitRequest.EditSettings("id=1003&limitmaxerror_2=100&limitmode_2=1&limitmaxerror_2_factor=0.2")
                },
                version: version
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_NeedsLimit()
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       2,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            channels[0].UpperErrorLimit = 50;
            channels[1].LowerErrorLimit = 25;

            Execute(
                c => c.SetChannelProperty(channels, ChannelProperty.ErrorLimitMessage, "test"),
                new[]
                {
                    UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=50&limitmaxerror_1_factor=0.1"),
                    UnitRequest.EditSettings("id=1002&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=25&limitminerror_2_factor=0.2")
                },
                version: RequestVersion.v18_1
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_NeedsLimitAsync()
        {
            var matrix = new[]
            {
                //SensorId, Id, Factor
                new double?[] {1001,       1,  0.1},
                new double?[] {1002,       2,  0.2}
            };

            var channels = GetFactorChannels(matrix);

            channels[0].UpperErrorLimit = 50;
            channels[1].LowerErrorLimit = 25;

            await ExecuteAsync(
                async c => await c.SetChannelPropertyAsync(channels, ChannelProperty.ErrorLimitMessage, "test"),
                new[]
                {
                    UnitRequest.EditSettings("id=1001&limiterrormsg_1=test&limitmode_1=1&limitmaxerror_1=50&limitmaxerror_1_factor=0.1"),
                    UnitRequest.EditSettings("id=1002&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=25&limitminerror_2_factor=0.2")
                },
                version: RequestVersion.v18_1
            );
        }

                #endregion
            #endregion

        private Channel[] GetFactorChannels(double?[][] matrix)
        {
            var list = new List<Channel>();

            foreach (var channel in matrix)
            {
                var c = new Channel
                {
                    SensorId = (int)channel[0].Value,
                    Id = (int)channel[1].Value
                };

                if (channel[2] != null)
                    SetFactors(c, channel[2]);

                list.Add(c);
            }

            return list.ToArray();
        }

        private Channel CreateChannel(int?[] limits, int i)
        {
            var channel = new Channel
            {
                Id = 1,
                SensorId = 1001 + i * 1000,
                UpperErrorLimit = limits[0],
                LowerErrorLimit = limits[1],
                UpperWarningLimit = limits[2],
                LowerWarningLimit = limits[3],
            };

            SetFactors(channel, 0.1);

            return channel;
        }

        private void SetFactors(Channel channel, double? value)
        {
            var factors = typeof(Channel).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Where(p => p.Name.EndsWith("Factor") && p.Name != nameof(Channel.Factor));

            foreach (var property in factors)
            {
                property.SetValue(channel, value);
            }
        }

        #endregion

        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_ExeFile_IncludesSecondaryProperty()
        {
            Execute(
                c => c.SetObjectProperty(
                    1001,
                    ObjectProperty.ExeFile,
                    "test.ps1"
                ),
                UnitRequest.EditSettings("id=1001&exefile_=test.ps1%7Ctest.ps1%7C%7C&exefilelabel_=test.ps1%7Ctest.ps1%7C%7C")
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Throws_ResolvingNonExistentChannel()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            AssertEx.Throws<InvalidOperationException>(
                () => client.SetChannelProperty(1001, 9, ChannelProperty.UpperErrorLimit, 1),
                "Channel ID '9' does not exist on sensor ID '1001'."
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetChannelProperty_Throws_ResolvingNonExistentChannelAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.SetChannelPropertyAsync(1001, 9, ChannelProperty.UpperErrorLimit, 1),
                "Channel ID '9' does not exist on sensor ID '1001'."
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Ignores_ForeignNumberString_AmericanCulture()
        {
            TestCustomCulture(() =>
            {
                //Normal
                TestFloatEncoding(0.1, "0.1");

                //Foreign
                TestFloatEncoding("0,1", "0%2C1");
            }, new CultureInfo("en-US"));
        }

        [UnitTest]
        [TestMethod]
        public void SetChannelProperty_Ignores_ForeignNumberString_EuropeanCulture()
        {
            TestCustomCulture(() =>
            {
                //Normal
                TestFloatEncoding(0.1, "0%2C1");

                //Foreign
                TestFloatEncoding("0.1", "0.1");
                
            }, new CultureInfo("de-DE"));
        }

        private void TestFloatEncoding(object val, string encoded)
        {
            Execute(
                c => c.SetChannelProperty(
                    1001,
                    1,
                    ChannelProperty.UpperErrorLimit,
                    val
                ),
                new[]
                {
                    UnitRequest.Channels(1001),
                    UnitRequest.ChannelProperties(1001, 1),
                    UnitRequest.EditSettings($"id=1001&limitmaxerror_1={encoded}&limitmode_1=1&limitmaxerror_1_factor=1")
                }
            );
        }
    }
}
