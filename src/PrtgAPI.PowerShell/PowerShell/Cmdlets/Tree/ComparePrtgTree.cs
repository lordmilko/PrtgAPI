using System;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Tree;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Compares two <see cref="PrtgNode"/> trees and creates a new tree that describes their differences.</para>
    ///
    /// <para type="description">The Compare-PrtgTree cndlet compares two <see cref="PrtgNode"/> trees and creates
    /// a comparison tree that describes their differences. If the -<see cref="Reduce"/> parameter is specified, the
    /// resulting tree will be reduced to only the branches that contain differences (if no branches contain differences
    /// the tree will be reduced to null).</para>
    ///
    /// <para type="description">By default all tree node difference types will be considered in the comparison.
    /// This can be overridden by specifying types to the -<see cref="Include"/> and -<see cref="Ignore"/> parameters. Any
    /// parameters specified to -<see cref="Ignore"/> will override any values that would otherwise be included.</para>
    ///
    /// <example>
    ///     <code>C:\> $firstTree | Compare-PrtgTree $secondTree</code>
    ///     <para>Compare two <see cref="PrtgNode"/> trees.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $firstTree | Compare-PrtgTree $secondTree -Reduce</code>
    ///     <para>Compare two <see cref="PrtgNode"/> trees, reducing the comparison to tree to only branches that contain differences.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $firstTree | Compare-PrtgTree $secondTree -Include Added,Removed -Reduce</code>
    ///     <para>Compare two <see cref="PrtgNode"/> trees, including only nodes that have been added or removed and then reducing the result to only the pertinent branches.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $firstTree | Compare-PrtgTree $secondTree -Ignore Value</code>
    ///     <para>Compare two <see cref="PrtgNode"/> trees, ignoring differences in <see cref="PropertyNode"/> values.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Tree-Manipulation">Online version:</para>
    /// <para type="link">Get-PrtgTree</para>
    /// </summary>
    [Cmdlet(VerbsData.Compare, "PrtgTree")]
    public class ComparePrtgTree : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The root of the first tree to compare.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public PrtgNode First { get; set; }

        /// <summary>
        /// <para type="description">The root of the second tree to compare.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public PrtgNode Second { get; set; }

        /// <summary>
        /// <para type="description">Specifies the types of comparisons to consider. By default all comparisons will be performed.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public TreeNodeDifference[] Include { get; set; }

        /// <summary>
        /// <para type="description">Specifies the comparison differences to ignore. Supersedes any conflicting values specified to -<see cref="Type"/>.</para> 
        /// </summary>
        [Parameter(Mandatory = false)]
        public TreeNodeDifference[] Ignore { get; set; }

        /// <summary>
        /// <para type="description">Reduce the resulting tree to only the branches that contain differences.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Reduce { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            var differences = GetDifferences();

            var comparison = First.CompareTo(Second, differences);

            if (Reduce)
                comparison = comparison.Reduce();

            WriteObject(comparison);
        }

        private TreeNodeDifference[] GetDifferences()
        {
            var includeFlags = new FlagEnum<TreeNodeDifference>(Include);
            var ignoreFlags = new FlagEnum<TreeNodeDifference>(Ignore);

            if (includeFlags == TreeNodeDifference.None && ignoreFlags != TreeNodeDifference.None)
                return (~ignoreFlags).GetValues().ToArray();

            var resultFlags = includeFlags & ~ignoreFlags;

            return resultFlags.GetValues().ToArray();
        }
    }
}
