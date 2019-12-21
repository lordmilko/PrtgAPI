using System.Management.Automation;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Tree;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a new <see cref="PropertyNode"/> for modelling a PRTG Tree.</para>
    ///
    /// <para type="description">The New-PropertyNode cmdlet creates a new <see cref="PropertyNode"/> object for modelling
    /// a PRTG Tree. Each <see cref="PropertyNode"/> object encapsulates a single <see cref="PropertyValuePair"/> object.</para>
    ///
    /// <para type="description">If an existing <see cref="PropertyValuePair"/> from a PRTG Tree is not specified or piped to the
    /// -<see cref="Value"/> parameter, the -<see cref="Id"/> or -<see cref="Object"/>, <see cref="Property"/> and optional
    /// <see cref="PropertyValue"/> can be specified to identify the information that should be encapsulated in a node. If a
    /// -<see cref="PropertyValue"/> is not specified, New-PropertyNode will perform a lookup against the specified -<see cref="Property"/>
    /// to get the current property value from PRTG. -<see cref="Property"/> accepts both raw property names as well as type safe
    /// <see cref="ObjectProperty"/> values.</para>
    ///
    /// <example>
    ///     <code>C:\> $node.Value | New-PropertyNode</code>
    ///     <para>Create a property node from the PropertyValuePair of an existing node.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1001 | New-PropertyNode Name Ping</code>
    ///     <para>Create a property node for the Name property with the value "Ping" for the object ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>New-PropertyNode -Id 1001 Name Ping</code>
    ///     <para>Create a property node for the Name property with the value "Ping" for the object ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>New-PropertyNode -Id 1001 Name</code>
    ///     <para>Create a property node with the current value of the Name property for the object with ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>New-PropertyNode -Id 1001 name_</code>
    ///     <para>Create a property node with the current value of the name_ raw property for the object with ID 1001.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Tree-Creation">Online version:</para>
    /// <para type="link">New-SensorNode</para>
    /// <para type="link">New-DeviceNode</para>
    /// <para type="link">New-GroupNode</para>
    /// <para type="link">New-ProbeNode</para>
    /// <para type="link">New-TriggerNode</para>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "PropertyNode", DefaultParameterSetName = ParameterSet.Default)]
    public class NewPropertyNode : PrtgNodeCmdlet<PropertyNode, PropertyValuePair>
    {
        /// <summary>
        /// <para type="description">The object that contains the property.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.ObjectWithManual, ValueFromPipeline = true)]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// <para type="description">The value to use for the node.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Default, ValueFromPipeline = true)]
        public PropertyValuePair Value { get; set; }

        /// <summary>
        /// <para type="description">The ID of the object containing the property.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.ManualWithManual)]
        public int Id { get; set; }

        /// <summary>
        /// <para type="description">The <see cref="ObjectProperty"/> or raw property to retrieve.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.ObjectWithManual)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.ManualWithManual)]
        public PrtgAPI.Either<ObjectProperty, string>? Property { get; set; } //Nullable to prevent issues with default Either<> not being allowed

        /// <summary>
        /// <para type="description">The value of the property. If this value is emitted, New-PropertyNode will resolve the <see cref="Property"/>
        /// value from PRTG.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = ParameterSet.ObjectWithManual)]
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = ParameterSet.ManualWithManual)]
        public object PropertyValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewPropertyNode"/> class.
        /// </summary>
        public NewPropertyNode() : base((p, c) => PrtgNode.Property(p), () => new GetObjectProperty())
        {
        }

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessingEx()
        {
            if (HasParameter(nameof(Property)) && !Property.Value.IsLeft)
            {
                ObjectProperty property;

                if (System.Enum.TryParse(Property.Value.Right, true, out property))
                    Property = property;
            }

            base.BeginProcessingEx();
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            switch (ParameterSetName)
            {
                case ParameterSet.Default:
                    WriteObject(PrtgNode.Property(Value));
                    break;
                case ParameterSet.ObjectWithManual:
                    ProcessManual(Object);
                    break;
                case ParameterSet.ManualWithManual:
                    ProcessManual(Id);
                    break;
                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }
        }

        private void ProcessManual(PrtgAPI.Either<IPrtgObject, int> objectOrId)
        {
            var value = ResolveValue(objectOrId);

            var node = PrtgNode.Property(objectOrId, Property.Value, value);

            WriteObject(node);
        }

        private object ResolveValue(PrtgAPI.Either<IPrtgObject, int> objectOrId)
        {
            if (!HasParameter(nameof(PropertyValue)))
            {
                if (Property.Value.IsLeft)
                    return client.GetObjectProperty(objectOrId, Property.Value.Left);
                else
                    return client.GetObjectPropertyRaw(objectOrId, Property.Value.Right);
            }
            else
                return PropertyValue;
        }
    }
}
