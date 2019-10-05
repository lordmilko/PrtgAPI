using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.Tree;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for cmdlets that create <see cref="PrtgNode"/> objects.
    /// </summary>
    /// <typeparam name="TNode">The type of node this cmdlet creates.</typeparam>
    /// <typeparam name="TValue">The type of value <typeparamref name="TNode"/> requires.</typeparam>
    public abstract class PrtgNodeCmdlet<TNode, TValue> : PrtgCmdlet
        where TNode : PrtgNode
        where TValue : ITreeValue
    {
        private Func<TValue, IEnumerable<PrtgNode>, TNode> createNode;

        private Func<PSCmdlet> getValues;

        private string parameterSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgNodeCmdlet{TNode, TValue}"/> class.
        /// </summary>
        /// <param name="createNode">A function used to create nodes for this cmdlet.</param>
        /// <param name="getValues">A cmdlet used to resolve the value to use for the cmdlet when manual parameters have been specified.</param>
        /// <param name="parameterSet">The parameter set the <paramref name="getValues"/> cmdlet should be invoked with.</param>
        protected PrtgNodeCmdlet(Func<TValue, IEnumerable<PrtgNode>, TNode> createNode, Func<PSCmdlet> getValues, string parameterSet = null)
        {
            this.createNode = createNode;
            this.getValues = getValues;
            this.parameterSet = parameterSet;
        }

        /// <summary>
        /// Resolves the value to use for the node based on the parameters that were specified to the cmdlet.
        /// </summary>
        /// <returns>The values to encapsulate in a node.</returns>
        protected TValue[] ResolveValues()
        {
            var innerCmdlet = getValues();

            var set = parameterSet;

            if (set == null)
            {
                var attrib = innerCmdlet.GetType().GetCustomAttribute<CmdletAttribute>();

                if (attrib.DefaultParameterSetName == null)
                    throw new NotImplementedException($"Parameter set for cmdlet {innerCmdlet.GetType().FullName} must be specified when a DefaultParameterSet is not present.");
                else
                    parameterSet = attrib.DefaultParameterSetName;
            }

            var invoker = new PSCmdletInvoker(this, innerCmdlet, parameterSet);

            var boundParameters = GetResolverParameters();

            invoker.Invoke(boundParameters);

            var result = invoker.Output;

            var casted = result.Cast<TValue>().ToArray();

            VerifyManualResults(casted);

            return casted;
        }

        /// <summary>
        /// Validates the results of a manual resolution.
        /// </summary>
        /// <param name="values">The values to validate.</param>
        /// <returns>If the validation completed successfully, true. Otherwise, false.</returns>
        protected virtual bool VerifyManualResults(TValue[] values)
        {
            if (values.Length == 0)
            {
                WriteInvalidOperation($"Failed to resolve any {IObjectExtensions.GetTypeDescription(typeof(TValue))} objects. Could not create a {typeof(TNode).Name}.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates nodes for all specified <paramref name="values"/> and emits them to the pipeline.
        /// </summary>
        /// <param name="values">The values to process.</param>
        /// <param name="children">The children that should be contained in each node.</param>
        protected void ProcessValues(TValue[] values, List<PrtgNode> children)
        {
            var nodes = values.Select(v => createNode(v, children));

            WriteObject(nodes, true);
        }

        /// <summary>
        /// Gets the bound parameters to use for the cmdlet that resolves the value to use for a node.
        /// </summary>
        /// <returns>The bound parameters to use.</returns>
        protected Dictionary<string, object> GetResolverParameters()
        {
            var parameters = MyInvocation.BoundParameters;

            object value;

            if (parameters.TryGetValue("Name", out value))
                parameters["Name"] = value.ToIEnumerable().Select(v => v?.ToString()).ToArray();

            return parameters;
        }
    }
}
