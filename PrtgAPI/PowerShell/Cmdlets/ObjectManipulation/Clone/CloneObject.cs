using System;
using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    //implement supportsshouldprocess

    //todo: is it possible to get a list of all auto discover templates and then auto discover USING one of them

    /// <summary>
    /// Base class for cmdlets that clone PRTG Objects.
    /// </summary>
    /// <typeparam name="T">The type of object the cmdlet will clone.</typeparam>
    public abstract class CloneObject<T> : NewObjectCmdlet<T>
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
        /// Resolves the object ID returned from a clone method to its resultant <see cref="PrtgObject"/> .
        /// </summary>
        /// <param name="id">The object ID to resolve.</param>
        /// <param name="getObjects">The method to execute to retrieve the resultant object.</param>
        protected void ResolveObject(int id, Func<int, List<T>> getObjects)
        {
            WriteObject(ResolveObject(() => getObjects(id), o => o.Count == 0, "Could not resolve object with ID '{id}'"), true);
        }
    }
}
