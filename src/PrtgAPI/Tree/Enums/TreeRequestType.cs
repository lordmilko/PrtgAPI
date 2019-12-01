using System;

namespace PrtgAPI.Tree
{
    [Flags]
    internal enum TreeRequestType
    {
        Synchronous = 1,
        Asynchronous = 2,
        Lazy = 4
    }
}
