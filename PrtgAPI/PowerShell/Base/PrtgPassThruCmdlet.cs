using System;
using System.Collections;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for generic operation cmdlets capable of passing their inputs out to the pipeline.
    /// </summary>
    public abstract class PrtgPassThruCmdlet : PrtgOperationCmdlet, IPrtgPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">Returns the original <see cref="PrtgObject"/> that was passed to this cmdlet, allowing the object to be further piped into additional cmdlets.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Executes an action and displays a progress message (if required).
        /// </summary>
        /// <param name="obj">The object to process.</param>
        /// <param name="action">The action to be performed.</param>
        /// <param name="activity">The title of the progress message to display.</param>
        /// <param name="progressMessage">The body of the progress message to display.</param>
        /// <param name="complete">Whether to allow <see cref="PrtgOperationCmdlet"/> to dynamically determine whether progress should be completed</param>
        protected void ExecuteOperation(object obj, Action action, string activity, string progressMessage, bool complete = true)
        {
            base.ExecuteOperation(action, activity, progressMessage, complete);
            PassThruObject(obj);
        }

        /// <summary>
        /// Writes the specified object to the pipeline if <see cref="PassThru"/> is specified.
        /// </summary>
        /// <param name="obj">The object to write to the pipeline.</param>
        public void PassThruObject(object obj)
        {
            if (PassThru)
            {
                if (obj is IEnumerable)
                {
                    foreach (var o in (IEnumerable)obj)
                        WriteObject(o);
                }
                else
                    WriteObject(obj);
            }
        }
    }
}
