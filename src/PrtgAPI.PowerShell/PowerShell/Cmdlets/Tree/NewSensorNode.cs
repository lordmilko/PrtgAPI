using System.Management.Automation;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Tree;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a new <see cref="SensorNode"/> for modelling a PRTG Tree.</para>
    ///
    /// <para type="description">The New-SensorNode cmdlet creates a new <see cref="SensorNode"/> object for modelling
    /// a PRTG Tree. Each <see cref="SensorNode"/> object encapsulates a single <see cref="Sensor"/> object along with any
    /// optional children (such as <see cref="TriggerNode"/> and <see cref="PropertyNode"/> items).</para>
    ///
    /// <para type="description">New-SensorNode supports many different ways of being invoked, however
    /// at its core there are two pieces of information that can be provided to the cmdlet: information
    /// that identifies the <see cref="Sensor"/> to encapsulate, and the nodes that should be used as the children
    /// of the node.</para>
    ///
    /// <para type="description">If an existing <see cref="Sensor"/> object retrieved from the Get-Sensor cmdlet
    /// is not specified or piped to the -Value parameter, one or more of -Name or -Id must
    /// be specified to identify the object to encapsulate. If multiple values are returned as a result of these parameters,
    /// a unique <see cref="SensorNode"/> object will be created for each item.</para>
    ///
    /// <para type="description">Child nodes to this node can either be specified via the pipeline, or declaratively
    /// via a ScriptBlock. When a ScriptBlock defining children is specified, <see cref="SensorNode"/> will incorporate a copy
    /// of the children in each <see cref="SensorNode"/> that is produced. While each <see cref="SensorNode"/> will end
    /// up with a unique copy of all its descendant nodes, the underlying Value of each nodw will  be the same.</para>
    ///
    /// <example>
    ///     <code>
    ///         C:\> Get-Sensor -Id 1001 | New-SensorNode
    ///     </code>
    ///     <para>Create a sensor node from the sensor with ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> New-SensorNode -Id 1001</code>
    ///     <para>Create a sensor node from the sensor with ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> New-SensorNode cpu*</code>
    ///     <para>Create a sensor node for each sensor whose name starts with "cpu".</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> SensorNode -Id 1001 {
    ///         >>      TriggerNode -ObjectId 1001
    ///         >> }
    ///     </code>
    ///     <para>Create a new sensor node from the object with ID 1001 with
    /// child nodes for each notification trigger under that object.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>New-TriggerNode -ObjectId 1001 | New-SensorNode -Id 1001</code>
    /// <para>Create a new sensor node from the object with ID 1001 with
    /// child nodes for each notification trigger under that object.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Tree-Creation">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">New-DeviceNode</para>
    /// <para type="link">New-GroupNode</para>
    /// <para type="link">New-ProbeNode</para>
    /// <para type="link">New-TriggerNode</para>
    /// <para type="link">New-PropertyNode</para>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "SensorNode", DefaultParameterSetName = ParameterSet.Default)]
    public class NewSensorNode : PrtgTableNodeCmdlet<SensorNode, Sensor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewSensorNode"/> class.
        /// </summary>
        public NewSensorNode() : base(PrtgNode.Sensor, () => new GetSensor())
        {
        }
    }
}
