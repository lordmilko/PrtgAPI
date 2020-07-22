using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Attributes;
using PrtgAPI.PowerShell;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.PowerShell.Cmdlets;
using PrtgAPI.Request;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.Infrastructure
{
    [TestClass]
    public class AssemblyTests
    {
        [UnitTest]
        [TestMethod]
        public void PrtgCmdletTypes_DontImplement_ProcessRecord()
        {
            var assembly = Assembly.GetAssembly(typeof(PrtgCmdlet));

            var types = assembly.GetTypes().Where(t => typeof(PrtgCmdlet).IsAssignableFrom(t)).ToList();
            var result = types.Where(t => t.GetMethod("ProcessRecord", BindingFlags.Instance | BindingFlags.NonPublic)?.DeclaringType == t && t != typeof(PrtgCmdlet)).ToList();

            if (result.Count > 0)
            {
                Assert.Fail($"Types that derive from {nameof(PrtgCmdlet)} are not allowed to override method ProcessRecord. The following types contain ProcessRecord: {string.Join(", ", result.Select(t => t.Name))}");
            }
        }

        [UnitTest]
        [TestMethod]
        public void PrtgOperationCmdlets_Implement_Id()
        {
            var assembly = Assembly.GetAssembly(typeof(PrtgCmdlet));

            var exclusions = new[]
            {
                typeof(AddDevice),
                typeof(AddGroup),
                typeof(AddSensor),
                typeof(NewSensor),
                typeof(AddNotificationTrigger),
                typeof(SetNotificationTrigger),
                typeof(SetNotificationTriggerProperty),
                typeof(RemoveNotificationTrigger),
                typeof(CloneObject),
                typeof(RestartPrtgCore),
                typeof(SetChannelProperty) //Has a SensorId instead
            };

            var types = assembly.GetTypes().Where(t => typeof(PrtgOperationCmdlet).IsAssignableFrom(t) && !exclusions.Contains(t) && !t.IsAbstract).ToList();

            foreach (var type in types)
            {
                var property = type.GetProperty("Id");

                if (property == null)
                    Assert.Fail($"'{type}' does not have an -Id parameter");

                var defaultSet = type.GetCustomAttribute<CmdletAttribute>().DefaultParameterSetName;

                if (defaultSet == null)
                    Assert.Fail($"'{type}' does not specify a DefaultParameterSetName");
            }
        }

        [UnitTest]
        [TestMethod]
        public void AllRequestVersions_HaveVersionMaps()
        {
            var values = Enum.GetValues(typeof(RequestVersion)).Cast<RequestVersion>().ToList();

            foreach(var value in values)
            {
                var version = VersionMap.Map[value];

                Assert.AreEqual(version.ToString(), value.ToString().TrimStart('v').Replace("_", "."));
            }
        }

        [UnitTest]
        [TestMethod]
        public void ObjectPropertyFields_Have_ObjectPropertyCategories()
        {
            var values = Enum.GetValues(typeof (ObjectProperty)).Cast<ObjectProperty>().ToList();

            foreach (var val in values)
            {
                var category = val.GetEnumAttribute<CategoryAttribute>(true);
            }
        }

        [UnitTest]
        [TestMethod]
        public void InjectedProperties_On_ILazy_AreMarkedInternal()
        {
            var assembly = Assembly.GetAssembly(typeof(PrtgCmdlet));

            var types = assembly.GetTypes().Where(t => typeof(ILazy).IsAssignableFrom(t)).ToList();

            foreach (var type in types)
            {
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(p =>
                    {
                        var attributes = p.GetCustomAttributes<XmlElementAttribute>().ToList();

                        if (attributes.Count() > 1)
                            return false;

                        return attributes.FirstOrDefault()?.ElementName.StartsWith("injected") == true;
                    });

                foreach (var property in properties)
                    Assert.IsTrue(property.SetMethod.IsAssembly, $"Property '{property.Name}' is not marked Private or Internal");
            }
        }

#if WINDOWS
        [TestMethod]
        [UnitTest(TestCategory.SkipCI)]
        public void AllTextFiles_UseSpaces_AndCRLF()
        {
            var path = TestHelpers.GetProjectRoot(true);

            var types = new[]
            {
                ".tt",
                ".cs",
                ".ps1",
                ".psm1",
                ".txt",
                ".md",
                ".ps1xml"
            };

            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(f =>
                types.Any(f.EndsWith) && IsNotExcludedFolder(path, f)
            );

            var badNewLines = new List<string>();

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);

                if (text.Contains("\t"))
                    throw new Exception($"{file} contains tabs");

                var matches = Regex.Matches(text, "\n");

                foreach (Match match in matches)
                {
                    var index = match.Index;

                    var prev = text[index - 1];

                    if (prev != '\r')
                    {
                        badNewLines.Add(new FileInfo(file).Name);
                        break;
                    }
                }
            }

            if (badNewLines.Count > 0)
                throw new Exception($"{string.Join(", ", badNewLines) } are missing CRLF");
        }
#endif

        [UnitTest]
        [TestMethod]
        public void AllTextFiles_UseEnvironmentNewLine_AndNotCRLF()
        {
            var path = TestHelpers.GetProjectRoot(true);

            var types = new[]
            {
                ".tt",
                ".cs",
                ".ps1",
                ".psm1"
            };

            var allowed = new[]
            {
                "Location.cs",
                "SensorSettings.cs",
                "RequestParser.cs",
                "NewSensor.cs",
                "ConnectGoPrtgServer.cs",
                "GetGoPrtgServer.cs",
                "UpdateGoPrtgCredential.cs",
                "New-AppveyorPackage.ps1",
                "Get-CIVersion.ps1",
                "Get-CodeCoverage.ps1",
                "Invoke-Process.ps1",
                "Appveyor.Tests.ps1",
                "Start-PrtgAPI.ps1",
                "MethodXmlDocBuilder.cs",
                "New-PowerShellPackage.ps1"
            };

            var exprs = new[]
            {
                "\\\\n",
                "\\\\r",
                "`r",
                "`n"
            };

            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(f =>
                types.Any(f.EndsWith) && IsNotExcludedFolder(path, f) && !f.Contains("PrtgAPI.Tests")
            );

            foreach (var file in files)
            {
                var info = new FileInfo(file);

                if (allowed.Contains(info.Name))
                    continue;

                var text = File.ReadAllText(file);

                foreach (var expr in exprs)
                {
                    var matches = Regex.Matches(text, expr);

                    foreach (Match match in matches)
                    {
                        var index = match.Index;

                        var prev = text[index - 1];
                        var next = text[index + 2];

                        if (prev == '\'' && next == '\'')
                            continue;

                        throw new Exception($"{file} contains {expr}");
                    }
                }
            }
        }

        private bool IsNotExcludedFolder(string root, string f)
        {
            var illegal = new[]
            {
                "obj",
                "packages"
            };

            var info = new FileInfo(new Uri(f).LocalPath);

            if (root.Length > info.DirectoryName.Length)
                return true; //File in the root

            var str = info.DirectoryName.Substring(root.Length);

            if (illegal.Any(n => str.Contains($"{n}{Path.DirectorySeparatorChar}")))
                return false;

            return true;
        }

        [UnitTest]
        [TestMethod]
        public void AllReflection_UsesCacheManagers()
        {
            WithTree((file, tree, model) =>
            {
                foreach (var item in tree.GetRoot().DescendantNodesAndTokens())
                {
                    if (item.IsKind(SyntaxKind.InvocationExpression))
                        InspectMethodCall(item, model.Value);
                }
            });
        }

        [UnitTest]
        [TestMethod]
        public void AllPowerShell_UsesErrorRecords()
        {
            var allowedUsages = new object[]
            {
                Tuple.Create("NewSensorDynamicParameterCategory.cs", "ValidateParameters",  "new InvalidOperationException(newMessage, ex)"),
                Tuple.Create("NewNotificationTriggerParameters.cs",  "CreateParameters",    "ex.InnerException"),
                Tuple.Create("PrtgCmdlet.cs",                        "BeginProcessing",     "new InvalidOperationException(\"You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer.\")"),
                Tuple.Create("GetPrtgClient.cs",                     "WriteDiagnostic",     "new InvalidOperationException(\"You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer.\")"),
                Tuple.Create("GetSensorHistory.cs",                  "ProcessRecordEx",     "new InvalidOperationException($\"Cannot retrieve downtime with an {nameof(Average)} of 0.\")"),
                Tuple.Create("NewSensorParameters.cs",               "CreateRawParameters", "new InvalidOperationException($\"Hashtable record '{NameParameter}' is mandatory, however a value was not specified.\")"),
                Tuple.Create("NewSensorParameters.cs",               "CreateRawParameters", "new InvalidOperationException($\"Hashtable record '{SensorTypeParameter}' is mandatory, however a value was not specified.\")"),
                Tuple.Create("ConnectPrtgServer.cs",                 "Connect",             "new InvalidOperationException($\"Already connected to server {PrtgSessionState.Client.Server}. To override please specify -Force.\")"),
                Tuple.Create("RestartProbe.cs",                      "WriteProbeProgress",  "new TimeoutException($\"Timed out waiting for {remaining} {(\"probe\".Plural(remaining))} to restart.\")"),
                Tuple.Create("RestartPrtgCore.cs",                   "WriteProgress",       "new TimeoutException($\"Timed out waiting for PRTG Core Service to restart.\")"),
                Tuple.Create("NewSensorFactoryDefinition.cs",        "MakeChannel",         "new InvalidOperationException($\"'{value}' is not a valid channel expression. Expression must not be null, empty or whitespace.\")"),
                Tuple.Create("NewSensorFactoryDefinition.cs",        "GetChannelName",      "new InvalidOperationException($\"'{finalName}' is not a valid channel name. Name must not be null, empty or whitespace.\")"),
                Tuple.Create("SetObjectPosition.cs",                 "ProcessRecordEx",     "new InvalidOperationException($\"Cannot modify position of object '{obj}' (ID: {obj.Id}, Type: {obj.Type}). Object must be a sensor, device, group or probe.\")"),
                Tuple.Create("PrtgTableNodeCmdlet.cs",               "ProcessValues",       "new InvalidOperationException($\"Expected -{nameof(ScriptBlock)} to return one or more values of type '{nameof(PrtgNode)}', however response contained an invalid value of type '{invalid[0].GetType().FullName}'.\")"),
                Tuple.Create("NewSensorParameters.cs",               "GetImplicit"),
                Tuple.Create("TriggerParameterParser.cs",            "UpdateNotificationAction"),
                Tuple.Create("UpdateGoPrtgCredential.cs"),
                Tuple.Create("GoPrtgCmdlet.cs"),
                Tuple.Create("InstallGoPrtgServer.cs"),
                Tuple.Create("UninstallGoPrtgServer.cs"),
                Tuple.Create("SetGoPrtgAlias.cs"),
                Tuple.Create("ProgressManager.cs")
            };

            var found = new List<Tuple<string, string, string>>();

            var allowedTypes = new[]
            {
                nameof(NotImplementedException),
                nameof(ArgumentNullException),
                nameof(ArgumentException),
                nameof(UnknownParameterSetException),
                nameof(PipelineStoppedException),
                nameof(NotSupportedException),
                nameof(ParameterBindingException),
                nameof(PSArgumentException),
                nameof(NonTerminatingException)
            };

            Func<string, bool> IsFileAllowed = f => allowedUsages.OfType<Tuple<string>>().Any(a => a.Item1 == f);
            Func<string, string, bool> IsMethodAllowed = (f, m) => allowedUsages.OfType<Tuple<string, string>>().Any(a => a.Item1 == f && a.Item2 == m);
            Func<string, string, string, bool> IsExceptionAllowed = (f, m, e) => allowedUsages.OfType<Tuple<string, string, string>>().Any(a => a.Item1 == f && a.Item2 == m && a.Item3 == e);

            Func<string, string, string, bool> IsAllowed = (f, m, e) => IsFileAllowed(f) || IsMethodAllowed(f, m) || IsExceptionAllowed(f, m, e);

            WithTree((file, tree, model) =>
            {
                var fileName = Path.GetFileName(file);

                foreach (var item in tree.GetRoot().DescendantNodes())
                {
                    if (item.IsKind(SyntaxKind.ThrowStatement))
                    {
                        var memberName = GetMemberName(item);

                        var throwSyntax = item as ThrowStatementSyntax;
                        var objectCreationSyntax = throwSyntax.Expression as ObjectCreationExpressionSyntax;

                        if (throwSyntax.Expression == null)
                            continue;

                        if (objectCreationSyntax == null)
                        {
                            var exceptionStr = throwSyntax.Expression.ToString();

                            if (IsAllowed(fileName, memberName, throwSyntax.Expression.ToString()))
                                continue;

                            found.Add(Tuple.Create(fileName, memberName, exceptionStr));
                        }
                        else
                        {
                            string type = null;

                            switch (objectCreationSyntax.Type.Kind())
                            {
                                case SyntaxKind.IdentifierName:
                                    type = ((IdentifierNameSyntax) objectCreationSyntax.Type).Identifier.ValueText;
                                    break;
                                case SyntaxKind.QualifiedName:
                                    type = ((QualifiedNameSyntax) objectCreationSyntax.Type).Right.Identifier.ValueText;
                                    break;
                                default:
                                    throw new NotImplementedException($"Don't know how to handle syntax of type '{objectCreationSyntax.Type.Kind()}'.");
                            }

                            var exceptionStr = objectCreationSyntax.ToString();

                            if (!allowedTypes.Contains(type))
                            {
                                if (IsAllowed(fileName, memberName, exceptionStr))
                                    continue;

                                found.Add(Tuple.Create(fileName, memberName, exceptionStr));
                            }
                        }
                    }
                }
            }, true);

            if (found.Count > 0)
            {
                var str = string.Join("\n\n", found.Select(f => $"{f.Item1} -> {f.Item2} -> {f.Item3}"));

                Assert.Fail($"Found {found.Count} potentially illegal {"exception".Plural(found)}:\n\n{str}\n\nPlease convert these exceptions to ErrorRecords or whitelist these items.");
            }
        }

        private string GetMemberName(SyntaxNode item)
        {
            string memberName = null;

            var member = item.FirstAncestorOrSelf<MemberDeclarationSyntax>();

            var method = member as MethodDeclarationSyntax;

            if (method != null)
                memberName = method.Identifier.ValueText;

            var constructor = member as ConstructorDeclarationSyntax;

            if (constructor != null)
                memberName = constructor.Identifier.ValueText;

            var property = member as PropertyDeclarationSyntax;

            if (property != null)
                memberName = property.Identifier.ValueText;

            if (memberName != null)
                return memberName;

            throw new InvalidOperationException($"Don't know how to get the member name of {member}");
        }

        [UnitTest]
        [TestMethod]
        public void AllPublicAsyncMethods_AcceptACancellationToken_OrHaveAnOverloadThatDoes()
        {
            var methods = typeof(PrtgClient).GetMethods().Where(m => m.Name.EndsWith("Async"));
            var groups = methods.GroupBy(m => m.Name);

            foreach (var group in groups)
            {
                var subGroups = group.GroupBy(m => string.Join(", ",
                    m.GetParameters().Where(p => p.ParameterType != typeof(CancellationToken))
                        .Select(p => p.ParameterType.Name + " " + p.Name)));

                foreach (var subGroup in subGroups)
                {
                    if (!subGroup.Any(m => m.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken))))
                        Assert.Fail($"{group.Key}({subGroup.Key}) does not accept a {nameof(CancellationToken)} or have an overload that does");
                }
            }
        }

        [UnitTest]
        [TestMethod]
        public async Task AllPublicAsyncMethods_FailWithACancelledCancellationToken()
        {
            var methods = typeof(PrtgClient).GetMethods().Where(m => m.Name.EndsWith("Async"));
            var groups = methods.GroupBy(m => m.Name);

            var client = PrtgClientTests.GetDefaultClient(true);

            foreach (var group in groups)
            {
                var subGroups = group.GroupBy(
                    m => string.Join(", ", m.GetParameters()
                        .Where(p => p.ParameterType != typeof(CancellationToken))
                        .Select(p => p.ParameterType.Name + " " + p.Name)
                    )
                );

                foreach (var subGroup in subGroups)
                {
                    var overloads = subGroup.Where(m =>
                        m.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)));

                    foreach (var overload in overloads)
                    {
                        var m = PrtgClientTests.GetCustomMethod(overload);

                        var cts = new CancellationTokenSource();
                        cts.Cancel();

                        var parameters = PrtgClientTests.GetParameters(overload);
                        parameters[parameters.Length - 1] = cts.Token;

                        var task = (Task)m.Invoke(client, parameters);

                        try
                        {
                            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await task, "was canceled.");
                        }
                        catch (AssertFailedException)
                        {
                            Assert.Fail($"Method '{m}' did not throw an exception");
                        }
                    }

                    var overloadsWithout = subGroup.Where(m => m.GetParameters().All(p => p.ParameterType != typeof(CancellationToken)));

                    foreach (var overload in overloadsWithout)
                    {
                        var m = PrtgClientTests.GetCustomMethod(overload);

                        var cts = new CancellationTokenSource();
                        cts.Cancel();

                        client.DefaultCancellationToken = cts.Token;

                        var parameters = PrtgClientTests.GetParameters(overload);

                        var task = (Task)m.Invoke(client, parameters);

                        try
                        {
                            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await task, "was canceled.");
                        }
                        catch (AssertFailedException)
                        {
                            Assert.Fail($"Method '{m}' did not throw an exception");
                        }
                    }
                }
            }
        }

        private SyntaxToken GetMethodName(InvocationExpressionSyntax invocationNode)
        {
            var expression = invocationNode.Expression;

            switch (expression.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                    return ((MemberAccessExpressionSyntax) expression).Name.Identifier;
                case SyntaxKind.IdentifierName:
                    return ((IdentifierNameSyntax) expression).Identifier;
                case SyntaxKind.GenericName:
                    return ((GenericNameSyntax) expression).Identifier;
                case SyntaxKind.MemberBindingExpression:
                    return ((MemberBindingExpressionSyntax) expression).Name.Identifier;
                case SyntaxKind.ParenthesizedExpression:
                case SyntaxKind.ElementAccessExpression:
                    return default(SyntaxToken);
                default:
                    throw new NotImplementedException($"Unknown expression kind {invocationNode.Expression.GetType()}");
            }
        }

        private void InspectMethodCall(SyntaxNodeOrToken item, SemanticModel model)
        {
            var reflectionMethods = new[]
            {
                "GetCustomAttribute",
                "GetProperties",
                "GetField"
            };

            var excludedClasses = new[]
            {
                //Caches
                "AttributeCache",
                "ReflectionExtensions",
                "TypeCache",
                "XmlSerializerMembers",

                //Mistaken identity
                "TreeBuilderLevel"
            };

            var invocationNode = (InvocationExpressionSyntax) item.AsNode();

            var methodName = GetMethodName(invocationNode).Text;

            var reflectionCall = reflectionMethods.FirstOrDefault(f => methodName.StartsWith(f));

            if (reflectionCall != null)
            {
                var myClass = invocationNode.FirstAncestorOrSelf<ClassDeclarationSyntax>(t => true);

                var className = myClass.Identifier.Text;

                if (!excludedClasses.Contains(className))
                {
                    var invocation = invocationNode.FirstAncestorOrSelf<InvocationExpressionSyntax>(t => true);

                    var symbol = model.GetSymbolInfo(invocation);

                    var sym = symbol.Symbol ?? symbol.CandidateSymbols.FirstOrDefault();

                    if (sym == null)
                        throw new Exception($"Encountered a suspicious use of '{reflectionCall}' at {className} {invocationNode.GetLocation().GetLineSpan()}");

                    if (!sym.ContainingAssembly.Name.Contains("MyCompilation"))
                        throw new Exception($"Class '{className}' contains calls to reflection method '{reflectionCall}' at {invocationNode.GetLocation().GetLineSpan()}");
                }
            }
        }

        [TestMethod]
        [UnitTest(TestCategory.SkipCoverage)]
        public void AllAwaits_Call_ConfigureAwaitFalse()
        {
            WithTree((file, tree, model) =>
            {
                foreach (var item in tree.GetRoot().DescendantNodesAndTokens())
                {
                    if (item.IsKind(SyntaxKind.AwaitExpression))
                    {
                        var awaitNode = (AwaitExpressionSyntax) item.AsNode();
                        var expression = FindConfigureAwaitExpression(awaitNode);

                        try
                        {
                            if (expression != null)
                                ValidateConfigureAwait(expression);
                            else
                                throw new Exception("Bad");
                        }
                        catch (Exception)
                        {
                            var child = (SyntaxNode) awaitNode;
                            var parent = child.Parent;

                            while (parent != null)
                            {
                                if (parent is MethodDeclarationSyntax)
                                    break;

                                child = parent;
                                parent = child.Parent;
                            }

                            var method = (MethodDeclarationSyntax) parent;

                            var location = awaitNode.GetLocation();

                            throw new Exception($"{file}: Missing ConfigureAwait with method{Environment.NewLine}{Environment.NewLine}{method.Identifier}{Environment.NewLine}{Environment.NewLine}at {location.GetLineSpan()}");
                        }
                    }
                }
            });
        }

        [TestMethod]
        [UnitTest(TestCategory.SkipCoverage)]
        public void AllPowerShellExamples_Have_ProperSpacingBetweenEachOne()
        {
            WithTree((file, tree, model) =>
            {
                if (!file.Contains("Cmdlets"))
                    return;

                foreach (var item in tree.GetRoot().DescendantNodes())
                {
                    if (item.IsKind(SyntaxKind.ClassDeclaration))
                    {
                        var trivia = item.GetLeadingTrivia().Select(t => t.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();

                        if (trivia == null)
                            return;

                        var summary = trivia.ChildNodes().OfType<XmlElementSyntax>().FirstOrDefault();

                        if (summary != null && summary.StartTag.Name.ToString() == "summary")
                        {
                            var examples = summary.ChildNodes().OfType<XmlElementSyntax>().Where(c => c.StartTag.Name.ToString() == "example").ToList();

                            for (var i = 0; i < examples.Count; i++)
                            {
                                var exampleChildren = examples[i].ChildNodes().Where(n => n is XmlElementSyntax || n is XmlEmptyElementSyntax).ToList();

                                var code = exampleChildren.First();
                                var para = exampleChildren.Skip(1).Take(exampleChildren.Count - 2);
                                var last = exampleChildren.Last();

                                Assert.IsInstanceOfType(code, typeof(XmlElementSyntax), $"File '{file}' contains an example with a <code> block of an invalid type. Example is '{examples[i]}'");
                                Assert.AreEqual("code", ((XmlElementSyntax)code).StartTag.Name.ToString(), $"File '{file}' contains an example that does not start with <code>...</code>. Example is '{examples[i]}'");

                                foreach (var p in para)
                                {
                                    Assert.IsInstanceOfType(p, typeof(XmlElementSyntax), $"File '{file}' contains an example with a <para> block of an invalid type. Example is '{examples[i]}'");
                                    Assert.AreEqual("para", ((XmlElementSyntax) p).StartTag.Name.ToString(), $"File '{file}' contains an example with an invalid tag where a <para> should be. Example is '{examples[i]}'");
                                }

                                if (i < examples.Count - 1)
                                {
                                    //If this is NOT the last example, it SHOULD end with an empty <para/>

                                    //first should be xmlelementsyntax with code
                                    //after first, all but last should be xmlelementsyntax with para
                                    //last should be empty xmlelementsyntax

                                    Assert.IsInstanceOfType(last, typeof(XmlEmptyElementSyntax), $"File '{file}' does not contain a trailing <para/> in example '{examples[i]}'");
                                    Assert.AreEqual("para", ((XmlEmptyElementSyntax)last).Name.ToString(), $"File '{file}' has an empty trailing tag of the wrong type instead of <para/> in example '{examples[i]}'");
                                }
                                else
                                {
                                    //If this IS the last example, it should NOT end with an empty <para/>
                                    Assert.IsNotInstanceOfType(last, typeof(XmlEmptyElementSyntax), $"The last example in file '{file}' contained an empty <para/>, but it shouldn't have");
                                    Assert.AreEqual("para", ((XmlElementSyntax)last).StartTag.Name.ToString(), $"The last example in file '{file}' did not end with a <para>...</para>, but it should have");
                                }
                            }
                        }
                    }
                }
            }, true);
        }

        [TestMethod]
        [UnitTest(TestCategory.SkipCoverage)]
        public void All_PrtgAPI_ExceptionMessages_EndInAPeriod()
        {
            WithTree(AllExceptionMessages_EndInAPeriodInternal);
        }

        [TestMethod]
        [UnitTest(TestCategory.SkipCoverage)]
        public void All_PowerShell_ExceptionMessages_EndInAPeriod()
        {
            WithTree(AllExceptionMessages_EndInAPeriodInternal, true);
        }

        private void AllExceptionMessages_EndInAPeriodInternal(string file, SyntaxTree tree, Lazy<SemanticModel> model)
        {
            foreach (var item in tree.GetRoot().DescendantNodes())
            {
                if (item.IsKind(SyntaxKind.ThrowStatement))
                {
                    var child = item.ChildNodes().FirstOrDefault();

                    if (child != null && child.IsKind(SyntaxKind.ObjectCreationExpression))
                    {
                        var syntax = child as ObjectCreationExpressionSyntax;

                        var args = syntax.ArgumentList.Arguments.Where(IsNotNameOfParameter).ToList();

                        if (args.Count > 0)
                        {
                            var str = args.Select(a => a.ToString().TrimEnd('"').TrimEnd('\\')).ToList();

                            if (str.Count == 1)
                            {
                                if (args[0].ToString() != "str")
                                    AssertEndsInPeriod(syntax, str.Single(), file);
                            }
                            else
                            {
                                var secondParameter = new[]
                                {
                                    "paramName",
                                    "ex",
                                    "(Exception) null",
                                    "Object",
                                    "Parameters"
                                };

                                if (str[0] == "paramName" && str.Count == 2)
                                {
                                    AssertEndsInPeriod(syntax, str.Last(), file);
                                }
                                else if (str.Count == 2 && secondParameter.Contains(str[1]))
                                {
                                    AssertEndsInPeriod(syntax, str.First(), file);
                                }
                                else
                                {
                                    var exceptionType = ((IdentifierNameSyntax) syntax.Type).Identifier.ToString();

                                    var allowedExceptions = new string[]
                                    {
                                        nameof(InvalidTypeException),
                                        nameof(Exceptions.Internal.MissingAttributeException),
                                        nameof(InvalidTriggerTypeException),
                                        nameof(XmlDeserializationException),
                                        nameof(MissingMemberException)
                                    };

                                    if (!allowedExceptions.Contains(exceptionType))
                                        throw new NotImplementedException($"Don't know where exception message should be in exception '{syntax}' in file '{file}'");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AssertEndsInPeriod(ObjectCreationExpressionSyntax syntax, string str, string file)
        {
            if (!str.EndsWith(".") && !str.EndsWith("?") && !str.EndsWith("EnsurePeriod()}") && !str.EndsWith("{str}") && (str.StartsWith("$") || str.StartsWith("\"")))
                Assert.Fail($"Exception message in exception '{syntax}' in file '{file}' does not end in a period.");
        }

        private bool IsNotNameOfParameter(ArgumentSyntax syntax)
        {
            var invocation = syntax.Expression as InvocationExpressionSyntax;

            if (invocation != null)
            {
                var identifier = invocation.Expression as IdentifierNameSyntax;

                if (identifier != null && identifier.Identifier.Value?.ToString() == "nameof")
                    return false;
            }

            return true;
        }

        [TestMethod]
        [UnitTest(TestCategory.SkipCoverage)]
        public void AllPowerShellCmdlets_HaveOnlineHelp()
        {
            WithTree((file, tree, model) =>
            {
                if (!file.Contains("Cmdlets"))
                    return;

                foreach (var item in tree.GetRoot().DescendantNodes())
                {
                    if (item.IsKind(SyntaxKind.ClassDeclaration))
                    {
                        var trivia = item.GetLeadingTrivia().Select(t => t.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();

                        if (trivia == null)
                            return;

                        var summary = trivia.ChildNodes().OfType<XmlElementSyntax>().FirstOrDefault();

                        if (summary != null && summary.StartTag.Name.ToString() == "summary")
                        {
                            var links = summary.ChildNodes().OfType<XmlElementSyntax>().Where(IsLinkPara).ToList();

                            var firstLink = links.FirstOrDefault();

                            if (firstLink == null)
                                Assert.Fail($"File '{file}' is missing an Online version link (has no links at all)");
                            else
                            {
                                var content = firstLink.Content.ToString();
                                var uri = firstLink.StartTag.Attributes.OfType<XmlTextAttributeSyntax>().FirstOrDefault(a => a.Name.ToString() == "uri");

                                Assert.AreEqual("Online version:", content, $"File '{file}' is missing an Online version link");
                                Assert.IsTrue(uri != null && uri.TextTokens.ToString().StartsWith("https://github.com/lordmilko/PrtgAPI/wiki"), $"File '{file}' is missing an Online version URI");
                            }
                        }
                    }
                }
            }, true);
        }

        [UnitTest]
        [TestMethod]
        public void AllDebuggerDisplayProperties_AreDebuggerHidden_WithNoCodeCoverage()
        {
            var types = typeof(PrtgClient).Assembly.GetTypes().ToList();
            types.AddRange(typeof(PrtgCmdlet).Assembly.GetTypes());

            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

            var properties = types.SelectMany(p => p.GetProperties(flags)).Where(p => p.Name.Contains("DebuggerDisplay")).ToList();

            var unique = properties.Where(p => p.DeclaringType == p.ReflectedType).ToList();

            foreach (var property in unique)
            {
                var attribtes = property.GetCustomAttributes();

                if (!attribtes.OfType<ExcludeFromCodeCoverageAttribute>().Any())
                    Assert.Fail($"Property '{property}' on type '{property.DeclaringType}' is missing a '{nameof(ExcludeFromCodeCoverageAttribute)}'");

                var browsable = attribtes.OfType<DebuggerBrowsableAttribute>().FirstOrDefault();

                if (browsable == null)
                    Assert.Fail($"Property '{property}' on type '{property.DeclaringType}' is missing a '{nameof(DebuggerBrowsableAttribute)}'");

                if (browsable.State != DebuggerBrowsableState.Never)
                    Assert.Fail($"Property '{property}' on type '{property.DeclaringType}' had a {nameof(DebuggerBrowsableAttribute)} of state '{browsable.State}' instead of '{DebuggerBrowsableState.Never}'");
            }
        }

        [UnitTest]
        [TestMethod]
        public void AllUnitTestMethods_HaveUnitTestAttribute() => ValidateTestMethodCategory<UnitTestAttribute>(GetType().Assembly);

        [UnitTest]
        [TestMethod]
        public void AllPrtgObjectTypes_HaveInterfaceTypes()
        {
            var xmlDocExceptions = new[]
            {
                Tuple.Create("PrtgAPI.Device", "PrtgAPI.IDevice", "Group"),
                Tuple.Create("PrtgAPI.Sensor", "PrtgAPI.ISensor", "Group"),
                Tuple.Create("PrtgAPI.Device", "PrtgAPI.IDevice", "Probe"),
                Tuple.Create("PrtgAPI.Group", "PrtgAPI.IGroup", "Probe"),
                Tuple.Create("PrtgAPI.Sensor", "PrtgAPI.ISensor", "Probe"),
                Tuple.Create("PrtgAPI.PrtgObject", "PrtgAPI.IPrtgObject", "Name")
            };

            var types = typeof(PrtgObject).Assembly.GetTypes().Where(t => typeof(PrtgObject).IsAssignableFrom(t)).Where(
                t => t != typeof(NotificationAction) && t != typeof(Schedule) && t != typeof(Ticket) && t != typeof(UserAccount) && t != typeof(UserGroup)
            ).ToList();

            var classesAndInterfaces = new List<SyntaxNode>();

            WithTree((file, tree, model) =>
            {
                var results = tree.GetRoot().DescendantNodesAndSelf()
                    .Where(n => n.IsKind(SyntaxKind.ClassDeclaration) || n.IsKind(SyntaxKind.InterfaceDeclaration));

                classesAndInterfaces.AddRange(results);
            });

            var classSyntaxes = classesAndInterfaces.OfType<ClassDeclarationSyntax>().ToArray();
            var interfaceSyntaxes = classesAndInterfaces.OfType<InterfaceDeclarationSyntax>().ToArray();

            //First, check every interface (like IDevice) implements all of the public properties that
            //the class type implements (like Device) just in case we moved any around. Check that both
            //the property exists and its type is the same
            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                var thisInterface = interfaces.FirstOrDefault(i => i.Name == "I" + type.Name);

                if (thisInterface == null)
                    Assert.Fail($"Type '{type}' does not implement interface 'I{type.Name}'.");

                var interfaceChain = new Type[] {thisInterface}
                    .Concat(thisInterface.GetInterfaces());

                var typeProperties = type.GetProperties();
                var interfaceProperties = interfaceChain
                    .SelectMany(i => i.GetProperties())
                    .ToArray();

                foreach (var property in typeProperties)
                {
                    var interfaceProperty = interfaceProperties.FirstOrDefault(p => p.Name == property.Name);

                    if (interfaceProperty == null)
                        Assert.Fail($"Property '{property}' is missing from interface '{thisInterface}'");

                    if (property.PropertyType != interfaceProperty.PropertyType)
                        Assert.Fail($"Expected property '{interfaceProperty}' on interface '{thisInterface}' to be of type '{property.PropertyType}', but was of type '{interfaceProperty.PropertyType}' instead");
                }

                //Next, check that the XML Docs on both the interface property and the class property are in sync.
                var classSyntax = classSyntaxes.First(c => c.Identifier.ValueText == type.Name);
                var interfaceSyntax = interfaceSyntaxes.First(c => c.Identifier.ValueText == thisInterface.Name);

                var classPropertySyntaxes = classSyntax.DescendantNodes()
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(p => p.Modifiers.Any(m => m.ValueText == "public"))
                    .ToArray();
                var interfacePropertySyntaxes = interfaceSyntax.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToArray();

                Func<SyntaxNode, XmlElementSyntax> getXmlDoc = node =>
                {
                    var trivia = node.GetLeadingTrivia().Select(t => t.GetStructure())
                        .OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();

                    var summary = trivia?.ChildNodes().OfType<XmlElementSyntax>().FirstOrDefault();

                    return summary;
                };

                foreach (var classPropertySyntax in classPropertySyntaxes)
                {
                    var interfacePropertySyntax = interfacePropertySyntaxes.FirstOrDefault(i => i.Identifier.ValueText == classPropertySyntax.Identifier.ValueText);

                    if (interfacePropertySyntax == null)
                    {
                        var interfaceNames = interfaceChain.Select(i => i.Name).ToArray();

                        var candidateSyntaxList = interfaceSyntaxes.Where(i => interfaceNames.Contains(i.Identifier.ValueText)).ToList();

                        interfacePropertySyntax = candidateSyntaxList
                            .SelectMany(c => c.DescendantNodes())
                            .OfType<PropertyDeclarationSyntax>()
                            .FirstOrDefault(p => p.Identifier.ValueText == classPropertySyntax.Identifier.ValueText);

                        if (interfacePropertySyntax == null)
                            Assert.Fail($"Don't know where property '{classPropertySyntax.Identifier.Value}' from class {classPropertySyntax.Identifier.Value} is.");
                    }

                    var classXml = getXmlDoc(classPropertySyntax)?.Content.ToString().Trim('\n', '\r', '/', ' ');
                    var interfaceXml = getXmlDoc(interfacePropertySyntax)?.Content.ToString().Trim('\n', '\r', '/', ' ');

                    if (classXml != interfaceXml)
                    {
                        var exception = xmlDocExceptions.FirstOrDefault(e =>
                            e.Item1 == type.FullName && e.Item2 == thisInterface.FullName &&
                            e.Item3 == classPropertySyntax.Identifier.ValueText);

                        if (exception == null)
                            Assert.Fail($"Class XML '{classXml}' did not match interface XML '{interfaceXml}'.\r\n\r\nClass: '{type}'\r\nInterface: '{thisInterface}'\r\nProperty: '{classPropertySyntax.Identifier.Value}'");
                    }
                }
            }
        }

        public static void ValidateTestMethodCategory<TCategory>(Assembly assembly) where TCategory : TestCategoryBaseAttribute
        {
            var methods = assembly.GetTypes().SelectMany(TestHelpers.GetTests).ToArray();

            foreach (var method in methods)
            {
                var category = method.GetCustomAttribute<TCategory>();

                if (category == null)
                    Assert.Fail($"Test '{method.Name}' in class '{method.DeclaringType}' is missing a '{typeof(TCategory).Name}'.");

                var allowedTypes = new[]
                {
                    typeof(TCategory),
                    typeof(TestMethodAttribute),
                    typeof(AsyncStateMachineAttribute),
                    typeof(DebuggerStepThroughAttribute),
#if MSTEST2
                    typeof(DoNotParallelizeAttribute)
#endif
                };

                var illegalAttributes = method.GetCustomAttributes().Where(a => !allowedTypes.Contains(a.GetType())).ToList();

                if (illegalAttributes.Count > 0)
                {
                    var str = string.Join(", ", illegalAttributes.Select(a => a.GetType().Name));

                    Assert.Fail($"Test '{method.Name}' in class '{method.DeclaringType}' is decorated with additional illegal types of type {str}.");
                }
            }
        }

        private bool IsLinkPara(XmlElementSyntax elm)
        {
            if (elm.StartTag.Name.ToString() == "para")
            {
                return elm.StartTag.Attributes.OfType<XmlTextAttributeSyntax>().Any(a => a.TextTokens.ToString() == "link");
            }

            return false;
        }

        private void WithTree(Action<string, SyntaxTree, Lazy<SemanticModel>> action, bool powerShell = false)
        {
            var path = TestHelpers.GetProjectRoot();

            if (powerShell)
                path += ".PowerShell";

            var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));

                var model = new Lazy<SemanticModel>(() =>
                {
                    var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                    var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { mscorlib });

                    return compilation.GetSemanticModel(tree);
                });

                action(file, tree, model);
            }
        }

        private InvocationExpressionSyntax FindConfigureAwaitExpression(SyntaxNode node)
        {
            foreach (var item in node.ChildNodes())
            {
                if (item is InvocationExpressionSyntax)
                    return (InvocationExpressionSyntax)item;

                return FindConfigureAwaitExpression(item);
            }

            return null;
        }

        private void ValidateConfigureAwait(InvocationExpressionSyntax expression)
        {
            var memberAccess = expression.Expression as MemberAccessExpressionSyntax;

            if (memberAccess == null)
                throw new Exception("Expression was not a member access expression"); //todo: what is a member access expression?

            if (!memberAccess.Name.Identifier.Text.Equals("ConfigureAwait"))
                throw new Exception("ConfigureAwait was not called");

            var args = expression.ArgumentList;

            if (args.Arguments.Count != 1)
                throw new Exception("ConfigureAwait is missing 'false'");

            if (args.Arguments.First().Expression.IsKind(SyntaxKind.FalseLiteralExpression) == false)
                throw new Exception("ConfigureAwait has a value other than 'false'");
        }
    }
}
