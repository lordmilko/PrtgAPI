using System;

namespace PrtgAPI.PowerShell
{
    interface IAlternateParameter
    {
        string Name { get; }

        string OriginalName { get; }

        Type Type { get; }
    }
}