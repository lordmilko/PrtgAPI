using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using PrtgAPI.Linq;
using PrtgAPI.Linq.Expressions.Pretty;
using PrtgAPI.Linq.Expressions.Visitors;

namespace PrtgAPI.Request.Serialization.CodeGen
{
#if DEBUG && DEBUG_SERIALIZATION
    class DynamicAssembly
    {
        const string name = "PrtgAPI.GeneratedCode";

        private static object lockObj = new object();

        static AssemblyBuilder assemblyBuilder;
        static AssemblyBuilder AssemblyBuilder
        {
            get
            {
                lock(lockObj)
                {
                    if (assemblyBuilder == null)
                    {
                        GenerateAssembly();
                        GenerateModule();
                    }
                }

                return assemblyBuilder;
            }
        }

        static ModuleBuilder moduleBuilder;

        static ModuleBuilder ModuleBuilder
        {
            get
            {
                lock(lockObj)
                {
                    if (moduleBuilder == null)
                    {
                        GenerateAssembly();
                        GenerateModule();
                    }
                }

                return moduleBuilder;
            }
        }

        TypeBuilder typeBuilder;
        List<MethodBuilder> methodBuilders = new List<MethodBuilder>();
        SymbolDocumentInfo symbolDocument;
        DebugInfoGenerator pdbGenerator = DebugInfoGenerator.CreatePdbGenerator();

        public DynamicAssembly(string sourceFileName)
        {
            symbolDocument = Expression.SymbolDocument(sourceFileName);
        }

        public static TDelegate Generate<TDelegate>(string typeName, LambdaExpression lambda, out TypeBuilder typeBuilder)
        {
            return (TDelegate)(object)Generate(typeName, lambda, out typeBuilder);
        }

        public static Delegate Generate(string typeName, LambdaExpression lambda, out TypeBuilder typeBuilder)
        {
            var temp = Path.GetTempPath();

            var file = $"{temp}\\{typeName}.cs";

            var dynamicAssembly = new DynamicAssembly(file);

            var ret = dynamicAssembly.GenerateInternal(typeName, lambda);

            typeBuilder = dynamicAssembly.typeBuilder;

            return ret;
        }

        private Delegate GenerateInternal(string typeName, LambdaExpression lambda)
        {
            ProxyFactory.Initialize(ModuleBuilder);
            GenerateType(typeName);

            GenerateMethods(lambda);

            var type = typeBuilder.CreateType();

            var methodInfo = type.GetMethod(lambda.Name);

            var delegateType = GetDelegateType(lambda);

            var action = Delegate.CreateDelegate(delegateType, methodInfo);

            return action;
        }

        private Type GetDelegateType(LambdaExpression lambda)
        {
            var args = lambda.Parameters.ToList();
            args.Add(ProxyExpression.typeBuilderParameter);

            var newLambda = Expression.Lambda(lambda.Body, args);

            return newLambda.Type;
        }

        private static void GenerateAssembly()
        {
            var assemblyName = new AssemblyName(name);

            //RunAndSave must be specified to allow debugging in memory (even if we don't actually save it)
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);

            //Specify a DebuggableAttribute to enable debugging
            var attribute = typeof(DebuggableAttribute);
            var ctor = attribute.GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) });

            var attributeBuilder = new CustomAttributeBuilder(ctor, new object[]
            {
                DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.Default
            });

            assemblyBuilder.SetCustomAttribute(attributeBuilder);
        }

        private static void GenerateModule()
        {
            //All assemblies contain at least one module. This implementation detail is typically invisible
            moduleBuilder = assemblyBuilder.DefineDynamicModule(name, name + ".dll", true);
        }

        private void GenerateType(string typeName)
        {
            typeBuilder = ModuleBuilder.DefineType($"{name}.{typeName}", TypeAttributes.Public);
        }

        private void GenerateMethods(LambdaExpression lambda)
        {
            var stringWriter = new StringWriter();
            var formatter = new TextFormatter(stringWriter);
            var writer = new CSharpWriter(formatter, null);

            var usings = ExpressionSearcher.Search(lambda, e => true).Select(e => e.Type.Namespace).Distinct().ToList();

            var ordered = usings.OrderBy(s => s).OrderBy(s => !s.StartsWith("System")).ToList();

            foreach (var @using in ordered)
            {
                formatter.Write("using");
                formatter.WriteSpace();
                formatter.Write(@using);
                formatter.Write(";");
                formatter.WriteLine();
            }

            formatter.WriteLine();

            formatter.Write("namespace");
            formatter.WriteSpace();
            formatter.Write(name);
            formatter.WriteLine();

            writer.VisitBlock(() =>
            {
                formatter.Write($"public class {typeBuilder.Name}");
                formatter.WriteLine();

                writer.VisitBlock(() =>
                {
                    WriteLambdas(lambda, formatter);
                });

                formatter.WriteLine();
            });

            File.WriteAllText(symbolDocument.FileName, stringWriter.ToString());
        }

        private void WriteLambdas(LambdaExpression lambda, TextFormatter formatter)
        {
            var lambdas = ExpressionSearcher.Search(lambda, e => e.NodeType == ExpressionType.Lambda).Cast<LambdaExpression>().DistinctBy(l => l.Name).ToList();

            for(var i = 0; i < lambdas.Count; i++)
            {
                if (string.IsNullOrEmpty(lambdas[i].Name))
                    throw new InvalidOperationException($"Lambda {lambdas[i]} is missing a name");

                var methodBuilder = typeBuilder.DefineMethod(lambdas[i].Name, MethodAttributes.Public | MethodAttributes.Static);

                lambda.Compile();

                var debugLambda = GetDebugLambda(lambdas[i], formatter);

                debugLambda.Compile();

                debugLambda.CompileToMethod(methodBuilder, pdbGenerator);

                if (i < lambdas.Count - 1)
                {
                    formatter.WriteLine();
                }
            }
        }

        private LambdaExpression GetDebugLambda(LambdaExpression expr, TextFormatter formatter)
        {
            var writer = new CSharpWriter(formatter, symbolDocument);

            var debugLambda = writer.Print(expr);

            return debugLambda;
        }

        public void Save()
        {
            assemblyBuilder.Save(name + ".dll");
        }
    }
#endif
}
