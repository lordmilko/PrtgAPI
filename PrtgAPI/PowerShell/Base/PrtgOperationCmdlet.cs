using System;

namespace PrtgAPI.PowerShell.Base
{
    public abstract class PrtgOperationCmdlet : PrtgCmdlet
    {
        protected void ExecuteOperation(Action action, string activity, string progressMessage)
        {
            ProgressManager.TryOverwritePreviousOperation(activity, progressMessage);
            action();

            if (ProgressManager.PipeFromVariable && ProgressManager.FirstRecord)
                ProgressManager.CompleteProgress();
        }

        protected T ExecuteOperation<T>(Func<T> action, string activity, string progressMessage)
        {
            ProgressManager.TryOverwritePreviousOperation(activity, progressMessage);
            var result = action();

            if (ProgressManager.PipeFromVariable && ProgressManager.FirstRecord)
                ProgressManager.CompleteProgress();

            return result;
        }
    }
}
