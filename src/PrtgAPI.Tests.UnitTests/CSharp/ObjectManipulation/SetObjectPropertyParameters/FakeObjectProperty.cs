using PrtgAPI.Attributes;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    enum FakeObjectProperty
    {
        #region SetObjectPropertyParameters

        [TypeLookup(typeof(FakeSettings))]
        NormalProperty,

        [TypeLookup(typeof(FakeSettings))]
        [System.ComponentModel.Description("missingxmlelementbuthasdescription")]
        MissingXmlElementButHasDescription,

        [TypeLookup(typeof(FakeSettings))]
        MissingXmlElementAndDescription,

        MissingTypeLookup,

        [TypeLookup(typeof(FakeSettings))]
        [SecondaryProperty(nameof(SecondaryProperty), typeof(FakeObjectProperty), SecondaryPropertyStrategy.MultipleSerializable)]
        HasSecondaryProperty,

        SecondaryProperty,

        [TypeLookup(typeof(FakeSettings))]
        MissingFromTypeLookupTarget,

        [TypeLookup(typeof(FakeSettings))]
        ParentProperty,

        [TypeLookup(typeof(FakeSettings))]
        [DependentProperty(nameof(ParentProperty), typeof(FakeObjectProperty), "value")]
        ChildProperty,

        [TypeLookup(typeof(FakeSettings))]
        ParentOfWrongType,

        [DependentProperty(nameof(ParentOfWrongType), typeof(FakeObjectProperty), true)]
        ChildWithWrongType,

        [TypeLookup(typeof(FakeSettings))]
        ParentOfReverseDependencyMissingTypeLookup,

        [DependentProperty(nameof(ParentOfReverseDependencyMissingTypeLookup), typeof(FakeObjectProperty), "value", true)]
        ChildPropertyWithReverseDependencyAndIsMissingTypeLookup,

        [TypeLookup(typeof(FakeSettings))]
        ParentOfReverseDependency,

        [TypeLookup(typeof(FakeSettings))]
        [DependentProperty(nameof(ParentOfReverseDependency), typeof(FakeObjectProperty), "value", true)]
        ChildPropertyWithReverseDependency,

        [TypeLookup(typeof(FakeSettings))]
        [DependentProperty(nameof(ParentOfReverseDependency), typeof(FakeObjectProperty), "value1", true)]
        ChildPropertyWithReverseDependencyAndWrongValue,

        [TypeLookup(typeof(FakeSettings))]
        [DependentProperty(nameof(ChildPropertyWithReverseDependencyAndWrongValue), typeof(FakeObjectProperty), "blah", true)]
        GrandChildProperty,

        PropertyParameterProperty,

        #endregion SetObjectPropertyParameters
        #region DynamicPropertyTypeParser
        
        [TypeLookup(typeof(FakeSettings))]
        ArrayPropertyMissingSplittableString,

        [Type(typeof(int))]
        [TypeLookup(typeof(FakeSettings))]
        TypeWithoutISerializable,

        [TypeLookup(typeof(FakeSettings))]
        ArrayProperty,

        [TypeLookup(typeof(FakeSettings))]
        NonSplittableArrayProperty,

        [TypeLookup(typeof(FakeSettings))]
        IntegerProperty,

        [TypeLookup(typeof(FakeSettings))]
        DoubleProperty,

        [TypeLookup(typeof(FakeSettings))]
        BoolProperty,

        [TypeLookup(typeof(FakeSettings))]
        EnumProperty,

        [TypeLookup(typeof(FakeSettings))]
        NumericEnumProperty,

        [TypeLookup(typeof(FakeSettings))]
        AlternateEnumProperty,

        [TypeLookup(typeof(FakeSettings))]
        NullableIntegerProperty,

        [Type(typeof(FakeSerializable))]
        [TypeLookup(typeof(FakeSettings))]
        SerializableProperty,

        [Type(typeof(int))]
        [TypeLookup(typeof(FakeSettings))]
        IllegalSerializableType,

        [TypeLookup(typeof(FakeSettings))]
        [ValueConverter(typeof(ValueConverterWithNullConversionConverter))]
        ValueConverterWithNullConversion,

        [TypeLookup(typeof(FakeSettings))]
        [ValueConverter(typeof(ValueConverterWithoutNullConversionConverter))]
        ValueConverterWithoutNullConversion,

        [TypeLookup(typeof(FakeSettings))]
        [DependentProperty(nameof(Status.Up), typeof(Status), Status.Up)]
        ChildOfEnum,

        [TypeLookup(typeof(FakeSettings))]
        ClassType,

        [Type(typeof(FakeSerializable))]
        [TypeLookup(typeof(FakeSettings))]
        [ValueConverter(typeof(ValueConverterWithNullConversionConverter))]
        SerializableValueConverter,

        [TypeLookup(typeof(FakeSettings))]
        ImplicitlyConvertable

        #endregion
    }
}