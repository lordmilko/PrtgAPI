using System;
using PrtgAPI.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Exceptions.Internal;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class SetObjectPropertyParametersTests
    {
        #region SetObjectPropertyParameters

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_NormalProperty()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.NormalProperty);
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_NormalProperty_SetsNull()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.NormalProperty, null);
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingXmlElementButHasDescription()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.MissingXmlElementButHasDescription);
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingXmlElementAndDescription_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<MissingAttributeException>(FakeObjectProperty.MissingXmlElementAndDescription, "missing a System.ComponentModel.DescriptionAttribute");
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingTypeLookupAttribute_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<MissingAttributeException>(FakeObjectProperty.MissingTypeLookup, "missing a PrtgAPI.Attributes.TypeLookupAttribute");
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_HasSecondaryProperty()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.HasSecondaryProperty, new FakeMultipleFormattable());
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingFromTypeLookupTarget()
        {
            ExecuteExceptionWithTypeLookup<MissingMemberException>(FakeObjectProperty.MissingFromTypeLookupTarget, "MissingFromTypeLookupTarget cannot be found on type");
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_SetsAParent()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.ParentProperty);
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_ChildDependsOnWrongValueType_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<InvalidTypeException>(FakeObjectProperty.ParentOfWrongType, "Dependencies of property 'ParentOfWrongType' should be of type System.String");
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_ParentOfChildWithReverseDependency()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.ParentOfReverseDependency);
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_ChildWithReverseDependency()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.ChildPropertyWithReverseDependency);
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_ChildHasReverseDependency_AndIsMissingTypeLookup_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<MissingAttributeException>(FakeObjectProperty.ParentOfReverseDependencyMissingTypeLookup, "missing a PrtgAPI.Attributes.TypeLookupAttribute");
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithPropertyParameter_NormalProperty()
        {
            ExecuteWithPropertyParameter(FakeObjectProperty.PropertyParameterProperty);
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithPropertyParameter_MissingPropertyParameterAttribute()
        {
            ExecuteExceptionWithPropertyParameter<MissingAttributeException>(FakeObjectProperty.NormalProperty, "missing a PrtgAPI.Attributes.PropertyParameterAttribute");
        }

        #endregion SetObjectPropertyParameters
        #region DynamicPropertyTypeParser

        [TestMethod]
        public void DynamicPropertyTypeParser_SetArray_WithoutSplittableStringAttribute_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookupAndValue<NotSupportedException>(FakeObjectProperty.ArrayPropertyMissingSplittableString, new[] {"a", "b"}, "missing a SplittableStringAttribute");
        }

        [TestMethod]
        public void DynamicPropertyTypeParser_ParseValue_WithTypeAttribute_AndNotIFormattable_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup<NotSupportedException>(FakeObjectProperty.TypeWithoutIFormattable, "does not implement IFormattable is not currently supported");
        }

        [TestMethod]
        public void DynamicPropertyTypeParser_SetArray_WithUntypedArray()
        {
            var arr = new object[] { "1", "2" };

            ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, arr);
        }

        [TestMethod]
        public void DynamicPropertyTypeParser_SetArray_WithNull()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.ArrayProperty, null);
        }

        [TestMethod]
        public void DynamicPropertyTypeParser_AssignsDouble_ToInt_WithoutDecimalPlaces_ShouldThrow()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.IntegerProperty, 1.0);
        }

        [TestMethod]
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
            var parameters = new FakeSetObjectPropertyParameters(BaseSetObjectPropertyParameters<FakeObjectProperty>.GetPropertyInfoViaTypeLookup);
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
            var parameters = new FakeSetObjectPropertyParameters(BaseSetObjectPropertyParameters<FakeObjectProperty>.GetPropertyInfoViaPropertyParameter<FakeSettings>);
            parameters.AddValue(property, value, true);
        }

        private void ExecuteExceptionWithPropertyParameter<T>(FakeObjectProperty property, string message) where T : Exception
        {
            AssertEx.Throws<T>(() => ExecuteWithPropertyParameter(property), message);
        }
    }
}
