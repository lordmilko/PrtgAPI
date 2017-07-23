using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    //implement supportsshouldprocess

    //todo: is it possible to get a list of all auto discover templates and then auto discover USING one of them

    /// <summary>
    /// Base class for cmdlets that clone PRTG Objects.
    /// </summary>
    /// <typeparam name="T">The type of object the cmdlet will clone.</typeparam>
    public abstract class CloneObject<T> : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The ID of the device (for sensors), group or probe (for groups and devices) that will hold the cloned object.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public int DestinationId { get; set; }

        /// <summary>
        /// <para type="description">The name to rename the cloned object to.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Indicates whether or not the cloned object should be resolved into a PrtgObject. By default this value is true. If this value is false, a summary of the clone operation will be provided.</para> 
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Resolve { get; set; } = SwitchParameter.Present;

        /// <summary>
        /// Executes this cmdlet's clone action and displays a progress message (if required).
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        /// <param name="name">The name of the object that will be cloned.</param>
        /// <param name="objectId">The ID of the object that will be cloned.</param>
        protected void ExecuteOperation(Action action, string name, int objectId)
        {
            ExecuteOperation(action, $"Cloning PRTG {typeof(T).Name}s", $"Cloning {typeof(T).Name.ToLower()} '{name}' (ID: {objectId})");
        }

        /// <summary>
        /// Resolves the object ID returned from a clone method to its resultant PrtgObject.
        /// </summary>
        /// <param name="id">The object ID to resolve.</param>
        /// <param name="getObjects">The method to execute to retrieve the resultant object.</param>
        protected void ResolveObject(int id, Func<int, List<T>> getObjects)
        {
            List<T> @object;

            var retriesRemaining = 10;
            var delay = 3;

            do
            {
                @object = getObjects(id);

                if (@object.Count == 0)
                {
                    WriteWarning($"'{MyInvocation.MyCommand}' failed to resolve {typeof(T).Name.ToLower()}: object is still being created. Retries remaining: {retriesRemaining}");
                    Thread.Sleep(delay * 1000);

                    delay *= 2;
                }

                if (Stopping)
                    break;

            } while (@object.Count == 0);

            WriteObject(@object, true);
        }
    }
}
