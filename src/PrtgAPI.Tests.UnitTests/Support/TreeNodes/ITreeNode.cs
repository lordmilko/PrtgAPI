namespace PrtgAPI.Tests.UnitTests.TreeNodes
{
    interface ITreeNode
    {
        int Id { get; set; }

        ITreeNode Parent { get; set; }

        string Name { get; set; }
    }
}
