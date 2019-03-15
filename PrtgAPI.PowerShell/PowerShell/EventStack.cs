using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PrtgAPI.PowerShell
{
    class EventStack<TEventArgs> where TEventArgs : EventArgs
    {
        Stack<Action<object, TEventArgs>> stack = new Stack<Action<object, TEventArgs>>();

        private Func<EventHandler<TEventArgs>> getHandler;
        private Action<Action<object, TEventArgs>> addEvent;
        private Action<Action<object, TEventArgs>> removeEvent;

        public EventStack(Func<EventHandler<TEventArgs>> getHandler, Action<Action<object, TEventArgs>> addEvent, Action<Action<object, TEventArgs>> removeEvent)
        {
            this.getHandler = getHandler;
            this.addEvent = addEvent;
            this.removeEvent = removeEvent;
        }

        public void Push(Action<object, TEventArgs> item)
        {
            var events = getHandler()?.GetInvocationList().Cast<EventHandler<TEventArgs>>().ToList();

            if (events != null)
            {
                foreach (var e in events)
                {
                    removeEvent(((Action<object, TEventArgs>) e.Target));

                    var events2 = getHandler()?.GetInvocationList().Cast<EventHandler<TEventArgs>>().ToList();

                    Debug.Assert(events2 == null, "Events still existed but they shouldn't have");
                }
            }

            stack.Push(item);
            addEvent(stack.Peek());

            var events3 = getHandler()?.GetInvocationList().Cast<EventHandler<TEventArgs>>().ToList();

            Debug.Assert(events3?.Count == 1);
        }

        public int Count => stack.Count;

        public void Pop()
        {
            removeEvent(stack.Pop());

            if (stack.Any())
                addEvent(stack.Peek());
        }

        public Action<object, TEventArgs> Peek()
        {
            return stack.Peek();
        }
    }
}