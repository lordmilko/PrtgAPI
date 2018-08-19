using System;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Request.Serialization.Cache;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    sealed class SetObjectPropertyParameters : BaseSetObjectPropertyParameters<ObjectProperty>, IObjectInternalProperty<ObjectPropertyInternal>
    {
        public int[] ObjectIds
        {
            get { return (int[])this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        protected override int[] ObjectIdsInternal
        {
            get { return ObjectIds; }
            set { ObjectIds = value; }
        }

        public SetObjectPropertyParameters(int[] objectIds, PropertyParameter[] parameters)
        {
            if (objectIds == null)
                throw new ArgumentNullException(nameof(objectIds));

            if (objectIds.Length == 0)
                throw new ArgumentException("At least one Object ID must be specified", nameof(objectIds));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Length == 0)
                throw new ArgumentException("At least one parameter must be specified", nameof(parameters));

            ObjectIds = objectIds;

            foreach (var param in parameters)
            {
                AddTypeSafeValue(@param.Property, @param.Value, false);
            }

            RemoveDuplicateParameters();
        }

        public SetObjectPropertyParameters(int[] objectIds, CustomParameter[] parameters)
        {
            if (objectIds == null)
                throw new ArgumentNullException(nameof(objectIds));

            if (objectIds.Length == 0)
                throw new ArgumentException("At least one Object ID must be specified", nameof(objectIds));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Length == 0)
                throw new ArgumentException("At least one parameter must be specified", nameof(parameters));

            ObjectIds = objectIds;
            this[Parameter.Custom] = parameters;
        }

        protected override PropertyCache GetPropertyCache(Enum property)
        {
            return GetPropertyInfoViaTypeLookup(property);
        }
    }
}
