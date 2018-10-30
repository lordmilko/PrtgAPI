using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the minimum version of PRTG a version specific request applies to.
    /// </summary>
    internal enum RequestVersion
    {
        /// <summary>
        /// PRTG Network Monitor 14.4
        /// </summary>
        v14_4,

        /// <summary>
        /// PRTG Network Monitor 17.4
        /// </summary>
        v17_4,

        /// <summary>
        /// PRTG Network Monitor 18.1
        /// </summary>
        v18_1
    }

    internal static class VersionMap
    {
        internal static ReadOnlyDictionary<RequestVersion, Version> Map { get; }

        static VersionMap()
        {
            Map = new ReadOnlyDictionary<RequestVersion, Version>(new Dictionary<RequestVersion, Version>
            {
                [RequestVersion.v14_4] = new Version(14, 4),
                [RequestVersion.v17_4] = new Version(17, 4),
                [RequestVersion.v18_1] = new Version(18, 1)
            });
        }
    }
}
