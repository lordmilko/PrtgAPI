using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.Tree
{
    [TestClass]
    public class TreeLazyTests : BaseTreeTest
    {
        [UnitTest]
        [TestMethod]
        public void Tree_Lazy_Id_OneAPIRequest()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.ContainerWithGrandChild));

            var validator = new EventValidator(client, new[]
            {
                UnitRequest.Objects("filter_objid=1001"),
                UnitRequest.Probes("filter_objid=1001&filter_parentid=0")
            });

            validator.MoveNext(2);

            var tree = client.GetTreeLazy(1001);
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Lazy_Object_NoAPIRequests()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.ContainerWithGrandChild));

            var validator = new EventValidator(client, new string[] {});

            var tree = client.GetTreeLazy(new Group {Id = 0});
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Lazy_Child_ResolvesOnlyOnce()
        {
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.MultiLevelContainer));

            var validator = new EventValidator(client, new[]
            {
                //Get Object
                UnitRequest.Objects("filter_objid=1001"),
                
                //Get Probe
                UnitRequest.Probes("filter_objid=1001&filter_parentid=0"),

                //Probe -> Devices/Groups
                UnitRequest.Devices("filter_parentid=1001"),
                UnitRequest.Groups("filter_parentid=1001"),

                //Probe -> Device -> Sensors
                UnitRequest.Sensors("filter_parentid=3001")
            });

            validator.MoveNext(2);
            var tree = client.GetTreeLazy(1001);

            validator.MoveNext(2);
            var child = tree.Children[0];
            var childAgain = tree.Children[0];

            validator.MoveNext();
            var grandChild = child.Children[0];
            var grandChildAgain = child.Children[0];
        }
    }
}
