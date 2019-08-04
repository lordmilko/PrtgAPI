using System.Management.Automation;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for cmdlets that add new table objects with parameters.
    /// </summary>
    /// <typeparam name="TParams">The type of parameters to use to create this object.</typeparam>
    /// <typeparam name="TObject">The type of object to create.</typeparam>
    /// <typeparam name="TDestination">The type of object this object will be added under.</typeparam>
    public abstract class AddParametersObject<TParams, TObject, TDestination> : AddObject<TParams, TObject>
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
        public new TParams Parameters
        {
            get { return base.Parameters; }
            set { base.Parameters = value; }
        }

        internal AddParametersObject(BaseType type) : base(type)
        {
        }

        internal void AddObjectInternal(TDestination destination)
        {
            Destination = destination;
            AddObjectInternal(destination.Id);
        }

        internal override int DestinationId => Destination.Id;

        internal override string ShouldProcessTarget =>
            $"{Parameters.Name} {WhatIfDescription()}(Destination: {Destination.Name} (ID: {Destination.Id}))";

        internal override string ProgressMessage =>
            $"Adding {type.ToString().ToLower()} '{Parameters.Name}' to {Destination.BaseType.ToString().ToLower()} '{Destination.Name}'";
    }
}
