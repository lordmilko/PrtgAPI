using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace PowerShell.TestAdapter
{
    [DefaultExecutorUri(TestExecutor.ExecutorUriString)]
    public class TestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink)
        {
            GetTests(sources, discoverySink, logger);
        }

        public static List<TestCase> GetTests(IEnumerable<string> sources, ITestCaseDiscoverySink discoverySink, IMessageLogger logger = null)
        {
            var tests = new List<TestCase>();

            foreach (var source in sources)
            {
                DiscoverPesterTests(discoverySink, logger, source, tests);
            }

            return tests;
        }

        private static void DiscoverPesterTests(ITestCaseDiscoverySink discoverySink, IMessageLogger logger, string source,
            List<TestCase> tests)
        {
            Log(TestMessageLevel.Informational, $"Searching for tests in {source}" , logger);

            Token[] tokens;
            ParseError[] errors;

            var script = Parser.ParseFile(source, out tokens, out errors);

            if (errors.Length > 0)
            {
                foreach (var error in errors)
                {
                    Log(TestMessageLevel.Error, $"Parser error. {error.Message}", logger);
                }

                return;
            }

            var describeBlocks = script.FindAll(
                m => (m is CommandAst) && string.Equals("Describe", (m as CommandAst).GetCommandName(), StringComparison.OrdinalIgnoreCase), 
                true
            );

            foreach (Ast describeBlock in describeBlocks)
            {
                string functionName = GetFunctionName(logger, describeBlock, "describe");

                var describeTags = GetDescribeTags(logger, describeBlock);

                var itBlocks = describeBlock.FindAll(
                    m => m is CommandAst && (m as CommandAst).GetCommandName() != null && (m as CommandAst)
                               .GetCommandName().Equals("it", StringComparison.OrdinalIgnoreCase), true);

                foreach (CommandAst itBlock in itBlocks)
                {
                    string itBlockName = GetFunctionName(logger, itBlock, "it");
                    string parentContextName = GetParentContextName(logger, itBlock);

                    if (string.IsNullOrEmpty(itBlockName))
                    {
                        Log(TestMessageLevel.Informational, "Test name was empty. Skipping test.", logger);
                    }
                    else
                    {
                        var testCase = new TestCase($"{functionName}.{parentContextName}.{itBlockName}", TestExecutor.ExecutorUri, source)
                        {
                            DisplayName = itBlockName,
                            CodeFilePath = source,
                            LineNumber = itBlock.Extent.StartLineNumber
                        };

                        foreach (string text in describeTags)
                        {
                            testCase.Traits.Add(text, string.Empty);
                        }

                        Log(TestMessageLevel.Informational, $"Adding test [{functionName}] in {source} at {testCase.LineNumber}.", logger);

                        if (discoverySink != null)
                        {
                            discoverySink.SendTestCase(testCase);
                        }

                        tests.Add(testCase);
                    }
                }
            }
        }

        private static string GetParentContextName(IMessageLogger logger, Ast ast)
        {
            if (ast.Parent is CommandAst && string.Equals("context", (ast.Parent as CommandAst).GetCommandName(), StringComparison.OrdinalIgnoreCase))
            {
                return GetFunctionName(logger, ast.Parent, "context");
            }

            if (ast.Parent != null)
            {
                return GetParentContextName(logger, ast.Parent);
            }

            return "No Context";
        }

        private static string GetFunctionName(IMessageLogger logger, Ast context, string functionName)
        {
            var contextAst = (CommandAst)context;
            var contextName = string.Empty;
            bool nextElementIsName1 = false;

            foreach (var element in contextAst.CommandElements)
            {
                if (element is StringConstantExpressionAst && !(element as StringConstantExpressionAst).Value.Equals(functionName, StringComparison.OrdinalIgnoreCase))
                {
                    contextName = (element as StringConstantExpressionAst).Value;
                    break;
                }

                if (nextElementIsName1 && element is StringConstantExpressionAst)
                {
                    contextName = (element as StringConstantExpressionAst).Value;
                    break;
                }

                if (element is CommandParameterAst && (element as CommandParameterAst).ParameterName.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    nextElementIsName1 = true;
                }
            }

            return contextName;
        }

        private static IEnumerable<string> GetDescribeTags(IMessageLogger logger, Ast context)
        {
            var contextAst = (CommandAst)context;
            var contextName = string.Empty;
            bool nextElementIsName1 = false;

            foreach (var element in contextAst.CommandElements)
            {
                if (nextElementIsName1)
                {
                    var tagStrings = element.FindAll(m => m is StringConstantExpressionAst, true);

                    foreach (StringConstantExpressionAst tag in tagStrings)
                    {
                        yield return tag.Value;
                    }
                    break;
                }

                if (element is CommandParameterAst && "tags".Contains((element as CommandParameterAst).ParameterName.ToLower()))
                {
                    nextElementIsName1 = true;
                }
            }
        }

        private static void Log(TestMessageLevel level, string message, IMessageLogger logger)
        {
            if (logger != null)
            {
                logger.SendMessage(level, message);
            }
        }
    }
}
