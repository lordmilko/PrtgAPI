using PrtgAPI.CodeGenerator.MethodBuilder.Builders;

namespace PrtgAPI.CodeGenerator.MethodBuilder
{
    internal class MethodRunner
    {
        MethodConfig methodConfig;

        public SourceWriter Writer { get; }

        private MethodXmlDocBuilder xmlDocBuilder;
        private MethodHeaderBuilder headerBuilder;
        private MethodBodyBuilder bodyBuilder;

        public MethodRunner(MethodConfig methodConfig, SourceWriter writer)
        {
            this.methodConfig = methodConfig;
            Writer = writer;

            xmlDocBuilder = new MethodXmlDocBuilder(methodConfig);
            headerBuilder = new MethodHeaderBuilder(methodConfig);
            bodyBuilder = new MethodBodyBuilder(methodConfig);
        }

        public void WriteMethod()
        {
            var xmlDoc = xmlDocBuilder.GetXmlDoc();
            var methodHeader = headerBuilder.GetHeader();
            var body = bodyBuilder.GetMethodBody(methodHeader);

            xmlDoc.Write(Writer);
            methodHeader.Write(Writer);
            body.Write(Writer);
        }
    }
}
