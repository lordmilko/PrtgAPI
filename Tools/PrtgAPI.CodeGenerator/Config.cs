using System.Collections.Generic;
using System.Collections.ObjectModel;
using PrtgAPI.CodeGenerator.Model;

namespace PrtgAPI.CodeGenerator
{
    internal class Config
    {
        public ReadOnlyCollection<Template> Templates { get; }

        public ReadOnlyDictionary<string, string> Resources { get; }

        public Config(ReadOnlyCollection<Template> templates, Dictionary<string, string> resources)
        {
            Templates = templates;
            Resources = new ReadOnlyDictionary<string, string>(resources);
        }
    }
}
