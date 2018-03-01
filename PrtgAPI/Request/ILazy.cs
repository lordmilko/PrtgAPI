using System;
using System.Xml.Linq;

namespace PrtgAPI.Request
{
    interface ILazy
    {
        Lazy<XDocument> LazyXml { get; set; }

        bool LazyInitialized { get; set; }

        object LazyLock { get; }

        T InitializeLazy<T>(Func<T> getValue);
    }
}
