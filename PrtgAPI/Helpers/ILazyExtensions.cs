using System;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Request;

namespace PrtgAPI.Helpers
{
    static class ILazyExtensions
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
