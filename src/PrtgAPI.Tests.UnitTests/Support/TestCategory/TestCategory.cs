namespace PrtgAPI.Tests.UnitTests
{
    /// <summary>
    /// Specifies test categories that can be applied to unit tests.
    /// </summary>
    public static class TestCategory
    {
        /// <summary>
        /// Specifies that the test should not be run under CI (Travis/Appveyor).
        /// </summary>
        public const string SkipCI = "SkipCI";

        /// <summary>
        /// Specifies that the test should be skipped when calculating code coverage (e.g. due to slow performance).
        /// </summary>
        public const string SkipCoverage = "SkipCoverage";

        /// <summary>
        /// Specifies that the test has had a tendency to be unreliable (e.g. due to subtle timing issues) and may need further investigation.
        /// </summary>
        public const string Unreliable = "Unreliable";
    }
}
