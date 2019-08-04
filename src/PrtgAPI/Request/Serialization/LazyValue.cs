using System;
using System.Threading;

namespace PrtgAPI.Request.Serialization
{
    class LazyValue<T> : Lazy<T>
    {
        public LazyValue(string raw, Func<T> valueFactory) : base(GetFunc(raw, valueFactory))
        {
        }

        public LazyValue(string raw, Func<T> valueFactory, LazyThreadSafetyMode mode) : base(GetFunc(raw, valueFactory), mode)
        {
        }

        private static Func<T> GetFunc(string raw, Func<T> valueFactory)
        {
            if (raw == null)
                return () => default(T);

            return valueFactory;
        }
    }
}
