using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Tree;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for table cmdlets that create <see cref="PrtgNode"/> objects.
    /// </summary>
    /// <typeparam name="TNode">The type of node this cmdlet creates.</typeparam>
    /// <typeparam name="TValue">The type of value <typeparamref name="TNode"/> requires.</typeparam>
    public abstract class PrtgTableNodeCmdlet<TNode, TValue> : PrtgNodeCmdlet<TNode, TValue>
        where TNode : PrtgNode
        where TValue : ITreeValue
    {
        /// <summary>
        /// <para type="description">The value to use for the node.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Default, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.AddFrom)]
        public TValue Value { get; set; }

        /// <summary>
        /// <para type="description">The name of the object to retrieve. Can include wildcards.</para>
        /// </summary>
        /// <remarks>If <see cref="Name"/> is not specified, <see cref="ScriptBlock"/> will be stored
        /// in Position 0 due to being unable to define a parameter set where <see cref="ScriptBlock"/>
        /// is at Position 0 (PowerShell will always convert it into a string and store in the <see cref="Name"/>).
        /// We work around this by detecting via the <see cref="FixupParameters"/> method, which calculates
        /// whether the value stored in <see cref="Name"/> should have actually been stored in <see cref="ScriptBlock"/>.</remarks>
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = ParameterSet.AddFromManual)]
        public object[] Name { get; set; }

        /// <summary>
        /// <para type="description">The ID of the object to retrieve.</para>
        /// </summary>
        [Alias("ObjectId")]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.AddFromManual)]
        public int[] Id { get; set; }

        /// <summary>
        /// <para type="description">A script block that returns the children to use for this node.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = ParameterSet.Manual)]
        public ScriptBlock ScriptBlock { get; set; }

        /// <summary>
        /// <para type="description">The children to use for the node.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.AddFrom, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.AddFromManual, ValueFromPipeline = true)]
        public PrtgNode[] Children { get; set; }

        private List<PrtgNode> queuedNodes = new List<PrtgNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgTableNodeCmdlet{TNode, TValue}"/> class.
        /// </summary>
        /// <param name="createNode">A function used to create nodes for this cmdlet.</param>
        /// <param name="getValues">A cmdlet used to resolve the <see cref="Value"/> when manual parameters have been specified.</param>
        /// <param name="parameterSet">The parameter set the <paramref name="getValues"/> cmdlet should be invoked with.</param>
        protected PrtgTableNodeCmdlet(Func<TValue, IEnumerable<PrtgNode>, TNode> createNode, Func<PSCmdlet> getValues, string parameterSet = null) :
            base(createNode, getValues, parameterSet)
        {
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            FixupParameters();

            switch (ParameterSetName)
            {
                //A -Value was specified (or piped)
                case ParameterSet.Default:
                    ProcessValues(Value);
                    break;

                //One or more -Ids were specified
                case ParameterSet.Manual:
                    var values = ResolveValues();
                    ProcessValues(values);
                    break;

                //One or more child nodes were specified
                case ParameterSet.AddFrom:
                case ParameterSet.AddFromManual:
                    queuedNodes.AddRange(Children);
                    break;
                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }
        }

        private void FixupParameters()
        {
            switch (ParameterSetName)
            {
                case ParameterSet.Default:
                case ParameterSet.Manual:
                    if (ScriptBlock == null && Name != null && Name.Length == 1 && Name[0] is ScriptBlock)
                    {
                        ScriptBlock = (ScriptBlock) Name[0];
                        Name = null;
                        MyInvocation.BoundParameters.Remove(nameof(Name));
                        MyInvocation.BoundParameters[nameof(ScriptBlock)] = ScriptBlock;
                    }
                    break;
            }
        }

        /// <summary>
        /// Provides an enhanced one-time, postprocessing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessingEx()
        {
            switch (ParameterSetName)
            {
                case ParameterSet.AddFrom:
                    ProcessValues(new[] { Value }, queuedNodes);
                    break;
                case ParameterSet.AddFromManual:
                    var values = ResolveValues();
                    ProcessValues(values, queuedNodes);
                    break;
            }
        }

        /// <summary>
        /// Creates nodes for all specified values and <see cref="ScriptBlock"/> results and emits them to the pipeline.
        /// </summary>
        /// <param name="values">The values to process.</param>
        protected void ProcessValues(params TValue[] values)
        {
            List<PrtgNode> children = null;

            if (ScriptBlock != null)
            {
                var result = PSObjectUtilities.CleanPSObject(ScriptBlock.Invoke())?.ToIEnumerable().ToArray();

                if (result != null)
                {
                    var invalid = result.Where(v => !(v is PrtgNode)).ToArray();

                    if (invalid.Length > 0)
                        throw new InvalidOperationException($"Expected -{nameof(ScriptBlock)} to return one or more values of type '{nameof(PrtgNode)}', however response contained an invalid value of type '{invalid[0].GetType().FullName}'.");

                    children = result.Cast<PrtgNode>().ToList();
                }
            }

            ProcessValues(values, children);
        }

        /// <summary>
        /// Validates the results of a manual resolution.
        /// </summary>
        /// <param name="values">The values to validate.</param>
        /// <returns>If the validation completed successfully, true. Otherwise, false.</returns>
        protected override bool VerifyManualResults(TValue[] values)
        {
            var result = true;

            if (base.VerifyManualResults(values))
            {
                if (HasParameter(nameof(Id)))
                {
                    var actualIds = values.Select(v => v.Id.Value);

                    foreach (var id in Id)
                    {
                        if (!actualIds.Contains(id))
                        {
                            result = false;

                            WriteInvalidOperation($"Could not resolve a {IObjectExtensions.GetTypeDescription(typeof(TValue))} with ID {id}.");
                        }   
                    }
                }
            }
            else
                result = false;

            return result;
        }
    }
}
