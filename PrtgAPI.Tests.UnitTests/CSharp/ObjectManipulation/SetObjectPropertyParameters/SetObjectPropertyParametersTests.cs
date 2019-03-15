using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class SetObjectPropertyParametersTests : BaseTest
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_SetsAChild_OfAnInternalProperty()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Hostv6, "dc-1"),
                "editsettings?id=1001&hostv6_=dc-1&ipversion_=1"
            );
        }

        #region SetObjectPropertyParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_NormalProperty()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.NormalProperty);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_NormalProperty_SetsNull()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.NormalProperty, null);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingXmlElementButHasDescription()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.MissingXmlElementButHasDescription);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingXmlElementAndDescription_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<MissingAttributeException>(FakeObjectProperty.MissingXmlElementAndDescription, "missing a System.ComponentModel.DescriptionAttribute");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingTypeLookupAttribute_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<MissingAttributeException>(FakeObjectProperty.MissingTypeLookup, "missing a PrtgAPI.Attributes.TypeLookupAttribute");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_HasSecondaryProperty()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.HasSecondaryProperty, new FakeMultipleSerializable());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingFromTypeLookupTarget()
        {
            ExecuteExceptionWithTypeLookup<MissingMemberException>(FakeObjectProperty.MissingFromTypeLookupTarget, "MissingFromTypeLookupTarget cannot be found on type");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_SetsAParent()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.ParentProperty);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_ChildDependsOnWrongValueType_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<InvalidTypeException>(FakeObjectProperty.ParentOfWrongType, "Dependencies of property 'ParentOfWrongType' should be of type System.String");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_ParentOfChildWithReverseDependency()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.ParentOfReverseDependency);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_ChildWithReverseDependency()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.ChildPropertyWithReverseDependency);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithTypeLookup_ChildHasReverseDependency_AndIsMissingTypeLookup_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<MissingAttributeException>(FakeObjectProperty.ParentOfReverseDependencyMissingTypeLookup, "missing a PrtgAPI.Attributes.TypeLookupAttribute");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithPropertyParameter_NormalProperty()
        {
            ExecuteWithPropertyParameter(FakeObjectProperty.PropertyParameterProperty);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectPropertyParameters_WithPropertyParameter_MissingPropertyParameterAttribute()
        {
            ExecuteExceptionWithPropertyParameter<MissingAttributeException>(FakeObjectProperty.NormalProperty, "missing a PrtgAPI.Attributes.PropertyParameterAttribute");
        }

        #endregion SetObjectPropertyParameters
        #region DynamicPropertyTypeParser

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicPropertyTypeParser_SetArray_WithoutSplittableStringAttribute_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookupAndValue<NotSupportedException>(FakeObjectProperty.ArrayPropertyMissingSplittableString, new[] {"a", "b"}, "missing a SplittableStringAttribute");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicPropertyTypeParser_ParseValue_WithTypeAttribute_AndNotIFormattable_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<NotSupportedException>(FakeObjectProperty.TypeWithoutIFormattable, "does not implement IFormattable is not currently supported");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicPropertyTypeParser_SetArray_WithUntypedArray()
        {
            var arr = new object[] { "1", "2" };

            ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, arr);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicPropertyTypeParser_SetArray_WithNull()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, null);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicPropertyTypeParser_AssignsDouble_ToInt_WithoutDecimalPlaces_ShouldThrow()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.IntegerProperty, 1.0);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DynamicPropertyTypeParser_AssignsDouble_ToInt_WithDecimalPlaces_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookupAndValue<InvalidTypeException>(FakeObjectProperty.IntegerProperty, 1.2, "Expected type: 'System.Int32'. Actual type: 'System.Double'");
        }

        #endregion

        private void ExecuteWithTypeLookup(FakeObjectProperty property)
        {
            ExecuteWithTypeLookupInternal(property, "value");
        }

        private void ExecuteWithTypeLookupInternal(FakeObjectProperty property, object value)
        {
            var parameters = new FakeSetObjectPropertyParameters(e => ObjectPropertyParser.GetPropertyInfoViaTypeLookup(e));
            parameters.AddValue(property, value, true);
        }

        private void ExecuteExceptionWithTypeLookup<T>(FakeObjectProperty property, string message) where T : Exception
        {
            AssertEx.Throws<T>(() => ExecuteWithTypeLookup(property), message);
        }

        private void ExecuteExceptionWithTypeLookupAndValue<T>(FakeObjectProperty property, object value, string message) where T : Exception
        {
            AssertEx.Throws<T>(() => ExecuteWithTypeLookupInternal(property, value), message);
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
