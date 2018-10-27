using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    class FakeSettings
    {
        #region SetObjectPropertyParameters

        [XmlElement("injected_normalproperty")]
        public string NormalProperty { get; set; }

        public string MissingXmlElementButHasDescription { get; set; }

        public string MissingXmlElementAndDescription { get; set; }

        [XmlElement("injected_hassecondaryproperty")]
        public FakeMultipleSerializable HasSecondaryProperty { get; set; }

        [XmlElement("injected_parentproperty")]
        public string ParentProperty { get; set; }

        [XmlElement("injected_childproperty")]
        public string ChildProperty { get; set; }

        [XmlElement("injected_parentofwrongtypeproperty")]
        public string ParentOfWrongType { get; set; }

        [XmlElement("injected_parentofreversedependencymissingtypelookup")]
        public string ParentOfReverseDependencyMissingTypeLookup { get; set; }

        [XmlElement("injected_parentofreversedependency")]
        public string ParentOfReverseDependency { get; set; }

        [XmlElement("injected_childpropertywithreversedependency")]
        public string ChildPropertyWithReverseDependency { get; set; }

        [XmlElement("injected_childpropertywithreversedependencyandwrongvalue")]
        public string ChildPropertyWithReverseDependencyAndWrongValue { get; set; }

        [XmlElement("injected_grandchildproperty")]
        public string GrandChildProperty { get; set; }

        [XmlElement("propertyparameterproperty")]
        [PropertyParameter(nameof(FakeObjectProperty.PropertyParameterProperty), typeof(FakeObjectProperty))]
        public string PropertyParameterProperty { get; set; }

        #endregion SetObjectPropertyParameters
        #region DynamicPropertyTypeParser

        [XmlElement("injected_arraypropertymissingsplittablestring")]
        public string[] ArrayPropertyMissingSplittableString { get; set; }

        [XmlElement("injected_typelookupwithoutiformattable")]
        public string TypeWithoutIFormattable { get; set; }

        [SplittableString(' ')]
        [XmlElement("injected_arrayproperty")]
        public string[] ArrayProperty { get; set; }

        [XmlElement("injected_integerproperty")]
        public int IntegerProperty { get; set; }

        [XmlElement("injected_childofenum")]
        public string ChildOfEnum { get; set; }

        #endregion
    }
}