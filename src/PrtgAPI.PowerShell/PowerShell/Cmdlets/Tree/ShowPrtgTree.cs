using System;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.PowerShell.Tree;
using PrtgAPI.Reflection;
using PrtgAPI.Tree;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Pretty prints a <see cref="TreeNode"/> object.</para>
    ///
    /// <para type="description">The Show-PrtgTree cmdlet pretty prints a PRTG Tree Object, such as a
    /// <see cref="PrtgNode"/> or <see cref="CompareNode"/>. When printing <see cref="PrtgNode"/> objects,
    /// key nodes are colored according to their state or purpose (e.g. sensors that are Up are green, Down red, etc).
    /// When printing <see cref="CompareNode"/> objects, nodes are colored according to the result of their comparisons.
    /// If an object is red, it was removed, green added, yellow changed.</para>
    ///
    /// <example>
    ///     <code>C:\> Get-PrtgTree -Id 1 | Show-PrtgTree</code>
    ///     <para>Prints the PRTG Tree for the object with ID 1001.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Tree-Manipulation#visualization-1">Online version:</para>
    /// <para type="link">Get-PrtgTree</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Show, "PrtgTree", DefaultParameterSetName = ParameterSet.Default)]
    public class ShowPrtgTree : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The tree to print.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        public TreeNode Tree { get; set; }

        /// <summary>
        /// <para type="description">Reduces the specified <see cref="Tree"/> before printing. If the specified
        /// tree does not support reducing, a <see cref="ParameterBindingException"/> will be thrown.</para> 
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Default)]
        public SwitchParameter Reduce { get; set; }

        /// <summary>
        /// <para type="description">The object whose tree should be printed.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Object)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">Specifies the types of descendants to include when constructing a <see cref="PrtgNode"/> tree.<para/>
        /// If no value is specified, <see cref="TreeParseOption.Common"/> will be used.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public TreeParseOption[] Options { get; }

        /// <summary>
        /// <para type="description">The ID of the object whose tree should be printed.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int Id { get; set; }

        internal override string ProgressActivity => "Printing PRTG Tree";

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            TreeNode tree = null;

            switch (ParameterSetName)
            {
                case ParameterSet.Default:
                    tree = Tree;

                    if (Reduce)
                    {
                        if (tree is CompareNode)
                            tree = ((CompareNode) tree).Reduce();
                        else
                            throw new ParameterBindingException($"Cannot reduce tree specified to parameter -{nameof(Tree)}: tree does not support reduction.");
                    }

                    break;

                case ParameterSet.Object:
                    tree = GetTree(Object);
                    break;

                case ParameterSet.Manual:
                    tree = GetTree(Id);
                    break;

                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }

            ProcessTree(tree);
        }

        private PrtgNode GetTree(PrtgAPI.Either<PrtgObject, int> objectOrId)
        {
            return client.GetTree(objectOrId, GetPrtgTree.GetOptions(Options), new PowerShellTreeProgressCallback(this, true));
        }

        private void ProcessTree(TreeNode tree)
        {
            if (tree is PrtgNode)
                ((PrtgNode) tree).PrettyPrint(new PowerShellPrettyColorWriter(this));
            else if (tree is CompareNode)
                ((CompareNode) tree).PrettyPrint(new PowerShellPrettyColorWriter(this));
            else
            {
                if (Reduce && tree == null)
                {
                    WriteWarning("The tree was reduced to nothing.");
                    return;
                }

                if (Tree.GetType().ImplementsRawGenericInterface(typeof(INodeList<>)))
                {
                    foreach (TreeNode node in ObjectExtensions.ToIEnumerable(Tree))
                        ProcessTree(node);
                }
                else
                    throw new NotImplementedException($"Don't know how to print a tree of type '{tree.GetType()}'.");
            }
        }
    }
}
