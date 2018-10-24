using System;
using System.Linq;
using System.Reflection;

namespace PrtgAPI.Tests.UnitTests.Support
{
    public static class TestReflectionUtilities
    {
        public static void NullifyProperties(object obj)
        {
            foreach (var prop in obj.GetType().GetNormalProperties())
            {
                prop.SetValue(obj, null);
            }
        }

        public static bool IsDefaultValue(MemberInfo info, object obj)
        {
            var val = info.GetValue(obj);
            var @default = GetDefault(GetType(info));

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

        private static Type GetType(MemberInfo info)
        {
            if (info is PropertyInfo)
                return ((PropertyInfo) info).PropertyType;

            if (info is FieldInfo)
                return ((FieldInfo) info).FieldType;

            throw new NotImplementedException();
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
