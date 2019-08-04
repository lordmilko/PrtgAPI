using System.Management.Automation;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Represents a cmdlet type capable of sending its pipeline inputs through as outputs.
    /// </summary>
    public interface IPrtgPassThruCmdlet
    {
        /// <summary>
        /// Specifies whether to return the original <see cref="IObject"/> that was passed to this cmdlet, allowing the object to be further piped into additional cmdlets.
        /// </summary>
        SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        object PassThruObject { get; }

        /// <summary>
        /// Writes the current <see cref="PassThruObject"/> to the pipeline if <see cref="PassThru"/> is specified.
        /// </summary>
        void WritePassThru();
    }
}
