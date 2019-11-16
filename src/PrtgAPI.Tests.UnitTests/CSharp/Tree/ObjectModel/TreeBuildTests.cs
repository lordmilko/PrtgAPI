using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.Tree
{
    [TestClass]
    public class TreeBuildTests : BaseTreeTest
    {
        [UnitTest]
        [TestMethod]
        public void Tree_Builds()
        {
            var response = new TreeRequestResponse(TreeRequestScenario.MultiLevelContainer);

            var client = BaseTest.Initialize_Client(response);

            var tree = client.GetTree();
        }

        [UnitTest]
        [TestMethod]
        public async Task Tree_BuildsAsync()
        {
            var response = new TreeRequestResponse(TreeRequestScenario.MultiLevelContainer, true);

            var client = BaseTest.Initialize_Client(response);

            var tree = await client.GetTreeAsync();
        }
    }
}
