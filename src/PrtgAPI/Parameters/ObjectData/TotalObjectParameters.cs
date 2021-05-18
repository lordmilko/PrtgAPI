using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class TotalObjectParameters : BaseParameters, IXmlParameters
    {
        XmlFunction IXmlParameters.Function => XmlFunction.TableData;

        public TotalObjectParameters(Content content)
        {
            Content = content;

            //Logs take longer if you ask for 0
            if (content == Content.Logs)
            {
                this[Parameter.Count] = 1;

                this[Parameter.Columns] = new[]
                {
                    Property.Id, Property.Name
                };
            }
            else
                this[Parameter.Count] = 0;

            if (content == Content.Probes)
                this[Parameter.FilterXyz] = new SearchFilter(Property.ParentId, "0");
        }

        public TotalObjectParameters(Content content, SearchFilter[] filters) : this(content)
        {
            SearchFilters = filters;
        }

        public Content Content
        {
            get { return (Content)this[Parameter.Content]; }
            set { this[Parameter.Content] = value; }
        }

        public SearchFilter[] SearchFilters
        {
            get { return (SearchFilter[])this[Parameter.FilterXyz]; }
            set { this[Parameter.FilterXyz] = value; }
        }
    }
}
