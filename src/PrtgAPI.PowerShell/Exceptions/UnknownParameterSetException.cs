using System;

namespace PrtgAPI.PowerShell
{
    internal class UnknownParameterSetException : NotImplementedException
    {
        internal UnknownParameterSetException(string parameterSetName) : base($"Implementation missing for handling parameter set '{parameterSetName}'.")
        {
        }
    }
}
