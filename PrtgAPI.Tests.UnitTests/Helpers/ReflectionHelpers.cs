using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PrtgAPI.Helpers;

namespace PrtgAPI.Tests.UnitTests.Helpers
{
    public static class TestReflectionHelpers
    {
        public static void NullifyProperties(object obj)
        {
            foreach (var prop in obj.GetType().GetNormalProperties())
            {
                prop.SetValue(obj, null);
            }
        }

        public static bool IsDefaultValue(PropertyInfo prop, object obj)
        {
            var val = prop.GetValue(obj, null);
            var @default = GetDefault(prop.PropertyType);

            if (val is IComparable)
            {
                var valComparable = val as IComparable;
                var valDefault = @default as IComparable;

                return valComparable.CompareTo(valDefault) == 0;
            }

            return val == @default;
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }

        public static object GetDefaultUnderlying(Type type, Func<object> @override = null)
        {
            var underlying = Nullable.GetUnderlyingType(type);

            if (underlying != null)
                type = underlying;

            if (@override != null)
                return @override();

            if (type.IsValueType || type.IsClass)
                return Activator.CreateInstance(type, true);
            return null;
        }

        public static void GetAllProperties(object obj)
        {
            foreach (var prop in obj.GetType().GetNormalProperties())
            {
                prop.GetValue(obj);
            }
        }
    }
}
