using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Helpers;

namespace PrtgAPI.Tests.UnitTests
{
    static class Assert2
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
                    Assert.IsFalse(ReflectionHelpers.IsDefaultValue(prop, obj), $"Property '{prop.Name}' had value did not have a value.");
            }
        }
    }
}
