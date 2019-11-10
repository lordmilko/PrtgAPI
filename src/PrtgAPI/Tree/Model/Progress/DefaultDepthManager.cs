namespace PrtgAPI.Tree.Progress
{
    /// <summary>
    /// Dummy Depth Manager for use with using() when a progress callback was not specified.
    /// </summary>
    internal class DefaultDepthManager : DepthManager
    {
        public override void Increment()
        {
        }

        public override void Decrement()
        {
        }

        public override void Dispose()
        {
        }
    }
}
