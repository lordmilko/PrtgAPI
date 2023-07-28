namespace PrtgAPI.Parameters
{
    class RootHtmlParameters : BaseParameters, IHtmlParameters
    {
        public HtmlFunction Function => HtmlFunction.Group;

        public RootHtmlParameters()
        {
            this[Parameter.Id] = 0;
            Cookie = true;
        }
    }
}
