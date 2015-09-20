using System.ComponentModel;

namespace Prtg
{
    /// <summary>
    /// Specifies operators that can be used for filtering results in a PRTG Request.
    /// </summary>
    public enum FilterOperator
    {
        /// <summary>
        /// Return results equal to a specified value.
        /// </summary>
        Equals,

        /// <summary>
        /// Return results not equal to a specified value.
        /// </summary>
        [Description("neq")]
        NotEquals,

        /// <summary>
        /// Return results greater than a specified value.
        /// </summary>
        [Description("above")]
        GreaterThan,

        /// <summary>
        /// Return results less than a specified value.
        /// </summary>
        [Description("below")]
        LessThan,

        /// <summary>
        /// Return results that contain a specified substring. Additional substrings can be specified (separated by a comma) to further refine results.
        /// </summary>
        [Description("sub")]
        Contains
    }
}
