using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for cmdlets that add new table objects.
    /// </summary>
    /// <typeparam name="TParams">The type of parameters to use to create this object.</typeparam>
    /// /// <typeparam name="TDestination">The type of object this object will be added under.</typeparam>
    public abstract class AddObject<TParams, TDestination> : PrtgOperationCmdlet where TParams : NewObjectParameters where TDestination : SensorOrDeviceOrGroupOrProbe
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
            if (ShouldProcess($"{Parameters.Name} ({Destination.BaseType} ID: {Destination.Id})"))
            {
                ExecuteOperation(() => client.AddObject(Destination.Id, Parameters, function), $"Adding PRTG {Destination.BaseType}s", $"Adding {type} '{Parameters.Name}' to {Destination.BaseType.ToString().ToLower()} ID {Destination.Id}");
            }
        }
    }
}
