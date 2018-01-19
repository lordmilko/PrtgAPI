using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.DataTests
{
    [TestClass]
    public class GetObjectProperties : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_GetObjectProperties_RetrievesAllRawProperties()
        {
            var dictionary = client.GetObjectPropertiesRaw(0, ObjectType.Group);

            Assert.AreEqual("Root", dictionary["name"], "Failed to retrieve name");
            Assert.AreEqual(Settings.Server, dictionary["windowslogindomain"], "Failed to retrieve domain");
            Assert.AreEqual(Settings.WindowsUserName.ToLower(), dictionary["windowsloginusername"].ToLower(), "Failed to retrieve username");
        }
    }
}
