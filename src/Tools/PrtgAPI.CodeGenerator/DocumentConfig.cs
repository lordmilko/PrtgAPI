using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.CodeGenerator.Model;

namespace PrtgAPI.CodeGenerator
{
    internal class DocumentConfig
    {
        public ReadOnlyCollection<Template> Templates { get; }

        public ReadOnlyDictionary<string, string> Resources { get; }

        public ReadOnlyDictionary<string, CommonParameter> CommonParameters { get; }

        public DocumentConfig(ReadOnlyCollection<Template> templates, Dictionary<string, string> resources, ReadOnlyCollection<CommonParameter> commonParameters)
        {
            Templates = templates;
            Resources = new ReadOnlyDictionary<string, string>(resources);
            CommonParameters = new ReadOnlyDictionary<string, CommonParameter>(commonParameters.ToDictionary(p => p.Name));
        }
    }
}
