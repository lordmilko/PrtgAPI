using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Attributes;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Helpers;
using PrtgAPI.Request;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests
{
    [TestClass]
    public class AssemblyTests
    {
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

        [TestMethod]
        public void ObjectPropertyFields_Have_ObjectPropertyCategories()
        {
            var values = Enum.GetValues(typeof (ObjectProperty)).Cast<ObjectProperty>().ToList();

            foreach (var val in values)
            {
                var category = val.GetEnumAttribute<CategoryAttribute>(true);
            }
        }

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
                    Assert.IsTrue(property.SetMethod.IsAssembly, $"Property '{property.Name}' is not marked Internal");
            }
        }

        [TestMethod]
        [TestCategory("SkipCI")]
        public void AllTextFiles_UseSpaces_AndCRLF()
        {
            var path = GetProjectRoot(true);

            var types = new[]
            {
                ".tt",
                ".cs",
                ".ps1",
                ".txt",
                ".md",
                ".ps1xml"
            };

            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(f =>
                types.Any(f.EndsWith)
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

        [TestMethod]
        public void AllReflection_UsesCacheManagers()
        {
            WithTree((file, tree, model) =>
            {
                foreach (var item in tree.GetRoot().DescendantNodesAndTokens())
                {
                    if (item.IsKind(SyntaxKind.InvocationExpression))
                        InspectMethodCall(item, model);
                }
            });
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

            var cacheClasses = new[]
            {
                "AttributeCache",
                "ReflectionHelpers",
                "TypeCache"
            };

            var invocationNode = (InvocationExpressionSyntax)item.AsNode();

            var methodName = GetMethodName(invocationNode).Text;

            var reflectionCall = reflectionMethods.FirstOrDefault(f => methodName.StartsWith(f));

            if (reflectionCall != null)
            {
                var myClass = invocationNode.FirstAncestorOrSelf<ClassDeclarationSyntax>(t => true);

                var className = myClass.Identifier.Text;

                if (!cacheClasses.Contains(className))
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

                            throw new Exception($"{file}: Missing ConfigureAwait with method\r\n\r\n{method.Identifier}\r\n\r\nat {location.GetLineSpan()}");
                        }
                    }
                }
            });
        }

        private void WithTree(Action<string, SyntaxTree, SemanticModel> action)
        {
            var path = GetProjectRoot();

            var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));

                var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { mscorlib });

                var model = compilation.GetSemanticModel(tree);

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

        private string GetProjectRoot(bool solution = false)
        {
            var dll = new Uri(typeof(PrtgClient).Assembly.CodeBase);
            var root = dll.Host + dll.PathAndQuery + dll.Fragment;
            var rootStr = Uri.UnescapeDataString(root);

            var thisProject = Assembly.GetExecutingAssembly().GetName().Name;

            var prefix = rootStr.IndexOf(thisProject, StringComparison.InvariantCulture);

            var solutionPath = rootStr.Substring(0, prefix);

            if (solution)
                return solutionPath;

            return solutionPath + "PrtgAPI";
        }
    }
}
