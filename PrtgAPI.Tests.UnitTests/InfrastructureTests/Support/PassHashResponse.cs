namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    class PassHashResponse : IWebResponse
    {
        public string GetResponseText(string address)
        {
            return "12345678";
        }
    }
}
