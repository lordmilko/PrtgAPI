using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Helpers;

namespace PrtgAPI.Tests.UnitTests
{
    public static class AssertEx
    {
        public static void AllPropertiesAreNull(object obj)
        {
            foreach (var prop in obj.GetType().GetProperties())
            {
                Assert.IsTrue(prop.GetValue(obj, null) == null, $"Property '{prop.Name}' was not null");
            }
        }

        public static void AllPropertiesAreNotDefault(object obj, Func<PropertyInfo, bool> customHandler = null)
        {
            if (customHandler == null)
                customHandler = p => false;

            foreach (var prop in obj.GetType().GetProperties())
            {
                if(!customHandler(prop))
                    Assert.IsFalse(TestReflectionHelpers.IsDefaultValue(prop, obj), $"Property '{prop.Name}' did not have a value.");
            }
        }

        public static void Throws<T>(Action action, string message) where T : Exception
        {
            try
            {
                action();

                Assert.Fail($"Expected an assertion of type {typeof (T)} to be thrown, however no exception occurred");
            }
            catch (T ex)
            {
                Assert.IsTrue(ex.Message.Contains(message), $"Exception message '{ex.Message}' did not contain string '{message}'");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(typeof (T), ex.GetType(), "Incorrect exception type thrown");
            }
        }

        public static async Task ThrowsAsync<T>(Func<Task> action, string message) where T : Exception
        {
            try
            {
                await action();

                Assert.Fail($"Expected an assertion of type {typeof(T)} to be thrown, however no exception occurred");
            }
            catch (T ex)
            {
                Assert.IsTrue(ex.Message.Contains(message), $"Exception message '{ex.Message}' did not contain string '{message}'");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(typeof(T), ex.GetType(), "Incorrect exception type thrown");
            }
        }
    }
}
