using PrtgAPI.Tree.Internal;
using PrtgAPI.Tree.Progress;

namespace PrtgAPI.Tree.Converters.Tree
{
    /// <summary>
    /// Constructs a <see cref="PrtgOrphan"/> tre from a specified <see cref="PrtgObject"/>.
    /// </summary>
    class TreeBuilder
    {
        private TreeProgressManager progressManager;
        private ObjectManager objectManager;
        private PrtgClient client;

        internal TreeBuilder(PrtgClient client, ITreeProgressCallback progressCallback)
        {
            progressManager = new TreeProgressManager(progressCallback);
            this.client = client;
            objectManager = new ObjectManager(client);
        }

        internal PrtgOrphan GetTree(Either<PrtgObject, int> objectOrId)
        {
            if (!objectOrId.IsLeft)
            {
                if (objectOrId.Right == WellKnownId.Root)
                    objectOrId = client.GetGroup(WellKnownId.Root);
                else
                    objectOrId = client.GetObject(objectOrId.Right, true);
            }

            //With each additional level we parse, a new TreeBuilderLevel will be constructed and recursed
            var level = new TreeBuilderLevel(objectOrId.Left, progressManager, objectManager);

            return level.ProcessObject();
        }
    }
}
