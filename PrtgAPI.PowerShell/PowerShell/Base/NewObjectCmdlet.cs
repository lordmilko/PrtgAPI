using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that create new objects.
    /// </summary>
    public abstract class NewObjectCmdlet : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">Indicates whether or not the new object should be resolved to a <see cref="PrtgObject"/>. By default this value is <see cref="SwitchParameter.Present"/>.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Resolve { get; set; } = SwitchParameter.Present;

        internal void DisplayResolutionError(Type type, int retriesRemaining)
        {
            var typeName = IObjectExtensions.GetTypeDescription(type).ToLower();

            WriteWarning($"'{MyInvocation.MyCommand}' failed to resolve {typeName}: object is still being created. Retries remaining: {retriesRemaining}");
        }

        internal bool ShouldStop() => Stopping;
        
        internal List<T> AddAndResolveObject<T>(int destinationId, NewObjectParameters parameters,
            Func<SearchFilter[], List<T>> getObjects) where T : SensorOrDeviceOrGroupOrProbe
        {
            return AddAndResolveRunner(
                () => client.AddObject(
                    new PrtgAPI.Either<IPrtgObject, int>(destinationId),
                    parameters,
                    (f, t) => getObjects(f),
                    true,
                    CancellationToken,
                    DisplayResolutionError,
                    ShouldStop,
                    typeof(T) == typeof(Sensor)
                )
            );
        }

        internal List<T> AddAndResolveRunner<T>(Func<List<T>> addObject)
        {
            var objs = addObject();

            ProgressManager.TotalRecords = objs.Count;

            return objs;
        }
    }
}
