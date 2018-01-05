using System;
using System.Reflection;

namespace PrtgAPI.Parameters
{
    sealed class SetObjectPropertyParameters : BaseSetObjectPropertyParameters<ObjectProperty>, IObjectInternalProperty<ObjectPropertyInternal>
    {
        public int[] ObjectIds
        {
            get { return (int[])this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public SetObjectPropertyParameters(int[] objectIds, ObjectProperty property, object value)
        {
            if (objectIds == null)
                throw new ArgumentNullException(nameof(objectIds));

            if (objectIds.Length == 0)
                throw new ArgumentException("At least one Object ID must be specified", nameof(objectIds));

            ObjectIds = objectIds;
            AddTypeSafeValue(property, value, false);
        }

        public SetObjectPropertyParameters(int[] objectIds, string property, string value)
        {
            if (objectIds == null)
                throw new ArgumentNullException(nameof(objectIds));

            if (objectIds.Length == 0)
                throw new ArgumentException("At least one Object ID must be specified", nameof(objectIds));

            ObjectIds = objectIds;
            this[Parameter.Custom] = new CustomParameter(property, value);
        }

        protected override PropertyInfo GetPropertyInfo(Enum property)
        {
            return GetPropertyInfoViaTypeLookup(property);
        }
    }
}
