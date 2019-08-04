using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support
{
    public static class TestHelpers
    {
        public static List<MethodInfo> GetTests(Type type)
        {
            return type.GetMethods().Where(m => m.GetCustomAttribute<TestMethodAttribute>() != null).ToList();
        }

        public static void Assert_TestClassHasMethods(Type testClass, List<string> expectedMethods)
        {
            var methods = GetTests(testClass);

            var missing = expectedMethods.Where(e => !methods.Any(
                    m => m.Name == e || m.Name.StartsWith($"{e}_")
                )
            ).OrderBy(m => m).ToList();

            if (missing.Count > 0)
                Assert.Fail($"{missing.Count}/{expectedMethods.Count} tests are missing: " + string.Join(", ", missing));
        }

        internal static string GetProjectRoot(bool solution = false)
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

        public static bool IsPrtgAPIClass(object obj)
        {
            if (obj == null)
                return false;

            var t = obj.GetType();

            return t.IsClass && t.Namespace.StartsWith("PrtgAPI");
        }

        internal static void WithPSObjectUtilities(Action action, IPSObjectUtilities utilities)
        {
            var instance = typeof(PSObjectUtilities).GetInternalStaticFieldInfo("instance");

            try
            {
                instance.SetValue(null, utilities);

                action();
            }
            finally
            {
                instance.SetValue(null, null);
            }
        }
    }
}
