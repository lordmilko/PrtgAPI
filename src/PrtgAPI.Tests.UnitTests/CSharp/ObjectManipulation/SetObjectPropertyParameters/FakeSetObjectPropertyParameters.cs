using System;
using PrtgAPI.Parameters;
using PrtgAPI.Reflection.Cache;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    class FakeSetObjectPropertyParameters : BaseSetObjectPropertyParameters<FakeObjectProperty>
    {
        private Func<Enum, PropertyCache> getPropertyCache;

        protected override int[] ObjectIdsInternal { get; set; }

        public FakeSetObjectPropertyParameters(Func<Enum, PropertyCache> getPropertyCache) : base(null)
        {
            this.getPropertyCache = getPropertyCache;
        }

        public void AddValue(Enum property, object value, bool disableDependentsOnNotReqiuiredValue)
        {
            AddTypeSafeValue(property, value, disableDependentsOnNotReqiuiredValue);
        }

        public override PropertyCache GetPropertyCache(Enum property)
        {
            return getPropertyCache(property);
        }
    }
}