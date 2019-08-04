using System;
using PrtgAPI.Parameters;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    sealed class MergeableAttribute : Attribute
    {
        public ObjectProperty Dependency { get; }

        public MergeableAttribute(ObjectProperty dependency)
        {
            Dependency = dependency;
        }

        public PropertyParameter Merge(PropertyParameter mergee, PropertyParameter original)
        {
            switch (original.Property)
            {
                case ObjectProperty.Location:
                    return MergeLocation(mergee, original);
                default:
                    throw new NotImplementedException($"Don't know how to merge property {original.Property} with {mergee.Property}.");
            }
        }

        private PropertyParameter MergeLocation(PropertyParameter mergee, PropertyParameter original)
        {
            switch (mergee.Property)
            {
                case ObjectProperty.LocationName:
                    ((Location) original.Value).Label = mergee.Value?.ToString();

                    return original;
                default:
                    throw new NotImplementedException($"Don't know how to merge property {original.Property} with {mergee.Property}.");
            }
        }
    }
}
