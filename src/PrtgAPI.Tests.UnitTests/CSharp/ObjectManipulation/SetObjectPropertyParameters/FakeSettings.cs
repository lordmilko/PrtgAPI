using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Targets;

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

        [XmlElement("injected_typelookupwithoutiserializable")]
        public string TypeWithoutISerializable { get; set; }

        [StandardSplittableString]
        [XmlElement("injected_arrayproperty")]
        public string[] ArrayProperty { get; set; }

        [XmlElement("injected_nonsplittablearrayproperty")]
        public string[] NonSplittableArrayProperty { get; set; }

        [XmlElement("injected_integerproperty")]
        public int IntegerProperty { get; set; }

        [XmlElement("injected_doubleproperty")]
        public double DoubleProperty { get; set; }

        [XmlElement("injected_boolproperty")]
        public bool BoolProperty { get; set; }

        [XmlElement("injected_enumproperty")]
        public Status EnumProperty { get; set; }

        [XmlElement("injected_numericenumproperty")]
        public Priority NumericEnumProperty { get; set; }

        [TypeLookup(typeof(XmlEnumAlternateName))]
        [XmlElement("injected_alternateenumproperty")]
        public HttpMode AlternateEnumProperty { get; set; }

        [XmlElement("injected_nullableintegerproperty")]
        public int? NullableIntegerProperty { get; set; }

        [XmlElement("injected_serializableproperty")]
        public string SerializableProperty { get; set; }

        [XmlElement("injected_illegalserializabletype")]
        public string IllegalSerializableType { get; set; }

        [XmlElement("injected_valueconverterwithnullconversion")]
        public Sensor ValueConverterWithNullConversion { get; set; }

        [XmlElement("injected_valueconverterwithoutnullconversion")]
        public Sensor ValueConverterWithoutNullConversion { get; set; }

        [XmlElement("injected_childofenum")]
        public string ChildOfEnum { get; set; }

        [XmlElement("injected_classtype")]
        public NotificationTypes ClassType { get; set; }

        [XmlElement("injected_serializablevalueconverter")]
        public string SerializableValueConverter { get; set; }

        [XmlElement("injected_implicitlyconvertable")]
        public ExeFileTarget ImplicitlyConvertable { get; set; }

        #endregion
    }
}