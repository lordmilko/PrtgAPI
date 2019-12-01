using System;
using System.Collections.Generic;
using System.Linq;
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
        internal FlagEnum<TreeParseOption> Options;
        internal FlagEnum<TreeRequestType> RequestType;

        private PrtgClient client;

        internal TreeBuilder(PrtgClient client, FlagEnum<TreeParseOption>? options, ITreeProgressCallback progressCallback, FlagEnum<TreeRequestType> requestType, CancellationToken token)
        {
            ProgressManager = new TreeProgressManager(progressCallback);
            ObjectManager = new ObjectManager(client);
            Token = token;
            Options = options ?? TreeParseOption.Common;
            RequestType = requestType;

            this.client = client;
        }

        internal PrtgOrphan GetTree(Either<PrtgObject, int> objectOrId)
        {
            if (IsRoot(objectOrId) && !RequestType.Contains(TreeRequestType.Lazy))
                return GetTreeFast();

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
            if (IsRoot(objectOrId) && !RequestType.Contains(TreeRequestType.Lazy))
                return await GetTreeFastAsync().ConfigureAwait(false);

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

        private PrtgOrphan GetTreeFast()
        {
            List<Probe> probes = null;
            List<Group> groups = null;
            List<Device> devices = null;
            List<Sensor> sensors = null;

            List<Action<int, int>> actions = new List<Action<int, int>>();

            using (ProgressManager.ProcessLevel())
            {
                if (Options.Contains(TreeParseOption.Probes))
                {
                    actions.Add((i, t) =>
                    {
                        ProgressManager.OnProcessType(PrtgNodeType.Probe, i, t);
                        probes = client.GetProbes();
                    });
                }

                if (Options.Contains(TreeParseOption.Groups))
                {
                    actions.Add((i, t) =>
                    {
                        ProgressManager.OnProcessType(PrtgNodeType.Group, i, t);
                        groups = client.GetGroups();
                    });
                }
                else
                {
                    actions.Add((i, t) => { groups = client.GetGroups(Property.Id, WellKnownId.Root); });
                }

                if (Options.Contains(TreeParseOption.Devices))
                {
                    actions.Add((i, t) =>
                    {
                        ProgressManager.OnProcessType(PrtgNodeType.Device, i, t);
                        devices = client.GetDevices();
                    });
                }

                if (Options.Contains(TreeParseOption.Sensors))
                {
                    actions.Add((i, t) =>
                    {
                        ProgressManager.OnProcessType(PrtgNodeType.Sensor, 4, 4);
                        sensors = client.GetSensors();
                    });
                }

                for (var i = 1; i <= actions.Count; i++)
                {
                    actions[i - 1](i, actions.Count);
                }

                ObjectManager = new CachedObjectManager(
                    client,
                    sensors,
                    devices,
                    groups,
                    probes
                );
            }

            ProgressManager = new TreeProgressManager(null);

            var root = groups.First(g => g.Id == WellKnownId.Root);

            var level = new TreeBuilderLevel(root, this);

            return level.ProcessObject();
        }

        private async Task<PrtgOrphan> GetTreeFastAsync()
        {
            var config = new FastAsyncHelper();

            if (Options.Contains(TreeParseOption.Probes))
                config.Probe.CreateTask = GetFastObjectsAsync(PrtgNodeType.Probe, client.GetProbesAsync());

            if (Options.Contains(TreeParseOption.Groups))
                config.Group.CreateTask = GetFastObjectsAsync(PrtgNodeType.Group, client.GetGroupsAsync());
            else
                config.Group.CreateTask = GetFastObjectsAsync(PrtgNodeType.Group, client.GetGroupsAsync(Property.Id, WellKnownId.Root), true);

            if (Options.Contains(TreeParseOption.Devices))
                config.Device.CreateTask = GetFastObjectsAsync(PrtgNodeType.Device, client.GetDevicesAsync());

            if (Options.Contains(TreeParseOption.Sensors))
                config.Sensor.CreateTask = GetFastObjectsAsync(PrtgNodeType.Sensor, client.GetSensorsAsync());

            await config.Stage1Async().ConfigureAwait(false);

            //Await sensors separately so we don't kill PRTG
            await config.Stage2Async().ConfigureAwait(false);

            ObjectManager = new CachedObjectManager(
                client,
                await config.Sensor.Task.ConfigureAwait(false),
                await config.Device.Task.ConfigureAwait(false),
                await config.Group.Task.ConfigureAwait(false),
                await config.Probe.Task.ConfigureAwait(false)
            );

            var root = (await config.Group.Task.ConfigureAwait(false)).First(g => g.Id == WellKnownId.Root);

            var level = new TreeBuilderLevel(root, this);

            return await level.ProcessObjectAsync().ConfigureAwait(false);
        }

        private object lockObj = new object();

        private Func<int, int, Task> GetFastObjectsAsync<T>(PrtgNodeType type, Task<List<T>> getObjectsAsync, bool skipProgress = false)
        {
            Func<int, int, Task<List<T>>> func = async (i, t) =>
            {
                if (!skipProgress)
                {
                    lock (lockObj)
                    {
                        ProgressManager.OnProcessType(type, i, t);
                    }
                }
                
                return await getObjectsAsync.ConfigureAwait(false);
            };

            return func;
        }

        private bool IsRoot(Either<PrtgObject, int> objectOrId)
        {
            if (objectOrId.IsLeft)
                return objectOrId.Left.Id == WellKnownId.Root;

            return objectOrId.Right == WellKnownId.Root;
        }

        #region Fast Async Helpers

        class FastAsyncHelper
        {
            internal FastAsyncTask<Sensor> Sensor { get; } = new FastAsyncTask<Sensor>();

            internal FastAsyncTask<Device> Device { get; } = new FastAsyncTask<Device>();

            internal FastAsyncTask<Group> Group { get; } = new FastAsyncTask<Group>();

            internal FastAsyncTask<Probe> Probe { get; } = new FastAsyncTask<Probe>();

            private int count;

            public async Task Stage1Async()
            {
                var fastTasks = new List<FastAsyncTask>();

                if (Probe.CreateTask != null)
                    fastTasks.Add(Probe);

                if (Group.CreateTask != null)
                    fastTasks.Add(Group);

                if (Device.CreateTask != null)
                    fastTasks.Add(Device);

                count = fastTasks.Count;

                if (Sensor.CreateTask != null)
                    count++;

                var tasks = fastTasks.Select((v, i) => v.GetTask(i, count));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            public async Task Stage2Async()
            {
                if (Sensor.CreateTask != null)
                    await Sensor.GetTask(count, count).ConfigureAwait(false);
            }
        }

        class FastAsyncTask
        {
            internal Func<int, int, Task> CreateTask;

            protected Task task;

            internal Task GetTask(int index, int total)
            {
                if (CreateTask != null && task == null)
                    task = CreateTask(index, total);

                return task;
            }
        }

        class FastAsyncTask<T> : FastAsyncTask
        {
            internal Task<List<T>> Task
            {
                get
                {
                    if (CreateTask != null && task == null)
                        throw new NotImplementedException();

                    if (task == null)
                        return System.Threading.Tasks.Task.FromResult(new List<T>());

                    return (Task<List<T>>) task;
                }
            }
        }

        #endregion
    }
}
