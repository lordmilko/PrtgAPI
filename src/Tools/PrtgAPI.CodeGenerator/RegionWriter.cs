using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.CodeGenerator.CSharp;

namespace PrtgAPI.CodeGenerator
{
    class RegionWriter : SourceWriter
    {
        public void Write(List<Region> regions)
        {
            foreach (var region in regions)
                WriteRegion(region, 1);
        }

        private void WriteRegion(Region region, int level)
        {
            StartRegion(region.Name, level);

            foreach (var r in region.Elements.OfType<Region>())
                WriteRegion(r, level + 1);

            var methods = region.Elements.OfType<Method>().ToReadOnlyList();

            if (methods.Count > 0)
                WriteLine("");

            if (region.GroupOverloads)
                methods = OrderOverloads(methods);

            foreach (var method in methods)
            {
                Write(method.Definition);
                WriteLine("");
            }

            EndRegion(level);
        }

        private ReadOnlyCollection<Method> OrderOverloads(ReadOnlyCollection<Method> methods)
        {
            var output = new List<Method>();

            foreach (var method in methods)
            {
                var matches = methods.Where(m => m.Name == method.Name && m.Type == method.Type && !output.Contains(m)).ToList();

                output.AddRange(matches);
            }

            return output.ToReadOnlyList();
        }
    }
}
