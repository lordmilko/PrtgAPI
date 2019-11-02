using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Targets;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class SetObjectPropertyParametersTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void SetObjectProperty_SetsAChild_OfAnInternalProperty()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Hostv6, "dc-1"),
                "editsettings?id=1001&hostv6_=dc-1&ipversion_=1"
            );
        }

        #region SetObjectPropertyParameters

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_NormalProperty()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.NormalProperty, "normalproperty_");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_NormalProperty_SetsNull()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.NormalProperty, null, "normalproperty_=");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingXmlElementButHasDescription()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.MissingXmlElementButHasDescription, "missingxmlelementbuthasdescription_");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingXmlElementAndDescription_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<MissingAttributeException>(FakeObjectProperty.MissingXmlElementAndDescription, "missing a System.ComponentModel.DescriptionAttribute");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingTypeLookupAttribute_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<MissingAttributeException>(FakeObjectProperty.MissingTypeLookup, "missing a PrtgAPI.Attributes.TypeLookupAttribute");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_HasSecondaryProperty()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.HasSecondaryProperty, new FakeMultipleSerializable(), "hassecondaryproperty_=firstValue", "secondaryproperty_=secondValue");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingFromTypeLookupTarget()
        {
            ExecuteExceptionWithTypeLookup<MissingMemberException>(FakeObjectProperty.MissingFromTypeLookupTarget, "MissingFromTypeLookupTarget cannot be found on type");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_SetsAParent()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.ParentProperty, "parentproperty_");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_ChildDependsOnWrongValueType_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<InvalidTypeException>(FakeObjectProperty.ParentOfWrongType, "Dependencies of property 'ParentOfWrongType' should be of type System.String");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_ParentOfChildWithReverseDependency()
        {
            ExecuteWithTypeLookupInternal(
                FakeObjectProperty.ParentOfReverseDependency,
                "value",

                "parentofreversedependency_=value",
                "childpropertywithreversedependency_=",
                "childpropertywithreversedependencyandwrongvalue_=",
                "grandchildproperty_="
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_ChildWithReverseDependency()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.ChildPropertyWithReverseDependency, "childpropertywithreversedependency_", "parentofreversedependency_");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_ChildHasReverseDependency_AndIsMissingTypeLookup_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<MissingAttributeException>(FakeObjectProperty.ParentOfReverseDependencyMissingTypeLookup, "missing a PrtgAPI.Attributes.TypeLookupAttribute");
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithPropertyParameter_NormalProperty()
        {
            ExecuteWithPropertyParameter(FakeObjectProperty.PropertyParameterProperty);
        }

        [UnitTest]
        [TestMethod]
        public void SetObjectPropertyParameters_WithPropertyParameter_MissingPropertyParameterAttribute()
        {
            ExecuteExceptionWithPropertyParameter<MissingAttributeException>(FakeObjectProperty.NormalProperty, "missing a PrtgAPI.Attributes.PropertyParameterAttribute");
        }

        #endregion SetObjectPropertyParameters
        #region DynamicPropertyTypeParser
        #region Array Serialization

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithoutSplittableStringAttribute_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookupAndValue<NotSupportedException>(FakeObjectProperty.ArrayPropertyMissingSplittableString, new[] { "a", "b" }, "missing a SplittableStringAttribute");
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithUntypedArray() => ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, new object[] { "1", "2" }, "arrayproperty_=1 2");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithNull() => ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, null, "arrayproperty_=");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithEmptyArray() => ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, new string[] { }, "arrayproperty_=");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithEmptyArrayIllegalType_Throws()
        {
            AssertEx.Throws<NotSupportedException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, new int[] { }, null),
                "Properties containing collections of type 'System.Int32' are not currently supported."
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithList() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, new List<string> { "first", "second" }, "arrayproperty_=first second");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithEmptyList() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, new List<string>(), "arrayproperty_=");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithEmptyListIllegalType_Throws()
        {
            AssertEx.Throws<NotSupportedException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, new List<int>(), string.Empty),
                "Properties containing collections of type 'System.Int32' are not currently supported."
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithEnumerable() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, new List<string> { "first", "second" }.Select(v => v), "arrayproperty_=first second");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithEmptyEnumerable() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, Enumerable.Empty<string>(), "arrayproperty_=");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithEmptyEnumerableIllegalType_Throws()
        {
            AssertEx.Throws<NotSupportedException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, Enumerable.Empty<int>(), string.Empty),
                "Properties containing collections of type 'System.Int32' are not currently supported."
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithIllegalTypeMembers()
        {
            AssertEx.Throws<NotSupportedException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, new object[] { 1, "2" }, string.Empty),
                "Properties containing collections of type 'System.Int32' are not currently supported."
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Array_WithRandomType()
        {
            AssertEx.Throws<InvalidTypeException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, true, string.Empty),
                "Value 'True' could not be assigned to property 'ArrayProperty'. Expected type: 'System.String[]'. Actual type: 'System.Boolean'."
            );
        }

        #endregion

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_ISerializable()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.SerializableProperty, new FakeSerializable(), "serializableproperty_=serializedValue");
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_ISerializable_WithNull()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.SerializableProperty, null, "serializableproperty_=");
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_ISerializable_WithRandomType_Throws()
        {
            AssertEx.Throws<InvalidTypeException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.SerializableProperty, true, null),
                "Value 'True' could not be assigned to property 'SerializableProperty'. Expected type: 'PrtgAPI.Tests.UnitTests.ObjectManipulation.FakeSerializable'. Actual type: 'System.Boolean'."
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_ISerializable_IllegalTypeAttributeType_Throws()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.IllegalSerializableType, true, null),
                "Property 'IllegalSerializableType' has a TypeAttribute of type 'System.Int32' which does not implement 'PrtgAPI.Request.ISerializable'"
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Enum_WithEnum_CorrectType() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.EnumProperty, Status.Up, "enumproperty_=3");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Enum_WithEnum_CastedInt()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.EnumProperty, (Status)(object)-1, null),
                "'-1'; value is not a member of type 'PrtgAPI.Status'"
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Enum_WithEnum_DifferentType()
        {
            AssertEx.Throws<ArgumentException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.EnumProperty, ObjectProperty.Active, null),
                "'Active' is not a valid value for type 'PrtgAPI.Status'. Please specify one of"
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_AlternateEnum_WithEnum() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.AlternateEnumProperty, HttpMode.HTTPS, "alternateenumproperty_=https");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Enum_WithValidString() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.EnumProperty, "up", "enumproperty_=3");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Enum_WithInvalidString_Throws()
        {
            AssertEx.Throws<ArgumentException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.EnumProperty, "potato", null),
                "'potato' is not a valid value for type 'PrtgAPI.Status'. Please specify one of"
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Enum_WithValidInt_Throws()
        {
            AssertEx.Throws<ArgumentException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.EnumProperty, 3, null),
                "'3' is not a valid value for type 'PrtgAPI.Status'. Please specify one of"
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_NumericEnum_WithValidInt() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.NumericEnumProperty, 3, "numericenumproperty_=3");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_NumericEnum_WithInvalidInt()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.NumericEnumProperty, -1, null),
                "'-1'; value is not a member of type 'PrtgAPI.Priority'"
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_ValueConverter_NonNull()
        {
            /* Strictly speaking we want to perform the following test
             *    TestDeserialization(FakeObjectProperty.ValueConverterWithNullConversion, "val", "deserializedval");
             * however we don't currently want to want or have a need for nonsense values to reach our ValueConverters
             * (since deserialization is currently only used for things like the null NotificationAction) so we instead
             * don't perform any deserialization when the incoming value's type is non-null and doesn't match the expected
             * outgoing value type
             */

            AssertEx.Throws<InvalidTypeException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.ValueConverterWithNullConversion, "val", null),
                "Value 'val' could not be assigned to property 'ValueConverterWithNullConversion'. Expected type: 'PrtgAPI.Sensor'. Actual type: 'System.String'."
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_ValueConverter_WithNull_WithNullConversion() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.ValueConverterWithNullConversion, null, "valueconverterwithnullconversion_=serializednull");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_ValueConverter_WithNull_WithoutNullConversion() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.ValueConverterWithoutNullConversion, null, "valueconverterwithoutnullconversion_=");


        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_ValueConverter_AndISerializable_WithNull()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.SerializableValueConverter, null, "serializablevalueconverter_=serializednull");
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_ValueConverter_AndISerializable_WithValue()
        {
            //Currently the only scenario we support where an ISerializable has a ValueConverter is for converting null values,
            //so having using an actual non-ISerializable value is not supported

            AssertEx.Throws<InvalidTypeException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.SerializableValueConverter, "val", null),
                "Value 'val' could not be assigned to property 'SerializableValueConverter'. Expected type: 'PrtgAPI.Tests.UnitTests.ObjectManipulation.FakeSerializable'. Actual type: 'System.String'."
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Bool_True() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.BoolProperty, true, "boolproperty_=1");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Bool_False() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.BoolProperty, false, "boolproperty_=0");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Bool_IntTrue() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.BoolProperty, 1, "boolproperty_=1");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Bool_IntFalse() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.BoolProperty, 0, "boolproperty_=0");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Bool_WithNull_Throws()
        {
            AssertEx.Throws<ArgumentNullException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.BoolProperty, null, null),
                "Value 'null' could not be assigned to property 'BoolProperty' of type 'System.Boolean'"
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_KnownType_WithRandomType_Throws()
        {
            AssertEx.Throws<ArgumentException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.BoolProperty, new TimeSpan(), null),
                "Value '00:00:00' could not be assigned to property 'BoolProperty'. Expected type: 'System.Boolean'. Actual type: 'System.TimeSpan'."
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_NonSerializableClassType_Throws()
        {
            AssertEx.Throws<InvalidTypeException>(
                () => ExecuteWithTypeLookupInternal(FakeObjectProperty.ClassType, new NotificationTypes(null), null),
                $"Cannot serialize value of type 'PrtgAPI.NotificationTypes'; type does not implement 'PrtgAPI.Request.ISerializable'."
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_WithTypeAttribute_AndNotISerializable_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<InvalidOperationException>(FakeObjectProperty.TypeWithoutISerializable, "Property 'TypeWithoutISerializable' has a TypeAttribute of type 'System.Int32' which does not implement 'PrtgAPI.Request.ISerializable'");
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Double_ToInt_WithoutDecimalPlaces_ShouldThrow()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.IntegerProperty, 1.0, "integerproperty_=1");
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Double_ToInt_WithDecimalPlaces_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookupAndValue<InvalidTypeException>(FakeObjectProperty.IntegerProperty, 1.2, "Expected type: 'System.Int32'. Actual type: 'System.Double'");
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Double_WithStringDouble() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.DoubleProperty, "1.2", "doubleproperty_=1.2");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Implicit_FromCorrectType() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.ImplicitlyConvertable, "test.ps1", "implicitlyconvertable_=test.ps1|test.ps1||");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Implicit_FromNull() =>
            TestIllegalNullDeserialization(FakeObjectProperty.ImplicitlyConvertable, typeof(ExeFileTarget));

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Implicit_FromIncorrectType() =>
            ExecuteExceptionWithTypeLookupAndValue<InvalidTypeException>(FakeObjectProperty.ImplicitlyConvertable, 2, "Expected type: 'PrtgAPI.Targets.ExeFileTarget'. Actual type: 'System.Int32'.");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Serializes_Implicit_FromFinalType() =>
            ExecuteWithTypeLookupInternal(FakeObjectProperty.ImplicitlyConvertable, (ExeFileTarget)"test.ps1", "implicitlyconvertable_=test.ps1|test.ps1||");

        #region Deserialization

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_FinalType() => TestDeserialization(FakeObjectProperty.BoolProperty, true, true);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_ISerializable()
        {
            var val = new FakeSerializable();

            TestDeserialization(FakeObjectProperty.SerializableProperty, val, val);
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_ISerializable_WithNull() =>
            TestIllegalNullDeserialization(FakeObjectProperty.SerializableProperty, typeof(FakeSerializable));

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_ISerializable_IllegalTypeAttributeType_Throws()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => TestDeserialization(FakeObjectProperty.IllegalSerializableType, true, null),
                "Property 'IllegalSerializableType' has a TypeAttribute of type 'System.Int32' which does not implement 'PrtgAPI.Request.ISerializable'"
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_String() =>
            TestDeserialization(FakeObjectProperty.NormalProperty, new Sensor(), "PrtgAPI.Sensor");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_String_WithNull() =>
            TestDeserialization(FakeObjectProperty.NormalProperty, null, null);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Double_WithInt() =>
            TestDeserialization(FakeObjectProperty.DoubleProperty, 1, 1.0);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Double_WithDouble() =>
            TestDeserialization(FakeObjectProperty.DoubleProperty, 1.0, 1.0);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Double_WithString() =>
            TestDeserialization(FakeObjectProperty.DoubleProperty, "1", 1.0);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Double_WithNull() =>
            TestDeserialization(FakeObjectProperty.DoubleProperty, null, null);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_ValueConverter_NonNull()
        {
            /* Strictly speaking we want to perform the following test
             *    TestDeserialization(FakeObjectProperty.ValueConverterWithNullConversion, "val", "deserializedval");
             * however we don't currently want to want or have a need for nonsense values to reach our ValueConverters
             * (since deserialization is currently only used for things like the null NotificationAction) so we instead
             * don't perform any deserialization when the incoming value's type is non-null and doesn't match the expected
             * outgoing value type
             */

            AssertEx.Throws<InvalidTypeException>(
                () => TestDeserialization(FakeObjectProperty.ValueConverterWithNullConversion, "val", null),
                "Value 'val' could not be assigned to property 'ValueConverterWithNullConversion'. Expected type: 'PrtgAPI.Sensor'. Actual type: 'System.String'."
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_ValueConverter_WithNull_WithNullConversion() =>
            TestDeserialization(FakeObjectProperty.ValueConverterWithNullConversion, null, "deserializednull");

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_ValueConverter_WithNull_WithoutNullConversion() =>
            TestDeserialization(FakeObjectProperty.ValueConverterWithoutNullConversion, null, null);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Bool_WithInt_True() =>
            TestDeserialization(FakeObjectProperty.BoolProperty, 1, true);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Bool_WithInt_False() =>
            TestDeserialization(FakeObjectProperty.BoolProperty, 0, false);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Bool_WithDouble() =>
            TestDeserialization(FakeObjectProperty.BoolProperty, 1.0, true);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Bool_WithIntString() =>
            TestDeserialization(FakeObjectProperty.BoolProperty, "1", true);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Bool_WithDoubleString() =>
            TestDeserialization(FakeObjectProperty.BoolProperty, "1", true);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Bool_WithBoolString() =>
            TestDeserialization(FakeObjectProperty.BoolProperty, "TRUE", true);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Bool_WithNull() =>
            TestIllegalNullDeserialization(FakeObjectProperty.BoolProperty, typeof(bool));

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Nullable_WithNull() =>
            TestDeserialization(FakeObjectProperty.NullableIntegerProperty, null, null);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Nullable_WithUnderlying() =>
            TestDeserialization(FakeObjectProperty.NullableIntegerProperty, 1, 1);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Nullable_WithBoxed() =>
            TestDeserialization(FakeObjectProperty.NullableIntegerProperty, 1, new int?(1));

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Class_WithRandomType()
        {
            AssertEx.Throws<InvalidTypeException>(
                () => TestDeserialization(FakeObjectProperty.SerializableProperty, new Sensor(), null),
                "Expected a value of type 'PrtgAPI.Tests.UnitTests.ObjectManipulation.FakeSerializable' while parsing property 'SerializableProperty' however received a value of type 'PrtgAPI.Sensor'."
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Enum_WithEnum_CorrectType() =>
            TestDeserialization(FakeObjectProperty.EnumProperty, Status.Up, Status.Up);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Enum_WithEnum_DifferentType()
        {
            AssertEx.Throws<ArgumentException>(
                () => TestDeserialization(FakeObjectProperty.EnumProperty, ObjectProperty.Active, null),
                "'Active' is not a valid value for type 'PrtgAPI.Status'. Please specify one of"
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_AlternateEnum_WithEnum() =>
            TestDeserialization(FakeObjectProperty.AlternateEnumProperty, HttpMode.HTTPS, HttpMode.HTTPS);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Enum_WithValidString() =>
            TestDeserialization(FakeObjectProperty.EnumProperty, "up", Status.Up);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Enum_WithInvalidString_Throws()
        {
            AssertEx.Throws<ArgumentException>(
                () => TestDeserialization(FakeObjectProperty.EnumProperty, "potato", null),
                "'potato' is not a valid value for type 'PrtgAPI.Status'. Please specify one of"
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Enum_WithValidInt_Throws()
        {
            AssertEx.Throws<ArgumentException>(
                () => TestDeserialization(FakeObjectProperty.EnumProperty, 3, null),
                "'3' is not a valid value for type 'PrtgAPI.Status'. Please specify one of"
            );
        }

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_NumericEnum_WithValidInt() =>
            TestDeserialization(FakeObjectProperty.NumericEnumProperty, 3, Priority.Three);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_NumericEnum_WithInvalidInt() =>
            TestDeserialization(FakeObjectProperty.NumericEnumProperty, -1, (Priority)(object)-1);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Int_WithDouble() =>
            TestDeserialization(FakeObjectProperty.IntegerProperty, 1.0, 1);

        #region Array Deserialization

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithUntypedArray() =>
            TestArrayDeserialization(
                FakeObjectProperty.ArrayProperty,
                new object[] { "1", "2" },
                new string[] { "1", "2" }
            );

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithUntypedArray_WithDifferentElementTypes() =>
            TestArrayDeserialization(
                FakeObjectProperty.ArrayProperty,
                new object[] { "1", new Sensor() },
                new string[] { "1", "PrtgAPI.Sensor" }
            );

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithNull() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, null, null);

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithEmptyArray() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, new string[] { }, new string[] { });

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithEmptyArrayIllegalType_Throws() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, new int[] { }, new string[] { });

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithList() =>
            TestArrayDeserialization(
                FakeObjectProperty.ArrayProperty,
                new List<string> { "first", "second" },
                new string[] { "first", "second" }
            );

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithEmptyList() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, new List<string>(), new string[] { });

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithEmptyListIllegalType() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, new List<int>(), new string[] { });

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithEnumerable() =>
            TestArrayDeserialization(
                FakeObjectProperty.ArrayProperty,
                new List<string> { "first", "second" }.Select(v => v),
                new string[] { "first", "second" }
            );

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithEmptyEnumerable() =>
            TestArrayDeserialization(
                FakeObjectProperty.ArrayProperty,
                Enumerable.Empty<string>(),
                new string[] {}
            );

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithEmptyEnumerableIllegalType() =>
            TestArrayDeserialization(
                FakeObjectProperty.ArrayProperty,
                Enumerable.Empty<int>(),
                new string[] { }
            );

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_FromSingleString() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, "first", new string[] { "first" });


        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_FromSingleString_NonSplittable() =>
            TestArrayDeserialization(FakeObjectProperty.NonSplittableArrayProperty, "first", new string[] { "first" });

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_FromSingleInt() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, 1, new string[] { "1" });

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithArray_ContainingNull() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, new[]{ "first", null }, new[]{ "first", null });

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_WithList_ContainingNull() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, new List<string> {"first", null}, new[] {"first"});

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_SplittableString_Space() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, "first second", new string[] { "first", "second" });

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_SplittableString_Comma() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, "first,second", new string[] { "first", "second" });

        [UnitTest]
        [TestMethod]
        public void DynamicPropertyTypeParser_Deserializes_Array_SplittableString_Hybrid() =>
            TestArrayDeserialization(FakeObjectProperty.ArrayProperty, "first second,third", new string[] { "first", "second", "third" });

        #endregion

        private void TestDeserialization(FakeObjectProperty property, object value, object expected)
        {
            var parser = new DynamicPropertyTypeParser(property, ObjectPropertyParser.GetPropertyInfoViaTypeLookup(property), value);

            var result = parser.DeserializeValue();

            Assert.AreEqual(expected, result);
        }

        private void TestArrayDeserialization(FakeObjectProperty property, object value, object expected)
        {
            var parser = new DynamicPropertyTypeParser(property, ObjectPropertyParser.GetPropertyInfoViaTypeLookup(property), value);

            var result = parser.DeserializeValue();

            if (result == null)
            {
                Assert.AreEqual(expected, result);
            }
            else
            {
                Assert.IsInstanceOfType(expected, typeof(string[]));
                Assert.IsInstanceOfType(result, typeof(string[]));

                AssertEx.AreEqualLists(((string[]) expected).ToList(), (((string[]) result).ToList()), "Expected and result were not equal");
            }
        }

        private void TestIllegalDeserialization(FakeObjectProperty property, object value, Type expectedType)
        {
            AssertEx.Throws<ArgumentException>(
                () => TestDeserialization(property, value, null),
                $"Value '{value}' could not be assigned to property '{property}' Expected type: '{expectedType}'. Actual type: '{value.GetType()}'."
            );
        }

        private void TestIllegalNullDeserialization(FakeObjectProperty property, Type propertyType)
        {
            AssertEx.Throws<ArgumentException>(
                () => TestDeserialization(property, null, null),
                $"Value 'null' could not be assigned to property '{property}' of type '{propertyType}'. Null may only be assigned to properties of type 'System.String', 'System.Int32' and 'System.Double'."
            );
        }

        #endregion
        #endregion

        private void ExecuteWithTypeLookup(FakeObjectProperty property, params string[] propertyName)
        {
            ExecuteWithTypeLookupInternal(property, "value", propertyName.Select(v => $"{v}=value").ToArray());
        }

        private void ExecuteWithTypeLookupInternal(FakeObjectProperty property, object value, params string[] expected)
        {
            var parameters = new FakeSetObjectPropertyParameters(e => ObjectPropertyParser.GetPropertyInfoViaTypeLookup(e));
            parameters.AddValue(property, value, true);

            AssertEx.AreEqualLists(parameters.CustomParameters.Select(p => p.ToString()).ToList(), expected.ToList(), "Parameter lists were not equal");
        }

        private void ExecuteExceptionWithTypeLookup<T>(FakeObjectProperty property, string message) where T : Exception
        {
            AssertEx.Throws<T>(() => ExecuteWithTypeLookup(property, string.Empty), message);
        }

        private void ExecuteExceptionWithTypeLookupAndValue<T>(FakeObjectProperty property, object value, string message) where T : Exception
        {
            AssertEx.Throws<T>(() => ExecuteWithTypeLookupInternal(property, value, null), message);
        }

        private void ExecuteWithPropertyParameter(FakeObjectProperty property)
        {
            ExecuteWithPropertyParameterInternal(property, "value");
        }

        private void ExecuteWithPropertyParameterInternal(FakeObjectProperty property, object value)
        {
            var parameters = new FakeSetObjectPropertyParameters(ObjectPropertyParser.GetPropertyInfoViaPropertyParameter<FakeSettings>);
            parameters.AddValue(property, value, true);
        }

        private void ExecuteExceptionWithPropertyParameter<T>(FakeObjectProperty property, string message) where T : Exception
        {
            AssertEx.Throws<T>(() => ExecuteWithPropertyParameter(property), message);
        }
    }
}
