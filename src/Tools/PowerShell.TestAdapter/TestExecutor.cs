using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation.Runspaces;
using SMA = System.Management.Automation;
using System.Text;
using Microsoft.PowerShell;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace PowerShell.TestAdapter
{
    [ExtensionUri(ExecutorUriString)]
    public class TestExecutor : ITestExecutor
    {
        public const string ExecutorUriString = "executor://PowerShellTestExecutor/v1";
        public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);

        bool cancelled;

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            SetupExecutionPolicy();

            var tests = TestDiscoverer.GetTests(sources, null);

            RunTests(tests, runContext, frameworkHandle);
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            cancelled = false;
            SetupExecutionPolicy();

            var testSets = GetTestSets(tests);

            foreach (var testSet in testSets)
            {
                if (cancelled)
                    break;

                StringBuilder testOutput = new StringBuilder();

                try
                {
                    Runspace runspace = RunspaceFactory.CreateRunspace(new TestAdapterHost
                    {
                        HostUi =
                        {
                            OutputString = s =>
                            {
                                if (!string.IsNullOrEmpty(s))
                                    testOutput.Append(s);
                            }
                        }
                    });

                    runspace.Open();

                    using (var powerShell = SMA.PowerShell.Create())
                    {
                        powerShell.Runspace = runspace;

                        //Execute all the It statements in a particular Describe block and emit their results
                        RunTestSet(powerShell, testSet, runContext);

                        foreach (TestResult testResult in testSet.TestResults)
                        {
                            frameworkHandle.RecordResult(testResult);
                        }
                    }
                }
                catch (Exception ex)
                {
                    foreach (TestCase testCase2 in testSet.TestCases)
                    {
                        frameworkHandle.RecordResult(new TestResult(testCase2)
                        {
                            Outcome = TestOutcome.Failed,
                            ErrorMessage = ex.Message,
                            ErrorStackTrace = ex.StackTrace
                        });
                    }
                }

                if (testOutput.Length > 0)
                {
                    frameworkHandle.SendMessage(0, testOutput.ToString());
                }
            }
        }

        private List<TestCaseSet> GetTestSets(IEnumerable<TestCase> tests)
        {
            List<TestCaseSet> list = new List<TestCaseSet>();

            //Enumerate over every It statement identified from the test files passed to the test adapter
            foreach (TestCase testCase in tests)
            {
                string describe = testCase.FullyQualifiedName.Split('.').First();
                string codeFile = testCase.CodeFilePath;
                TestCaseSet testCaseSet = list.FirstOrDefault(m => m.Describe.EqualsIgnoreCase(describe) && m.File.EqualsIgnoreCase(codeFile));

                //If this is the first time we've seen a test for a particular Describe, add that Describe to the test set list
                if (testCaseSet == null)
                {
                    testCaseSet = new TestCaseSet(codeFile, describe);
                    list.Add(testCaseSet);
                }

                testCaseSet.TestCases.Add(testCase);
            }

            return list;
        }

        public void RunTestSet(SMA.PowerShell powerShell, TestCaseSet testCaseSet, IRunContext runContext)
        {
            SetupExecutionPolicy();

            ImportPester(powerShell, runContext);

            FileInfo fileInfo = new FileInfo(testCaseSet.File);
            
            powerShell.AddCommand("Invoke-Pester")
                .AddParameter("Path", fileInfo.FullName)
                .AddParameter("TestName", testCaseSet.Describe)
                .AddParameter("PassThru");

            Collection<SMA.PSObject> psObjects = powerShell.Invoke();

            powerShell.Commands.Clear();

            Array testResults = GetTestResults(psObjects);
            testCaseSet.ProcessTestResults(testResults);
        }

        private static Array GetTestResults(Collection<SMA.PSObject> psObjects)
        {
            return psObjects.FirstOrDefault((SMA.PSObject o) => o.Properties["TestResult"] != null).Properties["TestResult"].Value as Array;
        }

        #region ImportPester

        private void ImportPester(SMA.PowerShell powerShell, IRunContext runContext)
        {
            var module = FindModule("Pester", runContext);

            powerShell.AddCommand("Import-Module").AddParameter("Name", module);
            powerShell.Invoke();
            powerShell.Commands.Clear();

            if (powerShell.HadErrors)
            {
                var errorRecord = powerShell.Streams.Error.FirstOrDefault();
                var errorMessage = errorRecord == null ? string.Empty : errorRecord.ToString();
                throw new Exception($"Failed to load Pester module. {errorMessage}");
            }
        }

        private string FindModule(string moduleName, IRunContext runContext)
        {
            var pesterPath = GetModulePath(moduleName, runContext.TestRunDirectory);

            if (string.IsNullOrEmpty(pesterPath))
            {
                pesterPath = GetModulePath(moduleName, runContext.SolutionDirectory);
            }

            if (string.IsNullOrEmpty(pesterPath))
            {
                pesterPath = moduleName;
            }

            return pesterPath;
        }

        private static string GetModulePath(string moduleName, string root)
        {
            if (root == null)
                return null;

            // Default packages path for nuget.
            var packagesRoot = Path.Combine(root, "packages");

            if (Directory.Exists(packagesRoot))
            {
                var packagePath = Directory.GetDirectories(packagesRoot, moduleName + "*", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (null != packagePath)
                {
                    var psd1 = Path.Combine(packagePath, $@"tools\{moduleName}.psd1");
                    if (File.Exists(psd1))
                    {
                        return psd1;
                    }

                    var psm1 = Path.Combine(packagePath, $@"tools\{moduleName}.psm1");
                    if (File.Exists(psm1))
                    {
                        return psm1;
                    }

                    var dll = Path.Combine(packagePath, $@"tools\{moduleName}.dll");
                    if (File.Exists(dll))
                    {
                        return dll;
                    }
                }
            }

            return null;
        }

        #endregion

        public void Cancel()
        {
            cancelled = true;
        }

        private static void SetupExecutionPolicy()
        {
            SetExecutionPolicy(ExecutionPolicy.RemoteSigned, ExecutionPolicyScope.Process);
        }

        private static void SetExecutionPolicy(ExecutionPolicy policy, ExecutionPolicyScope scope)
        {
            ExecutionPolicy currentPolicy = ExecutionPolicy.Undefined;

            using (var ps = SMA.PowerShell.Create())
            {
                ps.AddCommand("Get-ExecutionPolicy");

                foreach (var result in ps.Invoke())
                {
                    currentPolicy = ((ExecutionPolicy)result.BaseObject);
                    break;
                }

                if ((currentPolicy <= policy || currentPolicy == ExecutionPolicy.Bypass) && currentPolicy != ExecutionPolicy.Undefined) //Bypass is the absolute least restrictive, but as added in PS 2.0, and thus has a value of '4' instead of a value that corresponds to it's relative restrictiveness
                    return;

                ps.Commands.Clear();

                ps.AddCommand("Set-ExecutionPolicy").AddParameter("ExecutionPolicy", policy).AddParameter("Scope", scope).AddParameter("Force");
                ps.Invoke();
            }
        }
    }
}
