using System.Threading;
using System.Threading.Tasks;
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
        internal CancellationToken Token;
        internal FlagEnum<TreeBuilderOptions> Options;

        private PrtgClient client;

        internal TreeBuilder(PrtgClient client, ITreeProgressCallback progressCallback, FlagEnum<TreeBuilderOptions> options, CancellationToken token)
        {
            ProgressManager = new TreeProgressManager(progressCallback);
            ObjectManager = new ObjectManager(client);
            Token = token;
            Options = options;

            this.client = client;
        }

        internal PrtgOrphan GetTree(Either<PrtgObject, int> objectOrId)
        {
            if (!objectOrId.IsLeft)
            {
                Token.ThrowIfCancellationRequested();

                if (objectOrId.Right == WellKnownId.Root)
                    objectOrId = client.GetGroup(WellKnownId.Root);
                else
                    objectOrId = client.GetObject(objectOrId.Right, true);
            }

            //With each additional level we parse, a new TreeBuilderLevel will be constructed and recursed
            var level = new TreeBuilderLevel(objectOrId.Left, this);

            return level.ProcessObject();
        }

        internal async Task<PrtgOrphan> GetTreeAsync(Either<PrtgObject, int> objectOrId)
        {
            if (!objectOrId.IsLeft)
            {
                Token.ThrowIfCancellationRequested();

                if (objectOrId.Right == WellKnownId.Root)
                    objectOrId = await client.GetGroupAsync(WellKnownId.Root).ConfigureAwait(false);
                else
                    objectOrId = await client.GetObjectAsync(objectOrId.Right, true).ConfigureAwait(false);
            }

            //With each additional level we parse, a new TreeBuilderLevel will be constructed and recursed
            var level = new TreeBuilderLevel(objectOrId.Left, this);

            return await level.ProcessObjectAsync().ConfigureAwait(false);
        }
    }
}
