using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving <see cref="Probe"/> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProbeParameters : TableParameters<Probe>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProbeParameters"/> class.
        /// </summary>
        public ProbeParameters() : base(Content.ProbeNode)
        {
            base.SearchFilter = DefaultSearchFilter();
        }

        private SearchFilter[] DefaultSearchFilter()
        {
            return new[] {new SearchFilter(Property.ParentId, "0")};
        }

        /// <summary>
        /// Filter objects to those with a <see cref="Property"/> of a certain value. Specify multiple filters to limit results further.
        /// </summary>
        public override SearchFilter[] SearchFilter
        {
            get { return base.SearchFilter; }
            set
            {
                if (value.Any(item => item.Property == Property.ParentId && (item.Operator != FilterOperator.Equals || item.Value.ToString() != "0")))
                    throw new InvalidOperationException("Cannot filter for probes based on a ParentId other than 0.");

                if (!value.Any(item => item.Property == Property.ParentId && item.Operator == FilterOperator.Equals && item.Value.ToString() == "0"))
                    value = value.Concat(DefaultSearchFilter()).ToArray();

                base.SearchFilter = value;
            }
        }
    }
}
