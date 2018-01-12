using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that create new objects.
    /// </summary>
    /// <typeparam name="T">The type of object the cmdlet will create.</typeparam>
    public abstract class NewObjectCmdlet<T> : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">Indicates whether or not the new object should be resolved to a <see cref="PrtgObject"/>. By default this value is <see cref="SwitchParameter.Present"/>.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Resolve { get; set; } = SwitchParameter.Present;

        /// <summary>
        /// Resolves a newly created object to a <see cref="PrtgObject"/>.
        /// </summary>
        /// <param name="getObjects">A function used to retrieve the newly created object.</param>
        /// <param name="condition">A function used to validate whether the results returned from <paramref name="getObjects"/> contain the newly created object.</param>
        /// <param name="resolutionError">The error message to display if the object cannot be resolved.</param>
        /// <returns></returns>
        public List<T> ResolveObject(Func<List<T>> getObjects, Func<List<T>, bool> condition, string resolutionError = "Could not resolve object")
        {
            List<T> @object;

            var retriesRemaining = 4;
            var delay = 3;

            do
            {
                @object = getObjects();

                if (condition(@object))
                {
                    if (retriesRemaining == 0)
                    {
                        throw new ObjectResolutionException($"{resolutionError}: PRTG is taking too long to create the object. Confirm the object has been created in the Web UI and then attempt resolution again manually");
                    }

                    WriteWarning($"'{MyInvocation.MyCommand}' failed to resolve {typeof(T).Name.ToLower()}: object is still being created. Retries remaining: {retriesRemaining}");
                    retriesRemaining--;


#if DEBUG
                    if(!UnitTest())
#endif
                    Thread.Sleep(delay * 1000);

                    delay *= 2;
                }

                if (Stopping)
                    break;

            } while (condition(@object));

            return @object;
        }

        /// <summary>
        /// Resolves a newly created object to a <see cref="PrtgObject"/> by comparing the objects under its parent before and after it is created.
        /// </summary>
        /// <param name="createObject">The function to use to create the object.</param>
        /// <param name="getObjects">The function to use to retrieve the objects of the parent before and after the new object is created.</param>
        /// <param name="exceptFunc">The function to use to compare the sets of objects before and after the new object is created.</param>
        /// <returns></returns>
        public List<T> ResolveWithDiff(Action createObject, Func<List<T>> getObjects, Func<List<T>, List<T>, List<T>> exceptFunc)
        {
            var before = getObjects();

            createObject();

            var after = ResolveObject(getObjects, a => !exceptFunc(before, a).Any());

            return exceptFunc(before, after);
        }
    }
}
