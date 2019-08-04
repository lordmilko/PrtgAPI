namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class PassHashResponse : IWebResponse
    {
        private string response;

        public PassHashResponse(string response = "12345678")
        {
            this.response = response;
        }

        public string GetResponseText(ref string address)
        {
            return response;
        }
    }
}
