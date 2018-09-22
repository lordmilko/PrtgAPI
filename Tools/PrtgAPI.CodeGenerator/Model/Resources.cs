using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal class Resources
    {
        private ResourcesXml resourcesXml;

        public string StreamSummary => resourcesXml.StreamSummary;

        public string SerialSummary => resourcesXml.SerialSummary;

        public string StreamReturnDescription => resourcesXml.StreamReturnDescription;

        public string SerialReturnDescription => resourcesXml.SerialReturnDescription;

        public Resources(ResourcesXml resourcesXml)
        {
            this.resourcesXml = resourcesXml;
        }
    }
}
