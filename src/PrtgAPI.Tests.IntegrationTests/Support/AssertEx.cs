using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    class AssertEx
    {
        internal static bool HadFailure { get; set; }

        public static void AreEqual<T>(T expected, T actual, string message, bool retry = false)
        {
            ExecuteAssert(() => Assert.AreEqual(expected, actual, message), "Assert.AreEqual", retry);
        }

        public static void IsTrue(bool condition, string message)
        {
            ExecuteAssert(() => Assert.IsTrue(condition, message), "Assert.IsTrue");
        }

        public static void IsFalse(bool condition, string message)
        {
            ExecuteAssert(() => Assert.IsFalse(condition, message), "Assert.IsFalse");
        }

        public static void AreNotEqual<T>(T notExpected, T actual, string message)
        {
            ExecuteAssert(() => Assert.AreNotEqual(notExpected, actual, message), "Assert.AreNotEqual");
        }

        public static void Throws<T>(Action action, string message) where T : Exception
        {
            ExecuteAssert(() => UnitTests.AssertEx.Throws<T>(action, message), "AssertEx.Throws");
        }

        public static async Task ThrowsAsync<T>(Func<Task> action, string message) where T : Exception
        {
            await ExecuteAssertAsync(async () => await UnitTests.AssertEx.ThrowsAsync<T>(action, message), "AssertEx.Throws");
        }

        public static void AllPropertiesRetrieveValues(object value) => UnitTests.AssertEx.AllPropertiesRetrieveValues(value);

        private static void ExecuteAssert(Action assert, string assertName, bool retry = false)
        {
            try
            {
                HadFailure = false;
                assert();
            }
            catch (Exception ex)
            {
                HadFailure = true;

                if (!retry)
                {
                    var text = $"{assertName} failed. ";

                    if (ex.Message.StartsWith(text))
                        Logger.LogTestDetail(ex.Message.Substring(text.Length), true);
                    else
                        Logger.LogTestDetail(ex.Message, true);
                }

                throw;
            }
        }

        private static async Task ExecuteAssertAsync(Func<Task> assert, string assertName)
        {
            try
            {
                HadFailure = false;
                await assert();
            }
            catch (Exception ex)
            {
                HadFailure = true;

                var text = $"{assertName} failed. ";
                Logger.LogTestDetail(ex.Message.Substring(text.Length), true);
                throw;
            }
        }

        public static void Fail(string message, bool nonTest = false)
        {
            try
            {
                HadFailure = false;
                Assert.Fail(message);
            }
            catch (Exception ex)
            {
                HadFailure = true;

                var text = "Assert.Fail failed. ";

                if (nonTest)
                    Logger.LogTest(ex.Message.Substring(text.Length), true);
                else
                    Logger.LogTestDetail(ex.Message.Substring(text.Length), true);
                throw;
            }
        }

        public static void AreEqualLists<T>(List<T> first, List<T> second, IEqualityComparer<T> comparer, string message, bool retry = false) =>
            ExecuteAssert(() => UnitTests.AssertEx.AreEqualLists(first, second, comparer, message), "AssertEx.AreEqualLists", retry);

        public static void AreEqualLists<T>(List<T> first, List<T> second, string message) => AreEqualLists(first, second, null, message);

        public static void AllListElementsUnique<T>(List<T> list, IEqualityComparer<T> comparer) => ExecuteAssert(() => UnitTests.AssertEx.AllListElementsUnique(list, comparer), "AssertEx.AllListElementsUnique");
    }
}
