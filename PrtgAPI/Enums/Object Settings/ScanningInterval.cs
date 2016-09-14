using System.ComponentModel;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies for settings under the "Scanning Interval" header of PRTG Objects.
    /// </summary>
    public enum ScanningInterval
    {
        /// <summary>
        /// Whether Scanning Interval settings should be inherited from the parent object. 1: true. 0: false. WARNING: This value can only be set. Attempting to retrieve this value will throw an exception.
        /// </summary>
        [Description("intervalgroup_")]
        Inherit, //is this intervalgroup or inheritintervalgroup
        //Scanning Interval

        //whether to be inherited?
    }
}
