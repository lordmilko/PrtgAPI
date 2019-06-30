using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace PowerShell.TestAdapter
{
    class TestResultEx
    {
        private TestResult result;

        public TestResultEx(TestCase testCase, TestOutcome outcome, string errorMessage, string errorStackTrace)
        {
            result = new TestResult(testCase)
            {
                Outcome = outcome,
                ErrorMessage = errorMessage,
                ErrorStackTrace = errorStackTrace
            };
        }

        public static implicit operator TestResult(TestResultEx result)
        {
            return result.result;
        }
    }
}
