using PrtgAPI.Attributes;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
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
        [SecondaryProperty(nameof(SecondaryProperty))]
        HasSecondaryProperty,

        SecondaryProperty,

        [TypeLookup(typeof(FakeSettings))]
        MissingFromTypeLookupTarget,

        [TypeLookup(typeof(FakeSettings))]
        ParentProperty,

        [DependentProperty(nameof(ParentProperty), "value")]
        ChildProperty,

        [TypeLookup(typeof(FakeSettings))]
        ParentOfWrongType,

        [DependentProperty(nameof(ParentOfWrongType), true)]
        ChildWithWrongType,

        [TypeLookup(typeof(FakeSettings))]
        ParentOfReverseDependencyMissingTypeLookup,

        [DependentProperty(nameof(ParentOfReverseDependencyMissingTypeLookup), "value", true)]
        ChildPropertyWithReverseDependencyAndIsMissingTypeLookup,

        [TypeLookup(typeof(FakeSettings))]
        ParentOfReverseDependency,

        [TypeLookup(typeof(FakeSettings))]
        [DependentProperty(nameof(ParentOfReverseDependency), "value", true)]
        ChildPropertyWithReverseDependency,

        [TypeLookup(typeof(FakeSettings))]
        [DependentProperty(nameof(ParentOfReverseDependency), "value1", true)]
        ChildPropertyWithReverseDependencyAndWrongValue,

        [TypeLookup(typeof(FakeSettings))]
        [DependentProperty(nameof(ChildPropertyWithReverseDependencyAndWrongValue), "blah", true)]
        GrandChildProperty,

        PropertyParameterProperty,

        #endregion SetObjectPropertyParameters
        #region DynamicPropertyTypeParser
        
        [TypeLookup(typeof(FakeSettings))]
        ArrayProperty
        
        #endregion
    }
}