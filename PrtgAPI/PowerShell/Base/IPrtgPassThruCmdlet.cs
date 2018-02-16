using System.Management.Automation;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Represents a cmdlet type capable of sending its pipeline inputs through as outputs.
    /// </summary>
    public interface IPrtgPassThruCmdlet
    {
        /// <summary>
        /// Returns the original <see cref="PrtgObject"/> that was passed to this cmdlet, allowing the object to be further piped into additional cmdlets.
        /// </summary>
        SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Writes the specified object to the pipeline if <see cref="PassThru"/> is specified.
        /// </summary>
        /// <param name="obj">The object to write to the pipeline.</param>
        void PassThruObject(object obj);
    }
}
