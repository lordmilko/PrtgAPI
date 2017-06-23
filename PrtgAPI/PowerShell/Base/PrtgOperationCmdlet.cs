using System;

namespace PrtgAPI.PowerShell.Base
{
    public abstract class PrtgOperationCmdlet : PrtgCmdlet
    {
        protected void ExecuteOperation(Action action, string activity, string progressMessage)
        {
            ProgressManager.TryOverwritePreviousOperation(activity, progressMessage);
            action();
        }

        protected T ExecuteOperation<T>(Func<T> action, string activity, string progressMessage)
        {
            ProgressManager.TryOverwritePreviousOperation(activity, progressMessage);
            return action();
        }
    }
}
