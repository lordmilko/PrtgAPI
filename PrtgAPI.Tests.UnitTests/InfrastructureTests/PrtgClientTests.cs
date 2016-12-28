using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests
{
    [TestClass]
    public class PrtgClientTests
    {
        [TestMethod]
        public void PrtgClient_Constructor_RetrievesPassHash()
        {
            var webClient = new MockWebClient(new PassHashResponse());

            var client = new PrtgClient("prtg.example.com", "username", "password", AuthMode.Password, webClient);

            Assert.IsTrue(client.PassHash == "12345678");
        }

        /*private T[] CreateNullResponseItem<T>(T obj)
        {
            //var obj = Activator.CreateInstance(typeof (T));

            var properties = obj.GetType().GetProperties();

            foreach (var prop in properties)
            {
                prop.SetValue(obj, GetDefault(prop.PropertyType));
            }

            return new[] {obj};
        }*/       
    }
}
