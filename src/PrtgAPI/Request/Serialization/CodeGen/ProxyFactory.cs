using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PrtgAPI.Request.Serialization.CodeGen
{
#if DEBUG && DEBUG_SERIALIZATION
    class ProxyFactory
    {
        private static bool initialized;

        private static Dictionary<Type, Type> proxyMap = new Dictionary<Type, Type>();

        public static void Initialize(ModuleBuilder moduleBuilder)
        {
            if (initialized)
                return;

            var actions = new[]
            {
                typeof(Action), typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>),
                typeof(Action<,,,,>)
            };

            var funcs = new[]
            {
                typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>),
                typeof(Func<,,,,,>)
            };

            foreach(var action in actions)
                proxyMap[action] = ProxyBuilder.Build(moduleBuilder, action, false);

            foreach (var func in funcs)
                proxyMap[func] = ProxyBuilder.Build(moduleBuilder, func, true);

            initialized = true;
        }

        public static Type Get(Type delegateType)
        {
            return proxyMap[delegateType];
        }
    }
#endif
}
