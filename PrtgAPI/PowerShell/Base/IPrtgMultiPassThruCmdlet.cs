using System.Collections.Generic;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Represents a type capable of sending multiple pipeline inputs through as outputs all at once.
    /// </summary>
    public interface IPrtgMultiPassThruCmdlet : IPrtgPassThruCmdlet
    {
        /// <summary>
        /// The objects that should be output from the cmdlet.
        /// </summary>
        List<object> PassThruObjects { get; }

        /// <summary>
        /// Stores the last object that was output from the cmdlet.
        /// </summary>
        object CurrentMultiPassThru { get; set; }

        /// <summary>
        /// Writes all objects stored in <see cref="PassThruObjects"/> if <see cref="IPrtgPassThruCmdlet.PassThru"/> is specified.
        /// </summary>
        void WriteMultiPassThru();
    }
}