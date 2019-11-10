using PrtgAPI.PowerShell.Base;
using PrtgAPI.PowerShell.Progress;
using PrtgAPI.Tree.Progress;
using System.Collections.Generic;

namespace PrtgAPI.PowerShell.Tree
{
    internal class PowerShellDepthManager : DepthManager
    {
        private PrtgCmdlet cmdlet;

        private Stack<ProgressManager> progressManagers = new Stack<ProgressManager>();

        public ProgressManager CurrentManager => progressManagers.Peek();

        private bool internalProgress;

        public PowerShellDepthManager(PrtgCmdlet cmdlet, bool internalProgress)
        {
            this.cmdlet = cmdlet;
            this.internalProgress = internalProgress;
        }

        public override void Increment()
        {
            //If we're the first progress manager to be added, fake there being no PreviousRecord - this should be a standalone
            //progress bar
            ProgressManager manager;

            if (progressManagers.Count == 0)
                manager = cmdlet.ProgressManager;
            else
                manager = new ProgressManager(cmdlet);

            manager.InternalProgress = internalProgress;

            progressManagers.Push(manager);

            base.Increment();
        }

        public override void Decrement()
        {
            var manager = progressManagers.Pop();

            if (manager.ProgressWritten)
                manager.CompleteProgress(true);

            manager.Dispose();

            base.Decrement();
        }
    }
}
