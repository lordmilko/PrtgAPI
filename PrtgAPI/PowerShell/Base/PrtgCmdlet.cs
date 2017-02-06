using System;
using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Events;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets requiring authenticated access to a PRTG Server.
    /// </summary>
    public abstract class PrtgCmdlet : PSCmdlet
    {
        private static EventStack<RetryRequestEventArgs> retryEventStack = new EventStack<RetryRequestEventArgs>(
            () => PrtgSessionState.Client.retryRequest,
            e => PrtgSessionState.Client.retryRequest += e.Invoke,
            e => PrtgSessionState.Client.retryRequest -= e.Invoke
        );

        /// <summary>
        /// Provides access to the <see cref="PrtgClient"/> stored in the current PowerShell Session State.
        /// </summary>
        protected PrtgClient client => PrtgSessionState.Client;

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            if (PrtgSessionState.Client == null)
                throw new Exception("You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer.");
        }

        private bool retryEventAdded;
        private bool retryEventRemoved;

        private object lockEvents = new object();

        private void AddEvent(Action<object, RetryRequestEventArgs> item)
        {
            if (!retryEventAdded)
            {
                lock (lockEvents)
                {
                    retryEventStack.Push(item);
                    retryEventAdded = true;
                }
            }
        }

        private void RemoveEvent()
        {
            if (!retryEventRemoved)
            {
                lock (lockEvents)
                {
                    retryEventStack.Pop();
                    retryEventRemoved = true;
                }
            }
        }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected abstract void ProcessRecordEx();

        /// <summary>
        /// Performs record-by-record processing for the cmdlet. Do not override this method; override <see cref="ProcessRecordEx"/> instead. 
        /// </summary>
        protected override void ProcessRecord()
        {
            AddEvent(OnRetryRequest);

            try
            {
                ProcessRecordEx();
            }
            catch
            {
                RemoveEvent();
                throw;
            }
            finally
            {
                if (Stopping)
                {
                    RemoveEvent();
                }
            }
        }

        /// <summary>
        /// Performs one-time, post-processing functionality for the cmdlet. This function is only run when the cmdlet successfully runs to completion.
        /// </summary>
        protected override void EndProcessing()
        {
            RemoveEvent();
        }


        /// <summary>
        /// Writes a list to the output pipeline.
        /// </summary>
        /// <typeparam name="T">The type of the list that will be output.</typeparam>
        /// <param name="sendToPipeline">The list that will be output to the pipeline.</param>
        /// <param name="settings">How progress should be displayed while the list is being enumerated.</param>
        internal void WriteList<T>(IEnumerable<T> sendToPipeline, ProgressSettings settings)
        {
            ProgressRecord progress = null;

            var recordsProcessed = -1;

            if (settings != null)
            {
                recordsProcessed = 0;

                progress = new ProgressRecord(1, settings.ActivityName, settings.InitialDescription)
                {
                    PercentComplete = 0,
                    StatusDescription = $"{settings.InitialDescription} {recordsProcessed}/{settings.TotalRecords}"
                };

                WriteProgress(progress);
            }

            foreach (var item in sendToPipeline)
            {
                WriteObject(item);

                if (settings != null)
                {
                    recordsProcessed++;

                    progress.PercentComplete = (int)(recordsProcessed / Convert.ToDouble(settings.TotalRecords) * 100);
                    progress.StatusDescription = $"{settings.InitialDescription} ({recordsProcessed}/{settings.TotalRecords})";
                    WriteProgress(progress);
                }
            }
        }

        private void OnRetryRequest(object sender, RetryRequestEventArgs args)
        {
            var msg = args.Exception.Message.TrimEnd('.');

            WriteWarning($"'{MyInvocation.MyCommand}' timed out: {args.Exception.Message}. Retries remaining: {args.RetriesRemaining}");
        }
    }
}
