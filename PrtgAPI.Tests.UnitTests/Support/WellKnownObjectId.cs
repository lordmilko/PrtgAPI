namespace PrtgAPI.Tests.UnitTests.Support
{
    public static class WellKnownObjectId
    {
        public const int Root = 0;
        public const int CoreProbe = 1;

        /// <summary>
        /// Probe Device of the initial <see cref="CoreProbe"/>.
        /// </summary>
        public const int CoreProbeDevice = 40;
        public const int WebServerOptions = 810;

        /// <summary>
        /// System Health sensor of the initial <see cref="CoreProbeDevice"/>.
        /// </summary>
        public const int CoreSystemHealth_1001 = 1001;

        /// <summary>
        /// Core Health sensor of the initial <see cref="CoreProbeDevice"/>.
        /// </summary>
        public const int CoreHealth_1002 = 1002;

        /// <summary>
        /// Probe Health sensor of the initial <see cref="CoreProbeDevice"/>.
        /// </summary>
        public const int CoreProbeHealth_1003 = 1003;
    }
}
