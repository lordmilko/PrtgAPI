using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

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
            cmdlet.WriteProgress(progressRecord);
        }

        public void WriteProgress(long sourceId, ProgressRecordEx progressRecord)
        {
            cmdlet.CommandRuntime.WriteProgress(sourceId, progressRecord);
        }
    }
}
