using System.Collections.ObjectModel;
using System.Linq;

namespace PrtgAPI.CodeGenerator.CSharp
{
    /// <summary>
    /// Represents a fully constructed region ready to be emitted to a source file.
    /// </summary>
    internal class Region : IElement
    {
        public string Name { get; }

        public ReadOnlyCollection<IElement> Elements { get; }

        private Region[] Regions => Elements.OfType<Region>().ToArray();

        private Method[] Methods => Elements.OfType<Method>().ToArray();

        public bool GroupOverloads { get; }

        public Region(string name, ReadOnlyCollection<IElement> elements, bool groupOverloads)
        {
            Name = name;
            Elements = elements;
            GroupOverloads = groupOverloads;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
