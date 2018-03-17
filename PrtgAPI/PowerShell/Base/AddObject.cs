using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for cmdlets that add new table objects.
    /// </summary>
    /// <typeparam name="TParams">The type of parameters to use to create this object.</typeparam>
    /// <typeparam name="TObject">The type of object to create.</typeparam>
    /// <typeparam name="TDestination">The type of object this object will be added under.</typeparam>
    public abstract class AddObject<TParams, TObject, TDestination> : NewObjectCmdlet
        where TParams : NewObjectParameters
        where TObject : SensorOrDeviceOrGroupOrProbe, new()
        where TDestination : DeviceOrGroupOrProbe
    {
        /// <summary>
        /// <para type="description">The parent object to create an object under.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        public TDestination Destination { get; set; }

        /// <summary>
        /// <para type="description">A set of parameters whose properties describe the type of object to add, with what settings.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Default)]
        public TParams Parameters { get; set; }

        private CommandFunction function;

        private BaseType type;

        internal AddObject(BaseType type, CommandFunction function)
        {
            this.type = type;
            this.function = function;
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            AddObjectInternal(Destination);
        }

        internal void AddObjectInternal(TDestination destination)
        {
            if (ShouldProcess($"{Parameters.Name} {WhatIfDescription()}(Destination: {destination.Name} (ID: {destination.Id}))"))
            {
                ExecuteOperation(() =>
                {
                    if (Resolve)
                    {
                        var obj = AddAndResolveObject(destination.Id, Parameters, function, GetObjects);

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
                        client.AddObject(destination.Id, Parameters, function, GetObjects, false);

                }, $"Adding {type.ToString().ToLower()} '{Parameters.Name}' to {destination.BaseType.ToString().ToLower()} ID {destination.Id}");
            }
        }

        internal virtual string WhatIfDescription()
        {
            return string.Empty;
        }

        /// <summary>
        /// Resolves the children of the <see cref="Destination"/> object that match the new object's name.
        /// </summary>
        /// <param name="filters">An array of search filters used to retrieve all children of the <see cref="Destination"/> with the specified name.</param>
        /// <returns>All objects under the parent object that match the new object's name.</returns>
        protected abstract List<TObject> GetObjects(SearchFilter[] filters);

        internal override string ProgressActivity => $"Adding PRTG {PrtgProgressCmdlet.GetTypeDescription(typeof(TObject))}s";
    }
}
