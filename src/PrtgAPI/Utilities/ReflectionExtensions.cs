using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
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

        private static BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

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
        /// <returns>If the specified type is <see cref="Nullable"/>, the underlying type. Otherwise, the original type.</returns>
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

        public static object GetPublicProperty(this object obj, string name)
        {
            var info = obj.GetPublicPropertyInfo(name);

            if (info == null)
                throw new MissingMemberException(obj.GetType().Name, name);

            return info.GetValue(obj);
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
            return GetInternalMethod(obj.GetType(), name);
        }

        public static MethodInfo GetInternalMethod(this Type type, string name)
        {
            var method = type.GetMethod(name, internalFlags);

            return method;
        }

        /// <summary>
        /// Retrieve all properties from a type, excluding those that are not writable or are internal properties used by indexers
        /// </summary>
        /// <param name="type">The type to retrieve properties for.</param>
        /// <returns>All normal properties present on the type.</returns>
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

        internal static bool ImplementsRawGenericInterface(this Type type, Type generic)
        {
            return type.GetInterfaces().Where(i => i.IsGenericType).Any(i => i.GetGenericTypeDefinition() == generic);
        }

        internal static bool IsPrtgAPIProperty(Type callerType, PropertyInfo property)
        {
            return IsPrtgAPIType(callerType, property.PropertyType);
        }

        internal static bool IsPrtgAPIType(Type callerType, Type unknownType)
        {
            //Any type that calls this method is implicitly part of PrtgAPI. Therefore, any type
            //that is either in our assembly or the PrtgAPI.dll assembly is considered part of PrtgAPI

            var unknownTypeAssembly = unknownType.Assembly.FullName;
            var callerAssembly = callerType.Assembly.FullName;
            var prtgAPIAssembly = typeof(PrtgClient).Assembly.FullName;

            return unknownTypeAssembly == callerAssembly || unknownTypeAssembly == prtgAPIAssembly;
        }

        internal static Func<object, object> CreateGetValue(MemberInfo member)
        {
            var @this = Expression.Parameter(typeof(object), "obj");
            var val = Expression.Parameter(typeof(object), "val");

            var thisCast = Expression.Convert(@this, member.DeclaringType);

            var access = Expression.MakeMemberAccess(thisCast, member);
            var accessCast = Expression.Convert(access, typeof(object));

            var lambda = Expression.Lambda<Func<object, object>>(
                accessCast,
                @this
            );

            return lambda.Compile();
        }

        internal static Action<object, object> CreateSetValue(MemberInfo member, Type memberType)
        {
            var @this = Expression.Parameter(typeof(object), "obj");
            var val = Expression.Parameter(typeof(object), "val");

            var thisCast = Expression.Convert(@this, member.DeclaringType);
            var valCast = Expression.Convert(val, memberType);

            var access = Expression.MakeMemberAccess(thisCast, member);
            var assignment = Expression.Assign(access, valCast);

            var lambda = Expression.Lambda<Action<object, object>>(
                assignment,
                @this,
                val
            );

            return lambda.Compile();
        }

        internal static Action<T> CreateAction<T>(string methodName)
        {
            return CreateAction<T>(typeof(T).GetMethod(methodName, allFlags));
        }

        internal static Action<T> CreateAction<T>(MethodInfo method)
        {
            return CreateInvokec<T, Action<T>>(method);
        }

        internal static Func<T1, TResult> CreateFunc<T1, TResult>(string methodName)
        {
            return CreateFunc<T1, TResult>(typeof(T1).GetMethod(methodName, allFlags));
        }

        internal static Func<T1, TResult> CreateFunc<T1, TResult>(MethodInfo method)
        {
            return CreateInvokec<T1, Func<T1, TResult>>(method);
        }

        private static TDelegate CreateInvokec<TParameter, TDelegate>(MethodInfo method)
        {
            var @this = Expression.Parameter(typeof(TParameter), "obj");

            var call = Expression.Call(@this, method);

            var lambda = Expression.Lambda<TDelegate>(
                call,
                @this
            );

            return lambda.Compile();
        }
    }
}
