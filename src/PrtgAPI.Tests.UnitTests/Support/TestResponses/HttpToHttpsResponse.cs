namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class HttpToHttpsResponse : MultiTypeResponse
    {
        protected override IWebResponse GetResponse(ref string address, string function)
        {
            var response = base.GetResponse(ref address, function);

            address = address.Replace("http://", "https://");

            return response;
        }
    }
}
