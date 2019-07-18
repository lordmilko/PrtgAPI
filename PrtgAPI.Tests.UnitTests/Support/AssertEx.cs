using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Linq;
using PrtgAPI.Reflection;
using PrtgAPI.Request;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests
{
    public static class AssertEx
    {
        private static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static void AllPropertiesAreDefault(object obj, Func<PropertyInfo, bool> customHandler = null)
        {
            if (customHandler == null)
                customHandler = p => false;

            foreach (var prop in obj.GetType().GetProperties())
            {
                if (!customHandler(prop))
                    Assert.IsTrue(TestReflectionUtilities.IsDefaultValue(prop, obj), $"Property '{prop.Name}' was not its default value");
            }
        }

        public static void AllPropertiesAreNotDefault(object obj, Func<PropertyInfo, bool> customHandler = null)
        {
            if (customHandler == null)
                customHandler = p => false;

            foreach (var prop in obj.GetType().GetProperties())
            {
                if (!customHandler(prop))
                    Assert.IsFalse(TestReflectionUtilities.IsDefaultValue(prop, obj), $"Property '{prop.Name}' did not have a value.");
            }
        }

        public static void AllPropertiesAndFieldsAreNotDefault(object obj, Func<MemberInfo, bool> customHandler = null)
        {
            if (customHandler == null)
                customHandler = p => false;

            foreach (var prop in obj.GetType().GetProperties(flags).Where(p => !p.GetIndexParameters().Any()))
            {
                if (!customHandler(prop))
                    Assert.IsFalse(TestReflectionUtilities.IsDefaultValue(prop, obj), $"Property '{prop.Name}' did not have a value.");
            }

            foreach (var field in obj.GetType().GetFields(flags))
            {
                if (!customHandler(field))
                    Assert.IsFalse(TestReflectionUtilities.IsDefaultValue(field, obj), $"Property '{field.Name}' did not have a value.");
            }
        }

        public static void AllPropertiesRetrieveValues(object value)
        {
            var properties = value.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0);

            foreach (var prop in properties)
                prop.GetValue(value);
        }

        public static void AllPropertiesAndFieldsAreEqual<T>(T original, T clone)
        {
            AllPropertiesAndFieldsAreEqualInternal(original, clone, new Stack<object>());
        }

        public static void AllPropertiesAndFieldsAreEqualInternal<T>(T original, T clone, Stack<object> seen)
        {
            var firstProperties = original.GetType().GetProperties(flags).Where(p => !p.GetIndexParameters().Any()).ToList();
            var secondProperties = original.GetType().GetProperties(flags).Where(p => !p.GetIndexParameters().Any());

            var firstFields = original.GetType().GetFields(flags).ToList();
            var secondFields = original.GetType().GetFields(flags);

            foreach (var newProperty in secondProperties)
            {
                var originalProperty = firstProperties.Find(p => p == newProperty);

                if (IsILazyProperty(originalProperty) && IsILazyProperty(newProperty) || (IsLazy(originalProperty.PropertyType) && IsLazy(newProperty.PropertyType)))
                    continue;

                var newValue = newProperty.GetValue(clone);
                var originalValue = originalProperty.GetValue(original);

                seen.Push(newValue);
                seen.Push(originalValue);

                if (originalValue.IsIEnumerable() && newValue.IsIEnumerable())
                {
                    var newList = newValue.ToIEnumerable().ToList();
                    var originalList = originalValue.ToIEnumerable().ToList();

                    Assert.AreEqual(newList.Count, originalList.Count, $"Property {newProperty.Name} had different list counts");

                    for (var i = 0; i < newList.Count; i++)
                    {
                        if (newList[i].IsIEnumerable() && originalList[i].IsIEnumerable())
                            AreEqualLists(originalList[i].ToIEnumerable().ToList(), newList[i].ToIEnumerable().ToList(), $"Lists of property {newProperty.Name} at index '{i}' were not equal");
                        else
                        {
                            if (ReflectionExtensions.IsSubclassOfRawGeneric(newList[i].GetType(), typeof(KeyValuePair<,>)) && ReflectionExtensions.IsSubclassOfRawGeneric(originalList[i].GetType(), typeof(KeyValuePair<,>)))
                            {
                                AllPropertiesAndFieldsAreEqual(originalList[i], newList[i]);
                            }
                            else
                                Assert.AreEqual(newList[i], originalList[i], $"Property {newProperty.Name} had a different list member at index {i}");
                        }
                    }
                }
                else
                {
                    if (TestHelpers.IsPrtgAPIClass(originalValue) && TestHelpers.IsPrtgAPIClass(newValue) && seen.Where(v => v == originalValue).Count() < 3 && seen.Where(v => v == newValue).Count() < 3)
                    {
                        AllPropertiesAndFieldsAreEqual(originalValue, newValue);
                    }
                    else
                    {
                        if (originalValue is XElement && newValue is XElement)
                        {
                            originalValue = ((XElement)originalValue).ToString();
                            newValue = ((XElement)newValue).ToString();
                        }

                        Assert.AreEqual(newValue, originalValue, $"Expected property '{newProperty.Name}' new value to be '{originalValue}' ({(originalValue?.GetType().Name ?? "null")}) however value was actually {newValue} ({(newValue?.GetType().Name ?? "null")})");
                    }
                }

                seen.Pop();
                seen.Pop();
            }

            foreach (var newField in secondFields)
            {
                var originalField = firstFields.Find(p => p == newField);

                if (IsLazyValue(originalField) && IsLazyValue(newField) || (IsLazy(originalField.FieldType) || IsLazy(newField.FieldType)))
                    continue;

                if (originalField.Name.Contains(typeof(ILazy).FullName) && newField.Name.Contains(typeof(ILazy).FullName))
                    continue;

                var newValue = newField.GetValue(clone);
                var originalValue = originalField.GetValue(original);

                seen.Push(newValue);
                seen.Push(originalValue);

                if (TestHelpers.IsPrtgAPIClass(originalValue) && TestHelpers.IsPrtgAPIClass(newValue) && seen.Where(v => v == originalValue).Count() < 3 && seen.Where(v => v == newValue).Count() < 3)
                {
                    AllPropertiesAndFieldsAreEqualInternal(originalValue, newValue, seen);
                }
                else
                {
                    if (ReflectionExtensions.IsSubclassOfRawGeneric(newField.FieldType, typeof(IReadOnlyDictionary<,>)) && ReflectionExtensions.IsSubclassOfRawGeneric(originalField.FieldType, typeof(IReadOnlyDictionary<,>)))
                    {
                        AllPropertiesAndFieldsAreEqualInternal(originalValue, originalValue, seen);
                    }
                    else
                    {
                        if (newValue.IsIEnumerable() && originalValue.IsIEnumerable())
                            AreEqualLists(originalValue.ToIEnumerable().ToList(), newValue.ToIEnumerable().ToList(), $"Fields '{newField.Name}' were not equal");
                        else
                            Assert.IsTrue((newValue == null && originalValue == null) || newValue?.Equals(originalValue) == true, $"Expected field '{newField.Name}' new value to be '{originalValue}' however value was actually '{newValue}'");
                    }
                }

                seen.Pop();
                seen.Pop();
            }
        }

        private static bool IsILazyProperty(PropertyInfo info)
        {
            return info.Name.StartsWith(typeof(ILazy).FullName);
        }

        private static bool IsLazy(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Lazy<>);
        }

        private static bool IsLazyValue(FieldInfo field)
        {
            return field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(LazyValue<>);
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

        public static void AreEqualLists<T>(List<T> first, List<T> second, string message) => AreEqualListsInternal(first, second, null, null, message);

        public static void AreEqualLists<T>(List<T> first, List<T> second, Action<T, T> assert, string message) => AreEqualListsInternal(first, second, null, assert, message);

        public static void AreEqualLists<T>(List<T> first, List<T> second, IEqualityComparer<T> comparer, string message) => AreEqualListsInternal(first, second, comparer, null, message);

        private static void AreEqualListsInternal<T>(List<T> first, List<T> second, IEqualityComparer<T> comparer, Action<T, T> assert, string message)
        {
            if (first == null && second == null)
                return;

            if (first == null && second != null)
                Assert.Fail("First was null but second wasn't");

            if (first != null && second == null)
                Assert.Fail("First was not null but second wasn't");

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
                    {
                        if (assert != null)
                            assert(first[i], second[i]);
                        else
                            Assert.AreEqual(first[i], second[i], message);
                    }
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

        internal static void AssertErrorResponseAllLanguages<T>(string english, string german, string japanese, string exceptionMessage, Action<PrtgClient> action) where T : Exception
        {
            var englishClient = BaseTest.Initialize_Client(new BasicResponse(english));
            Throws<T>(() => action(englishClient), exceptionMessage);

            var germanClient = BaseTest.Initialize_Client(new BasicResponse(german));
            Throws<T>(() => action(germanClient), exceptionMessage);

            var japaneseClient = BaseTest.Initialize_Client(new BasicResponse(japanese));
            Throws<T>(() => action(japaneseClient), exceptionMessage);
        }

        internal static async Task AssertErrorResponseAllLanguagesAsync<T>(string english, string german, string japanese, string exceptionMessage, Func<PrtgClient, Task> action) where T : Exception
        {
            var englishClient = BaseTest.Initialize_Client(new BasicResponse(english));
            await ThrowsAsync<T>(async () => await action(englishClient), exceptionMessage);

            var germanClient = BaseTest.Initialize_Client(new BasicResponse(german));
            await ThrowsAsync<T>(async () => await action(germanClient), exceptionMessage);

            var japaneseClient = BaseTest.Initialize_Client(new BasicResponse(japanese));
            await ThrowsAsync<T>(async () => await action(japaneseClient), exceptionMessage);
        }

        internal static void UrlsEquivalent(string first, string second)
        {
            if (first.Length != second.Length)
                Assert.Fail($"Url '{first}' is not equivalent to '{second}': lengths were different ({first.Length} vs {second.Length}).");

            var firstSorted = OrderUri(first);
            var secondSorted = OrderUri(second);

            var result = Uri.Compare(firstSorted, secondSorted, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped,
                StringComparison.OrdinalIgnoreCase);

            Assert.IsTrue(result == 0, "Urls were not equal");
        }

        private static Uri OrderUri(string str)
        {
            var sorted = new NameValueCollection();

            var unsorted = UrlUtilities.CrackUrl(str);

            foreach (var key in unsorted.AllKeys.OrderBy(k => k))
            {
                sorted.Add(key, unsorted[key]);
            }

            var builder = new UriBuilder(str);
            builder.Query = UrlUtilities.QueryCollectionToString(sorted);

            return builder.Uri;
        }
    }
}
