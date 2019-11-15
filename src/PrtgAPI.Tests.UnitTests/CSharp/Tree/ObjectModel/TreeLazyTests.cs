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
                UnitRequest.Groups("filter_objid=0")
            });

            validator.MoveNext(1);

            var tree = client.GetTreeLazy(WellKnownId.Root);
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
            var client = BaseTest.Initialize_Client(new TreeRequestResponse(TreeRequestScenario.ContainerWithGrandChild));

            var validator = new EventValidator(client, new[]
            {
                //Get Root
                UnitRequest.Groups("filter_objid=0"),

                //Get probes, Root Properties
                UnitRequest.Probes("filter_parentid=0"),
                UnitRequest.RequestObjectData(0),

                //Probe Children, Triggers
                UnitRequest.Groups("filter_parentid=1"),
                UnitRequest.Triggers(1),
                UnitRequest.RequestObjectData(810),
                UnitRequest.RequestObjectData(1)
            });

            validator.MoveNext();
            var tree = client.GetTreeLazy(WellKnownId.Root);

            validator.MoveNext(2);
            var child = tree.Children[0];
            var childAgain = tree.Children[0];

            validator.MoveNext(4);
            var grandChild = child.Children[0];
            var grandChildAgain = child.Children[0];
        }
    }
}
