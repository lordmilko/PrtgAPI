using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that perform actions or manipulate multiple objects in a single request against PRTG.
    /// </summary>
    public abstract class PrtgMultiOperationCmdlet : PrtgPostProcessCmdlet, IPrtgMultiPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">Specifies whether this cmdlet should queue all objects piped to this cmdlet to execute a single
        /// request against PRTG for processing all objects. By default this value is true.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Batch
        {
            get { return batch ?? false; }
            set { batch = value; }
        }

        /// <summary>
        /// <para type="description">Specifies whether to return the original <see cref="PrtgObject"/> that was passed to this cmdlet, allowing the object to be further piped into additional cmdlets.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        internal bool? batch = SwitchParameter.Present;

        internal List<IObject> objects = new List<IObject>();

        private void QueueObject(IObject obj)
        {
            objects.Add(obj);
        }

        /// <summary>
        /// If <see cref="Batch"/> is true, queues the object for processing after all items have been identified. Otherwise, executes this cmdlet's action immediately. 
        /// </summary>
        /// <param name="obj">The object to process.</param>
        protected void ExecuteOrQueue(IObject obj)
        {
            if (obj != null)
                ExecuteOrQueue(obj, $"Queuing {obj.GetTypeDescription().ToLower()} '{obj.Name}'" + (obj is PrtgObject ? $" (ID: {obj.GetId()})" : ""));
            else
                ExecuteOrQueue(obj, null);
        }

        /// <summary>
        /// If <see cref="Batch"/> is true, queues the object for processing after all items have been identified. Otherwise, executes this cmdlet's action immediately. 
        /// </summary>
        /// <param name="obj">The object to process.</param>
        /// <param name="progressMessage">The progress message to display.</param>
        protected internal void ExecuteOrQueue(IObject obj, string progressMessage)
        {
            if (Batch.IsPresent && obj != null)
                ExecuteQueueOperation(obj, progressMessage);
            else
                PerformSingleOperation();
        }

        /// <summary>
        /// Queues an object for execution after all objects have been identified.
        /// </summary>
        /// <param name="obj">The object to queue.</param>
        /// <param name="progressMessage">The progress message to display.</param>
        private void ExecuteQueueOperation(IObject obj, string progressMessage)
        {
            ExecuteOperation(() => QueueObject(obj), progressMessage);
        }
        
        /// <summary>
        /// Executes this cmdlet's action on all queued objects.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="progressMessage">The progress message to display.</param>
        /// <param name="complete">Whether progress should be completed after calling this method.<para/>
        /// If the queued objects are to be split over several method calls, this value should only be true on the final call.</param>
        protected internal void ExecuteMultiOperation(Action action, string progressMessage, bool complete = true)
        {
            progressMessage += $" ({objects.Count}/{objects.Count})";

            DisplayMultiOperationProgress(progressMessage);

            action();

            ProgressManager.RecordsProcessed = -1;

            WriteMultiPassThru();

            CompletePostProcessProgress(complete);
        }

        private void DisplayMultiOperationProgress(string progressMessage)
        {
            if (ProgressManager.ProgressEnabled && !ProgressManagerEx.PipeFromSingleVariable)
                ProgressManager.ProcessPostProcessProgress(ProgressActivity, progressMessage);
        }

        internal string GetMultiTypeListSummary()
        {
            var toProcess = objects;
            var remaining = 0;

            if (objects.Count > 10)
            {
                var amount = 10;

                if (objects.Count == 11)
                    amount = 9;

                toProcess = objects.Take(amount).ToList();
                remaining = objects.Count - amount;
            }

            var grouped = toProcess.GroupBy(IObjectExtensions.GetTypeDescription).ToList();

            var builder = new StringBuilder();

            for (int i = 0; i < grouped.Count; i++)
            {
                //Add the BaseType (e.g. sensor(s))
                builder.Append(grouped[i].Key.ToLower());

                if (grouped[i].Count() > 1)
                    builder.Append("s");

                builder.Append(" ");

                var groupItems = grouped[i].ToList();

                //Add each object name (e.g. 'Ping', 'DNS' and 'Uptime'
                for (int j = 0; j < groupItems.Count(); j++)
                {
                    builder.Append($"'{groupItems[j].Name}'");

                    if (j < groupItems.Count - 2)
                        builder.Append(", ");
                    else if (j == groupItems.Count - 2)
                    {
                        //If there's more than 1 group and we're not the last group, OR if there are remaining items, dont say "and"

                        if ((grouped.Count > 1 && i == grouped.Count - 1 || grouped.Count == 1) && remaining == 0)
                            builder.Append(" and ");
                        else
                            builder.Append(", ");
                    }
                        
                }

                //Add a delimiter for the next group based on whether we'll also have to
                //report the number of objects we didn't name afterwards
                if (i < grouped.Count - 2)
                    builder.Append(", ");
                else if (i == grouped.Count - 2)
                    builder.Append(remaining > 0 ? ", " : " and ");
            }

            if (remaining > 0)
                builder.Append($" and {remaining} others");

            return builder.ToString();
        }
        
        internal string GetListSummary(List<IObject> list = null, Func<IObject, string> descriptor = null)
        {
            if (list == null)
                list = objects;

            if (descriptor == null)
                descriptor = o => $"'{o.Name}'";

            return GetListSummary<IObject>(list, descriptor);
        }

        internal string GetListSummary<T>(List<T> list, Func<T, string> descriptor)
        {
            var toProcess = list;
            var remaining = 0;

            if (list.Count > 10)
            {
                var amount = 10;

                if (list.Count == 11)
                    amount = 9;

                toProcess = list.Take(amount).ToList();
                remaining = list.Count - amount;
            }

            var builder = new StringBuilder();

            for (int i = 0; i < toProcess.Count; i++)
            {
                builder.Append(descriptor(toProcess[i]));

                if (i < toProcess.Count - 2)
                    builder.Append(", ");
                else if (i == toProcess.Count - 2)
                {
                    builder.Append(remaining > 0 ? ", " : " and ");
                }
            }

            if (remaining > 0)
                builder.Append($" and {remaining} others");

            return builder.ToString();
        }

        internal string GetCommonObjectBaseType()
        {
            var baseType = objects.First().GetTypeDescription().ToLower();

            return baseType.Plural(objects.Count);
        }

        /// <summary>
        /// Provides an enhanced one-time, postprocessing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessingEx()
        {
            if (objects.Count > 0 && Batch.IsPresent)
            {
                var ids = objects.Select(o => o.GetId()).ToArray();

                if (ProgressManager.OperationSeemsLikePipeFromVariable && ProgressManager.PreviousRecord == null && ProgressManager.EntirePipeline != null && ProgressManager.CacheManager.GetPreviousPrtgCmdlet() == null)
                {
                    ProgressManager.TotalRecords = ProgressManager.EntirePipeline.List.Count;
                }
                else
                    ProgressManager.TotalRecords = objects.Count;

                PerformMultiOperation(ids);
            }
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected abstract void PerformSingleOperation();

        /// <summary>
        /// Invokes this cmdlet's action against all queued items from the pipeline.
        /// </summary>
        /// <param name="ids">The Object IDs of all queued items.</param>
        protected abstract void PerformMultiOperation(int[] ids);

        /// <summary>
        /// Writes the current <see cref="PassThruObject"/> to the pipeline if <see cref="PassThru"/> is specified.
        /// </summary>
        public void WritePassThru()
        {
            if (PassThru)
            {
                WriteObject(PassThruObject);
            }
        }

        /// <summary>
        /// The objects that should be output from the cmdlet.
        /// </summary>
        public List<object> PassThruObjects => objects.Cast<object>().ToList();

        /// <summary>
        /// Stores the last object that was output from the cmdlet.
        /// </summary>
        public object CurrentMultiPassThru { get; set; }

        /// <summary>
        /// Writes all objects stored in <see cref="PassThruObjects"/> if <see cref="IPrtgPassThruCmdlet.PassThru"/> is specified.
        /// </summary>
        public void WriteMultiPassThru()
        {
            if (PassThru)
            {
                foreach (var o in PassThruObjects)
                {
                    CurrentMultiPassThru = o;
                    WriteObject(o);
                }
            }
        }

        /// <summary>
        /// Whether this cmdlet will execute its post processing operation.
        /// </summary>
        protected override bool ShouldPostProcess() => Batch.IsPresent;

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public abstract object PassThruObject { get; }
    }
}
