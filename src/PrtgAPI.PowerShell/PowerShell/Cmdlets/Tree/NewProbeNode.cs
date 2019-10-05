using System.Management.Automation;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Tree;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a new <see cref="ProbeNode"/> for modelling a PRTG Tree.</para>
    ///
    /// <para type="description">The New-ProbeNode cmdlet creates a new <see cref="ProbeNode"/> object for modelling
    /// a PRTG Tree. Each <see cref="ProbeNode"/> object encapsulates a single <see cref="Probe"/> object along with any
    /// optional children (such as <see cref="GroupNode"/> and <see cref="DeviceNode"/> items).</para>
    ///
    /// <para type="description">New-ProbeNode supports many different ways of being invoked, however
    /// at its core there are two pieces of information that can be provided to the cmdlet: information
    /// that identifies the <see cref="Probe"/> to encapsulate, and the nodes that should be used as the children
    /// of the node.</para>
    ///
    /// <para type="description">If an existing <see cref="Probe"/> object retrieved from the Get-Probe cmdlet
    /// is not specified or piped to the -Value parameter, one or more of -Name or -Id must
    /// be specified to identify the object to encapsulate. If multiple values are returned as a result of these parameters,
    /// a unique <see cref="ProbeNode"/> object will be created for each item.</para>
    ///
    /// <para type="description">Child nodes to this node can either be specified via the pipeline, or declaratively
    /// via a ScriptBlock. When a ScriptBlock defining children is specified, <see cref="ProbeNode"/> will incorporate a copy
    /// of the children in each <see cref="ProbeNode"/> that is produced. While each <see cref="ProbeNode"/> will end
    /// up with a unique copy of all its descendant nodes, the underlying Value of each nodw will  be the same.</para>
    ///
    /// <example>
    ///     <code>
    ///         C:\> Get-Probe -Id 3001 | New-ProbeNode
    ///     </code>
    ///     <para>Create a probe node from the probe with ID 3001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> New-ProbeNode -Id 3001</code>
    ///     <para>Create a probe node from the probe with ID 3001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> New-ProbeNode windows*</code>
    ///     <para>Create a probe node for each probe whose name starts with "windows".</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> ProbeNode -Id 3001 {
    ///         >>      DeviceNode -Id 2001 {
    ///         >>          SensorNode -Id 1001
    ///         >>      }
    ///         >>
    ///         >>      DeviceNode -Id 2002 {
    ///         >>          SensorNode -Id 1002
    ///         >>      }
    ///         >> }
    ///     </code>
    ///     <para>Create a new probe node from the object with ID 3001 with
    /// child nodes for the devices with IDs 2001 and 2002, which contain
    /// the sensors with IDs 1001 and 1002 respectively.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>New-Device -Id 2001,2002 | New-ProbeNode -Id 3001</code>
    /// <para>Create a new probe node from the object with ID 3001 with
    /// child nodes for the devices with IDs 2001 and 2002.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Tree-Creation">Online version:</para>
    /// <para type="link">Get-Probe</para>
    /// <para type="link">New-SensorNode</para>
    /// <para type="link">New-DeviceNode</para>
    /// <para type="link">New-GroupNode</para>
    /// <para type="link">New-TriggerNode</para>
    /// <para type="link">New-PropertyNode</para>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "ProbeNode", DefaultParameterSetName = ParameterSet.Default)]
    public class NewProbeNode : PrtgTableNodeCmdlet<ProbeNode, Probe>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewProbeNode"/> class.
        /// </summary>
        public NewProbeNode() : base(PrtgNode.Probe, () => new GetProbe())
        {
        }
    }
}