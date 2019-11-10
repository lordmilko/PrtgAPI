namespace PrtgAPI.Tree.Progress
{
    /// <summary>
    /// Specifies behaviors a type should implement to retrieve notifications of tree node construction.
    /// </summary>
    public interface ITreeProgressCallback
    {
        /// <summary>
        /// Gets a Depth Manager capable of managing how the display format should be altered when
        /// displaying progress at a different level.
        /// </summary>
        DepthManager DepthManager { get; }

        /// <summary>
        /// Called when a new level has begun.
        /// </summary>
        /// <param name="parent">The parent of the items being processed at the current level.</param>
        /// <param name="parentType">The type of the parent object.</param>
        /// <param name="depth">The depth of the new level. This value is the same as <see cref="DepthManager.Depth"/>.</param>
        void OnLevelBegin(ITreeValue parent, PrtgNodeType parentType, int depth);

        /// <summary>
        /// Called when the total number of items at the current level is finally known.
        /// </summary>
        /// <param name="parent">The parent of the items at the current level.</param>
        /// <param name="parentType">The type of the parent object.</param>
        /// <param name="width">The total number of items at the current level.</param>
        void OnLevelWidthKnown(ITreeValue parent, PrtgNodeType parentType, int width);

        /// <summary>
        /// Called when a new object at the current level is about to be processed.
        /// </summary>
        /// <param name="value">The value that will be processed.</param>
        void OnProcessValue(ITreeValue value);
    }
}
