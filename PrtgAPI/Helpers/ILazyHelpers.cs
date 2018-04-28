using System;
using PrtgAPI.Objects.Deserialization;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Request;

namespace PrtgAPI.Helpers
{
    static class ILazyHelpers
    {
        internal static TProperty Get<TProperty>(this PrtgObject obj, Func<TProperty> getValue)
        {
            if (obj is ILazy)
            {
                var lazy = (ILazy) obj;

                lock (lazy.LazyLock)
                {
                    if (lazy.LazyInitialized == false)
                    {
                        if (lazy.LazyXml != null)
                            XmlDeserializer<PrtgObject>.UpdateType(lazy.LazyXml.Value, obj);

                        lazy.LazyInitialized = true;
                    }
                }
            }

            return getValue();
        }
    }
}
