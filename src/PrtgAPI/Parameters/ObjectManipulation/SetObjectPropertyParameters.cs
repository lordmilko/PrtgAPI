using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Reflection.Cache;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    internal sealed class SetObjectPropertyParameters : BaseSetObjectPropertyParameters<ObjectProperty>
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

        public SetObjectPropertyParameters(int[] objectIds, PropertyParameter[] parameters) : base(ValidateIds(objectIds))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Length == 0)
                throw new ArgumentException("At least one parameter must be specified.", nameof(parameters));

            foreach (var param in parameters)
            {
                AddTypeSafeValue(@param.Property, @param.Value, false);
            }

            RemoveDuplicateParameters();
        }

        public SetObjectPropertyParameters(int[] objectIds, CustomParameter[] parameters) : base(ValidateIds(objectIds))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Length == 0)
                throw new ArgumentException("At least one parameter must be specified.", nameof(parameters));

            if (parameters.Any(p => p == null))
                throw new ArgumentException("Cannot process a null parameter.", nameof(parameters));

            ObjectIds = objectIds;
            this[Parameter.Custom] = parameters;
        }

        private static int[] ValidateIds(int[] objectIds)
        {
            if (objectIds == null)
                throw new ArgumentNullException(nameof(objectIds));

            if (objectIds.Length == 0)
                throw new ArgumentException("At least one Object ID must be specified.", nameof(objectIds));

            return objectIds;
        }

        public override PropertyCache GetPropertyCache(Enum property)
        {
            return ObjectPropertyParser.GetPropertyInfoViaTypeLookup(property);
        }
    }
}
