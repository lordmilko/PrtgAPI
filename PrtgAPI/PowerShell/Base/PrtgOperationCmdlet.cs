using System;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that perform actions or manipulate objects in PRTG.
    /// </summary>
    public abstract class PrtgOperationCmdlet : PrtgCmdlet
    {
        /// <summary>
        /// Executes an action and displays a progress message (if required).
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        /// <param name="activity">The title of the progress message to display.</param>
        /// <param name="progressMessage">The body of the progress message to display.</param>
        protected void ExecuteOperation(Action action, string activity, string progressMessage)
        {
            ProgressManager.TryOverwritePreviousOperation(activity, progressMessage);
            action();
            
            if (ProgressManager.PipeFromVariableWithProgress && ProgressManager.PipelineIsPure)
                ProgressManager.CompleteProgress();
        }

        /// <summary>
        /// Executes an action that returns a value and displays a progress message (if required).
        /// </summary>
        /// <typeparam name="T">The type of value to be returned by the action.</typeparam>
        /// <param name="action">The action to be performed.</param>
        /// <param name="activity">The title of the progress message to display.</param>
        /// <param name="progressMessage">The body of the progress message to display.</param>
        /// <returns></returns>
        protected T ExecuteOperation<T>(Func<T> action, string activity, string progressMessage)
        {
            ProgressManager.TryOverwritePreviousOperation(activity, progressMessage);
            var result = action();

            if (ProgressManager.PipeFromVariableWithProgress && ProgressManager.PipelineIsPure)
                ProgressManager.CompleteProgress();

            return result;
        }
    }
}
