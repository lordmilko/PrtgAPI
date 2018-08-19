using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Helpers;
using PrtgAPI.Linq;
using PrtgAPI.Tests.UnitTests.Helpers;

namespace PrtgAPI.Tests.UnitTests
{
    public static class AssertEx
    {
        private static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

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

        public static void AllPropertiesAndFieldsAreNotDefault(object obj, Func<MemberInfo, bool> customHandler = null)
        {
            if (customHandler == null)
                customHandler = p => false;

            foreach (var prop in obj.GetType().GetProperties(flags).Where(p => !p.GetIndexParameters().Any()))
            {
                if (!customHandler(prop))
                    Assert.IsFalse(TestReflectionHelpers.IsDefaultValue(prop, obj), $"Property '{prop.Name}' did not have a value.");
            }

            foreach (var field in obj.GetType().GetFields(flags))
            {
                if (!customHandler(field))
                    Assert.IsFalse(TestReflectionHelpers.IsDefaultValue(field, obj), $"Property '{field.Name}' did not have a value.");
            }
        }

        public static void AllPropertiesAndFieldsAreEqual<T>(T original, T clone)
        {
            var firstProperties = original.GetType().GetProperties(flags).Where(p => !p.GetIndexParameters().Any()).ToList();
            var secondProperties = original.GetType().GetProperties(flags).Where(p => !p.GetIndexParameters().Any());

            var firstFields = original.GetType().GetFields(flags).ToList();
            var secondFields = original.GetType().GetFields(flags);

            foreach (var newProperty in secondProperties)
            {
                var originalProperty = firstProperties.Find(p => p == newProperty);

                var newValue = newProperty.GetValue(clone);
                var originalValue = originalProperty.GetValue(original);

                if (originalValue.IsIEnumerable() && newValue.IsIEnumerable())
                {
                    var newList = newValue.ToIEnumerable().ToList();
                    var originalList = originalValue.ToIEnumerable().ToList();

                    Assert.AreEqual(newList.Count, originalList.Count, $"Property {newProperty.Name} had different list counts");

                    for (var i = 0; i < newList.Count; i++)
                    {
                        Assert.AreEqual(newList[i], originalList[i], $"Property {newProperty.Name} had a different list member at index {i}");
                    }
                }
                else
                    Assert.AreEqual(newValue, originalValue, $"Expected property {newProperty.Name} new value to be {originalValue} ({originalValue.GetType()}) however value was actually {newValue} ({newValue.GetType()})");
            }

            foreach (var newField in secondFields)
            {
                var originalField = firstFields.Find(p => p == newField);

                var newValue = newField.GetValue(clone);
                var originalValue = originalField.GetValue(original);

                Assert.IsTrue(newValue == originalValue, $"Expected field {newField.Name} new value to be {originalValue} however value was actually {newValue}");
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
            catch (Exception ex) when (!(ex is AssertFailedException))
            {
                throw;
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
            catch (Exception ex) when (!(ex is AssertFailedException))
            {
                throw;
            }
        }

        public static void AreEqualLists<T>(List<T> first, List<T> second, string message) => AreEqualLists(first, second, null, message);

        public static void AreEqualLists<T>(List<T> first, List<T> second, IEqualityComparer<T> comparer, string message)
        {
            for (var i = 0; i < first.Count; i++)
            {
                if (i < second.Count)
                {
                    if (comparer != null)
                    {
                        string msg = $"{message}. {first[i]} was not equal to {second[i]}";

                        if (comparer is LogEqualityComparer)
                        {
                            var firstStr = LogEqualityComparer.Stringify((Log)(object)first[i]);
                            var secondStr = LogEqualityComparer.Stringify((Log)(object)second[i]);

                            msg = $"{message}. {first[i]} ({firstStr}) was not equal to {second[i]} ({secondStr})";
                        }
                        else
                            Assert.IsTrue(comparer.Equals(first[i], second[i]), msg);
                    }
                    else
                        Assert.AreEqual(first[i], second[i], message);
                }
                else
                {
                    var missing = first.Skip(i).ToList();

                    Assert.Fail($"{message}. Elements " + string.Join(", ", missing) + " were missing from second");
                }
            }

            if (second.Count > first.Count)
            {
                var missing = second.Skip(first.Count).ToList();

                Assert.Fail($"{message}. Elements " + string.Join(", ", missing) + " were missing from first");
            }
        }

        public static void AllListElementsUnique<T>(List<T> list, IEqualityComparer<T> comparer)
        {
            for (var i = 0; i < list.Count; i++)
            {
                for (var j = i + 1; j < list.Count; j++)
                {
                    Assert.IsFalse(comparer.Equals(list[i], list[j]), $"{list[i]} was equal to {list[j]}, however this should not have been the case");
                }
            }
        }
    }
}
