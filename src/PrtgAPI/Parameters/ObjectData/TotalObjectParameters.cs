using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class TotalObjectParameters : BaseParameters, IXmlParameters, IResponseParser
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
            {
                this[Parameter.Count] = "*";
                this[Parameter.FilterXyz] = new SearchFilter(Property.ParentId, WellKnownId.Root);
                this[Parameter.Columns] = new[]
                {
                    Property.Type
                };
            } 
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

        PrtgResponse IResponseParser.ParseResponse(HttpResponseMessage requestMessage)
        {
            if (Content != Content.Probes)
                return null;

            var content = requestMessage.Content.ReadAsStringAsync().Result;

            return ResponseParser.ParseProbeResponse(content);
        }

        async Task<PrtgResponse> IResponseParser.ParseResponseAsync(HttpResponseMessage requestMessage)
        {
            if (Content != Content.Probes)
                return null;

            var content = await requestMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            return ResponseParser.ParseProbeResponse(content);
        }
    }
}
