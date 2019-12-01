using System;

namespace PrtgAPI.Tree.Progress
{
    /// <summary>
    /// Manages the invocation of <see cref="ITreeProgressCallback"/> and <see cref="DepthManager"/> members.
    /// </summary>
    internal class TreeProgressManager
    {
        private ITreeProgressCallback progressCallback;

        private DepthManager depthManager;

        internal TreeProgressManager(ITreeProgressCallback progressCallback)
        {
            this.progressCallback = progressCallback;

            //In order to use DepthManager as an IDisposable everywhere, we need to have a dummy type
            //to call Dispose() on. As such, if we don't have a progressCallback we use the DefaultDepthManager.
            depthManager = progressCallback?.DepthManager ?? new DefaultDepthManager();
        }

        /// <summary>
        /// Increments the current level at which nodes are being processed and decrements it after the level has completed.
        /// </summary>
        /// <returns>A Depth Manager that will decrement the level when processing has completed.</returns>
        public IDisposable ProcessLevel()
        {
            depthManager.Increment();

            return depthManager;
        }

        public void OnProcessValue(ITreeValue value)
        {
            progressCallback?.OnProcessValue(value);
        }

        public void OnLevelBegin(ITreeValue value, PrtgNodeType type)
        {
            progressCallback?.OnLevelBegin(value, type, depthManager.Depth);
        }

        public void OnLevelWidthKnown(ITreeValue parent, PrtgNodeType parentType, int width)
        {
            progressCallback?.OnLevelWidthKnown(parent, parentType, width);
        }

        public void OnProcessType(PrtgNodeType type, int index, int total)
        {
            progressCallback?.OnProcessType(type, index, total);
        }
    }
}
