using System;
using System.Reflection;

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
            
            if (ProgressManager.PipeFromVariableWithProgress && ProgressManager.PipelineIsProgressPure)
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

            if (ProgressManager.PipeFromVariableWithProgress && ProgressManager.PipelineIsProgressPure)
                ProgressManager.CompleteProgress();

            return result;
        }

        protected object ParseValueIfRequired(PropertyInfo property, object value)
        {
            //Types that can have possible enum values (such as TriggerChannel) possess a static Parse method for type conversion by the PowerShell runtime.
            //Only parse types that are defined in the PrtgAPI assembly.
            if (property.PropertyType.Assembly.FullName == GetType().Assembly.FullName && !property.PropertyType.IsEnum)
            {
                var method = property.PropertyType.GetMethod("Parse", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);

                if (method != null)
                {
                    try
                    {
                        var newValue = method.Invoke(null, new[] { value });
                        value = newValue;
                    }
                    catch (Exception ex)
                    {
                        //Don't care if our value wasn't parsable
                    }
                }
            }

            return value;
        }
    }
}
