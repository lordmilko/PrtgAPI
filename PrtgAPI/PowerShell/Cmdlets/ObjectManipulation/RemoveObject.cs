using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Permanently remove an object from PRTG.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "Object", SupportsShouldProcess = true)]
    public class RemoveObject : PrtgCmdlet
    {
        /// <summary>
        /// The object to remove.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// Forces an object to be removed without displaying a confirmation prompt.
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (Object.Name == null && Object.Id == default(int))
                throw new ParameterBindingException($"Cannot bind an incomplete object to parameter '{Object}'");

            if(ShouldProcess($"'{Object.Name}' (ID: {Object.Id})"))
            {
                if(Force.IsPresent || ShouldContinue($"Are you sure you want to delete {Object.BaseType.ToString().ToLower()} '{Object.Name}' (ID: {Object.Id})", "WARNING!"))
                    client.Delete(Object.Id);
            }
        }
    }
}
