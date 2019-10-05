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
        /// <remarks>This property is abstract so that derived classes are forced to specify what parameter sets this property will be a part of.</remarks>
        public abstract TDestination Destination { get; set; }

        /// <summary>
        /// <para type="description">The parent object to create an object under.</para>
        /// </summary>
        protected TDestination DestinationInternal { get; set; }

        /// <summary>
        /// <para type="description">A set of parameters whose properties describe the type of object to add, with what settings.</para>
        /// </summary>
        /// <remarks>This property is abstract so that derived classes are forced to specify what parameter sets this property will be a part of.</remarks>
        public abstract TParams Parameters { get; set; }

        internal AddParametersObject(BaseType type) : base(type)
        {
        }

        internal void AddObjectInternal(TDestination destination)
        {
            DestinationInternal = destination;
            AddObjectInternal(destination.Id);
        }

        internal override int DestinationId => DestinationInternal.Id;

        internal override string ShouldProcessTarget =>
            $"{Parameters.Name} {WhatIfDescription()}(Destination: {DestinationInternal.Name} (ID: {DestinationInternal.Id}))";

        internal override string ProgressMessage =>
            $"Adding {type.ToString().ToLower()} '{Parameters.Name}' to {DestinationInternal.BaseType.ToString().ToLower()} '{DestinationInternal.Name}'";
    }
}
