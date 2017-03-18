using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.PowerShell.Cmdlets;
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

        [TestMethod]
        [ExpectedException(typeof(PrtgRequestException))]
        public void PrtgClient_Constructor_CantRetrievePassHash()
        {
            var webClient = new MockWebClient(new PassHashResponse("PRTG Network Monitor is starting"));

            var client = new PrtgClient("prtg.example.com", "username", "password", AuthMode.Password, webClient);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PrtgClient_Constructor_ServerCannotBeNull()
        {
            var client = new PrtgClient(null, "username", "password");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PrtgClient_Constructor_UsernameCannotBeNull()
        {
            var client = new PrtgClient("prtg.example.com", null, "password");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PrtgClient_Constructor_PasswordCannotBeNull()
        {
            var client = new PrtgClient("prtg.example.com", "username", null);
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
