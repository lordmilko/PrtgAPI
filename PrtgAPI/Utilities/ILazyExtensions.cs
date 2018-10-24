using System;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Request;

namespace PrtgAPI.Utilities
{
    static class ILazyExtensions
    {
        public static IXmlSerializer Serializer { get; set; } = new XmlExpressionSerializer();

        internal static TProperty Get<TObject, TProperty>(this TObject obj, Func<TProperty> getValue) where TObject : PrtgObject
        {
            if (obj is ILazy)
            {
                var lazy = (ILazy) obj;

                lock (lazy.LazyLock)
                {
                    if (lazy.LazyInitialized == false)
                    {
                        if (lazy.LazyXml != null)
                            Serializer.Update(lazy.LazyXml.Value.CreateReader(), obj);

                        lazy.LazyInitialized = true;
                    }
                }
            }

            return getValue();
        }
    }
}
