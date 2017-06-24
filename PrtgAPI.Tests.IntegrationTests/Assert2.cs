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
        public static void AreEqual<T>(T expected, T actual, string message)
        {
            try
            {
                Assert.AreEqual(expected, actual, message);
            }
            catch(Exception ex)
            {
                var text = "Assert.AreEqual failed. ";
                Logger.LogTestDetail(ex.Message.Substring(text.Length), true);
                throw;
            }
        }

        public static void IsTrue(bool condition, string message)
        {
            try
            {
                Assert.IsTrue(condition, message);
            }
            catch (Exception ex)
            {
                var text = "Assert.IsTrue failed. ";
                Logger.LogTestDetail(ex.Message.Substring(text.Length), true);
                throw;
            }
        }
    }
}
