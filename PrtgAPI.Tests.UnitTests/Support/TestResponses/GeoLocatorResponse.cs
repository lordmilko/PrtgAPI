using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    enum GeoLocatorResponseType
    {
        Normal,
        NoResults,
        NoProvider,
        NoAPIKey
    }

    class GeoLocatorResponse : MultiTypeResponse, IWebStatusResponse, IWebContentHeaderResponse
    {
        public Action<HttpContentHeaders> HeaderAction { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        GeoLocatorResponseType type;

        public GeoLocatorResponse(GeoLocatorResponseType type = GeoLocatorResponseType.Normal)
        {
            this.type = type;

            if (type == GeoLocatorResponseType.NoProvider)
                StatusCode = (HttpStatusCode)530;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(JsonFunction.GeoLocator):
                    if (address.Contains("dom=0"))
                        return new BasicResponse(GoogleResponse());

                    return new BasicResponse(HereResponse());

                default:
                    return base.GetResponse(ref address, function);
            }
        }

        #region Google

        private string GoogleResponse()
        {
            switch(type)
            {
                case GeoLocatorResponseType.Normal:
                    return GoogleNormal();
                case GeoLocatorResponseType.NoResults:
                    return GoogleNoResults();
                case GeoLocatorResponseType.NoProvider:
                    return GoogleNoProvider();
                case GeoLocatorResponseType.NoAPIKey:
                    return GoogleNoAPIKey(); //todo: also need an integration test?
                default:
                    throw new NotImplementedException($"Don't know how to handle Google geolocation response '{type}'");
            }
        }

        private string GoogleNormal()
        {
            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("   \"results\" : [");
            builder.Append("      {");
            builder.Append("         \"formatted_address\" : \"23 Fleet St, Boston, MA 02113, USA\",");
            builder.Append("         \"geometry\" : {");
            builder.Append("            \"location\" : {");
            builder.Append("               \"lat\" : 42.3643847,");
            builder.Append("               \"lng\" : -71.05279969999999");
            builder.Append("            }");
            builder.Append("         }");
            builder.Append("      }");
            builder.Append("   ]");
            builder.Append("}");

            return builder.ToString();
        }

        private string GoogleNoResults()
        {
            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("   \"results\" : []");
            builder.Append("}");

            return builder.ToString();
        }

        private string GoogleNoProvider()
        {
            HeaderAction = headers => headers.ContentType.MediaType = "image/png";

            return string.Empty;
        }

        private string GoogleNoAPIKey()
        {
            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("   \"error_message\": \"Keyless access to Google Maps Platform is deprecated. Please use an API key with all your API calls to avoid service interruption. For further details please refer to http://g.co/dev/maps-no-account\",");
            builder.Append("   \"results\" : [],");
            builder.Append("   \"status\": \"OVER_QUERY_LIMIT\"");
            builder.Append("}");

            return builder.ToString();
        }

        #endregion
        #region Here

        private string HereResponse()
        {
            switch(type)
            {
                case GeoLocatorResponseType.Normal:
                    return HereNormal();
                case GeoLocatorResponseType.NoResults:
                    return HereNoResults();
                case GeoLocatorResponseType.NoProvider:
                    return HereNoProvider();
                default:
                    throw new NotImplementedException($"Don't know how to handle Here geolocation response '{type}'");
            }
        }

        private string HereNormal()
        {
            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("    \"Response\":{");
            builder.Append("        \"View\":[{");
            builder.Append("            \"Result\":[");
            builder.Append("                {");
            builder.Append("                    \"Location\":{");
            builder.Append("                        \"DisplayPosition\":{");
            builder.Append("                            \"Latitude\":62.3643847,");
            builder.Append("                            \"Longitude\":-91.05279969999999");
            builder.Append("                        },");
            builder.Append("                        \"Address\":{");
            builder.Append("                            \"Label\":\"100 HERE Lane\"");
            builder.Append("                        }");
            builder.Append("                    }");
            builder.Append("                }");
            builder.Append("            ]");
            builder.Append("        }]");
            builder.Append("    }");
            builder.Append("}");

            return builder.ToString();
        }

        private string HereNoResults()
        {
            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("    \"Response\":{");
            builder.Append("        \"View\":[]");
            builder.Append("    }");
            builder.Append("}");

            return builder.ToString();
        }

        private string HereNoProvider()
        {
            //Status code 530 will be caught by the response parser; actual response text
            //won't be read. In reality, HttpClient will simply timeout itself and we won't even get
            //to validation stage
            return "Error retrieving Proxy URL";
        }

        #endregion
    }
}
