using System.Management.Automation;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Tree;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a new <see cref="DeviceNode"/> for modelling a PRTG Tree.</para>
    ///
    /// <para type="description">The New-DeviceNode cmdlet creates a new <see cref="DeviceNode"/> object for modelling
    /// a PRTG Tree. Each <see cref="DeviceNode"/> object encapsulates a single <see cref="Device"/> object along with any
    /// optional children (such as <see cref="SensorNode"/> and <see cref="TriggerNode"/> items).</para>
    ///
    /// <para type="description">New-DeviceNode supports many different ways of being invoked, however
    /// at its core there are two pieces of information that can be provided to the cmdlet: information
    /// that identifies the <see cref="Device"/> to encapsulate, and the nodes that should be used as the children
    /// of the node.</para>
    ///
    /// <para type="description">If an existing <see cref="Device"/> object retrieved from the Get-Device cmdlet
    /// is not specified or piped to the -Value parameter, one or more of -Name or -Id must
    /// be specified to identify the object to encapsulate. If multiple values are returned as a result of these parameters,
    /// a unique <see cref="DeviceNode"/> object will be created for each item.</para>
    ///
    /// <para type="description">Child nodes to this node can either be specified via the pipeline, or declaratively
    /// via a ScriptBlock. When a ScriptBlock defining children is specified, <see cref="DeviceNode"/> will incorporate a copy
    /// of the children in each <see cref="DeviceNode"/> that is produced. While each <see cref="DeviceNode"/> will end
    /// up with a unique copy of all its descendant nodes, the underlying Value of each nodw will  be the same.</para>
    ///
    /// <example>
    ///     <code>
    ///         C:\> Get-Device -Id 2001 | New-DeviceNode
    ///     </code>
    ///     <para>Create a device node from the device with ID 2001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> New-DeviceNode -Id 2001</code>
    ///     <para>Create a device node from the device with ID 2001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> New-DeviceNode dc*</code>
    ///     <para>Create a device node for each device whose name starts with "dc".</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> DeviceNode -Id 2001 {
    ///         >>      SensorNode -Id 1001 {
    ///         >>          TriggerNode -ObjectId 1001
    ///         >>      }
    ///         >>
    ///         >>      SensorNode -Id 1002 {
    ///         >>          TriggerNode -ObjectId 1002
    ///         >>      }
    ///         >> }
    ///     </code>
    ///     <para>Create a new device node from the object with ID 2001 with
    /// child nodes for the sensors with IDs 1001 and 1002, each of which contains child nodes for
    /// each of their descendant notification triggers.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>New-Sensor -Id 1001,1002 | New-DeviceNode -Id 2001</code>
    /// <para>Create a new device node from the object with ID 2001 with
    /// child nodes for the sensors with IDs 1001 and 1002.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Tree-Creation">Online version:</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">New-SensorNode</para>
    /// <para type="link">New-GroupNode</para>
    /// <para type="link">New-ProbeNode</para>
    /// <para type="link">New-TriggerNode</para>
    /// <para type="link">New-PropertyNode</para>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DeviceNode", DefaultParameterSetName = ParameterSet.Default)]
    public class NewDeviceNode : PrtgTableNodeCmdlet<DeviceNode, Device>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewDeviceNode"/> class.
        /// </summary>
        public NewDeviceNode() : base(PrtgNode.Device, () => new GetDevice())
        {
        }
    }
}