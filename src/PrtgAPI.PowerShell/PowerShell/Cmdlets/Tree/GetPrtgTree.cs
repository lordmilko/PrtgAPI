using System.Management.Automation;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Tree;
using PrtgAPI.PowerShell.Tree;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves a PRTG Tree for a PRTG Object.</para>
    ///
    /// <para type="description">The Get-PrtgTree cmdlet retrieves a PRTG Tree for a PRTG Object.
    /// Get-PrtgTree will recurse all children of a specified object to construct a hierarchy of nodes
    /// representing the specified object and all of its descendants. PRTG Node Trees can then be further
    /// manipulated using various extension methods, or passed to additional PRTG Tree cmdlets for further
    /// processing.</para>
    ///
    /// <example>
    ///     <code>C:\> Get-PrtgTree</code>
    ///     <para>Construct a PRTG Tree from the Root Object (ID: 0).</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe -Id 1 | Get-PrtgTree</code>
    ///     <para>Construct a PRTG Tree from the Probe with ID 1.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-PrtgTree -Id 1</code>
    ///     <para>Construct a PRTG Tree from the object with ID 1.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Tree-Creation#powershell">Online version:</para>
    /// <para type="link">Show-PrtgTree</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "PrtgTree", DefaultParameterSetName = ParameterSet.Default)]
    public class GetPrtgTree : PrtgProgressCmdlet
    {
        /// <summary>
        /// <para type="description">The object to construct a tree for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">Specifies the types of descendants to include when constructing a <see cref="PrtgNode"/> tree.<para/>
        /// If no value is specified, <see cref="TreeParseOption.Common"/> will be used.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public TreeParseOption[] Options { get; }

        /// <summary>
        /// <para type="description">The ID of the object to construct a tree for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int Id { get; set; }

        /// <summary>
        /// <para type="description">Specifies that child nodes should be retrieved lazily on demand rather than synchronously all at once.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Lazy { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPrtgTree"/> class.
        /// </summary>
        public GetPrtgTree() : base("Tree")
        {
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            PrtgNode tree = null;

            switch (ParameterSetName)
            {
                case ParameterSet.Default:

                    if (Object == null)
                        tree = GetTree(WellKnownId.Root);
                    else
                        tree = GetTree(Object);
                    break;

                case ParameterSet.Manual:
                    tree = GetTree(Id);
                    break;

                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }

            WriteObject(tree);
        }

        private PrtgNode GetTree(PrtgAPI.Either<PrtgObject, int> objectOrId)
        {
            var options = GetOptions(Options);

            if (Lazy)
                return client.GetTreeLazy(objectOrId, options);

            return client.GetTree(objectOrId, options, new PowerShellTreeProgressCallback(this));
        }

        internal static FlagEnum<TreeParseOption> GetOptions(TreeParseOption[] options)
        {
            if (options?.Length > 0)
                return new FlagEnum<TreeParseOption>(options);

            return TreeParseOption.Common;
        }
    }
}
