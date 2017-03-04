using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Permanently remove an object from PRTG.</para>
    /// 
    /// <para type="description">The Remove-Object cmdlet permanently removes an object from PRTG. Some objects cannot be deleted
    /// (e.g. the root PRTG group (ID 0) and will generate an exception if you attempt to do so.</para>
    /// <para type="description">If invoked with no arguments other than the object to be deleted, Remove-Object will prompt for
    /// confirmation of each object to be deleted. If you wish to delete multiple objects, it is recommend to first run
    /// Remove-Object with the -WhatIf parameter, and then re-run with the -Force parameter if the results of -WhatIf look correct.</para>
    /// <para type="description">When invoked with -WhatIf, Remove-Object will list all objects that would have been deleted,
    /// along with their corresponding object IDs. Even if you are sure of the objects you wish to delete,
    /// it is recommended to always run with -WhatIf first to confirm you have specified the correct objects
    /// and that PrtgAPI has interpreted your request in the way you intended.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Device dc-1 | Remove-Object -WhatIf</code>
    ///     <para>"What if: Performing the operation "Remove-Object" on target "'dc-1' (ID: 2001)""</para>
    ///     <para>Preview what will happen when you attempt to remove all devices named 'dc-1'</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device dc-1 | Remove-Object -Force</code>
    ///     <para>Remove all devices with name 'dc-1' without prompting for confirmation.</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "Object", SupportsShouldProcess = true)]
    public class RemoveObject : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The object to remove.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">Forces an object to be removed without displaying a confirmation prompt.</para>
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
                    client.DeleteObject(Object.Id);
            }
        }
    }
}
