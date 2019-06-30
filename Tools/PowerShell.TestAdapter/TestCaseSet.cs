using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using SMA = System.Management.Automation;

namespace PowerShell.TestAdapter
{
    public class TestCaseSet
    {
        public TestCaseSet(string fileName, string describe)
        {
            File = fileName;
            Describe = describe;
            TestCases = new List<TestCase>();
        }

        public string File { get; }

        public string Describe { get; }

        public List<TestCase> TestCases { get; }

        public List<TestResult> TestResults { get; private set; }

        public void ProcessTestResults(Array results)
        {
            TestResults = new List<TestResult>();

            foreach (object obj in results)
            {
                SMA.PSObject psobject = (SMA.PSObject)obj;
                string describe = psobject.Properties["Describe"].Value as string;

                if (!HandleParseError(psobject, describe))
                {
                    break;
                }

                string context = psobject.Properties["Context"].Value as string;
                string name = psobject.Properties["Name"].Value as string;

                if (string.IsNullOrEmpty(context))
                {
                    context = "No Context";
                }

                TestCase testCase = TestCases.FirstOrDefault(m => m.FullyQualifiedName == $"{describe}.{context}.{name}");

                if (testCase != null)
                {
                    TestResult testResult = new TestResult(testCase);
                    testResult.Outcome = GetOutcome(psobject.Properties["Result"].Value as string);
                    string errorStackTrace = psobject.Properties["StackTrace"].Value as string;
                    string errorMessage = psobject.Properties["FailureMessage"].Value as string;
                    testResult.ErrorStackTrace = errorStackTrace;
                    testResult.ErrorMessage = errorMessage;

                    TestResults.Add(testResult);
                }
            }
        }

        private bool HandleParseError(SMA.PSObject result, string describe)
        {
            string value = string.Format("Error in {0}", File);

            if (describe.Contains(value))
            {
                string errorStackTrace = result.Properties["StackTrace"].Value as string;
                string errorMessage = result.Properties["FailureMessage"].Value as string;

                foreach (TestCase testCase in TestCases)
                {
                    TestResult testResult = new TestResult(testCase);
                    testResult.Outcome = TestOutcome.Failed;
                    testResult.ErrorMessage = errorMessage;
                    testResult.ErrorStackTrace = errorStackTrace;
                    TestResults.Add(testResult);
                }

                return false;
            }
            return true;
        }

        private TestOutcome GetOutcome(string testResult)
        {
            switch (testResult?.ToLower())
            {
                case null:
                case "":
                    return TestOutcome.NotFound;
                case "passed":
                    return TestOutcome.Passed;
                case "skipped":
                case "pending":
                    return TestOutcome.Skipped;
                default:
                    return TestOutcome.Failed;
            }
        }
    }
}
