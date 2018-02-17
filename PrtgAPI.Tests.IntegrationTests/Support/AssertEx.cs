using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    class AssertEx
    {
        internal static bool HadFailure { get; set; }

        public static void AreEqual<T>(T expected, T actual, string message)
        {
            ExecuteAssert(() => Assert.AreEqual(expected, actual, message), "Assert.AreEqual");
        }

        public static void IsTrue(bool condition, string message)
        {
            ExecuteAssert(() => Assert.IsTrue(condition, message), "Assert.IsTrue");
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

        private static void ExecuteAssert(Action assert, string assertName)
        {
            try
            {
                HadFailure = false;
                assert();
            }
            catch (Exception ex)
            {
                HadFailure = true;

                var text = $"{assertName} failed. ";
                Logger.LogTestDetail(ex.Message.Substring(text.Length), true);
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
    }
}
