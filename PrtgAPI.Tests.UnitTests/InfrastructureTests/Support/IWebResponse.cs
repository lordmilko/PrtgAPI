namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    public interface IWebResponse
    {
        string GetResponseText(ref string address);
    }
}
