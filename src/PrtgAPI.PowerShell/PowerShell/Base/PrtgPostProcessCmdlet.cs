using System;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that perform operations after all pipeline elements have been processed.
    /// </summary>
    public abstract class PrtgPostProcessCmdlet : PrtgOperationCmdlet
    {
        /// <summary>
        /// Whether this cmdlet will execute its post processing operation.
        /// </summary>
        public bool PostProcess => ShouldPostProcess();

        /// <summary>
        /// Executes an action and displays a progress message (if required).
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        /// <param name="progressMessage">The body of the progress message to display.</param>
        /// <param name="complete">Whether to allow <see cref="PrtgOperationCmdlet"/> to dynamically determine whether progress should be completed</param>
        /// <param name="incrementRecord">Whether to increment the progress record of this cmdlet. If this value is false, the value will be incremented then decremented.</param>
        protected internal override void ExecuteOperation(Action action, string progressMessage, bool complete = true, bool incrementRecord = true)
        {
            base.ExecuteOperation(action, progressMessage, complete, incrementRecord);

            if (PostProcess)
            {
                if (ProgressManagerEx.CachedRecord == null)
                {
                    if (ProgressManager.ProgressWritten)
                        ProgressManagerEx.CachedRecord = ProgressManager.CurrentRecord;
                    else if (ProgressManager.PreviousRecord != null)
                        ProgressManagerEx.CachedRecord = ProgressManager.PreviousRecord;
                }
            }
        }

        /// <summary>
        /// Display a progress message during an <see cref="EndProcessing"/> block.
        /// </summary>
        /// <param name="activity">The title of the progress message to display.</param>
        /// <param name="progressMessage">The body of the progress message to display.</param>
        /// <param name="completedPercent">The percentage this process has been completed.</param>
        /// <param name="secondsRemaining">The number of seconds remaining in this operation. If this value is null, seconds remaining will not be displayed.</param>
        /// <param name="currentOperation">The current operation that is being performed. If this value is null, the current operation will not be displayed.</param>
        /// <param name="complete">Whether or not the progress should be completed after displaying.</param>
        protected void DisplayPostProcessProgress(string activity, string progressMessage, int completedPercent, int? secondsRemaining, string currentOperation, bool complete = false)
        {
            if (ProgressManager.ProgressEnabled)
                ProgressManager.ProcessPostProcessProgress(activity, progressMessage, completedPercent, secondsRemaining, currentOperation);

            CompletePostProcessProgress(complete);
        }

        /// <summary>
        /// Completes a progress record created in the <see cref="EndProcessing"/> block.
        /// </summary>
        /// <param name="complete"></param>
        protected void CompletePostProcessProgress(bool complete = true)
        {
            //If we're a PassThru cmdlet and the next cmdlet is a multi operation cmdlet, we should leave our progress
            //open so the next cmdlet can complete it
            var nextCmdlet = ProgressManager.CacheManager.GetNextPrtgCmdlet() as PrtgPostProcessCmdlet;

            if (nextCmdlet?.ProgressManager?.PostProcessMode() == true)
                return;

            if (ProgressManager.ProgressEnabled && complete && !ProgressManagerEx.PipeFromSingleVariable)
                ProgressManager.CompleteProgress(true, true);
        }

        /// <summary>
        /// Implements the cmdlet specific algorithm for <see cref="PostProcess"/>.
        /// </summary>
        /// <returns>If this cmdlet should execute its post processing operation, true. Otherwise, false.</returns>
        protected abstract bool ShouldPostProcess();

        /// <summary>
        /// Provides a one-time, postprocessing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
            if (PostProcess)
                ExecuteWithCoreState(EndProcessingEx);

            EndProcessing(false);
        }
    }
}
