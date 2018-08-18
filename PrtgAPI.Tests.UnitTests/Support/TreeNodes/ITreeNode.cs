namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.TreeNodes
{
    interface ITreeNode
    {
        int Id { get; set; }

        ITreeNode Parent { get; set; }

        string Name { get; set; }
    }
}
