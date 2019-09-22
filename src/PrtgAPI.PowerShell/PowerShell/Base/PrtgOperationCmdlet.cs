using System;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

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
        /// <param name="progressMessage">The body of the progress message to display.</param>
        /// <param name="complete">Whether to allow <see cref="PrtgOperationCmdlet"/> to dynamically determine whether progress should be completed</param>
        /// <param name="incrementRecord">Whether to increment the progress record of this cmdlet. If this value is false, the value will be incremented then decremented.</param>
        protected internal virtual void ExecuteOperation(Action action, string progressMessage, bool complete = true, bool incrementRecord = true)
        {
            ProgressManager.ProcessOperationProgress(ProgressActivity, progressMessage, incrementRecord);

            try
            {
                action();
            }
            catch (PipelineStoppedException)
            {
                //Invoked when Select was the last cmdlet in the pipeline
                ProgressManager.TryCompleteProgress();

                throw;
            }

            var passThru = this as IPrtgPassThruCmdlet;

            if (passThru != null && !ProgressManager.PostProcessMode())
                passThru.WritePassThru();

            if (complete)
                CompleteOperationProgress();
        }

        /// <summary>
        /// Parse a value into its expected type. Requires the target <paramref name="property"/> contain a Parse method.
        /// </summary>
        /// <param name="property">The property the value applies to.</param>
        /// <param name="value">The value to apply to the property.</param>
        /// <returns>If the target property type contains a Parse method which did not throw upon being called, the parsed value. Otherwise, the original value.</returns>
        protected object ParseValueIfRequired(PropertyInfo property, object value)
        {
            value = PSObjectUtilities.CleanPSObject(value);

            //Types that can have possible enum values (such as TriggerChannel) possess a static Parse method for type conversion by the PowerShell runtime.
            //Only parse types that are defined in the PrtgAPI assembly.
            if (ReflectionExtensions.IsPrtgAPIProperty(GetType(), property) && !property.PropertyType.IsEnum)
            {
                var method = property.PropertyType.GetMethod("Parse", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);

                if (method != null)
                {
                    try
                    {
                        var newValue = method.Invoke(null, new[] { value });
                        value = newValue;
                    }
                    catch (Exception)
                    {
                        //Don't care if our value wasn't parsable
                    }
                }
            }
            else
            {
                //Try and parse the value if the property type is an Enum or Nullable Enum
                var type = property.PropertyType;

                if (!type.IsEnum)
                    type = Nullable.GetUnderlyingType(property.PropertyType);

                if (type?.IsEnum == true)
                {
                    if (Enum.GetNames(type).Any(e => e.ToLower() == value?.ToString().ToLower()))
                        return Enum.Parse(type, value.ToString(), true);
                }
            }

            return value;
        }

        private void CompleteOperationProgress()
        {
            if (ProgressManager.ProgressEnabled)
            {
                if (ProgressManager.PipeFromVariableWithProgress && ProgressManager.PipelineIsProgressPure)
                    ProgressManager.CompleteProgress();
                else
                {
                    if (ProgressManager.PreviousCmdletIsSelectObject)
                        ProgressManager.MaybeCompletePreviousProgress();
                    else
                    {
                        if (ProgressManager.PipelineUpstreamContainsBlockingCmdlet || ProgressManager.PostProcessMode())
                            ProgressManager.CompleteProgress();
                    }
                }
            }
        }

        internal string GetShouldProcessMessage(PrtgObject obj, int[] ids, string details = null)
        {
            if (obj == null && ids.Length == 0)
                throw new ArgumentException($"{nameof(ids)} must be specified when {nameof(obj)} is null.");

            if (obj != null)
            {
                var str = $"'{obj.Name}' (ID: {obj.Id}";

                if (details != null)
                    str += $", {details})";
                else
                    str += ")";

                return str;
            }

            string idStr;

            if (ids.Length == 1)
                idStr = $"ID {ids[0]}";
            else
                idStr = $"{("ID".Plural(ids))} {(string.Join(", ", ids))}";

            if (details != null)
                idStr += $" ({details})";

            return idStr;
        }

        internal string GetSingleOperationProgressMessage(PrtgObject obj, int[] ids, string action, string typeDescription, string operationDescription = null)
        {
            if (obj == null && ids.Length == 0)
                throw new ArgumentException($"{nameof(ids)} must be specified when {nameof(obj)} is null.");

            string str;

            //e.g. Acknowledging sensor 'Ping' forever
            if (obj != null)
                str = $"{action} {typeDescription} '{obj.Name}' (ID: {obj.Id})";
            else
                str = $"{action} {typeDescription} {("ID".Plural(ids))} {(string.Join(", ", ids))}";

            if (operationDescription != null)
                str += $" {operationDescription}";

            return str;
        }

        internal string TypeDescriptionOrDefault(PrtgObject obj, string @default = "object")
        {
            if (obj == null)
                return @default;

            return obj.GetTypeDescription().ToLower();
        }

        internal int[] GetSingleOperationId(PrtgObject obj, int[] id)
        {
            if (obj != null)
                return new[] { obj.Id };

            return id;
        }

        internal abstract string ProgressActivity { get; }
    }
}
