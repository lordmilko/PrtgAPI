using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
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
    public abstract class AddObject<TParams, TObject, TDestination> : NewObjectCmdlet<TObject>
        where TParams : NewObjectParameters
        where TObject : SensorOrDeviceOrGroupOrProbe, new()
        where TDestination : DeviceOrGroupOrProbe
    {
        /// <summary>
        /// <para type="description">The parent object to create an object under.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public TDestination Destination { get; set; }

        /// <summary>
        /// <para type="description">A set of parameters whose properties describe the type of object to add, with what settings.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Default")]
        public TParams Parameters { get; set; }

        private CommandFunction function;

        private BaseType type;

        internal AddObject(BaseType type, CommandFunction function)
        {
            this.type = type;
            this.function = function;
        }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"{Parameters.Name} ({Destination.BaseType} ID: {Destination.Id}) (Destination: {Destination.Name} (ID: {Destination.Id}))"))
            {
                ExecuteOperation(() =>
                {
                    if (Resolve)
                    {
                        var filters = new[]
                        {
                            new SearchFilter(Property.ParentId, Destination.Id),
                            new SearchFilter(Property.Name, Parameters.Name)
                        };

                        var obj = ResolveWithDiff(
                            () => client.AddObject(Destination.Id, Parameters, function),
                            () => GetObjects(filters),
                            Except
                        ).OrderBy(o => o.Id).First();

                        WriteObject(obj);
                    }
                    else
                        client.AddObject(Destination.Id, Parameters, function);
                }, $"Adding PRTG {Destination.BaseType}s", $"Adding {type} '{Parameters.Name}' to {Destination.BaseType.ToString().ToLower()} ID {Destination.Id}");
            }
        }

        private List<TObject> Except(List<TObject> before, List<TObject> after)
        {
            var beforeIds = before.Select(b => b.Id).ToList();

            return after.Where(a => !beforeIds.Contains(a.Id)).ToList();
        }

        /// <summary>
        /// Resolves the children of the <see cref="Destination"/> object that match the new object's name.
        /// </summary>
        /// <param name="filters">An array of search filters used to retrieve all children of the <see cref="Destination"/> with the specified name.</param>
        /// <returns>All objects under the parent object that match the new object's name.</returns>
        protected abstract List<TObject> GetObjects(SearchFilter[] filters);
    }
}
