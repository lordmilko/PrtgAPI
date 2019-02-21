using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Reflection;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Request;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.ObjectData.Query;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.Serialization;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.Infrastructure
{
    [TestClass]
    public class SerializationTests
    {
        private string ExceptionException(string value, string type, string message)
        {
            return $"An error occurred while attempting to deserialize an object of type '{type}', possibly caused by the following XML: '<property>{value}</property>'. {message}";
        }

        private string NullException(string type, string property = "property")
        {
            return $"An error occurred while attempting to deserialize XML element '{property}': cannot assign 'null' to value type '{type}'.";
        }

        private string EnumException(string value, string type)
        {
            return $"Could not deserialize value '{value}' as it is not a valid member of type '{type}'. Could not process XML '<property>{value}</property>'";
        }

        #region Engine
        #region String

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_String_Normal() => DeserializeElementDummy("test", "test");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_String_Null() => DeserializeElementDummy<string>(null, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_String_EmptyString() => DeserializeElementDummy<string>(string.Empty, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_String_Missing() => DeserializeElementDummy<string>(null, null, false);

        #endregion
        #region Integer

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Integer_Normal() => DeserializeElementDummy("3", 3);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Integer_Double() => DeserializeElementInvalid<int, XmlDeserializationException>("4.5", ExceptionException("4.5", "Int32", "Input string was not in a correct format."));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Integer_Invalid() => DeserializeElementInvalid<int, XmlDeserializationException>("banana", ExceptionException("banana", "Int32", "Input string was not in a correct format."));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Integer_Null() => DeserializeElementInvalid<int, XmlDeserializationException>(null, NullException("Int32"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Integer_EmptyString() => DeserializeElementInvalid<int, XmlDeserializationException>(string.Empty, NullException("Int32"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Integer_Missing() => DeserializeElementInvalid<int, XmlDeserializationException>(null, NullException("Int32"), false);

        #endregion
        #region Nullable Integer

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableInteger_Normal() => DeserializeElementDummy<int?>("3", 3);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableInteger_Double() => DeserializeElementInvalid<int?, XmlDeserializationException>("4.5", ExceptionException("4.5", "Int32", "Input string was not in a correct format."));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableInteger_Invalid() => DeserializeElementInvalid<int?, XmlDeserializationException>("banana", ExceptionException("banana", "Int32", "Input string was not in a correct format."));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableInteger_Null() => DeserializeElementDummy<int?>(null, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableInteger_EmptyString() => DeserializeElementDummy<int?>(string.Empty, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableInteger_Missing() => DeserializeElementDummy<int?>(null, null, false);

        #endregion
        #region Double

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Double_Normal() => DeserializeElementDummy("4.5", 4.5);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Double_Integer() => DeserializeElementDummy<double>("4", 4);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Double_Invalid() => DeserializeElementInvalid<double, XmlDeserializationException>("banana", ExceptionException("banana", "Double", "Input string was not in a correct format"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Double_Null() => DeserializeElementInvalid<double, XmlDeserializationException>(null, NullException("Double"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Double_EmptyString() => DeserializeElementInvalid<double, XmlDeserializationException>(string.Empty, NullException("Double"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Double_Missing() => DeserializeElementInvalid<double, XmlDeserializationException>(null, NullException("Double"), false);

        #endregion
        #region Nullable Double

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableDouble_Normal() => DeserializeElementDummy<double?>("4.5", 4.5);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableDouble_Integer() => DeserializeElementDummy<double?>("4", 4);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableDouble_Invalid() => DeserializeElementInvalid<double?, XmlDeserializationException>("banana", ExceptionException("banana", "Double", "Input string was not in a correct format."));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableDouble_Null() => DeserializeElementDummy<double?>(null, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableDouble_EmptyString() => DeserializeElementDummy<double?>(string.Empty, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableDouble_Missing() => DeserializeElementDummy<double?>(null, null, false);

        #endregion
        #region Bool

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Bool_True() => DeserializeElementDummy("1", true);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Bool_False() => DeserializeElementDummy("0", false);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Bool_NegativeTrue() => DeserializeElementDummy("-1", true);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Bool_Invalid() =>
            DeserializeElementInvalid<bool, XmlDeserializationException>("banana", ExceptionException("banana", "Boolean", "The string 'banana' is not a valid Boolean value."));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Bool_EmptyString() => DeserializeElementInvalid<bool, XmlDeserializationException>(string.Empty, NullException("Boolean"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Bool_Null() => DeserializeElementInvalid<bool, XmlDeserializationException>(null, NullException("Boolean"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Bool_Missing() => DeserializeElementInvalid<bool, XmlDeserializationException>(null, NullException("Boolean"), false);

        #endregion
        #region Nullable Bool

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableBool_True() => DeserializeElementDummy<bool?>("1", true);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableBool_False() => DeserializeElementDummy<bool?>("0", false);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableBool_NegativeTrue() => DeserializeElementDummy("-1", true);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableBool_Invalid() =>
            DeserializeElementInvalid<bool?, XmlDeserializationException>("banana", ExceptionException("banana", "Boolean", "The string 'banana' is not a valid Boolean value."));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableBool_Null() => DeserializeElementDummy<bool?>(null, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableBool_EmptyString() => DeserializeElementDummy<bool?>(string.Empty, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableBool_Missing() => DeserializeElementDummy<bool?>(null, null, false);

        #endregion
        #region DateTime

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_DateTime_Normal() =>
            DeserializeElementDummy(TypeHelpers.ConvertToPrtgDateTime(Time.Today).ToString(), Time.Today.ToLocalTime());

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_DateTime_Invalid() =>
            DeserializeElementInvalid<DateTime, XmlDeserializationException>("banana", ExceptionException("banana", "DateTime", "Input string was not in a correct format."));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_DateTime_Null() => DeserializeElementInvalid<DateTime, XmlDeserializationException>(null, NullException("DateTime"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_DateTime_EmptyString() => DeserializeElementInvalid<DateTime, XmlDeserializationException>(string.Empty, NullException("DateTime"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_DateTime_Missing() => DeserializeElementInvalid<DateTime, XmlDeserializationException>(null, NullException("DateTime"), false);

        #endregion
        #region Nullable DateTime

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableDateTime_Normal() =>
            DeserializeElementDummy<DateTime?>(TypeHelpers.ConvertToPrtgDateTime(Time.Today).ToString(), Time.Today.ToLocalTime());

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableDateTime_Invalid() =>
            DeserializeElementInvalid<DateTime?, XmlDeserializationException>("banana", ExceptionException("banana", "DateTime", "Input string was not in a correct format."));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableDateTime_Null() => DeserializeElementDummy<DateTime?>(null, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableDateTime_EmptyString() => DeserializeElementDummy<DateTime?>(string.Empty, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableDateTime_Missing() => DeserializeElementDummy<DateTime?>(null, null);

        #endregion
        #region TimeSpan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_TimeSpan_Normal() => DeserializeElementDummy("60", TimeSpan.FromSeconds(60));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_TimeSpan_Invalid() => DeserializeElementInvalid<TimeSpan, XmlDeserializationException>("banana", "Input string was not in a correct format.");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_TimeSpan_Null() => DeserializeElementInvalid<TimeSpan, XmlDeserializationException>(null, NullException("TimeSpan"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_TimeSpan_EmptyString() => DeserializeElementInvalid<TimeSpan, XmlDeserializationException>(string.Empty, NullException("TimeSpan"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_TimeSpan_Missing() => DeserializeElementInvalid<TimeSpan, XmlDeserializationException>(null, NullException("TimeSpan"), false);

        #endregion
        #region Nullable TimeSpan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableTimeSpan_Normal() => DeserializeElementDummy<TimeSpan?>("60", TimeSpan.FromSeconds(60));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableTimeSpan_Invalid() => DeserializeElementInvalid<TimeSpan?, XmlDeserializationException>("banana", "Input string was not in a correct format.");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableTimeSpan_Null() => DeserializeElementDummy<TimeSpan?>(null, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableTimeSpan_EmptyString() => DeserializeElementDummy<TimeSpan?>(string.Empty, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableTimeSpan_Missing() => DeserializeElementDummy<TimeSpan?>(null, null, false);

        #endregion
        #region Enum

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Enum_Normal() => DeserializeElementDummy("3", Status.Up);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Enum_AlternateName() => DeserializeAttributeDummy("https", HttpMode.HTTPS);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Enum_NoAttribute() => DeserializeElementInvalid<AuthMode, XmlDeserializationException>("Password", EnumException("Password", "PrtgAPI.AuthMode"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Enum_NoAttribute_AmongstHasAttribute()
        {
            DeserializeElementDummy("WithAttribute", PartialMissingXmlEnum.WithAttribute);
            DeserializeElementInvalid<PartialMissingXmlEnum, XmlDeserializationException>("WithoutAttribute", EnumException("WithoutAttribute", "PrtgAPI.Tests.UnitTests.Support.Serialization.PartialMissingXmlEnum"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Enum_Invalid() => DeserializeElementInvalid<Status, XmlDeserializationException>("banana", EnumException("banana", "PrtgAPI.Status"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Enum_Null() => DeserializeElementInvalid<Status, XmlDeserializationException>(null, NullException("Status"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Enum_EmptyString() => DeserializeElementInvalid<Status, XmlDeserializationException>(string.Empty, NullException("Status"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Enum_Missing() => DeserializeElementInvalid<Status, XmlDeserializationException>(null, NullException("Status"), false);

        #endregion
        #region Nullable Enum

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableEnum_Normal() => DeserializeElementDummy<Status?>("3", Status.Up);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableEnum_Invalid() => DeserializeElementInvalid<Status?, XmlDeserializationException>("banana", EnumException("banana", "PrtgAPI.Status"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableEnum_Null() => DeserializeElementDummy<Status?>(null, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableEnum_EmptyString() => DeserializeElementDummy<Status?>(string.Empty, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_NullableEnum_Missing() => DeserializeElementDummy<Status?>(null, null, false);

        #endregion
        #region Splittable String

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_StringArray_Normal() => DeserializeElementDummy("first second", new[] { "first", "second" });

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_StringArray_Null() => DeserializeElementDummy<string[]>(null, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_StringArray_EmptyString() => DeserializeElementDummy<string[]>(string.Empty, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_StringArray_Missing() => DeserializeElementDummy<string[]>(null, null, false);

        #endregion
        #region Attribute

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Attribute_Normal() => DeserializeAttributeDummy("3", Status.Up);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Attribute_Invalid() => DeserializeAttributeInvalid<int, XmlDeserializationException>("banana", $"An error occurred while attempting to deserialize an object of type 'Int32', possibly caused by the following XML: '<item property=\"banana\" />'. Input string was not in a correct format");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Attribute_InvalidEnum() => DeserializeAttributeInvalid<Status?, XmlDeserializationException>("banana", "Could not deserialize value 'banana' as it is not a valid member of type 'PrtgAPI.Status'. Could not process XML '<item property=\"banana\" />'");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Attribute_Null() => DeserializeElementInvalid<Status, XmlDeserializationException>(null, NullException("Status"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Attribute_EmptyString() => DeserializeElementInvalid<Status, XmlDeserializationException>(string.Empty, NullException("Status"));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Attribute_Missing() => DeserializeElementInvalid<Status, XmlDeserializationException>(null, NullException("Status"), false);

        #endregion
        #region Text

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Text_Normal()
        {
            DeserializeText("1", true);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Text_Null_NonNullable()
        {
            DeserializeTextInvalid<bool, XmlDeserializationException>(null, NullException("Boolean", "item"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Text_EmptyString_NonNullable()
        {
            DeserializeTextInvalid<bool, XmlDeserializationException>(string.Empty, NullException("Boolean", "item"));
        }

        #endregion
        #region Root

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_Root()
        {
            var xml = new XDocument(new XElement("sensors",
                new XAttribute("totalcount", 1),
                new XElement("item",
                    new XElement("property",
                        new XElement("value", "1")
                    )
                )
            ));

            DeserializeReflectionDummyElement<DummyElementRoot>(xml);
            DeserializeManual<TableData<DummyElement<DummyElementRoot>>>(xml, r => new DummyRootSerializerManual(r));
            DeserializeExpressionDummyElement<DummyElementRoot>(xml);
        }

        #endregion
        #region Object Settings

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_ObjectProperty_Single_Normal()
        {
            DeserializeSingleObjectProperty(ObjectProperty.InheritWindowsCredentials, "1", true);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_ObjectProperty_Single_Null_Nullable()
        {
            DeserializeSingleObjectProperty<bool?>(ObjectProperty.InheritWindowsCredentials, null, null);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_ObjectProperty_BackingProperty()
        {
            DeserializeSingleObjectProperty(ObjectProperty.Interval, "60|60 seconds", ScanningInterval.SixtySeconds);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_ObjectProperty_Single_Null_NonNullable()
        {
            DeserializeSingleObjectProperty_InvalidNull<bool>(ObjectProperty.Active, null);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_ObjectProperty_Single_EmptyString_Nullable()
        {
            DeserializeSingleObjectProperty<bool?>(ObjectProperty.InheritWindowsCredentials, string.Empty, null);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_ObjectProperty_Single_EmptyString_NonNullable()
        {
            DeserializeSingleObjectProperty_InvalidNull<bool?>(ObjectProperty.Active, string.Empty);
        }

        #endregion
        #region Value Converter

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_ValueConverter_DeserializesValue()
        {
            Serializer_Engine_ValueConverter_Internal<double>();
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_ValueConverter_DeserializesNullableValue()
        {
            Serializer_Engine_ValueConverter_Internal<double?>();
        }

        private void Serializer_Engine_ValueConverter_Internal<T>()
        {
            var xml = new XDocument(new XElement("sensors",
                new XAttribute("totalcount", 1),
                new XElement("item",
                    new XElement("property", "100")
                )
            ));

            var expected = 0.01;
            Func<DummyElementValueConverter<T>, T> getVal = i => i.Property;

            ValidateInternal(DeserializeReflection<TableData<DummyElementValueConverter<T>>>(xml), (T)(object)expected, getVal);
            ValidateInternal(
                DeserializeManual<TableData<DummyElementValueConverter<T>>>(xml, r => new DummyValueConverterSerializerManual<T>(r)),
                (T)(object)expected,
                getVal
            );
            ValidateInternal(DeserializeExpression<TableData<DummyElementValueConverter<T>>>(xml), (T)(object)expected, getVal);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_ValueConverter_ProcessesConvertableAndUnConvertable()
        {
            var xml = new XDocument(new XElement("sensors",
                new XAttribute("totalcount", 1),
                new XElement("item",
                    new XElement("property", "100"),
                    new XElement("propertyConvertable", "100")
                )
            ));

            Action<DummyElementValueConvertableAndUnConvertable> validate = i =>
            {
                Assert.AreEqual(100, i.Property);
                Assert.AreEqual(0.01, i.PropertyConvertable);
            };

            ValidateInternal(DeserializeReflection<TableData<DummyElementValueConvertableAndUnConvertable>>(xml), validate);
            ValidateInternal(
                DeserializeManual<TableData<DummyElementValueConvertableAndUnConvertable>>(xml, r => new DummyValueConvertableAndUnConvertableSerializerManual(r)),
                validate
            );
            ValidateInternal(DeserializeExpression<TableData<DummyElementValueConvertableAndUnConvertable>>(xml), validate);
        }

        #endregion
        #region Update Existing Object

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_UpdateExisting_Normal()
        {
            DeserializeUpdateDummy(
                () => new DummyUpdate<int, int> { Property1 = 3 },
                "4", "5",
                3, 5
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_Engine_UpdateExisting_MissingMandatory()
        {
            DeserializeUpdateInvalid<int, int, XmlDeserializationException>(
                () => new DummyUpdate<int, int> { Property1 = 3 },
                "4", "5",
                NullException("Int32"),
                true, false
            );
        }

        #endregion
        #region Engine Equality

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Serializer_Engine_ReflectionExpression_Equal()
        {
            var actions = new Func<PrtgClient, object>[]
            {
                c => c.GetSensors(),                                      c => c.GetSensorsAsync(),
                c => c.GetDevices(),                                      c => c.GetDevicesAsync(),
                c => c.GetGroups(),                                       c => c.GetGroupsAsync(),
                c => c.GetProbes(),                                       c => c.GetProbesAsync(),
                c => c.GetChannels(1001),                                 c => c.GetChannelsAsync(1001),
                c => c.GetNotificationActions(),                          c => c.GetNotificationActionsAsync(),
                c => c.GetSchedules(),                                    c => c.GetSchedulesAsync(),
                c => c.GetNotificationTriggers(2001),                     c => c.GetNotificationTriggersAsync(2001),
                c => c.GetSensorProperties(3001),                         c => c.GetSensorPropertiesAsync(3001),
                c => c.GetDeviceProperties(4001),                         c => c.GetDevicePropertiesAsync(4001),
                c => c.GetObjectProperty(5001, ObjectProperty.DBAuthMode), c => c.GetObjectPropertyAsync(5001, ObjectProperty.DBAuthMode)
            };

            await ProcessEngineActions(actions);
        }

        private async Task ProcessEngineActions(Func<PrtgClient, object>[] actions)
        {
            for (var i = 0; i < actions.Length; i = i + 2)
            {
                var reflectionResponse = await GetEngineResponse(actions[i], new XmlReflectionSerializer());
                var reflectionAsyncResponse = await GetEngineResponse(actions[i + 1], new XmlReflectionSerializer());

                var expressionResponse = await GetEngineResponse(actions[i], new XmlExpressionSerializer());
                var expressionAsyncResponse = await GetEngineResponse(actions[i + 1], new XmlExpressionSerializer());

                CompareResults(reflectionResponse, expressionResponse);
                CompareResults(reflectionAsyncResponse, reflectionAsyncResponse);
                CompareResults(expressionResponse, expressionAsyncResponse);
            }
        }

        private void CompareResults(object reflectionResponse, object expressionResponse)
        {
            if (reflectionResponse.IsIEnumerable() && !expressionResponse.IsIEnumerable())
                throw new InvalidOperationException($"Reflection response '{reflectionResponse}' was enumerable but expression response '{expressionResponse}' wasn't");

            if (expressionResponse.IsIEnumerable() && !reflectionResponse.IsIEnumerable())
                throw new InvalidOperationException($"Expression response '{expressionResponse}' was enumerable but expression response '{reflectionResponse}' wasn't");

            if (reflectionResponse.IsIEnumerable() && expressionResponse.IsIEnumerable())
            {
                CompareEnumerableResults(reflectionResponse, expressionResponse);
            }
            else
            {
                if (TestHelpers.IsPrtgAPIClass(reflectionResponse) && TestHelpers.IsPrtgAPIClass(expressionResponse))
                    AssertEx.AllPropertiesAndFieldsAreEqual(reflectionResponse, expressionResponse);
                else
                    Assert.AreEqual(reflectionResponse, expressionResponse);
            }
        }

        private void CompareEnumerableResults(object reflectionResponse, object expressionResponse)
        {
            var reflectionList = reflectionResponse.ToIEnumerable().ToList();
            var expressionList = expressionResponse.ToIEnumerable().ToList();

            AssertEx.AreEqualLists(reflectionList, expressionList, AssertEx.AllPropertiesAndFieldsAreEqual, "Reflection list was not equal to expression list");
        }

        private async Task<object> GetEngineResponse(Func<PrtgClient, object> action, IXmlSerializer serializer)
        {
            var originalSerializer = ILazyExtensions.Serializer;

            try
            {
                var webClient = new MockWebClient(new MultiTypeResponse());

                var client = new PrtgClient("prtg.example.com", "username", "12345678", AuthMode.PassHash, webClient, serializer);
                ILazyExtensions.Serializer = serializer;

                var result = action(client);

                if (ReflectionExtensions.IsSubclassOfRawGeneric(result.GetType(), typeof(Task<>)))
                {
                    var task = (Task)result;
                    await task;
                    return ((dynamic)task).Result;
                }

                return result;
            }
            finally
            {
                ILazyExtensions.Serializer = originalSerializer;
            }
        }

        #endregion
        #endregion

        private void Validate<TDummy, T>(TableData<TDummy> result, T deserialized) where TDummy : IDummy<T>
        {
            ValidateInternal(result, deserialized, i => i.Property);
        }

        #region DummyElement

        private void DeserializeElementDummy<T>(string serialized, T deserialized, bool hasProperty = true)
        {
            var xml = new XDocument(new XElement("sensors",
                new XAttribute("totalcount", 1),
                new XElement("item",
                    (hasProperty ? new XElement("property", serialized) : null)
                )
            ));

            Validate(DeserializeReflectionDummyElement<T>(xml), deserialized);
            Validate(DeserializeManualDummyElement<T>(xml), deserialized);
            Validate(DeserializeExpressionDummyElement<T>(xml), deserialized);
        }

        private void DeserializeElementInvalid<TValue, TException>(string serialized, string message, bool hasProperty = true) where TException : Exception
        {
            var xml = new XDocument(new XElement("sensors",
                new XAttribute("totalcount", 1),
                new XElement("item",
                    (hasProperty ? new XElement("property", serialized) : null)
                )
            ));

            ValidateFail<TException>(() => DeserializeReflectionDummyElement<TValue>(xml), message);
            ValidateFail<TException>(() => DeserializeManualDummyElement<TValue>(xml), message);
            ValidateFail<TException>(() => DeserializeExpressionDummyElement<TValue>(xml), message);
        }

        private TableData<DummyElement<T>> DeserializeReflectionDummyElement<T>(XDocument doc)
        {
            return DeserializeReflection<TableData<DummyElement<T>>>(doc);
        }

        private TableData<DummyElement<T>> DeserializeExpressionDummyElement<T>(XDocument doc)
        {
            return DeserializeExpression<TableData<DummyElement<T>>>(doc);
        }

        private TableData<DummyElement<T>> DeserializeManualDummyElement<T>(XDocument doc)
        {
            return DeserializeManual<TableData<DummyElement<T>>>(doc, r => new DummyElementSerializerManual<T>(r));
        }

        #endregion
        #region DummyAttribute

        private void DeserializeAttributeDummy<T>(string serialized, T deserialized, bool hasProperty = true)
        {
            var xml = new XDocument(new XElement("sensors",
                new XAttribute("totalcount", 1),
                new XElement("item",
                    (hasProperty ? new XAttribute("property", serialized) : null)
                )
            ));

            Validate(DeserializeReflectionDummyAttribute<T>(xml), deserialized);
            Validate(DeserializeManualDummyAttribute<T>(xml), deserialized);
            Validate(DeserializeExpressionDummyAttribute<T>(xml), deserialized);
        }

        private void DeserializeAttributeInvalid<TValue, TException>(string serialized, string message, bool hasProperty = true) where TException : Exception
        {
            var xml = new XDocument(new XElement("sensors",
                new XAttribute("totalcount", 1),
                new XElement("item",
                    (hasProperty ? new XAttribute("property", serialized) : null)
                )
            ));

            ValidateFail<TException>(() => DeserializeReflectionDummyAttribute<TValue>(xml), message);
            ValidateFail<TException>(() => DeserializeManualDummyAttribute<TValue>(xml), message);
            ValidateFail<TException>(() => DeserializeExpressionDummyAttribute<TValue>(xml), message);
        }

        private TableData<DummyAttribute<T>> DeserializeReflectionDummyAttribute<T>(XDocument doc)
        {
            return DeserializeReflection<TableData<DummyAttribute<T>>>(doc);
        }

        private TableData<DummyAttribute<T>> DeserializeExpressionDummyAttribute<T>(XDocument doc)
        {
            return DeserializeExpression<TableData<DummyAttribute<T>>>(doc);
        }

        private TableData<DummyAttribute<T>> DeserializeManualDummyAttribute<T>(XDocument doc)
        {
            return DeserializeManual<TableData<DummyAttribute<T>>>(doc, r => new DummyAttributeSerializerManual<T>(r));
        }

        #endregion
        #region DummyText

        private void DeserializeText<T>(string serialized, T deserialized)
        {
            var xml = new XDocument(new XElement("sensors",
                new XAttribute("totalcount", 1),
                new XElement("item", serialized)
            ));

            Action<DummyText<T>> validator = i => Assert.AreEqual(i.Value, deserialized);

            ValidateInternal(DeserializeReflection<TableData<DummyText<T>>>(xml), validator);
            ValidateInternal(DeserializeManual<TableData<DummyText<T>>>(xml, r => new DummyTextSerializerManual<T>(r)), validator);
            ValidateInternal(DeserializeExpression<TableData<DummyText<T>>>(xml), validator);
        }

        private void DeserializeTextInvalid<TValue, TException>(string serialized, string message) where TException : Exception
        {
            var xml = new XDocument(new XElement("sensors",
                new XAttribute("totalcount", 1),
                new XElement("item", serialized)
            ));

            ValidateFail<TException>(() => DeserializeReflection<TableData<DummyText<TValue>>>(xml), message);
            ValidateFail<TException>(() => DeserializeManual<TableData<DummyText<TValue>>>(xml, r => new DummyTextSerializerManual<TValue>(r)), message);
            ValidateFail<TException>(() => DeserializeExpression<TableData<DummyText<TValue>>>(xml), message);
        }

        #endregion
        #region DummyUpdate

        private void DeserializeUpdateDummy<T1, T2>(Func<DummyUpdate<T1, T2>> target, string s1, string s2, T1 d1, T2 d2, bool hasProp1 = true, bool hasProp2 = true)
        {
            var xml = new XDocument(new XElement("sensors",
                new XAttribute("totalcount", 1),
                new XElement("item",
                    (hasProp1 ? new XElement("objid", s1) : null),
                    (hasProp2 ? new XElement("property", s2) : null)
                )
            ));

            DeserializeUpdateDummyInternal(target, xml, DeserializeReflectionDummyUpdate, d1, d2);
            DeserializeUpdateDummyInternal(target, xml, DeserializeManualDummyUpdate, d1, d2);
            DeserializeUpdateDummyInternal(target, xml, DeserializeExpressionDummyUpdate, d1, d2);
        }

        private void DeserializeUpdateDummyInternal<T1, T2>(Func<DummyUpdate<T1, T2>> target, XDocument doc, Action<XDocument, DummyUpdate<T1, T2>> serializer, T1 d1, T2 d2)
        {
            var t = target();
            serializer(doc, t);

            Assert.AreEqual(d1, t.Property1);
            Assert.AreEqual(d2, t.Property2);
        }

        private void DeserializeUpdateInvalid<TValue1, TValue2, TException>(Func<DummyUpdate<TValue1, TValue2>> target, string s1, string s2, string message, bool hasProp1 = true, bool hasProp2 = true) where TException : Exception
        {
            var xml = new XDocument(new XElement("sensors",
                new XAttribute("totalcount", 1),
                new XElement("item",
                    (hasProp1 ? new XElement("objid", s1) : null),
                    (hasProp2 ? new XElement("property", s2) : null)
                )
            ));

            ValidateFail<TException>(() => DeserializeReflectionDummyUpdate(xml, target()), message);
            ValidateFail<TException>(() => DeserializeManualDummyUpdate(xml, target()), message);
            ValidateFail<TException>(() => DeserializeExpressionDummyUpdate(xml, target()), message);
        }

        private void DeserializeReflectionDummyUpdate<T1, T2>(XDocument doc, DummyUpdate<T1, T2> target)
        {
            var serializer = new XmlReflectionSerializerImpl(typeof(DummyUpdate<T1, T2>));

            serializer.DeserializeExisting(doc, target);
        }

        private void DeserializeExpressionDummyUpdate<T1, T2>(XDocument doc, DummyUpdate<T1, T2> target)
        {
            var serializer = new XmlExpressionSerializer();

            serializer.Update(doc.CreateReader(), target);
        }

        private void DeserializeManualDummyUpdate<T1, T2>(XDocument doc, DummyUpdate<T1, T2> target)
        {
            var serializer = new DummyUpdateSerializerManual<T1, T2>(doc.CreateReader());

            serializer.Update(target);
        }

        #endregion
        #region ObjectProperty

        private void DeserializeSingleObjectProperty<T>(ObjectProperty property, string rawValue, T expected)
        {
            Assert.AreEqual(DeserializeSinglePropertyReflection<T>(property, rawValue), expected);
            Assert.AreEqual(DeserializeSinglePropertyManual<T>(property, rawValue), expected);
            Assert.AreEqual(DeserializeSinglePropertyExpression<T>(property, rawValue), expected);
        }

        private void DeserializeSingleObjectProperty_InvalidNull<T>(ObjectProperty property, string rawValue)
        {
            var underlying = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            AssertEx.Throws<XmlDeserializationException>(
                () => DeserializeSinglePropertyReflection<T>(property, rawValue),
                $"An error occurred while attempting to deserialize XML element 'injected_active': cannot assign 'null' to value type '{underlying.Name}'"
            );

            AssertEx.Throws<XmlDeserializationException>(
                () => DeserializeSinglePropertyManual<T>(property, rawValue),
                $"An error occurred while attempting to deserialize XML element 'active': cannot assign 'null' to value type '{underlying.Name}'"
            );

            AssertEx.Throws<XmlDeserializationException>(
                () => DeserializeSinglePropertyExpression<T>(property, rawValue),
                $"An error occurred while attempting to deserialize XML element 'active': cannot assign 'null' to value type '{underlying.Name}'"
            );
        }

        private T DeserializeSinglePropertyReflection<T>(ObjectProperty property, string rawValue)
        {
            var cache = ObjectPropertyParser.GetPropertyInfoViaTypeLookup(property);
            var rawName = ObjectPropertyParser.GetObjectPropertyNameViaCache(property, cache);

            return (T)XmlReflectionSerializerImpl.DeserializeRawPropertyValue(property, rawName, rawValue);
        }

        private T DeserializeSinglePropertyManual<T>(ObjectProperty property, string rawValue)
        {
            return (T)XmlSerializerManual.DeserializeObjectProperty(property, rawValue);
        }

        private T DeserializeSinglePropertyExpression<T>(ObjectProperty property, string rawValue)
        {
            return (T)new XmlExpressionSerializer().DeserializeObjectProperty(property, rawValue);
        }

        #endregion
        #region Internal

        private T DeserializeReflection<T>(XDocument doc)
        {
            var serializer = new XmlReflectionSerializerImpl(typeof(T));

            var result = (T)serializer.Deserialize(doc, null, true);

            return result;
        }

        private T DeserializeExpression<T>(XDocument doc)
        {
            var serializer = new XmlExpressionSerializer();

            var result = serializer.Deserialize<T>(doc.CreateReader());

            //todo: the second dynamic type we try and debug doesnt show locals?
            //serializer.Deserialize<TableData<Dummy<Status>>>(doc.CreateReader());

            return result;
        }

        private T DeserializeManual<T>(XDocument doc, Func<XmlReader, XmlSerializerManual> getSerializer)
        {
            var serializer = getSerializer(doc.CreateReader());

            var result = (T)serializer.Deserialize();

            return result;
        }

        private void ValidateInternal<TItem>(TableData<TItem> result, Action<TItem> validate)
        {
            validate(result.Items.First());
        }

        private void ValidateInternal<TItem, TProperty>(
            TableData<TItem> result,
            TProperty deserialized,
            Func<TItem, TProperty> getProperty)
        {
            if (typeof(TProperty).IsArray)
            {
                var expected = ((IEnumerable)deserialized)?.Cast<string>().ToList();
                var actual = ((IEnumerable)getProperty(result.Items.First()))?.Cast<string>().ToList();

                AssertEx.AreEqualLists(expected, actual, "Lists were not equal");
            }
            else
                Assert.AreEqual(deserialized, getProperty(result.Items.First()));
        }

        private void ValidateFail<T>(Action action, string message) where T : Exception
        {
            AssertEx.Throws<T>(action, message);
        }

        #endregion

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AllPrtgObjectTypes_HaveSerializerTests()
        {
            //Normal, Invalid, Null, EmptyString, MissingTag

            //StringEnum is calculated in the getter; not even the reflection deserialization engine handles it
            var properties = PrtgObjectFilterTests.GetPrtgObjectProperties(new[] { "NotificationTypes" }).Where(p => p.PropertyType != typeof(StringEnum<ObjectType>));

            var propertyTypes = PrtgAPIHelpers.DistinctBy(properties.SelectMany(p =>
            {
                var types = new List<Type>();

                if (p.PropertyType.IsEnum)
                    types.Add(typeof(Enum));
                else
                {
                    types.Add(p.PropertyType);

                    var underlying = Nullable.GetUnderlyingType(p.PropertyType);

                    if (underlying != null)
                        types.Add(underlying);
                    else
                    {
                        if (p.PropertyType.IsValueType)
                        {
                            types.Add(typeof(Nullable<>).MakeGenericType(p.PropertyType));
                        }
                    }
                }

                return types;
            }), t =>
            {
                var underlying = Nullable.GetUnderlyingType(t);

                if (underlying != null)
                    return $"Nullable{underlying.Name}";

                return t.Name;
            }).ToList();

            var methods = propertyTypes.SelectMany(t =>
            {
                var list = new List<string>();

                var name = t.Name;

                var underlying = Nullable.GetUnderlyingType(t);

                if (underlying != null)
                    name = $"Nullable{underlying.Name}";

                name = name.Replace("Boolean", "Bool").Replace("Int32", "Integer").Replace("[]", "Array").Replace("`1", "");

                var prefix = $"Serializer_Engine_{name}_";

                if (name.Contains("Bool"))
                {
                    list.Add($"{prefix}True");
                    list.Add($"{prefix}False");
                    list.Add($"{prefix}NegativeTrue");
                }
                else
                {
                    list.Add($"{prefix}Normal");
                }

                if (t != typeof(string) && t != typeof(string[]))
                    list.Add($"{prefix}Invalid");

                list.Add($"{prefix}Null");
                list.Add($"{prefix}EmptyString");
                list.Add($"{prefix}Missing");

                return list;
            }).ToList();

            TestHelpers.Assert_TestClassHasMethods(GetType(), methods);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Serializer_ThrowsInvalidEnum()
        {
            var webClient = new MockWebClient(new SensorResponse(new SensorItem(status: "banana", statusRaw: "8765")));

            var client = new PrtgClient("prtg.example.com", "username", "password", AuthMode.PassHash, webClient, new XmlExpressionSerializer());

            try
            {
                client.GetSensors();
            }
            catch (Exception ex)
            {
                if (ex.Message != "Could not deserialize value '8765' as it is not a valid member of type 'PrtgAPI.Status'. Could not process XML '<status>banana</status><status_raw>8765</status_raw><message><div class=\"status\">OK<div class=\"moreicon\"></div></div></message>'")
                    throw;
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ScanningInterval_ParsesCustomTimeSpanSerializedString()
        {
            var interval = ScanningInterval.Parse("20|20 seconds");

            Assert.AreEqual(interval.TimeSpan.TotalSeconds, 20, "TimeSpan was not correct");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ScanningInterval_SerializesTimeSpanCorrectly()
        {
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 0, 1), "1|1 second (Not officially supported)");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 0, 3), "3|3 seconds (Not officially supported)");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 0, 30), "30|30 seconds");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 0, 60), "60|60 seconds");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 5, 0), "300|5 minutes");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 10, 0), "600|10 minutes");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 15, 0), "900|15 minutes");
            CheckScanningIntervalSerializedValue(new TimeSpan(0, 30, 0), "1800|30 minutes");
            CheckScanningIntervalSerializedValue(new TimeSpan(1, 0, 0), "3600|1 hour");
            CheckScanningIntervalSerializedValue(new TimeSpan(4, 0, 0), "14400|4 hours");
            CheckScanningIntervalSerializedValue(new TimeSpan(6, 0, 0), "21600|6 hours");
            CheckScanningIntervalSerializedValue(new TimeSpan(12, 0, 0), "43200|12 hours");
            CheckScanningIntervalSerializedValue(new TimeSpan(24, 0, 0), "86400|24 hours");
            CheckScanningIntervalSerializedValue(new TimeSpan(2, 0, 0, 0), "172800|2 days");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ScanningInterval_SerializesEnumToTimeSpan()
        {
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.ThirtySeconds, "30|30 seconds");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.SixtySeconds, "60|60 seconds");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.FiveMinutes, "300|5 minutes");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.TenMinutes, "600|10 minutes");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.FifteenMinutes, "900|15 minutes");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.ThirtyMinutes, "1800|30 minutes");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.OneHour, "3600|1 hour");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.FourHours, "14400|4 hours");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.SixHours, "21600|6 hours");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.TwelveHours, "43200|12 hours");
            ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval.TwentyFourHours, "86400|24 hours");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ScanningInterval_From_ScanningInterval()
        {
            var interval = ScanningInterval.FiveMinutes;

            var newInterval = ScanningInterval.Parse(interval);

            Assert.AreEqual(newInterval.ToString(), interval.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ScanningInterval_From_Int()
        {
            var interval = ScanningInterval.Parse(300);

            Assert.AreEqual(300, interval.TimeSpan.TotalSeconds);
        }
        
        [TestMethod]
        [TestCategory("UnitTest")]
        public void ScanningInterval_From_TimeSpan()
        {
            var interval = ScanningInterval.Parse(new TimeSpan(0, 5, 0));

            Assert.AreEqual(300, interval.TimeSpan.TotalSeconds);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Object_DeserializesSchedule()
        {
            var webClient = new MockWebClient(new MultiTypeResponse());

            var client = new PrtgClient("server", "username", "1234567890", AuthMode.PassHash, webClient, new XmlExpressionSerializer());

            var properties = client.GetSensorProperties(1001);

            Assert.AreEqual("Weekdays [GMT+0800]", properties.Schedule.ToString(), "Schedule was not correct");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Object_Deserializes_PropertyWithMultipleAttributes_WhenOneValueIsSet()
        {
            var xDoc = new XDocument(
                new XElement("properties",
                    new XElement("injected_propertyname", "3"),
                    new XElement("propertyname")
                )
            );

            var val = new XmlExpressionSerializer().Deserialize<CustomType>(xDoc.CreateReader());

            Assert.AreEqual(3, val.Property);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Object_Deserializes_PropertyWithMultipleAttributes_WhenBothValuesAreSet()
        {
            var xDoc = new XDocument(
                new XElement("properties",
                    new XElement("injected_propertyname", "3"),
                    new XElement("propertyname", "4")
                )
            );

            var val = new XmlExpressionSerializer().Deserialize<CustomType>(xDoc.CreateReader());

            Assert.AreEqual(3, val.Property);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Object_Throws_DeserializingHtmlWithDuplicateInputTags()
        {
            var builder = new StringBuilder();

            builder.Append("<input name=\"propertyName\" type=\"checkbox\" value=\"1\"/>");
            builder.Append("<input name=\"propertyName\" type=\"radio\" value=\"1\"/>");

            var client = BaseTest.Initialize_Client(new BasicResponse(builder.ToString()));

            AssertEx.Throws<NotImplementedException>(
                () => client.GetSensorProperties(1001),
                "Two properties were found with the name 'propertyName' but had different types: 'Radio' (<input name=\"propertyName\" type=\"radio\" value=\"1\"/>), 'Checkbox' (<input name=\"propertyName\" type=\"checkbox\" value=\"1\"/>)"
            );
        }

        private void CheckScanningIntervalSerializedValue(ScanningInterval interval, string value)
        {
            Assert.AreEqual(((ISerializable) interval).GetSerializedFormat(), value, "Serialized format was not correct");
        }

        private void ParseScanningIntervalAndCheckSerializedValue(StandardScanningInterval interval, string value)
        {
            CheckScanningIntervalSerializedValue(ScanningInterval.Parse(interval), value);
        }
    }
}
