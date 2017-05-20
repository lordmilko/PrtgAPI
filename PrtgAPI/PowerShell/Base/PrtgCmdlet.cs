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
        private static readonly EventStack<RetryRequestEventArgs> RetryEventStack = new EventStack<RetryRequestEventArgs>(
            () => PrtgSessionState.Client.retryRequest,
            e => PrtgSessionState.Client.retryRequest += e.Invoke,
            e => PrtgSessionState.Client.retryRequest -= e.Invoke
        );

        private static readonly EventStack<LogVerboseEventArgs> LogVerboseEventStack = new EventStack<LogVerboseEventArgs>(
            () => PrtgSessionState.Client.logVerbose,
            e => PrtgSessionState.Client.logVerbose += e.Invoke,
            e => PrtgSessionState.Client.logVerbose -= e.Invoke
        );

        protected static Stack<ProgressRecord> ProgressRecords = new Stack<ProgressRecord>();

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

        private EventState retryEventState = new EventState();
        private EventState logVerboseEventState = new EventState();

        private object lockEvents = new object();

        protected int? pipelineRecords;
        protected bool pipeToPrtgCmdlet;

        public int RunningPrtgCmdlets => LogVerboseEventStack.Count;

        private ProgressSettings progressSettings;

        private void AddEvent1(Action<object, RetryRequestEventArgs> item)
        {
            if (!retryEventState.EventAdded)
            {
                lock (lockEvents)
                {
                    RetryEventStack.Push(item);
                    retryEventState.EventAdded = true;
                }
            }
        }

        private void AddEvent<T>(Action<object, T> item, EventState state, EventStack<T> stack) where T : EventArgs
        {
            if (!state.EventAdded)
            {
                lock (lockEvents)
                {
                    stack.Push(item);
                    state.EventAdded = true;
                }
            }
        }

        private void RemoveEvent1()
        {
            if (!retryEventState.EventRemoved && retryEventState.EventAdded)
            {
                lock (lockEvents)
                {
                    RetryEventStack.Pop();
                    retryEventState.EventRemoved = true;
                }
            }
        }

        private void RemoveEvent<T>(EventState state, EventStack<T> stack, bool resetState) where T : EventArgs
        {
            if (!state.EventRemoved && state.EventAdded)
            {
                lock (lockEvents)
                {
                    stack.Pop();
                    state.EventRemoved = true;
                }
            }

            if (resetState)
            {
                state.EventAdded = false;
                state.EventRemoved = false;
            }
                
        }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected abstract void ProcessRecordEx();

        private void RegisterEvents()
        {
            AddEvent(OnRetryRequest, retryEventState, RetryEventStack);
            AddEvent(OnLogVerbose, logVerboseEventState, LogVerboseEventStack);
        }

        /// <summary>
        /// Performs record-by-record processing for the cmdlet. Do not override this method; override <see cref="ProcessRecordEx"/> instead. 
        /// </summary>
        protected override void ProcessRecord()
        {
            RegisterEvents();

            var pipeline = CommandRuntime.GetPipelineInput(this);

            if (pipeline?.Length > 0)
            {
                pipelineRecords = pipeline.Length;
            }

            pipeToPrtgCmdlet = MyInvocation.MyCommand.ModuleName == CommandRuntime.GetDownstreamCmdlet()?.ModuleName;

            try
            {
                ProcessRecordEx();
            }
            catch
            {
                CleanupState(false);
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
            if (LogVerboseEventStack.Peek().Target == this)
            {
                UnregisterEvents(true);
            }
        }

        /// <summary>
        /// Performs one-time, post-processing functionality for the cmdlet. This function is only run when the cmdlet successfully runs to completion.
        /// </summary>
        protected override void EndProcessing()
        {
            UnregisterEvents(false);
        }

        private void CleanupState(bool resetEventState)
        {
            UnregisterEvents(resetEventState);

            if (!resetEventState)
            {
                if(ProgressRecords.Count > 0)
                    ProgressRecords.Pop();
            }
        }

        private void UnregisterEvents(bool resetState)
        {
            RemoveEvent(retryEventState, RetryEventStack, resetState);
            RemoveEvent(logVerboseEventState, LogVerboseEventStack, resetState);
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

            //if we're getting piped to we need to show the initial progress of 0 items retrieved before we retrieve the items
            //also, as an optimization we should consider trying to extract the total number of items from our parent object?
            //no, that wont work - cos we might be doing additional filtering

            var recordsProcessed = -1;
            long sourceId = 0;
            if (settings != null)
            {
                recordsProcessed = 0;

                if (ProgressRecords.Count > 0 && RunningPrtgCmdlets > 1)
                {
                    var r = ProgressRecords.Peek();

                    r.CurrentOperation = null;

                    WriteProgressEx(r, CommandRuntime.GetLastProgressSourceId());
                }

                if (pipeToPrtgCmdlet && RunningPrtgCmdlets == 1)
                {
                    progress = ProgressRecords.Peek();
                }
                else
                {
                    progress = new ProgressRecord(RunningPrtgCmdlets, settings.ActivityName, settings.InitialDescription);

                    ProgressRecords.Push(progress);
                }

                progress.PercentComplete = 0;
                progress.StatusDescription = $"{settings.InitialDescription} {recordsProcessed}/{settings.TotalRecords}";


                //we shouldnt show this progress unless there is a cmdlet below us that will cause the user to benefit from this info
                //similarly, we shouldnt even have a settings variable set, i dont think. thats the root of the issue
                //what we do need to do is peek the last progress record if runningprtgcmdlets > 1 and remove the current operation

                //todo: make the current operation say the name of the object we're processing

                if (RunningPrtgCmdlets > 1)
                {
                    progress.ParentActivityId = RunningPrtgCmdlets - 1;
                    sourceId = CommandRuntime.GetLastProgressSourceId();
                }

                WriteProgressEx(progress, sourceId);
            }

            foreach (var item in sendToPipeline)
            {
                WriteObject(item);

                if (settings != null)
                {
                    recordsProcessed++;

                    progress.PercentComplete = (int)(recordsProcessed / Convert.ToDouble(settings.TotalRecords) * 100);
                    progress.StatusDescription = $"{settings.InitialDescription} ({recordsProcessed}/{settings.TotalRecords})";
                    WriteProgressEx(progress, sourceId);
                }

            }

            if (RunningPrtgCmdlets > 1) //i think this is actually unnecessary
            {
                sourceId = CommandRuntime.GetLastProgressSourceId();

                var record = ProgressRecords.Peek();

                record.CurrentOperation = null;

                //WriteProgressEx(record, sourceId);
            }

            if (settings != null)
            {
                ProgressRecords.Pop();

                progress.RecordType = ProgressRecordType.Completed;
                WriteProgressEx(progress, sourceId);
            }
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

        protected void WriteProgressEx(ProgressRecord progress, long sourceId)
        {
            if (sourceId == 0)
                WriteProgress(progress);
            else
                CommandRuntime.WriteProgress(sourceId, progress);
        }
    }
}
