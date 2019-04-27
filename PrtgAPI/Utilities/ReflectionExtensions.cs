using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using PrtgAPI.Reflection.Cache;

namespace PrtgAPI.Reflection
{
    /// <summary>
    /// Defines helper extension methods used for performing reflection.
    /// </summary>
    [ExcludeFromCodeCoverage]
    static class ReflectionExtensions
    {
        private static BindingFlags internalFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        public static CacheValue<TypeCache> GetCacheValue(this Type type) => ReflectionCacheManager.GetTypeCache(type);

        public static TypeCache GetTypeCache(this Type type) => ReflectionCacheManager.GetTypeCache(type).Cache;

        public static TypeCache GetTypeCache(this object obj) => ReflectionCacheManager.Get(obj.GetType());

        public static EnumCache GetEnumTypeCache(this Enum value) => (EnumCache) ReflectionCacheManager.Get(value.GetType());

        public static FieldCache GetEnumFieldCache(this Enum value) => ReflectionCacheManager.GetEnumFieldCache(value);

        public static TAttribute GetAttribute<TAttribute>(this PropertyInfo info) where TAttribute : Attribute =>
            ReflectionCacheManager.Get(info.ReflectedType).Properties.First(p => p.Property == info).GetAttribute<TAttribute>();

        public static TAttribute[] GetAttributes<TAttribute>(this PropertyInfo info) where TAttribute : Attribute =>
            ReflectionCacheManager.Get(info.ReflectedType).Properties.First(p => p.Property == info).GetAttributes<TAttribute>();

        /// <summary>
        /// Returns the underlying type of this type is <see cref="Nullable"/>; otherwise, returns this type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetUnderlyingType(this Type type)
        {
            var cache = type.GetCacheValue();

            if (cache.Underlying != null)
                return cache.Underlying;

            return cache.Cache.Type;
        }

        public static bool IsNullable(this Type type)
        {
            return type.GetCacheValue().Underlying != null;
        }

        /// <summary>
        /// Retrieve the value of an internal property of an object.
        /// </summary>
        /// <param name="obj">The object to retrieve the property from.</param>
        /// <param name="name">The name of the property whose value should be retrieved.</param>
        /// <returns>The value of the retrieved property.</returns>
        public static object GetInternalProperty(this object obj, string name)
        {
            var info = obj.GetInternalPropertyInfo(name);

            if (info == null)
                throw new MissingMemberException(obj.GetType().Name, name);

            return info.GetValue(obj);
        }

        public static PropertyInfo GetPublicPropertyInfo(this object obj, string name)
        {
            var prop = obj.GetType().GetProperty(name);

            return prop;
        }

        /// <summary>
        /// Retrieve the property info metadata of an internal property.
        /// </summary>
        /// <param name="obj">The object to retrieve the property from.</param>
        /// <param name="name">The name of the property whose info should be retrieved.</param>
        /// <returns>The property info of the specified property. If the property cannot be found or is not internal, this method returns null.</returns>
        public static PropertyInfo GetInternalPropertyInfo(this object obj, string name)
        {
            var prop = obj.GetType().GetProperty(name, internalFlags);

            return prop;
        }

        /// <summary>
        /// Retrieve the value of an internal field of an object.
        /// </summary>
        /// <param name="obj">The object to retrieve the field from.</param>
        /// <param name="name">The name of the field whose value should be retrieved.</param>
        /// <returns>The value of the retrieved field.</returns>
        public static object GetInternalField(this object obj, string name)
        {
            var info = obj.GetInternalFieldInfo(name);

            if (info == null)
                throw new MissingMemberException(obj.GetType().Name, name);

            return info.GetValue(obj);
        }

        /// <summary>
        /// Retrieve the field info metadata of an internal field.
        /// </summary>
        /// <param name="obj">The object to retrieve the field from.</param>
        /// <param name="name">The name of the field whose info should be retrieved.</param>
        /// <returns>The field info of the specified field. If the field cannot be found or is not internal, this method returns null.</returns>
        public static FieldInfo GetInternalFieldInfo(this object obj, string name)
        {
            var info = obj.GetType().GetField(name, internalFlags);

            return info;
        }

        public static FieldInfo GetInternalFieldInfo(this Type type, string name)
        {
            return type.GetField(name, internalFlags);
        }

        public static FieldInfo GetInternalFieldInfoFromBase(this Type type, string name)
        {
            var info = GetInternalFieldInfo(type, name);

            if (info != null)
                return info;
            else
            {
                if (type.BaseType != null)
                    return type.BaseType.GetInternalFieldInfoFromBase(name);
                else
                    return null;
            }
        }

        public static FieldInfo GetInternalStaticFieldInfo(this Type type, string name)
        {
            return type.GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static object GetInternalStaticField(this object obj, string name)
        {
            var info = obj.GetType().GetInternalStaticFieldInfo(name);

            if (info == null)
                throw new MissingMemberException(obj.GetType().Name, name);

            return info.GetValue(null);
        }

        public static object GetInternalStaticField(this Type type, string name)
        {
            var info = type.GetInternalStaticFieldInfo(name);

            if (info == null)
                throw new MissingMemberException(type.Name, name);

            return info.GetValue(null);
        }

        /// <summary>
        /// Retrieve an internal method from an object.
        /// </summary>
        /// <param name="obj">The object to retrieve the method from.</param>
        /// <param name="name">The name of the method to retrieve.</param>
        /// <returns>The method info of the specified method. If the specified method cannot be found or is not internal, this method returns null.<para/>
        /// If more than one method is found with the specified name, this method throws a <see cref="AmbiguousMatchException"/></returns>
        public static MethodInfo GetInternalMethod(this object obj, string name)
        {
            var method = obj.GetType().GetMethod(name, internalFlags);

            return method;
        }

        /// <summary>
        /// Retrieve all properties from a type, excluding those that are not writable or are internal properties used by indexers
        /// </summary>
        /// <param name="type">The type to retrieve properties for.</param>
        /// <returns></returns>
        public static IEnumerable<PropertyCache> GetNormalProperties(this Type type)
        {
            return type.GetTypeCache().Properties.Where(p => !p.Property.GetIndexParameters().Any() && p.Property.CanWrite);
        }

        //https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
        internal static bool IsSubclassOfRawGeneric(this Type type, Type generic)
        {
            while (type != null && type != typeof(object))
            {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (generic == cur)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        internal static bool IsPrtgAPIProperty(Type thisType, PropertyInfo property)
        {
            var propertyAssembly = property.PropertyType.Assembly.FullName;
            var thisAssembly = thisType.Assembly.FullName;
            var prtgAPIAssembly = typeof(PrtgClient).Assembly.FullName;

            return propertyAssembly == thisAssembly || propertyAssembly == prtgAPIAssembly;
        }
    }
}
