using System.Collections.ObjectModel;

namespace PrtgAPI.CodeGenerator.CSharp
{
    /// <summary>
    /// Represents a fully constructed region ready to be emitted to a source file.
    /// </summary>
    internal class Region
    {
        public string Name { get; }

        public ReadOnlyCollection<Region> Regions { get; }

        public ReadOnlyCollection<Method> Methods { get; }

        public bool GroupOverloads { get; }

        public Region(string name, ReadOnlyCollection<Region> regions, ReadOnlyCollection<Method> methods, bool groupOverloads)
        {
            Name = name;
            Regions = regions;
            Methods = methods;
            GroupOverloads = groupOverloads;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
