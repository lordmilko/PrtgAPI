using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.Events;
using PrtgAPI.Helpers;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets requiring authenticated access to a PRTG Server.
    /// </summary>
    public abstract class PrtgCmdlet : PSCmdlet
    {
        /// <summary>
        /// Provides access to the <see cref="PrtgClient"/> stored in the current PowerShell Session State.
        /// </summary>
        protected PrtgClient client => PrtgSessionState.Client;

        internal ProgressManager ProgressManager;

        private EventManager eventManager = new EventManager();

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            if (PrtgSessionState.Client == null)
                throw new Exception("You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer.");
        }

        /// <summary>
        /// Performs record-by-record processing for the cmdlet. Do not override this method; override <see cref="ProcessRecordEx"/> instead. 
        /// </summary>
        protected override void ProcessRecord()
        {
            RegisterEvents();

            try
            {
                using (ProgressManager = new ProgressManager(this))
                {
                    ProcessRecordEx();
                }
            }
            catch
            {
                UnregisterEvents(false);
                throw;
            }
            finally
            {
                if (Stopping)
                {
                    UnregisterEvents(false);
                }
            }

            //If we're the last cmdlet in the pipeline, we need to unregister ourselves so that the upstream cmdlet
            //regains the ability to invoke its events when its control is returned to it
            if (EventManager.LogVerboseEventStack.Peek().Target == this)
            {
                UnregisterEvents(true);
            }
        }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected abstract void ProcessRecordEx();

        /// <summary>
        /// Performs one-time, post-processing functionality for the cmdlet. This function is only run when the cmdlet successfully runs to completion.
        /// </summary>
        protected override void EndProcessing()
        {
            UnregisterEvents(false);
        }

        #region Events

        private void RegisterEvents()
        {
            eventManager.AddEvent(OnRetryRequest, eventManager.RetryEventState, EventManager.RetryEventStack);
            eventManager.AddEvent(OnLogVerbose, eventManager.LogVerboseEventState, EventManager.LogVerboseEventStack);
        }

        private void UnregisterEvents(bool resetState)
        {
            eventManager.RemoveEvent(eventManager.RetryEventState, EventManager.RetryEventStack, resetState);
            eventManager.RemoveEvent(eventManager.LogVerboseEventState, EventManager.LogVerboseEventStack, resetState);
        }

        private void OnRetryRequest(object sender, RetryRequestEventArgs args)
        {
            var msg = args.Exception.Message.TrimEnd('.');

            WriteWarning($"'{MyInvocation.MyCommand}' timed out: {msg}. Retries remaining: {args.RetriesRemaining}");
        }

        private void OnLogVerbose(object sender, LogVerboseEventArgs args)
        {
            WriteVerbose($"{MyInvocation.MyCommand}: {args.Message}");
        }

        #endregion
    }
}
