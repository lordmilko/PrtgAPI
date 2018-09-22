using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal class Methods
    {
        public ReadOnlyCollection<RegionImpl> Regions { get; }

        public Methods(MethodsXml methods)
        {
            Regions = methods.Regions.Select(r => new RegionImpl(r)).ToReadOnlyList();
        }
    }
}