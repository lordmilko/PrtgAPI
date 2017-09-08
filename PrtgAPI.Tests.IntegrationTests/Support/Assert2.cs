using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    class Assert2
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
