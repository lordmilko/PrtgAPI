using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Events;

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
    }
}
