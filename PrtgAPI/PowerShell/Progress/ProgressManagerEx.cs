using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell.Progress
{
    /// <summary>
    /// Extended progress state that must be saved between <see cref="ProgressManager"/> teardowns with each call to ProcessRecord
    /// </summary>
    class ProgressManagerEx
    {
        internal Pipeline BlockingSelectPipeline { get; set; }
    }
}
