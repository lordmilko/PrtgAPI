using System;

namespace PrtgAPI.PowerShell
{
    class AlternateSensorTargetParameter : IAlternateParameter
    {
        public string Name { get; }

        public string OriginalName { get; }

        public Type Type { get; }

        public AlternateSensorTargetParameter(string name, string originalName, Type type)
        {
            Name = name;
            OriginalName = originalName;
            Type = type;
        }
    }
}