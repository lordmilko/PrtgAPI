using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class OrganizationTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void Rename_CanExecute() =>
            Execute(c => c.RenameObject(1001, "new name"), "api/rename.htm?id=1001&value=new+name");

        [UnitTest]
        [TestMethod]
        public async Task Rename_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.RenameObjectAsync(1001, "new name"), "api/rename.htm?id=1001&value=new+name");

        [UnitTest]
        [TestMethod]
        public void Remove_CanExecute() =>
            Execute(c => c.RemoveObject(1001), "api/deleteobject.htm?id=1001&approve=1");

        [UnitTest]
        [TestMethod]
        public async Task Remove_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.RemoveObjectAsync(1001), "api/deleteobject.htm?id=1001&approve=1");

        [UnitTest]
        [TestMethod]
        public void Sort_CanExecute() =>
            Execute(c => c.SortAlphabetically(1001), "api/sortsubobjects.htm?id=1001");

        [UnitTest]
        [TestMethod]
        public async Task Sort_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.SortAlphabeticallyAsync(1001), "api/sortsubobjects.htm?id=1001");

        [UnitTest]
        [TestMethod]
        public void Move_CanExecute() =>
            Execute(c => c.MoveObject(1001, 2001), "moveobjectnow.htm?id=1001&targetid=2001");

        [UnitTest]
        [TestMethod]
        public async Task Move_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.MoveObjectAsync(1001, 2001), "moveobjectnow.htm?id=1001&targetid=2001");
    }
}
