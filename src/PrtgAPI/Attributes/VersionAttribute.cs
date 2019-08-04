using System;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Utilities;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies the target application element exerts different behavior from a specific version of PRTG.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class VersionAttribute : Attribute
    {
        /// <summary>
        /// The PRTG version this attribute pertains to.
        /// </summary>
        internal RequestVersion Version { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionAttribute"/> class.
        /// </summary>
        /// <param name="version">The PRTG version this attribute pertains to.</param>
        internal VersionAttribute(RequestVersion version)
        {
            Version = version;
        }

        internal bool IsActive(Version version)
        {
            var v = new Version(Version.ToString().TrimStart('v').Replace('_', '.'));

            return version >= v;
        }
    }
}
