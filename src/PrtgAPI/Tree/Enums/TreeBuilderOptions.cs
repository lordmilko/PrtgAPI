using System;

namespace PrtgAPI.Tree
{
    [Flags]
    internal enum TreeBuilderOptions
    {
        Synchronous = 1,
        Asynchronous = 2,
        Lazy = 4
    }
}
