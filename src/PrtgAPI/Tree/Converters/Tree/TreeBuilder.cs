using PrtgAPI.Tree.Internal;
using PrtgAPI.Tree.Progress;

namespace PrtgAPI.Tree.Converters.Tree
{
    /// <summary>
    /// Constructs a <see cref="PrtgOrphan"/> tre from a specified <see cref="PrtgObject"/>.
    /// </summary>
    class TreeBuilder
    {
        internal TreeProgressManager ProgressManager;
        internal ObjectManager ObjectManager;
        internal FlagEnum<TreeBuilderOptions> Options;

        private PrtgClient client;

        internal TreeBuilder(PrtgClient client, ITreeProgressCallback progressCallback, FlagEnum<TreeBuilderOptions> options)
        {
            ProgressManager = new TreeProgressManager(progressCallback);
            ObjectManager = new ObjectManager(client);
            Options = options;

            this.client = client;
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
            var level = new TreeBuilderLevel(objectOrId.Left, this);

            return level.ProcessObject();
        }
    }
}
