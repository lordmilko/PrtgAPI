using System.Linq;

namespace Prtg.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="T:Prtg.PrtgUrl"/> for retrieving <see cref="T:Prtg.Probe"/> objects.
    /// </summary>
    public class ProbeParameters : TableParameters<Probe>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Prtg.Parameters.ProbeParameters"/> class.
        /// </summary>
        public ProbeParameters() : base(Content.Objects)
        {
            base.ContentFilter = DefaultContentFilter();
        }

        private ContentFilter[] DefaultContentFilter()
        {
            return new[] {new ContentFilter(Property.ParentId, "0")};
        }

        /// <summary>
        /// Filter objects to those with a <see cref="T:Prtg.Property"/> of a certain value. Specify multiple filters to limit results further.
        /// </summary>
        public override ContentFilter[] ContentFilter
        {
            get { return base.ContentFilter; }
            set
            {
                if (!value.Any(item => item.Property == Property.ParentId && item.Operator == FilterOperator.Equals && item.Value == "0"))
                    value = value.Concat(DefaultContentFilter()).ToArray();

                base.ContentFilter = value;
            }
        }
    }
}
