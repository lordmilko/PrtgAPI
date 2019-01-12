using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class OrganizationTests : BaseTest
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Rename_CanExecute() =>
            Execute(c => c.RenameObject(1001, "new name"), "api/rename.htm?id=1001&value=new+name");

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Rename_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.RenameObjectAsync(1001, "new name"), "api/rename.htm?id=1001&value=new+name");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Remove_CanExecute() =>
            Execute(c => c.RemoveObject(1001), "api/deleteobject.htm?id=1001&approve=1");

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Remove_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.RemoveObjectAsync(1001), "api/deleteobject.htm?id=1001&approve=1");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Sort_CanExecute() =>
            Execute(c => c.SortAlphabetically(1001), "api/sortsubobjects.htm?id=1001");

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Sort_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.SortAlphabeticallyAsync(1001), "api/sortsubobjects.htm?id=1001");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Move_CanExecute() =>
            Execute(c => c.MoveObject(1001, 2001), "moveobjectnow.htm?id=1001&targetid=2001");

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Move_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.MoveObjectAsync(1001, 2001), "moveobjectnow.htm?id=1001&targetid=2001");
    }
}
