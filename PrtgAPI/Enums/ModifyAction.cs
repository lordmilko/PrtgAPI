using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies ways in which an object can be modified.
    /// </summary>
    public enum ModifyAction
    {
        /// <summary>
        /// Add a new object.
        /// </summary>
        Add,

        /// <summary>
        /// Edit an existing object.
        /// </summary>
        Edit
    }
}
