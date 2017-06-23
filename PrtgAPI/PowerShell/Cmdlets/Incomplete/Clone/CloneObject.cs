using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    //we should output the object id created and have it have a column. newobjectid or something better
    //implement supportsshouldprocess

    //input object, new name, target location

    //todo: is it possible to get a list of all auto discover templates and then auto discover USING one of them

    //make a note that when you clone an object by default we set it to use the same name as the source

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

        [Parameter(Mandatory = false)]
        public SwitchParameter Resolve { get; set; }

        protected TRet ExecuteOperation<TRet>(Func<TRet> action, string name, int objectId)
        {
            return ExecuteOperation(action, $"Cloning PRTG {typeof(T).Name}s", $"Cloning {typeof(T).Name.ToLower()} '{name}' (ID: {objectId})");
        }

        protected void ResolveObject(int id, Func<int, List<T>> getObjects)
        {
            List<T> @object;

            var retriesRemaining = 10;
            var delay = 3;

            do
            {
                //todo: maybe show some progress here to show the number of attempts we'll make

                @object = getObjects(id);

                if (@object.Count == 0)
                {
                    WriteWarning($"'{MyInvocation.MyCommand}' failed to resolve {typeof(T).Name.ToLower()}: object is still being created. Retries remaining: {retriesRemaining}");
                    Thread.Sleep(delay * 1000);

                    delay *= 2;
                }


            } while (@object.Count == 0);

            WriteObject(@object, true);
        }
    }
}
