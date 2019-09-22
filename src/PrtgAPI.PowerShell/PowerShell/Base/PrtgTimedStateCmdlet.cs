using System;
using System.Management.Automation;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that modify the state of an object for a specified duration.
    /// </summary>
    public abstract class PrtgTimedStateCmdlet : PrtgMultiOperationCmdlet
    {
        internal PrtgObject ObjectInternal { get; set; }

        internal int DurationInternal { get; set; }

        internal DateTime UntilInternal { get; set; }

        internal int[] IdInternal { get; set; }

        internal SwitchParameter ForeverInternal { get; set; }

        /// <summary>
        /// The action the cmdlet performs. e.g. "Acknowledging", "Pausing".
        /// </summary>
        private string cmdletAction;

        /// <summary>
        /// The type of object this cmdlet processes. e.g. "sensor" or "object".
        /// </summary>
        private string singleTypeDescription;

        /// <summary>
        /// Whether this cmdlet processes a single type of object.
        /// </summary>
        private bool isMultiTyped;

        internal int? duration;
        private string minutesDescription;
        private string durationDescription;
        private string whatIfDescription;

        internal override string ProgressActivity => $"{cmdletAction} PRTG {singleTypeDescription.ToSentenceCase()}s";

        internal abstract void Action(int[] ids);

        internal PrtgTimedStateCmdlet(string cmdletAction, string singleTypeDescription, bool isMultiTyped)
        {
            this.cmdletAction = cmdletAction;
            this.singleTypeDescription = singleTypeDescription;
            this.isMultiTyped = isMultiTyped;
        }

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessingEx()
        {
            var effectiveParameterSet = GetEffectiveParameterSet();

            switch (effectiveParameterSet)
            {
                case ParameterSet.Default:
                case ParameterSet.Manual:
                    duration = DurationInternal;
                    break;

                case ParameterSet.Until:
                case ParameterSet.UntilManual:
                    duration = (int)Math.Ceiling((UntilInternal - DateTime.Now).TotalMinutes);
                    break;

                case ParameterSet.Forever:
                case ParameterSet.ForeverManual:
                    break;

                default:
                    throw new UnknownParameterSetException(effectiveParameterSet);
            }

            if (duration < 1 && (effectiveParameterSet != ParameterSet.Forever && effectiveParameterSet != ParameterSet.ForeverManual))
                throw new ArgumentException("Duration evaluated to less than one minute. Please specify -Forever or a duration greater than or equal to one minute.");

            minutesDescription = "minute".Plural(duration ?? 0);
            durationDescription = ForeverInternal.IsPresent ? "forever" : $"for {duration} {minutesDescription}";
            whatIfDescription = ForeverInternal.IsPresent ? "forever" : $"{duration} {minutesDescription}";
        }

        private string GetEffectiveParameterSet()
        {
            //When we call a parameter set like -Until, since one parameter set (UntilSet) accepts pipeline input (which isn't
            //known in the Begin block) and the other (UntilManualSet) doesn't, we have know way of knowing which set we're in
            //until the Process block. Work around this by analyzing the bound parameters

            if (ParameterSetName != string.Empty)
                return ParameterSetName;

            if (MyInvocation.BoundParameters.ContainsKey("Duration"))
                return ParameterSet.Default;

            if (MyInvocation.BoundParameters.ContainsKey("Until"))
                return ParameterSet.Until;

            if (MyInvocation.BoundParameters.ContainsKey("Forever"))
                return ParameterSet.Forever;

            return ParameterSetName;
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess(GetShouldProcessMessage(ObjectInternal, IdInternal, $"Duration: {whatIfDescription}")))
                ExecuteOrQueue(ObjectInternal);
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformSingleOperation()
        {
            var message = GetSingleOperationProgressMessage(ObjectInternal, IdInternal, cmdletAction, TypeDescriptionOrDefault(ObjectInternal, singleTypeDescription), durationDescription);

            ExecuteOperation(() => Action(GetSingleOperationId(ObjectInternal, IdInternal)), message);
        }

        /// <summary>
        /// Invokes this cmdlet's action against all queued items from the pipeline.
        /// </summary>
        /// <param name="ids">The Object IDs of all queued items.</param>
        protected override void PerformMultiOperation(int[] ids)
        {
            var message = GetMultiOperationProgressMessage();

            ExecuteMultiOperation(() => Action(ids), message);
        }

        private string GetMultiOperationProgressMessage()
        {
            if (isMultiTyped)
                return $"{cmdletAction} {GetMultiTypeListSummary()} {durationDescription}";

            return $"{cmdletAction} {GetCommonObjectBaseType()} {GetListSummary()} {durationDescription}";
        }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => ObjectInternal;
    }
}
