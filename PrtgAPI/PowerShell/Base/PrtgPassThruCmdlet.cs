using System.Management.Automation;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for generic operation cmdlets capable of passing their inputs out to the pipeline.
    /// </summary>
    public abstract class PrtgPassThruCmdlet : PrtgOperationCmdlet, IPrtgPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">Specifies whether to return the original <see cref="IObject"/> that was passed to this cmdlet, allowing the object to be further piped into additional cmdlets.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Writes the current <see cref="PassThruObject"/> to the pipeline if <see cref="PassThru"/> is specified.
        /// </summary>
        public void WritePassThru()
        {
            if (PassThru)
            {
                WriteObject(PassThruObject);
            }
        }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public abstract object PassThruObject { get; }
    }
}
