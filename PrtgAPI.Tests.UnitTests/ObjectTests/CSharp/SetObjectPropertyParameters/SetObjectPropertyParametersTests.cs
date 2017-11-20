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
            ExecuteExceptionWithTypeLookup(FakeObjectProperty.MissingXmlElementAndDescription, typeof(MissingAttributeException), "missing a System.ComponentModel.DescriptionAttribute");
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingTypeLookupAttribute_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup(FakeObjectProperty.MissingTypeLookup, typeof(MissingAttributeException), "missing a PrtgAPI.Attributes.TypeLookupAttribute");
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_HasSecondaryProperty()
        {
            ExecuteWithTypeLookupInternal(FakeObjectProperty.HasSecondaryProperty, new FakeMultipleFormattable());
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_MissingFromTypeLookupTarget()
        {
            ExecuteExceptionWithTypeLookup(FakeObjectProperty.MissingFromTypeLookupTarget, typeof(MissingMemberException), "MissingFromTypeLookupTarget cannot be found on type");
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_SetsAParent()
        {
            ExecuteWithTypeLookup(FakeObjectProperty.ParentProperty);
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithTypeLookup_ChildDependsOnWrongValueType_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookup(FakeObjectProperty.ParentOfWrongType, typeof(InvalidTypeException), "Dependencies of property 'ParentOfWrongType' should be of type System.String");
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
            ExecuteExceptionWithTypeLookup(FakeObjectProperty.ParentOfReverseDependencyMissingTypeLookup, typeof(MissingAttributeException), "missing a PrtgAPI.Attributes.TypeLookupAttribute");
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithPropertyParameter_NormalProperty()
        {
            ExecuteWithPropertyParameter(FakeObjectProperty.PropertyParameterProperty);
        }

        [TestMethod]
        public void SetObjectPropertyParameters_WithPropertyParameter_MissingPropertyParameterAttribute()
        {
            ExecuteExceptionWithPropertyParameter(FakeObjectProperty.NormalProperty, typeof(MissingAttributeException), "missing a PrtgAPI.Attributes.PropertyParameterAttribute");
        }

        #endregion SetObjectPropertyParameters
        #region DynamicPropertyTypeParser

        [TestMethod]
        public void DynamicPropertyTypeParser_SetArray_WithoutSplittableStringAttribute_ShouldThrow()
        {
            ExecuteExceptionWithTypeLookupAndValue(FakeObjectProperty.ArrayProperty, new[] {"a", "b"}, typeof(NotSupportedException), "missing a SplittableStringAttribute");
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

        private void ExecuteExceptionWithTypeLookup(FakeObjectProperty property, Type exceptionType, string message)
        {
            try
            {
                ExecuteWithTypeLookup(property);
                Assert.Fail($"Expected an exception of type ${exceptionType.Name} to be thrown");
            }
            catch (Exception ex)
            {
                if (ex.GetType() == exceptionType)
                {
                    if (!ex.Message.Contains(message))
                        throw;
                }
            }
        }

        private void ExecuteExceptionWithTypeLookupAndValue(FakeObjectProperty property, object value, Type exceptionType, string message)
        {
            try
            {
                ExecuteWithTypeLookupInternal(property, value);
                Assert.Fail($"Expected an exception of type ${exceptionType.Name} to be thrown");
            }
            catch (Exception ex)
            {
                if (ex.GetType() == exceptionType)
                {
                    if (!ex.Message.Contains(message))
                        throw;
                }
            }
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

        private void ExecuteExceptionWithPropertyParameter(FakeObjectProperty property, Type exceptionType, string message)
        {
            try
            {
                ExecuteWithPropertyParameter(property);
                Assert.Fail($"Expected an exception of type {exceptionType.Name} to be thrown");
            }
            catch (Exception ex)
            {
                if (ex.GetType() == exceptionType)
                {
                    if (!ex.Message.Contains(message))
                        throw;
                }
                else
                    throw;
            }
        }
    }
}
