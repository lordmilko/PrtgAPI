using System;

namespace PrtgAPI.Tree.Progress
{
    /// <summary>
    /// Manages the depth at which progress is currently being processed.
    /// </summary>
    public class DepthManager : IDisposable
    {
        /// <summary>
        /// Gets the current progress depth.
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Increments the progress depth for traversing to a lower level.
        /// </summary>
        public virtual void Increment()
        {
            Depth++;
        }

        /// <summary>
        /// Decrements the progress for traversing back to a higher level.
        /// </summary>
        public virtual void Decrement()
        {
            Depth--;
        }

        /// <summary>
        /// Decrements the progress back to the level it was at when this depth manager began.
        /// </summary>
        public virtual void Dispose()
        {
            Decrement();
        }
    }
}
