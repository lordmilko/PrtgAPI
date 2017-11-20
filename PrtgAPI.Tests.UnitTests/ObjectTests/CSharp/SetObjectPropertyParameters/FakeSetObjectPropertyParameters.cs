using System;
using System.Reflection;
using PrtgAPI.Parameters;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    class FakeSetObjectPropertyParameters : BaseSetObjectPropertyParameters<FakeObjectProperty>
    {
        private Func<Enum, PropertyInfo> getPropertyInfo;

        public FakeSetObjectPropertyParameters(Func<Enum, PropertyInfo> getPropertyInfo)
        {
            this.getPropertyInfo = getPropertyInfo;
        }

        public void AddValue(Enum property, object value, bool disableDependentsOnNotReqiuiredValue)
        {
            AddTypeSafeValue(property, value, disableDependentsOnNotReqiuiredValue);
        }

        protected override PropertyInfo GetPropertyInfo(Enum property)
        {
            return getPropertyInfo(property);
        }
    }
}