using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.PowerShell.Progress
{
    /// <summary>
    /// Specifies the progress processing mode of a cmdlet.
    /// </summary>
    public enum ProcessingOperation
    {
        /// <summary>
        /// Retrieving items from the server.
        /// </summary>
        Retrieving,

        /// <summary>
        /// Processing previously retrieved items from the server.
        /// </summary>
        Processing
    };
}
