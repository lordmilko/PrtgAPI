using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for retrieving <see cref="Probe"/> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProbeParameters : TableParameters<Probe>, IShallowCloneable<ProbeParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProbeParameters"/> class.
        /// </summary>
        public ProbeParameters() : base(Content.Probes)
        {
            SearchFilters = DefaultSearchFilter();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProbeParameters"/> class with one or more conditions to filter results by.
        /// </summary>
        /// <param name="filters">A list of conditions to filter results by.</param>
        public ProbeParameters(params SearchFilter[] filters) : this()
        {
            SearchFilters = filters.ToList();
        }

        private List<SearchFilter> DefaultSearchFilter()
        {
            return new List<SearchFilter> {new SearchFilter(Property.Type, ObjectType.Probe)};
        }

        internal override void SetSearchFilter(List<SearchFilter> value)
        {
            if (value == null)
                base.SetSearchFilter(value);
            else
            {
                foreach (var v in value)
                {
                    if (v.Property == Property.Type)
                    {
                        if (v.Operator != FilterOperator.Equals || ValueEquals(v, ObjectType.Probe, (a, b) => a != b))
                            throw new InvalidOperationException($"Probes can only be filtered where \"{nameof(Property.Type)} {FilterOperator.Equals} '{nameof(ObjectType.Probe)}'\". Illegal filter was specified: \"{v.Property} {v.Operator} '{GetDisplayValue(v.Value)}'\". Please remove this illegal {nameof(Property.Type)} filter and try again.");

                        //We don't know whether the specified value was ObjectType.Probe or not, but at the very least if it didn't throw, we know the user is _trying_ to refer to this value
                        v.Value = ObjectType.Probe;
                    }
                }

                if (!value.Any(item => item.Property == Property.Type && item.Operator == FilterOperator.Equals && ValueEquals(item, ObjectType.Probe, (a, b) => a == b)))
                    value = value.Union(DefaultSearchFilter()).ToList();

                base.SetSearchFilter(value);
            }
        }

        private string GetDisplayValue(object value)
        {
            if (value.IsIEnumerable())
                return string.Join(", ", value.ToIEnumerable());

            return value.ToString();
        }

        private bool ValueEquals(SearchFilter filter, ObjectType value, Func<ObjectType?, ObjectType, bool> func)
        {
            if (filter.Value.IsIEnumerable())
                return filter.Value.ToIEnumerable().Any(v => func(AsObjectType(v), value));

            return func(AsObjectType(filter.Value), value);
        }

        private ObjectType? AsObjectType(object value)
        {
            if (value is ObjectType)
                return (ObjectType?) value;

            ObjectType enumValue = default(ObjectType);

            var str = value?.ToString();

            if (str.TryParseDescriptionToEnum(out enumValue) == true)
                return enumValue;

            if (Enum.TryParse(str, true, out enumValue))
                return enumValue;

            return null;
        }

        ProbeParameters IShallowCloneable<ProbeParameters>.ShallowClone()
        {
            var newParameters = new ProbeParameters();

            ShallowClone(newParameters);

            return newParameters;
        }

        [ExcludeFromCodeCoverage]
        object IShallowCloneable.ShallowClone() => ((IShallowCloneable<ProbeParameters>)this).ShallowClone();
    }
}
