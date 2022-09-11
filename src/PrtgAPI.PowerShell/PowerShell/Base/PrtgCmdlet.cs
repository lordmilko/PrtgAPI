using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using PrtgAPI.PowerShell.Progress;
using PrtgAPI.Reflection;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets requiring authenticated access to a PRTG Server.
    /// </summary>
    public abstract class PrtgCmdlet : PSCmdlet, IDisposable
    {
        /// <summary>
        /// Provides access to the <see cref="PrtgClient"/> stored in the current PowerShell Session State.
        /// </summary>
        protected PrtgClient client => PrtgSessionState.Client;

        private bool noClient;

        internal ProgressManager ProgressManager;

        internal ProgressManagerEx ProgressManagerEx = new ProgressManagerEx();

        private EventManager eventManager = new EventManager();

        private bool disposed;

        /// <summary>
        /// A cancellation token source to use with long running tasks that may need to be interrupted by Ctrl+C.
        /// </summary>
        private readonly CancellationTokenSource TokenSource = new CancellationTokenSource();

        internal CancellationToken CancellationToken => TokenSource.Token;

        internal bool HasParameter(string name) => MyInvocation.BoundParameters.ContainsKey(name);

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            if (PrtgSessionState.Client == null)
                throw new InvalidOperationException("You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer.");

            BeginProcessingEx();
        }

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected virtual void BeginProcessingEx()
        {
        }

        /// <summary>
        /// Performs record-by-record processing for the cmdlet. Do not override this method; override <see cref="ProcessRecordEx"/> instead. 
        /// </summary>
        protected override void ProcessRecord()
        {
            ExecuteWithCoreState(ProcessRecordEx);
        }

        internal void ExecuteWithCoreState(Action action)
        {
            RegisterEvents();

            try
            {
                if (client != null)
                    client.DefaultCancellationToken = TokenSource.Token;

                using (ProgressManager = new ProgressManager(this))
                {
                    try
                    {
                        action();
                    }
                    catch (NonTerminatingException ex)
                    {
                        ProgressManager.CompleteUncompleted(true);
                        WriteInvalidOperation(ex.InnerException, ex.TargetObject, ex.ErrorCategory);
                    }
                    catch (PrtgRequestException ex)
                    {
                        ProgressManager.CompleteUncompleted(true);
                        WriteInvalidOperation(ex);
                    }
                    catch (Exception ex)
                    {
                        if (!(PipeToSelectObject() && ex is PipelineStoppedException))
                            ProgressManager.TryCompleteProgress();

                        ProgressManager.CompleteUncompleted(true);

                        throw;
                    }
                }
            }
            catch(Exception)
            {
                UnregisterEvents(false);
                throw;
            }
            finally
            {
                if (client != null)
                    client.DefaultCancellationToken = CancellationToken.None;

                if (Stopping)
                {
                    UnregisterEvents(false);
                }
            }

            //If we're the last cmdlet in the pipeline, we need to unregister ourselves so that the upstream cmdlet
            //regains the ability to invoke its events when its control is returned to it
            if (!noClient && !Stopping && EventManager.LogVerboseEventStack.Peek().Target == this)
            {
                UnregisterEvents(true);
            }
        }

        internal void WriteInvalidOperation(Exception ex, object targetObject = null, ErrorCategory errorCategory = ErrorCategory.InvalidOperation)
        {
            WriteError(new ErrorRecord(
                ex,
                ex.GetType().Name,
                errorCategory,
                targetObject
            ));
        }

        internal void WriteInvalidOperation(string message, object targetObject = null)
        {
            WriteInvalidOperation(new InvalidOperationException(message), targetObject);
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected abstract void ProcessRecordEx();

        /// <summary>
        /// Performs one-time, post-processing functionality for the cmdlet. This function is only run when the cmdlet successfully runs to completion.
        /// </summary>
        protected override void EndProcessing()
        {
            EndProcessing();
        }

        internal void EndProcessing(bool endExtended = true)
        {
            if (endExtended)
                EndProcessingEx();

            ProgressManager?.CompleteUncompleted();

            UnregisterEvents(false);
        }

        /// <summary>
        /// Provides an enhanced one-time, postprocessing functionality for the cmdlet.
        /// </summary>
        protected virtual void EndProcessingEx()
        {
        }

        /// <summary>
        /// Interrupts the currently running code to signal the cmdlet has been requested to stop.<para/>
        /// Do not override this method; override <see cref="StopProcessingEx"/> instead.
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected override void StopProcessing()
        {
            StopProcessingEx();

            TokenSource.Cancel();
        }

        /// <summary>
        /// Interrupts the currently running code to signal the cmdlet has been requested to stop.
        /// </summary>
        protected virtual void StopProcessingEx()
        {
        }

        /// <summary>
        /// Disposes of all managed and unmanaged resources used by the cmdlet.
        /// </summary>
        public void Dispose()
        {
            if (disposed == false)
            {
                TokenSource.Dispose();

                disposed = true;
            }
        }

        private bool PipeToSelectObject()
        {
            var commands = ProgressManager.CacheManager.GetPipelineCommands();

            var myIndex = commands.IndexOf(this);

            return commands.Skip(myIndex + 1).Where(SelectObjectDescriptor.IsSelectObjectCommand).Any(c => new SelectObjectDescriptor((PSCmdlet) c).HasFilters);
        }

        internal void Sleep(int milliseconds)
        {
            TokenSource.Token.WaitHandle.WaitOne(milliseconds);
        }

        #region Events

        private void RegisterEvents()
        {
            //Cmdlets that optionally depend on a PrtgClient (such as New-SensorParameters) might not have a Client to use for events
            if (PrtgSessionState.Client != null)
            {
                eventManager.AddEvent(OnRetryRequest, eventManager.RetryEventState, EventManager.RetryEventStack);
                eventManager.AddEvent(OnLogVerbose, eventManager.LogVerboseEventState, EventManager.LogVerboseEventStack);
            }
            else
                noClient = true;
        }

        private void UnregisterEvents(bool resetState)
        {
            eventManager.RemoveEvent(eventManager.RetryEventState, EventManager.RetryEventStack, resetState);
            eventManager.RemoveEvent(eventManager.LogVerboseEventState, EventManager.LogVerboseEventStack, resetState);
        }

        [ExcludeFromCodeCoverage]
        private void OnRetryRequest(object sender, RetryRequestEventArgs args)
        {
            var msg = args.Exception.Message.TrimEnd('.');

            WriteWarning($"'{MyInvocation.MyCommand}' timed out: {msg}. Retries remaining: {args.RetriesRemaining}");
        }

        private void OnLogVerbose(object sender, LogVerboseEventArgs args)
        {
            //Lazy values will execute in the context of the previous command when retrieved from the next cmdlet
            //(such as Select-Object)
            if (CommandRuntime.GetInternalProperty("PipelineProcessor").GetInternalField("_permittedToWrite") == this || CommandRuntime is DummyRuntime)
                WriteVerbose($"{MyInvocation.MyCommand}: {args.Message}");

            Debug.WriteLine($"{MyInvocation.MyCommand}: {args.Message}");
        }

        #endregion
    }
}
