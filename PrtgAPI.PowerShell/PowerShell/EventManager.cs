using System;

namespace PrtgAPI.PowerShell
{
    class EventManager
    {
        internal static readonly EventStack<RetryRequestEventArgs> RetryEventStack = new EventStack<RetryRequestEventArgs>(
            () => PrtgSessionState.Client.retryRequest,
            e => PrtgSessionState.Client.retryRequest += e.Invoke,
            e => PrtgSessionState.Client.retryRequest -= e.Invoke
        );

        internal static readonly EventStack<LogVerboseEventArgs> LogVerboseEventStack = new EventStack<LogVerboseEventArgs>(
            () => PrtgSessionState.Client.logVerbose,
            e => PrtgSessionState.Client.logVerbose += e.Invoke,
            e => PrtgSessionState.Client.logVerbose -= e.Invoke
        );

        internal EventState RetryEventState = new EventState();
        internal EventState LogVerboseEventState = new EventState();

        private readonly object lockEvents = new object();

        internal void AddEvent<T>(Action<object, T> item, EventState state, EventStack<T> stack) where T : EventArgs
        {
            //If the event hasn't been added, add it and mark it as being added
            if (!state.EventAdded)
            {
                lock (lockEvents)
                {
                    stack.Push(item);
                    state.EventAdded = true;
                }
            }
        }

        internal void RemoveEvent<T>(EventState state, EventStack<T> stack, bool resetState) where T : EventArgs
        {
            //If the event hasn't been removed yet and was actually added, remove it
            if (!state.EventRemoved && state.EventAdded)
            {
                //Remove the event from the stack, and mark it as removed
                lock (lockEvents)
                {
                    stack.Pop();
                    state.EventRemoved = true;
                }
            }

            //The event was never added or removed in the first place. This allows us to re-add/remove
            //events again
            if (resetState)
            {
                state.EventAdded = false;
                state.EventRemoved = false;
            }
        }
    }
}
