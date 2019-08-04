using System.Diagnostics;
using System.Management.Automation;

namespace PrtgAPI.PowerShell.Progress
{
    class ProgressWriter : IProgressWriter
    {
        private PSCmdlet cmdlet;

        internal ProgressWriter(PSCmdlet cmdlet)
        {
            this.cmdlet = cmdlet;
        }

        public void WriteProgress(ProgressRecordEx progressRecord)
        {
#if DEBUG
            Debug.WriteLine($"Writing progress record {progressRecord}");
#endif

            cmdlet.WriteProgress(progressRecord);
        }

        public void WriteProgress(long sourceId, ProgressRecordEx progressRecord)
        {
            cmdlet.CommandRuntime.WriteProgress(sourceId, progressRecord);
        }
    }
}
