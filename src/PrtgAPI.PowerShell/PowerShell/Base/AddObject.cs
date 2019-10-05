using System.Collections.Generic;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for cmdlets that add new table objects.
    /// </summary>
    /// <typeparam name="TParams">The type of parameters to use to create this object.</typeparam>
    /// <typeparam name="TObject">The type of object to create.</typeparam>
    public abstract class AddObject<TParams, TObject> : NewObjectCmdlet
        where TParams : NewObjectParameters
        where TObject : SensorOrDeviceOrGroupOrProbe, new()
    {
        /// <summary>
        /// A set of parameters whose properties describe the type of object to add, with what settings.
        /// </summary>
        protected TParams ParametersInternal { get; set; }

        internal BaseType type;

        internal AddObject(BaseType type)
        {
            this.type = type;
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            AddObjectInternal(DestinationId);
        }

        internal void AddObjectInternal(int destinationId)
        {
            if (ShouldProcess(ShouldProcessTarget, ShouldProcessAction))
            {
                ExecuteOperation(() => ExecuteOperationAction(destinationId), ProgressMessage);
            }
        }

        internal void ExecuteOperationAction(int destinationId)
        {
            if (Resolve)
            {
                var obj = AddAndResolveObject(destinationId, ParametersInternal, GetObjects);

                foreach (var o in obj)
                {
                    if (ProgressManager.RecordsProcessed < 0)
                        ProgressManager.RecordsProcessed++;

                    ProgressManager.RecordsProcessed++;

                    WriteObject(o);
                }

                ProgressManager.RecordsProcessed = -1;
            }
            else
                client.AddObject(new PrtgAPI.Either<IPrtgObject, int>(destinationId), ParametersInternal, (f, t) => GetObjects(f), false, CancellationToken);
        }

        internal virtual string WhatIfDescription()
        {
            return string.Empty;
        }

        internal abstract int DestinationId { get; }

        internal abstract string ShouldProcessTarget { get; }

        internal virtual string ShouldProcessAction => MyInvocation.MyCommand.Name;

        internal abstract string ProgressMessage { get; }

        /// <summary>
        /// Resolves the children of the <see cref="DestinationId"/> object that match the new object's name.
        /// </summary>
        /// <param name="filters">An array of search filters used to retrieve all children of the <see cref="DestinationId"/> with the specified name.</param>
        /// <returns>All objects under the parent object that match the new object's name.</returns>
        protected abstract List<TObject> GetObjects(SearchFilter[] filters);

        internal override string ProgressActivity => $"Adding PRTG {IObjectExtensions.GetTypeDescription(typeof(TObject))}s";
    }
}
