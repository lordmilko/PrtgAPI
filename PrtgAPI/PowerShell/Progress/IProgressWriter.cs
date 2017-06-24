using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell
{
    interface IProgressWriter
    {
        /// <summary>
        /// Write a new process record.
        /// </summary>
        /// <param name="progressRecord">The progress record to write.</param>
        void WriteProgress(ProgressRecord progressRecord);

        /// <summary>
        /// Update a previous progress record, or if the progressRecord.ParentActivityId is specified, make a progress record a child of its parent.
        /// </summary>
        /// <param name="sourceId">The source ID of the previously written record.</param>
        /// <param name="progressRecord">The progress record to write.</param>
        void WriteProgress(long sourceId, ProgressRecord progressRecord);
    }
}
